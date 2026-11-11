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
    database = name + "-postgres"
    try:
        # The game needs PostgreSQL; give it a throwaway server on a private network.
        docker("network", "create", name)
        docker("run", "--detach", "--name", database, "--network", name,
               "--env", "POSTGRES_PASSWORD=smoke", "postgres:18")
        deadline = time.monotonic() + 60
        while subprocess.run(["docker", "exec", database, "pg_isready", "-U", "postgres"],
                             capture_output=True).returncode != 0:
            if time.monotonic() > deadline:
                raise RuntimeError("PostgreSQL did not become ready")
            time.sleep(1)
        docker("run", "--detach", "--init", "--name", name, "--network", name,
               "--env", "SPRK_SERVICE_MODE=game",
               "--env", f"DATABASE_URL=postgres://postgres:smoke@{database}:5432/postgres", args.image)
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
        subprocess.run(["docker", "rm", "--force", "--volumes", name, database], stdout=subprocess.DEVNULL,
                       stderr=subprocess.DEVNULL)
        subprocess.run(["docker", "network", "rm", name], stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)


if __name__ == "__main__":
    main()
