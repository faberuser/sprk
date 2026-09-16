//! Form parsing shared by the APIs. Arrays are JSON strings in this client.
use crate::{
    error::{Result, ServerError},
    state::AppState,
};
use std::collections::{BTreeSet, HashMap};

pub struct Request(pub HashMap<String, String>);
impl Request {
    pub fn parse(body: &[u8]) -> Result<Self> {
        serde_urlencoded::from_bytes(body)
            .map(Self)
            .map_err(|e| ServerError::InvalidRequest(e.to_string()))
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
    pub fn number(&self, key: &str, default: i64) -> Result<i64> {
        match self.0.get(key) {
            Some(v) => v
                .parse()
                .map_err(|_| ServerError::InvalidRequest(format!("Invalid {key}"))),
            None => Ok(default),
        }
    }
    pub fn ids(&self, key: &str) -> Result<Vec<i64>> {
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
        if values.len() > 100 || values.iter().any(|v| *v <= 0) {
            return Err(ServerError::InvalidRequest(format!("Invalid {key}")));
        }
        Ok(values
            .into_iter()
            .collect::<BTreeSet<_>>()
            .into_iter()
            .collect())
    }
}
