//! Custom profile pictures (SPRK). A player may store one small square PNG,
//! shown instead of the leader hero's icon as their profile picture. The
//! leader hero itself (lobby avatar, My Info card) is unaffected.
//!
//! The client (`SprkProfileImage`) crops and scales the picture before
//! uploading, so the server only checks that it is a PNG of a sensible size.
//! Every reply carries `"SprkProfileImage":1`: the generic fallback answers
//! unknown routes with Success, and the client must not mistake an older
//! server's fallback for a stored picture.
use axum::{body::Bytes, extract::State, Json};
use base64::{engine::general_purpose::STANDARD, Engine};
use serde_json::{json, Value};
use crate::{api::system::request::Request, error::Result, state::AppState};

/// Decoded PNG size limit; the client sends 256x256 pictures (usually < 200 KiB).
pub(crate) const MAX_BYTES: usize = 512 * 1024;
pub(crate) const MIN_SIDE: u32 = 16;
pub(crate) const MAX_SIDE: u32 = 512;

/// Checks the PNG signature, the IHDR dimensions and the IEND trailer.
/// Pixel data is never decoded here; clients decode it with Texture2D.LoadImage.
fn validate(png: &[u8]) -> bool {
    png.len() <= MAX_BYTES && png_size(png).is_some_and(|(width, height)|
        (MIN_SIDE..=MAX_SIDE).contains(&width) && (MIN_SIDE..=MAX_SIDE).contains(&height))
}

/// Width and height of a structurally plausible PNG (signature, IHDR, IEND).
pub(crate) fn png_size(png: &[u8]) -> Option<(u32, u32)> {
    const SIGNATURE: &[u8] = b"\x89PNG\r\n\x1a\n";
    if png.len() < 8 + 25 + 12 || !png.starts_with(SIGNATURE) {
        return None;
    }
    let ihdr = &png[8..];
    if ihdr[0..4] != [0, 0, 0, 13] || &ihdr[4..8] != b"IHDR" || &png[png.len() - 8..png.len() - 4] != b"IEND" {
        return None;
    }
    let side = |at: usize| u32::from_be_bytes([ihdr[at], ihdr[at + 1], ihdr[at + 2], ihdr[at + 3]]);
    Some((side(8), side(12)))
}

/// `AccountId` (optional, default: the caller) selects whose picture to read,
/// so other players' views can show it too. Missing pictures return `Image=""`.
pub async fn get(State(state): State<AppState>, body: Bytes) -> Result<Json<Value>> {
    let req = Request::parse(&body)?;
    let caller = req.account(&state)?;
    let target = req.number("AccountId", caller)?;
    let image: Option<(Vec<u8>, String)> = crate::db::query_as("SELECT image,updated_at FROM profile_images WHERE account_id=$1")
        .bind(target).fetch_optional(&state.db).await?;
    let (image, version) = image.map(|(png, at)| (STANDARD.encode(png), at)).unwrap_or_default();
    Ok(Json(json!({"BaseResult":"Success","SprkProfileImage":1,"Result":"Success","AccountId":target,"Image":image,"Version":version})))
}

/// Most players have no picture, so lists (friends, guild, chat, rankings) ask
/// first which of up to 100 `AccountIds` (JSON array) have one, and at which
/// `Version`; clients then `get` only new or changed pictures.
/// `Versions` maps account id (string) to version, for accounts with a picture.
pub async fn list(State(state): State<AppState>, body: Bytes) -> Result<Json<Value>> {
    let req = Request::parse(&body)?;
    req.account(&state)?;
    let ids = req.ids_with_limit("AccountIds", MAX_LIST)?;
    let rows: Vec<(i64, String)> = if ids.is_empty() { Vec::new() } else {
        crate::db::query_as("SELECT account_id,updated_at FROM profile_images WHERE account_id IN (SELECT jsonb_array_elements_text($1::jsonb)::bigint)")
            .bind(serde_json::to_string(&ids).unwrap_or_default()).fetch_all(&state.db).await?
    };
    let versions: serde_json::Map<String, Value> = rows.into_iter().map(|(id, at)| (id.to_string(), Value::String(at))).collect();
    Ok(Json(json!({"BaseResult":"Success","SprkProfileImage":1,"Result":"Success","Versions":versions})))
}

