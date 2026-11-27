//! Form parsing shared by the APIs. Arrays are JSON strings in this client.
use crate::{
    error::{Result, ServerError},
    state::AppState,
};
use std::collections::{BTreeSet, HashMap};

pub struct Request(pub HashMap<String, String>);
impl Request {
    /// Unity's WWWForm emits one field per array element. Keep every element,
    /// while also accepting JSON arrays used by API clients and tests.
    pub fn parse_with_arrays(body: &[u8], array_fields: &[&str]) -> Result<Self> {
        let pairs: Vec<(String, String)> = serde_urlencoded::from_bytes(body)
            .map_err(|e| ServerError::InvalidRequest(e.to_string()))?;
        let mut fields = HashMap::new();
        let mut arrays: HashMap<String, Vec<String>> = HashMap::new();
        for (key, value) in pairs {
            let value = crate::api::account::name_tag::strip_markers(value);
            if array_fields.contains(&key.as_str()) {
                arrays.entry(key).or_default().push(value);
            } else {
                fields.insert(key, value);
            }
        }
        for (key, mut values) in arrays {
            let value = if values.len() == 1
                && (values[0].trim_start().starts_with('[')
                    || values[0].is_empty() || values[0] == "null")
            {
                values.remove(0)
            } else {
                serde_json::to_string(&values)
                    .map_err(|e| ServerError::InvalidRequest(e.to_string()))?
            };
            fields.insert(key, value);
        }
        Ok(Self(fields))
    }
    /// Player text never carries name-tag markers: only the server adds them,
    /// so a pasted marker cannot fake a tag in chat, names or notices.
    pub fn parse(body: &[u8]) -> Result<Self> {
        serde_urlencoded::from_bytes::<HashMap<String, String>>(body)
            .map(|fields| Self(fields.into_iter()
                .map(|(k, v)| (k, crate::api::account::name_tag::strip_markers(v))).collect()))
            .map_err(|e| ServerError::InvalidRequest(e.to_string()))
    }
    /// Keep native repeated integer arrays without losing their order or pairing.
    pub fn parse_with_integer_arrays(body: &[u8], array_fields: &[&str]) -> Result<Self> {
        let mut req = Self::parse_with_arrays(body, array_fields)?;
        for key in array_fields {
            if let Ok(values) = serde_json::from_str::<Vec<serde_json::Value>>(req.text(key)) {
                let normalized: Option<Vec<i64>> = values.iter().map(|v|
                    v.as_i64().or_else(|| v.as_str().and_then(|s| s.parse().ok()))).collect();
                if let Some(values) = normalized {
                    req.0.insert((*key).into(), serde_json::json!(values).to_string());
                }
            }
        }
        Ok(req)
    }
    pub fn account(&self, state: &AppState) -> Result<i64> {
        let key = self
            .0
            .get("SessionKey")
            .or(self.0.get("SessionId"))
            .ok_or(ServerError::SessionExpired)?;
        let session = state.get_session(key).ok_or(ServerError::SessionExpired)?;
        if session.account_id <= 0 {
            return Err(ServerError::SessionExpired);
        }
        state.touch_session(key);
        Ok(session.account_id)
    }
    pub fn text(&self, key: &str) -> &str {
        self.0.get(key).map(String::as_str).unwrap_or("")
    }
    /// Free text from the native client. `WebServiceRequester` escapes every
    /// value with `WWW.EscapeURL` before `WWWForm` encodes it again, so
    /// non-ASCII text arrives as `%E3%85%8B` after form decoding. Undo that
    /// second layer (`+` is a space there; a literal `+` arrives as `%2B`).
    /// Plain values from API clients and tests pass through unchanged.
    pub fn escaped_text(&self, key: &str) -> String {
        let raw = self.text(key);
        if !raw.contains('%') && !raw.contains('+') {
            return raw.to_string();
        }
        let decoded = urlencoding::decode(&raw.replace('+', " "))
            .map(|s| s.into_owned())
            .unwrap_or_else(|_| raw.to_string());
        crate::api::account::name_tag::strip_markers(decoded)
    }
    pub fn number(&self, key: &str, default: i64) -> Result<i64> {
        match self.0.get(key) {
            Some(v) => v
                .parse()
                .map_err(|_| ServerError::InvalidRequest(format!("Invalid {key}"))),
            None => Ok(default),
        }
    }
    pub fn ids(&self, key: &str) -> Result<Vec<i64>> {
        self.ids_with_limit(key, 100)
    }
    pub fn ids_with_limit(&self, key: &str, limit: usize) -> Result<Vec<i64>> {
        let Some(value) = self.0.get(key) else {
            return Ok(Vec::new());
        };
        let values: Vec<i64> = serde_json::from_str(value)
            .or_else(|_| {
                value
                    .parse::<i64>()
                    .map(|v| vec![v])
                    .map_err(<serde_json::Error as serde::de::Error>::custom)
            })
            .map_err(|_| ServerError::InvalidRequest(format!("Invalid {key}")))?;
        if values.len() > limit || values.iter().any(|v| *v <= 0) {
            return Err(ServerError::InvalidRequest(format!("Invalid {key}")));
        }
        Ok(values
            .into_iter()
            .collect::<BTreeSet<_>>()
            .into_iter()
            .collect())
    }
}
