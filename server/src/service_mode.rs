//! Select services before initializing any game state.
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub enum ServiceMode {
    Game,
    Updates,
    All,
}

impl ServiceMode {
    pub fn from_env() -> anyhow::Result<Self> {
        match std::env::var("SPRK_SERVICE_MODE") {
            Ok(value) => Self::parse(&value),
            Err(std::env::VarError::NotPresent) => Ok(Self::Game),
            Err(error) => Err(error.into()),
        }
    }

    fn parse(value: &str) -> anyhow::Result<Self> {
        match value.trim().to_ascii_lowercase().as_str() {
            "game" => Ok(Self::Game),
            "updates" => Ok(Self::Updates),
            "all" => Ok(Self::All),
            _ => {
                anyhow::bail!("Invalid SPRK_SERVICE_MODE {value:?}; expected game, updates, or all")
            }
        }
    }

    pub fn runs_game(self) -> bool {
        matches!(self, Self::Game | Self::All)
    }

    pub fn runs_updates(self) -> bool {
        matches!(self, Self::Updates | Self::All)
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn modes_select_only_requested_services() {
        for (value, game, updates) in [
            ("game", true, false),
            ("updates", false, true),
            ("all", true, true),
        ] {
            let mode = ServiceMode::parse(value).unwrap();
            assert_eq!((mode.runs_game(), mode.runs_updates()), (game, updates));
        }
        assert_eq!(ServiceMode::parse(" GAME ").unwrap(), ServiceMode::Game);
        for invalid in ["", "update", "games", "both"] {
            assert!(ServiceMode::parse(invalid).is_err());
        }
    }
}