pub(crate) const MAX_LIST: usize = 100;

/// `Image`: base64 PNG. Replaces any earlier picture.
pub async fn set(State(state): State<AppState>, body: Bytes) -> Result<Json<Value>> {
    let req = Request::parse(&body)?;
    let account = req.account(&state)?;
    let png = match STANDARD.decode(req.text("Image").trim()) {
        Ok(png) if validate(&png) => png,
        _ => return Ok(Json(json!({"BaseResult":"Success","SprkProfileImage":1,"Result":"InvalidImage"}))),
    };
    let version: String = crate::db::query_scalar("INSERT INTO profile_images(account_id,image) VALUES($1,$2) ON CONFLICT(account_id) DO UPDATE SET image=EXCLUDED.image,updated_at=sprk_now() RETURNING updated_at")
        .bind(account).bind(&png).fetch_one(&state.db).await?;
    Ok(Json(json!({"BaseResult":"Success","SprkProfileImage":1,"Result":"Success","Version":version})))
}

pub async fn remove(State(state): State<AppState>, body: Bytes) -> Result<Json<Value>> {
    let req = Request::parse(&body)?;
    let account = req.account(&state)?;
    crate::db::query("DELETE FROM profile_images WHERE account_id=$1").bind(account).execute(&state.db).await?;
    Ok(Json(json!({"BaseResult":"Success","SprkProfileImage":1,"Result":"Success"})))
}

#[cfg(test)]
mod tests {
    use super::*;
    use crate::api::community::tests::{login, setup};

    /// A structurally valid PNG header + IEND; enough for the server's checks.
    fn png(width: u32, height: u32, padding: usize) -> Vec<u8> {
        let mut out = b"\x89PNG\r\n\x1a\n".to_vec();
        out.extend([0, 0, 0, 13]);
        out.extend(b"IHDR");
        out.extend(width.to_be_bytes());
        out.extend(height.to_be_bytes());
        out.extend([8, 6, 0, 0, 0, 0, 0, 0, 0]);
        out.extend(vec![0u8; padding]);
        out.extend([0, 0, 0, 0]);
        out.extend(b"IEND");
        out.extend([0xae, 0x42, 0x60, 0x82]);
        out
    }

    fn form(user: &Value, extra: &str) -> Bytes {
        Bytes::from(format!("SessionKey={}&{extra}", user["UserInfo"]["SessionKey"].as_str().unwrap()))
    }

    fn image_field(png: &[u8]) -> String {
        format!("Image={}", urlencoding::encode(&STANDARD.encode(png)))
    }

    #[test]
    fn only_bounded_pngs_are_accepted() {
        assert!(validate(&png(256, 256, 0)));
        assert!(validate(&png(MIN_SIDE, MAX_SIDE, 100)));
        assert!(!validate(&png(MIN_SIDE - 1, 256, 0)));
        assert!(!validate(&png(256, MAX_SIDE + 1, 0)));
        assert!(!validate(&png(256, 256, MAX_BYTES)));
        let mut jpeg = png(256, 256, 0);
        jpeg[0] = 0xff;
        assert!(!validate(&jpeg));
        let mut truncated = png(256, 256, 10);
        truncated.truncate(truncated.len() - 12);
        assert!(!validate(&truncated));
        assert!(!validate(b""));
    }

