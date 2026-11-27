use super::*;
use crate::api::community::tests::{login, setup};

/// A structurally valid PNG header + IEND; enough for the server's checks.
fn png(side: u32) -> Vec<u8> {
    let mut out = b"\x89PNG\r\n\x1a\n".to_vec();
    out.extend([0, 0, 0, 13]);
    out.extend(b"IHDR");
    out.extend(side.to_be_bytes());
    out.extend(side.to_be_bytes());
    out.extend([8, 6, 0, 0, 0, 0, 0, 0, 0]);
    out.extend([0, 0, 0, 0]);
    out.extend(b"IEND");
    out.extend([0xae, 0x42, 0x60, 0x82]);
    out
}

async fn gm_call(s: &AppState, args: &str) -> Result<Value> {
    gm(State(s.clone()), Bytes::from(args.to_owned())).await.map(|j| j.0)
}

fn icon_field(png: &[u8]) -> String {
    format!("Icon={}", urlencoding::encode(&STANDARD.encode(png)))
}

fn marked(tag: i64, nick: &str) -> String {
    format!("{MARK_START}{tag}{MARK_END}{nick}")
}

#[tokio::test]
async fn gm_creates_assigns_and_deletes_tags_shown_before_the_raw_nick() {
    let s = setup().await;
    let a = login(&s, "name-tag-a").await;
    let b = login(&s, "name-tag-b").await;
    let a_id = a["UserInfo"]["AccountId"].as_i64().unwrap();
    let nick = a["UserInfo"]["Nick"].as_str().unwrap().to_owned();
    assert!(!nick.contains(MARK_START));

    // Validation: handle, label, colours, icon size; nothing is stored.
    for bad in [
        "Action=create&Name=&Label=WAO",
        "Action=create&Name=Bad Name&Label=WAO",
        "Action=create&Name=wao&Label=",
        "Action=create&Name=wao&Label=TOOLONGTAG",
        "Action=create&Name=wao&Label=%5Bb%5D",
        "Action=create&Name=wao&Label=WAO&TextColor=red",
        "Action=bogus",
    ] {
        assert!(gm_call(&s, bad).await.is_err(), "{bad}");
    }
    assert!(gm_call(&s, &format!("Action=create&Name=wao&Label=WAO&{}", icon_field(&png(512)))).await.is_err());
    assert_eq!(gm_call(&s, "Action=list").await.unwrap()["Tags"], json!([]));

    // Create with a small icon and colours; the handle is case-insensitive.
    let created = gm_call(&s, &format!("Action=create&Name=WAO&Label=WAO&TextColor=%23a8e6a1&BackgroundColor=1e1f22&{}", icon_field(&png(64)))).await.unwrap();
    let tag = created["Tag"]["TagId"].as_i64().unwrap();
    assert_eq!(created["Tag"]["Name"], "wao");
    assert_eq!(created["Tag"]["TextColor"], "A8E6A1");
    assert_eq!(created["Tag"]["HasIcon"], true);
    assert!(gm_call(&s, "Action=create&Name=wao&Label=DUP").await.is_err());

    // Assigning to a missing account or with an unknown tag fails.
    assert!(gm_call(&s, "Action=assign&Name=wao&AccountId=999999").await.is_err());
    assert!(gm_call(&s, &format!("Action=assign&Name=nope&AccountId={a_id}")).await.is_err());

    // Before assignment the name is plain everywhere.
    let shown: String = crate::db::query_scalar("SELECT sprk_display_nick(account_id,nick) FROM accounts WHERE account_id=$1")
        .bind(a_id).fetch_one(&s.db).await.unwrap();
    assert_eq!(shown, nick);

    gm_call(&s, &format!("Action=assign&Name=wao&AccountId={a_id}")).await.unwrap();
    // The stored nick stays raw; what players see carries the marker,
    // including the tagged player's own login snapshot.
    let raw: String = crate::db::query_scalar("SELECT nick FROM accounts WHERE account_id=$1").bind(a_id).fetch_one(&s.db).await.unwrap();
    assert_eq!(raw, nick);
    let again = login(&s, "name-tag-a").await;
    assert_eq!(again["UserInfo"]["Nick"], marked(tag, &nick));
    let members = gm_call(&s, "Action=members&Name=wao").await.unwrap();
    assert_eq!(members["Members"][0]["AccountId"], a_id);
    assert_eq!(members["Members"][0]["Nickname"], nick);

    // Other players fetch the tag's look (with the icon) by id.
    let session = |u: &Value| u["UserInfo"]["SessionKey"].as_str().unwrap().to_owned();
    assert!(get(State(s.clone()), Bytes::from(format!("TagIds=[{tag}]"))).await.is_err());
    let look = get(State(s.clone()), Bytes::from(format!("SessionKey={}&TagIds=[{tag},987654]", session(&b)))).await.unwrap().0;
    assert_eq!(look["SprkNameTag"], 1);
    assert_eq!(look["Tags"][tag.to_string()]["Label"], "WAO");
    assert_eq!(STANDARD.decode(look["Tags"][tag.to_string()]["Icon"].as_str().unwrap()).unwrap(), png(64));
    assert!(look["Tags"].get("987654").is_none());

    // Updates keep unspecified fields; RemoveIcon drops the icon.
    let updated = gm_call(&s, "Action=update&Name=wao&Label=GM&RemoveIcon=true").await.unwrap();
    assert_eq!(updated["Tag"]["Label"], "GM");
    assert_eq!(updated["Tag"]["TextColor"], "A8E6A1");
    assert_eq!(updated["Tag"]["HasIcon"], false);

    // A second tag replaces the first (one per player); unassign clears it.
    let second = gm_call(&s, "Action=create&Name=staff&Label=STAFF").await.unwrap()["Tag"]["TagId"].as_i64().unwrap();
    gm_call(&s, &format!("Action=assign&Name=staff&AccountId={a_id}")).await.unwrap();
    assert_eq!(login(&s, "name-tag-a").await["UserInfo"]["Nick"], marked(second, &nick));
    let removed = gm_call(&s, &format!("Action=unassign&AccountId={a_id}")).await.unwrap();
    assert_eq!(removed["Removed"], "staff");
    assert_eq!(login(&s, "name-tag-a").await["UserInfo"]["Nick"], nick);

    // Deleting a tag unassigns its members.
    gm_call(&s, &format!("Action=assign&Name=wao&AccountId={a_id}")).await.unwrap();
    let deleted = gm_call(&s, "Action=delete&Name=wao").await.unwrap();
    assert_eq!(deleted["Unassigned"], 1);
    assert_eq!(login(&s, "name-tag-a").await["UserInfo"]["Nick"], nick);
    assert_eq!(crate::db::query_scalar::<i64>("SELECT COUNT(*) FROM account_name_tags").fetch_one(&s.db).await.unwrap(), 0);
}

#[tokio::test]
async fn player_text_cannot_carry_markers() {
    let fake = format!("{MARK_START}1{MARK_END}Admin");
    let encoded = urlencoding::encode(&fake).into_owned();
    // Plain form decoding, the native client's double escaping, and arrays.
    let req = Request::parse(format!("Nick={encoded}&Chat={}", urlencoding::encode(&encoded)).as_bytes()).unwrap();
    assert_eq!(req.text("Nick"), "1Admin");
    assert_eq!(req.escaped_text("Chat"), "1Admin");
    let req = Request::parse_with_arrays(format!("Names={encoded}&Names=x").as_bytes(), &["Names"]).unwrap();
    assert!(!req.text("Names").contains(MARK_START));
    assert_eq!(strip_markers(fake), "1Admin");
}
