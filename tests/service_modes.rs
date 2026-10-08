//! Exercise the actual executable in isolated working directories.
use std::{
    io::{Read, Write},
    net::{TcpListener, TcpStream},
    path::PathBuf,
    process::{Child, Command, Stdio},
    time::{Duration, Instant},
};

struct Server {
    process: Child,
    root: PathBuf,
    port: u16,
}

impl Server {
    fn start(mode: &str) -> Self {
        let root =
            std::env::temp_dir().join(format!("sprk-service-modes-{}", uuid::Uuid::new_v4()));
        std::fs::create_dir_all(root.join("client-updates/stable")).unwrap();
        std::fs::write(
            root.join("client-updates/stable/manifest.json"),
            br#"{"version":"test"}"#,
        )
        .unwrap();
        // Bind ephemeral chat/battle ports in game mode. Invalid addresses and
        // an unusable DB path ensure updates mode truly bypasses game startup.
        if mode == "updates" {
            std::fs::create_dir(root.join("sprk.db")).unwrap();
        }
        let reservation = TcpListener::bind("127.0.0.1:0").unwrap();
        let port = reservation.local_addr().unwrap().port();
        drop(reservation);
        let log = std::fs::File::create(root.join("server.log")).unwrap();
        let process = Command::new(env!("CARGO_BIN_EXE_sprk-server"))
            .current_dir(&root)
            .env_remove("SPRK_SERVICE_MODE")
            .envs((mode != "default").then_some(("SPRK_SERVICE_MODE", mode)))
            .env("SPRK_UPDATES_DIR", root.join("client-updates"))
            .env("PORT", port.to_string())
            .env("GAME_TABLES_PATH", root.join("missing-tables"))
            .env(
                "CHAT_BIND",
                if mode == "updates" {
                    "invalid address"
                } else {
                    "127.0.0.1"
                },
            )
            .env("CHAT_PORT", "0")
            .env(
                "BATTLE_BIND",
                if mode == "updates" {
                    "invalid address"
                } else {
                    "127.0.0.1"
                },
            )
            .env("BATTLE_PORT", "0")
            .env_remove("BATTLE_WORKER_EXECUTABLE")
            .env_remove("BATTLE_SERVICE_KEY")
            .stdout(Stdio::from(log.try_clone().unwrap()))
            .stderr(Stdio::from(log))
            .spawn()
            .unwrap();
        let mut server = Self {
            process,
            root,
            port,
        };
        let deadline = Instant::now() + Duration::from_secs(15);
        loop {
            if TcpStream::connect(("127.0.0.1", port)).is_ok() {
                break;
            }
            if let Some(status) = server.process.try_wait().unwrap() {
                panic!(
                    "{mode} startup exited {status}: {}",
                    std::fs::read_to_string(server.root.join("server.log")).unwrap()
                );
            }
            assert!(Instant::now() < deadline, "{mode} startup timed out");
            std::thread::sleep(Duration::from_millis(50));
        }
        server
    }

    fn get(&self, path: &str) -> String {
        let mut stream = TcpStream::connect(("127.0.0.1", self.port)).unwrap();
        stream
            .set_read_timeout(Some(Duration::from_secs(5)))
            .unwrap();
        write!(
            stream,
            "GET {path} HTTP/1.1\r\nHost: localhost\r\nConnection: close\r\n\r\n"
        )
        .unwrap();
        let mut response = String::new();
        stream.read_to_string(&mut response).unwrap();
        response
    }
}

impl Drop for Server {
    fn drop(&mut self) {
        let _ = self.process.kill();
        let _ = self.process.wait();
        let _ = std::fs::remove_dir_all(&self.root);
    }
}

#[test]
fn updates_mode_runs_without_a_database_tables_or_game_transports() {
    let server = Server::start("updates");
    assert!(server.get("/health").starts_with("HTTP/1.1 200"));
    assert!(server
        .get("/updates/stable/manifest.json")
        .contains("{\"version\":\"test\"}"));
    assert!(server.get("/host.json").starts_with("HTTP/1.1 404"));
    assert!(server.root.join("sprk.db").is_dir());
    let log = std::fs::read_to_string(server.root.join("server.log")).unwrap();
    assert!(!log.contains("Database initialized"));
    assert!(!log.contains("Message server listening"));
    assert!(!log.contains("Conquest battle server listening"));
}

#[test]
fn game_and_all_modes_expose_the_expected_endpoints() {
    for mode in ["default", "game", "all"] {
        let server = Server::start(mode);
        assert!(server.get("/health").starts_with("HTTP/1.1 200"));
        assert!(server.get("/host.json").starts_with("HTTP/1.1 200"));
        assert!(server.root.join("sprk.db").is_file());
        let expected = if mode == "all" {
            "HTTP/1.1 200"
        } else {
            "HTTP/1.1 404"
        };
        assert!(
            server
                .get("/updates/stable/manifest.json")
                .starts_with(expected),
            "{mode}"
        );
    }
}
