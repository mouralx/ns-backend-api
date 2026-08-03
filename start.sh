#!/bin/bash
# ============================================================
# Massage Booking — Start All Services Locally
# ============================================================
# Usage:
#   ./start.sh          Start everything (manual mode)
#   ./start.sh --docker Start via Docker Compose
#   ./start.sh --stop   Stop Docker Compose services
#   ./start.sh --help   Show help
# ============================================================

set -e

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PARENT_DIR="$(dirname "$SCRIPT_DIR")"
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m'

usage() {
    echo "Usage: $0 [OPTIONS]"
    echo ""
    echo "Options:"
    echo "  --docker    Start all services via Docker Compose"
    echo "  --manual    Start services manually (default)"
    echo "  --stop      Stop Docker Compose services"
    echo "  --help      Show this help"
    echo ""
    echo "Manual mode starts:"
    echo "  1. PostgreSQL + Redis via Docker"
    echo "  2. Backend API via dotnet run"
    echo "  3. Backoffice via npm run dev"
    echo "  4. Mobile via npx expo start --web"
    echo ""
    echo "Docker mode starts everything in containers."
    exit 0
}

stop_docker() {
    echo -e "${YELLOW}Stopping Docker Compose services...${NC}"
    cd "$SCRIPT_DIR"
    docker compose down
    echo -e "${GREEN}All services stopped.${NC}"
}

start_docker() {
    echo -e "${CYAN}Starting all services via Docker Compose...${NC}"
    cd "$SCRIPT_DIR"

    if ! docker info > /dev/null 2>&1; then
        echo -e "${RED}Docker is not running. Please start Docker Desktop.${NC}"
        exit 1
    fi

    docker compose up --build
}

check_tool() {
    if ! command -v "$1" &> /dev/null; then
        echo -e "${RED}Error: $1 is not installed.${NC}"
        echo "$2"
        exit 1
    fi
}

clone_if_missing() {
    local dir="$1"
    local repo="$2"
    local dest="$PARENT_DIR/$dir"
    if [ ! -d "$dest" ]; then
        echo -e "${YELLOW}Cloning $dir...${NC}"
        git clone "https://github.com/mouralx/$repo.git" "$dest"
    else
        echo -e "${GREEN}$dir already exists.${NC}"
    fi
}