    #[tokio::test]
    async fn profile_images_upload_replace_read_by_others_and_remove() {
        let s = setup().await;
        let a = login(&s, "profile-image-a").await;
        let b = login(&s, "profile-image-b").await;
        let a_id = a["UserInfo"]["AccountId"].as_i64().unwrap();
        let get_image = |user: &Value, extra: &str| {
            let (s, body) = (s.clone(), form(user, extra));
            async move { get(State(s), body).await.unwrap().0 }
        };

        // Nothing stored yet; a session is required for every action.
        assert_eq!(get_image(&a, "").await["Image"], "");
        assert!(set(State(s.clone()), Bytes::from(image_field(&png(64, 64, 0)))).await.is_err());
        assert!(get(State(s.clone()), Bytes::from("AccountId=1")).await.is_err());

        // Invalid data is rejected without storing anything.
        for bad in ["Image=", "Image=not-base64!", &image_field(&png(4096, 4096, 0)), &image_field(b"GIF89a")] {
            assert_eq!(set(State(s.clone()), form(&a, bad)).await.unwrap().0["Result"], "InvalidImage", "{bad}");
        }
        assert_eq!(get_image(&a, "").await["Image"], "");

        // Upload, then replace: one row per player, latest picture wins.
        let first = png(256, 256, 1);
        let second = png(128, 128, 2);
        for image in [&first, &second] {
            let out = set(State(s.clone()), form(&a, &image_field(image))).await.unwrap().0;
            assert_eq!(out["Result"], "Success");
            assert!(!out["Version"].as_str().unwrap().is_empty());
        }
        assert_eq!(crate::db::query_scalar::<i64>("SELECT COUNT(*) FROM profile_images").fetch_one(&s.db).await.unwrap(), 1);
        let own = get_image(&a, "").await;
        assert_eq!(own["SprkProfileImage"], 1);
        assert_eq!(STANDARD.decode(own["Image"].as_str().unwrap()).unwrap(), second);
        assert_eq!(own["AccountId"], a_id);

        // Another player can read it by AccountId but has none of their own.
        let seen = get_image(&b, &format!("AccountId={a_id}")).await;
        assert_eq!(seen["Image"], own["Image"]);
        assert_eq!(get_image(&b, "").await["Image"], "");

        // Removing affects only the caller.
        assert_eq!(set(State(s.clone()), form(&b, &image_field(&first))).await.unwrap().0["Result"], "Success");
        assert_eq!(remove(State(s.clone()), form(&a, "")).await.unwrap().0["Result"], "Success");
        assert_eq!(get_image(&a, "").await["Image"], "");
        assert_ne!(get_image(&b, "").await["Image"], "");
        assert_eq!(remove(State(s.clone()), form(&a, "")).await.unwrap().0["Result"], "Success");
    }

    #[tokio::test]
    async fn profile_image_lists_report_versions_only_for_players_with_pictures() {
        let s = setup().await;
        let a = login(&s, "profile-image-list-a").await;
        let b = login(&s, "profile-image-list-b").await;
        let (a_id, b_id) = (a["UserInfo"]["AccountId"].as_i64().unwrap(), b["UserInfo"]["AccountId"].as_i64().unwrap());
        let ids = |list: &str| format!("AccountIds={}", urlencoding::encode(list));
        let versions = |user: &Value, extra: String| {
            let (s, body) = (s.clone(), form(user, &extra));
            async move {
                let out = list(State(s), body).await.unwrap().0;
                assert_eq!((out["Result"].as_str(), out["SprkProfileImage"].as_i64()), (Some("Success"), Some(1)));
                out["Versions"].as_object().unwrap().clone()
            }
        };
        assert!(versions(&b, ids(&format!("[{a_id},{b_id}]"))).await.is_empty());
        assert!(versions(&b, String::new()).await.is_empty());
        let first = set(State(s.clone()), form(&a, &image_field(&png(64, 64, 0)))).await.unwrap().0["Version"].clone();
        // A single id, a JSON array and unknown ids all work; only A has a picture.
        for list_ids in [a_id.to_string(), format!("[{a_id},{b_id},999999]")] {
            let seen = versions(&b, ids(&list_ids)).await;
            assert_eq!(seen.len(), 1, "{list_ids}");
            assert_eq!(seen[&a_id.to_string()], first);
        }
        // Replacing changes the version, so clients re-download it.
        tokio::time::sleep(std::time::Duration::from_millis(1100)).await;
        let second = set(State(s.clone()), form(&a, &image_field(&png(64, 64, 1)))).await.unwrap().0["Version"].clone();
        assert_ne!(second, first);
        assert_eq!(versions(&b, ids(&a_id.to_string())).await[&a_id.to_string()], second);
        assert_eq!(remove(State(s.clone()), form(&a, "")).await.unwrap().0["Result"], "Success");
        assert!(versions(&b, ids(&a_id.to_string())).await.is_empty());
        // Bounded, positive ids, and a session.
        let too_many = format!("[{}]", (1..=MAX_LIST as i64 + 1).map(|i| i.to_string()).collect::<Vec<_>>().join(","));
        assert!(list(State(s.clone()), form(&b, &ids(&too_many))).await.is_err());
        assert!(list(State(s.clone()), form(&b, &ids("[0]"))).await.is_err());
        assert!(list(State(s.clone()), Bytes::from(ids(&a_id.to_string()))).await.is_err());
    }

