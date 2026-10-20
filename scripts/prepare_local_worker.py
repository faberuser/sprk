"""Restore the generated local battle worker before starting local services."""
import json
import os
from pathlib import Path

from client_automation import REPO, RUNS, client_view


def prepare():
    config_path = REPO / "conquest-service.json"
    configured = os.environ.get("BATTLE_WORKER_EXECUTABLE")
    if configured is None and config_path.exists():
        configured = json.loads(config_path.read_text(encoding="utf-8-sig")).get("Executable")
    if configured is None:
        return
    executable = Path(configured)
    if not executable.is_absolute():
        executable = REPO / executable
    if executable.is_file():
        return
    generated = RUNS / "clients/battle-worker/SprkTest-battle-worker.exe"
    if executable.resolve() != generated.resolve():
        raise RuntimeError(
            f"Configured battle worker does not exist: {executable}. "
            "Restore it or correct BATTLE_WORKER_EXECUTABLE / conquest-service.json."
        )
    client = REPO.parent / "sprk-client"
    if not (client / "King's Raid.exe").is_file():
        raise RuntimeError(f"Cannot restore the local battle worker: client missing at {client}")
    restored = client_view(client, "battle-worker")
    if not restored.is_file():
        raise RuntimeError(f"Battle worker preparation failed: {restored}")
    print("Restored local battle worker from the installed client.")


if __name__ == "__main__":
    prepare()
