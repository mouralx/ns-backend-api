#!/bin/bash
# Full Containerized Setup — clones frontends, builds and starts everything
set -e
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PARENT_DIR="$(dirname "$SCRIPT_DIR")"

if [ "$1" = "--stop" ]; then
    echo "Stopping all containers..."
    cd "$SCRIPT_DIR" && podman compose down
    echo "All containers stopped."
    exit 0
fi
if [ "$1" = "--help" ]; then
    echo "Usage: $0 [--stop]"
    exit 0
fi

echo "=== Massage Booking — Containerized Setup ==="

if ! podman info > /dev/null 2>&1; then
    echo "Error: Podman is not running."
    exit 1
fi

# Clone/update frontend repos
for repo in ns-frontend-backoffice ns-frontend-mobile; do
    dest="$PARENT_DIR/$repo"
    if [ ! -d "$dest" ]; then
        echo "Cloning $repo..."
        git clone "https://github.com/mouralx/$repo.git" "$dest"
    else
        echo "$repo exists, pulling..."
        cd "$dest" && git pull --rebase origin main 2>/dev/null || true
    fi
done

# Build and start
echo "Building and starting all containers..."
cd "$SCRIPT_DIR" && podman compose up --build -d

# Wait for API
echo "Waiting for API..."
for i in $(seq 1 60); do
    if podman compose exec -T api curl -s http://localhost:8080/health > /dev/null 2>&1; then
        echo "API is ready."
        break
    fi
    sleep 2
done

echo ""
echo "=== All services running ==="
echo "  API:         http://localhost:5001"
echo "  Backoffice:  http://localhost:5173"
echo "  Mobile:      http://localhost:8080"
echo "  PostgreSQL:  localhost:5432"
echo "  Redis:       localhost:6379"
echo ""
echo "  Stop: ./setup.sh --stop"
