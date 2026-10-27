//! Account-scoped acknowledgement of mission-category New markers.
use super::*;

pub(crate) async fn sync(State(s): State<AppState>, body: Bytes) -> Result<Json<Value>> {
    let req = Request::parse(&body)?;
    let account = req.account(&s)?;
    let incoming: Vec<String> = serde_json::from_str(
        req.0
            .get("SeenCategories")
            .map(String::as_str)
            .unwrap_or("[]"),
    )
    .map_err(|_| rule("InvalidValue"))?;
    if incoming.len() > 2048 {
        return Err(rule("InvalidCount"));
    }
    let mut tx = s.db.begin().await?;
    let mut pairs = BTreeSet::new();
    if !incoming.is_empty() {
        let opened = mission_categories(&mut tx, &s, account).await?;
        for key in incoming {
            let (main, sub) = key.split_once(':').ok_or_else(|| rule("InvalidValue"))?;
            let (main, sub): (i64, i64) = (
                main.parse().map_err(|_| rule("InvalidValue"))?,
                sub.parse().map_err(|_| rule("InvalidValue"))?,
            );
            if !opened
                .as_array()
                .unwrap()
                .iter()
                .any(|v| n(v, "MainCategoryIndex") == main && n(v, "SubCategoryIndex") == sub)
            {
                return Err(rule("InvalidValue"));
            }
            pairs.insert((main, sub));
        }
    }
    for (main, sub) in pairs {
        sqlx::query("INSERT OR IGNORE INTO mission_category_seen(account_id,main_category,sub_category) VALUES(?,?,?)")
            .bind(account).bind(main).bind(sub).execute(&mut *tx).await?;
    }
    let seen: Vec<(i64,i64)> = sqlx::query_as("SELECT main_category,sub_category FROM mission_category_seen WHERE account_id=? ORDER BY main_category,sub_category")
        .bind(account).fetch_all(&mut *tx).await?;
    tx.commit().await?;
    Ok(Json(
        json!({"BaseResult":"Success","Result":"Success","SeenMissionCategories":seen.iter().map(|(main,sub)|format!("{main}:{sub}")).collect::<Vec<_>>()}),
    ))
}

#[cfg(test)]
mod tests {
    use super::*;
    #[tokio::test]
    async fn seen_categories_merge_across_sessions_and_isolate_accounts() {
        let (s, u) = super::super::tests::setup().await;
        let a = u.user_info.account_id;
        let opened = mission_categories(&mut s.db.acquire().await.unwrap(), &s, a)
            .await
            .unwrap();
        let first = &opened[0];
        let key = format!(
            "{}:{}",
            n(first, "MainCategoryIndex"),
            n(first, "SubCategoryIndex")
        );
        let session = &u.user_info.session_key;
        let send = |key: &str, values: Vec<String>| {
            Bytes::from(
                serde_urlencoded::to_string([
                    ("SessionKey", key.to_owned()),
                    ("SeenCategories", json!(values).to_string()),
                ])
                .unwrap(),
            )
        };
        let _ = sync(
            State(s.clone()),
            send(session, vec![key.clone(), key.clone()]),
        )
        .await
        .unwrap();
        let restarted = AppState::new(s.db.clone(), s.tables.as_ref().clone());
        let session = "seen-category-device";
        restarted.create_session(session.into(), a, String::new());
        let out = sync(State(restarted.clone()), send(&session, vec![]))
            .await
            .unwrap()
            .0;
        assert_eq!(out["SeenMissionCategories"], json!([key]));
        assert!(sync(
            State(restarted.clone()),
            send(&session, vec!["999999:999999".into()])
        )
        .await
        .is_err());
        assert_eq!(
            sqlx::query_scalar::<_, i64>("SELECT count(*) FROM mission_category_seen")
                .fetch_one(&s.db)
                .await
                .unwrap(),
            1
        );
        restarted.create_session("other-device".into(), a + 999, String::new());
        assert_eq!(
            sync(State(restarted), send("other-device", vec![]))
                .await
                .unwrap()
                .0["SeenMissionCategories"],
            json!([])
        );
    }
}
