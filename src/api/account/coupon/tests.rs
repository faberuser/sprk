use super::*;
use crate::{
    api::{account::user, community::mail, inventory::item},
    database,
};

async fn setup() -> (AppState, String, i64) {
    let db = sqlx::sqlite::SqlitePoolOptions::new()
        .max_connections(1)
        .connect("sqlite::memory:")
        .await
        .unwrap();
    database::create_tables(&db).await.unwrap();
    let s = AppState::new(db, crate::tables::test_tables());
    let u = user::test_login(State(s.clone()), Bytes::from_static(b"LoginId=coupon-test"))
        .await
        .unwrap()
        .0;
    (s, u.user_info.session_key, u.user_info.account_id)
}
fn body(key: &str, fields: &str) -> Bytes {
    Bytes::from(format!("SessionKey={key}&{fields}"))
}
async fn redeem(s: &AppState, key: &str, fields: &str) -> Value {
    use_coupon(State(s.clone()), body(key, fields))
        .await
        .unwrap()
        .0
}

#[tokio::test]
async fn coupon_mail_claim_and_sale_rejection() {
    let (s, key, a) = setup().await;
    assert_eq!(
        redeem(&s, &key, "CouponCode=PDFABERGIFT&CountryCode=TH").await["Result"],
        "Success"
    );
    assert_eq!(
        redeem(&s, &key, "CouponCode=%20pdfabergift%20&CountryCode=TH").await["Result"],
        "AlreadyReceived"
    );
    let mails: i64 =
        sqlx::query_scalar("SELECT COUNT(*) FROM mails WHERE account_id=? AND title='PDFABERGIFT'")
            .bind(a)
            .fetch_one(&s.db)
            .await
            .unwrap();
    assert_eq!(mails, 1);
    let id: i64 = sqlx::query_scalar("SELECT mail_id FROM coupon_redemptions WHERE account_id=?")
        .bind(a)
        .fetch_one(&s.db)
        .await
        .unwrap();
    let claimed = mail::receive_mail(State(s.clone()), body(&key, &format!("MailIndex={id}")))
        .await
        .unwrap()
        .0;
    assert_eq!(claimed["Result"], "Success", "{claimed}");
    let count: i64 =
        sqlx::query_scalar("SELECT count FROM items WHERE account_id=? AND item_index=9900001")
            .bind(a)
            .fetch_one(&s.db)
            .await
            .unwrap();
    assert_eq!(count, 1);
    let sold = item::sell_item(
        State(s.clone()),
        body(&key, "ItemIndices=[9900001]&ItemCount=[1]"),
    )
    .await
    .unwrap()
    .0;
    assert_ne!(sold["Result"], "Success");
    let count: i64 =
        sqlx::query_scalar("SELECT count FROM items WHERE account_id=? AND item_index=9900001")
            .bind(a)
            .fetch_one(&s.db)
            .await
            .unwrap();
    assert_eq!(count, 1);
    let data = &s.tables.inventory.items[&9900001];
    assert_eq!(data["Type"], 14);
    assert_eq!(data["NotForSale"], true);
    assert_eq!(s.tables.get_item_index("SPRK_XDD"), Some(9900001));
    // A different account gets its own gift.
    let other = user::test_login(
        State(s.clone()),
        Bytes::from_static(b"LoginId=coupon-other"),
    )
    .await
    .unwrap()
    .0;
    assert_eq!(
        redeem(
            &s,
            &other.user_info.session_key,
            "CouponCode=PDFABERGIFT&CountryCode=US"
        )
        .await["Result"],
        "Success"
    );
}

#[tokio::test]
async fn coupon_validation_and_authentication() {
    let (mut s, key, _) = setup().await;
    for (fields, result) in [
        ("CountryCode=TH", "MissingCouponCode"),
        ("CouponCode=PDFABERGIFT", "MissingCountryCode"),
        ("CouponCode=NOPE&CountryCode=TH", "CouponNotFound"),
    ] {
        assert_eq!(redeem(&s, &key, fields).await["Result"], result);
    }
    assert!(use_coupon(
        State(s.clone()),
        Bytes::from_static(b"CouponCode=PDFABERGIFT&CountryCode=TH")
    )
    .await
    .is_err());
    std::sync::Arc::make_mut(&mut std::sync::Arc::make_mut(&mut s.tables).coupons)
        .coupons
        .get_mut("PDFABERGIFT")
        .unwrap()
        .countries = vec!["TH".into()];
    assert_eq!(
        redeem(&s, &key, "CouponCode=PDFABERGIFT&CountryCode=US").await["Result"],
        "CountryCodeMismatch"
    );
    std::sync::Arc::make_mut(&mut std::sync::Arc::make_mut(&mut s.tables).coupons)
        .coupons
        .get_mut("PDFABERGIFT")
        .unwrap()
        .expires_at = Some(1);
    assert_eq!(
        redeem(&s, &key, "CouponCode=PDFABERGIFT&CountryCode=TH").await["Result"],
        "CouponExpired"
    );
    let count: i64 = sqlx::query_scalar("SELECT COUNT(*) FROM coupon_redemptions")
        .fetch_one(&s.db)
        .await
        .unwrap();
    assert_eq!(count, 0);
}

#[tokio::test]
async fn coupon_retries_and_failed_mail_are_atomic() {
    let (s, key, _) = setup().await;
    sqlx::query("CREATE TRIGGER fail_coupon_mail BEFORE INSERT ON mails WHEN NEW.title='PDFABERGIFT' BEGIN SELECT RAISE(ABORT,'test failure'); END")
        .execute(&s.db).await.unwrap();
    assert!(use_coupon(
        State(s.clone()),
        body(&key, "CouponCode=PDFABERGIFT&CountryCode=TH")
    )
    .await
    .is_err());
    let count: i64 = sqlx::query_scalar("SELECT COUNT(*) FROM coupon_redemptions")
        .fetch_one(&s.db)
        .await
        .unwrap();
    assert_eq!(count, 0);
    sqlx::query("DROP TRIGGER fail_coupon_mail")
        .execute(&s.db)
        .await
        .unwrap();
    let (one, two) = tokio::join!(
        redeem(&s, &key, "CouponCode=PDFABERGIFT&CountryCode=TH"),
        redeem(&s, &key, "CouponCode=PDFABERGIFT&CountryCode=TH")
    );
    let mut results = vec![
        one["Result"].as_str().unwrap(),
        two["Result"].as_str().unwrap(),
    ];
    results.sort();
    assert_eq!(results, vec!["AlreadyReceived", "Success"]);
}
