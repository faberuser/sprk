"""Boot an isolated image and require an authenticated, capable Unity worker."""
import argparse
import json
import subprocess
import time
import uuid


def docker(*args):
    return subprocess.check_output(["docker", *args], text=True).strip()


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("image")
    args = parser.parse_args()
    name = "sprk-worker-smoke-" + uuid.uuid4().hex[:12]
    try:
        docker("run", "--detach", "--init", "--name", name,
               "--env", "SPRK_SERVICE_MODE=game", args.image)
        deadline = time.monotonic() + 240
        while time.monotonic() < deadline:
            state = json.loads(docker("inspect", "--format", "{{json .State}}", name))
            if not state["Running"]:
                raise RuntimeError("Game container exited before worker readiness")
            result = subprocess.run(["docker", "exec", name, "/usr/local/bin/sprk-healthcheck"], capture_output=True)
            if result.returncode == 0:
                print("Linux Unity worker authenticated with Arena and Challenge Raid capabilities.")
                return
            time.sleep(2)
        raise RuntimeError("Unity worker did not become ready within 240 seconds")
    except Exception:
        subprocess.run(["docker", "logs", "--tail", "100", name])
        subprocess.run(["docker", "exec", name, "tail", "-100", "/data/conquest-worker.log"])
        raise
    finally:
        subprocess.run(["docker", "rm", "--force", "--volumes", name], stdout=subprocess.DEVNULL)


if __name__ == "__main__":
    main()