start_manual() {
    echo -e "${CYAN}Starting services manually...${NC}"
    echo ""

    echo -e "${YELLOW}Checking prerequisites...${NC}"
    check_tool docker "Install Docker: https://docs.docker.com/get-docker/"
    check_tool dotnet ".NET 8 SDK required: https://dotnet.microsoft.com/download/dotnet/8.0"
    check_tool node "Install Node.js: https://nodejs.org"
    check_tool npm "Install Node.js: https://nodejs.org"
    echo -e "${GREEN}All prerequisites found.${NC}"
    echo ""

    echo -e "${YELLOW}Checking frontend repos...${NC}"
    clone_if_missing "ns-frontend-backoffice" "ns-frontend-backoffice"
    clone_if_missing "ns-frontend-mobile" "ns-frontend-mobile"
    echo ""

    # 1. Start PostgreSQL and Redis
    echo -e "${CYAN}[1/4] Starting PostgreSQL and Redis...${NC}"
    cd "$SCRIPT_DIR"
    docker compose up -d postgres redis

    echo "Waiting for PostgreSQL..."
    for i in $(seq 1 30); do
        if docker compose exec -T postgres pg_isready -U postgres > /dev/null 2>&1; then
            echo -e "${GREEN}PostgreSQL is ready.${NC}"
            break
        fi
        [ "$i" -eq 30 ] && { echo -e "${RED}PostgreSQL failed.${NC}"; exit 1; }
        sleep 1
    done

    echo "Waiting for Redis..."
    for i in $(seq 1 15); do
        if docker compose exec -T redis redis-cli ping > /dev/null 2>&1; then
            echo -e "${GREEN}Redis is ready.${NC}"
            break
        fi
        [ "$i" -eq 15 ] && { echo -e "${RED}Redis failed.${NC}"; exit 1; }
        sleep 1
    done
    echo ""

    # 2. Start Backend API
    echo -e "${CYAN}[2/4] Starting Backend API on http://localhost:5000...${NC}"
    cd "$SCRIPT_DIR/src/MassageBooking.Api"
    export ASPNETCORE_ENVIRONMENT=Development
    export ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=massage_booking;Username=postgres;Password=postgres"
    export ConnectionStrings__Redis="localhost:6379"
    export ConnectionStrings__Hangfire="Host=localhost;Port=5432;Database=massage_booking;Username=postgres;Password=postgres"
    export Jwt__Key="YourSuperSecretKeyThatIsAtLeast32CharactersLong!"
    export Jwt__Issuer="MassageBookingApi"
    export Jwt__Audience="MassageBookingApp"
    export Jwt__Expiry="60"
    dotnet run &
    API_PID=$!
    echo -e "${GREEN}API started (PID: $API_PID).${NC}"
    echo ""

    echo "Waiting for API..."
    for i in $(seq 1 30); do
        if curl -s http://localhost:5000/health > /dev/null 2>&1; then
            echo -e "${GREEN}API is ready.${NC}"
            break
        fi
        [ "$i" -eq 30 ] && echo -e "${YELLOW}API may still be starting.${NC}"
        sleep 2
    done
    echo ""

    # 3. Start Backoffice
    BO_DIR="$PARENT_DIR/ns-frontend-backoffice"
    echo -e "${CYAN}[3/4] Starting Backoffice on http://localhost:5173...${NC}"
    cd "$BO_DIR"
    [ ! -d "node_modules" ] && { echo "Installing dependencies..."; npm install; }
    export VITE_API_URL="http://localhost:5000"
    npm run dev &
    BO_PID=$!
    echo -e "${GREEN}Backoffice started (PID: $BO_PID).${NC}"
    echo ""

    # 4. Start Mobile
    MOBILE_DIR="$PARENT_DIR/ns-frontend-mobile"
    echo -e "${CYAN}[4/4] Starting Mobile App on http://localhost:8080...${NC}"
    cd "$MOBILE_DIR"
    [ ! -d "node_modules" ] && { echo "Installing dependencies..."; npm install; }
    export EXPO_PUBLIC_API_URL="http://localhost:5000"
    npx expo start --web &
    MOBILE_PID=$!
    echo -e "${GREEN}Mobile started (PID: $MOBILE_PID).${NC}"
    echo ""

    echo -e "${GREEN}============================================${NC}"
    echo -e "${GREEN}  All services started!${NC}"
    echo -e "${GREEN}============================================${NC}"
    echo ""
    echo "  API:         http://localhost:5000"
    echo "  Swagger:     http://localhost:5000/swagger"
    echo "  Backoffice:  http://localhost:5173"
    echo "  Mobile:      http://localhost:8080"
    echo "  PostgreSQL:  localhost:5432"
    echo "  Redis:       localhost:6379"
    echo ""
    echo "  Press Ctrl+C to stop all services."
    echo ""

    cleanup() {
        echo ""
        echo -e "${YELLOW}Stopping services...${NC}"
        kill $API_PID $BO_PID $MOBILE_PID 2>/dev/null || true
        cd "$SCRIPT_DIR"
        docker compose stop postgres redis
        echo -e "${GREEN}All services stopped.${NC}"
    }
    trap cleanup EXIT INT TERM

    wait
}

MODE="manual"
for arg in "$@"; do
    case $arg in
        --docker) MODE="docker" ;;
        --manual) MODE="manual" ;;
        --stop) stop_docker; exit 0 ;;
        --help) usage ;;
        *) echo "Unknown option: $arg"; usage ;;
    esac
done

case $MODE in
    docker) start_docker ;;
    manual) start_manual ;;
esac