    /// Through the real router and middleware: the client sends the largest
    /// accepted picture form-encoded (base64, then URL-escaped).
    #[tokio::test]
    async fn profile_image_routes_accept_the_largest_picture_through_the_game_router() {
        use tower::ServiceExt;
        let s = setup().await;
        let a = login(&s, "profile-image-route").await;
        let largest = png(MAX_SIDE, MAX_SIDE, MAX_BYTES - png(MAX_SIDE, MAX_SIDE, 0).len());
        assert_eq!(largest.len(), MAX_BYTES);
        let call = |path: &'static str, body: Bytes| {
            let app = crate::game_routes(s.clone());
            async move {
                let response = app.oneshot(axum::http::Request::post(path)
                    .header("content-type", "application/x-www-form-urlencoded")
                    .body(axum::body::Body::from(body)).unwrap()).await.unwrap();
                assert!(response.status().is_success(), "{path}: {}", response.status());
                let bytes = axum::body::to_bytes(response.into_body(), usize::MAX).await.unwrap();
                serde_json::from_slice::<Value>(&bytes).unwrap()
            }
        };
        let upload = form(&a, &image_field(&largest));
        assert!(upload.len() > 512 * 1024 && upload.len() < 2 * 1024 * 1024, "{}", upload.len());
        assert_eq!(call("/sprk/profile_image/set", upload).await["Result"], "Success");
        let got = call("/sprk/profile_image/get", form(&a, "")).await;
        assert_eq!(STANDARD.decode(got["Image"].as_str().unwrap()).unwrap(), largest);
        let removed = call("/sprk/profile_image/remove", form(&a, "")).await;
        assert_eq!((removed["Result"].as_str(), removed["SprkProfileImage"].as_i64()), (Some("Success"), Some(1)));
        // An unknown route reaches the generic fallback, which has no marker.
        assert!(call("/sprk/profile_image/unknown", form(&a, "")).await.get("SprkProfileImage").is_none());
    }

    #[tokio::test]
    async fn profile_images_are_removed_with_a_deleted_account() {
        let db = crate::database::test_pool().await;
        crate::db::query("INSERT INTO accounts(account_id,login_id) VALUES(1,'profile-image-delete')").execute(&db).await.unwrap();
        crate::db::query("INSERT INTO profile_images(account_id,image) VALUES(1,$1)").bind(png(64, 64, 0)).execute(&db).await.unwrap();
        crate::db::query("DELETE FROM accounts WHERE account_id=1").execute(&db).await.unwrap();
        assert_eq!(crate::db::query_scalar::<i64>("SELECT COUNT(*) FROM profile_images").fetch_one(&db).await.unwrap(), 0);
    }
}
