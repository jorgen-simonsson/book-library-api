#!/usr/bin/env bash
set -euo pipefail

BACKUP_ROOT="/srv/backups/postgres"
TIMESTAMP=$(date +"%y%m%d_%H%M%S")
BACKUP_DIR="${BACKUP_ROOT}/backup_${TIMESTAMP}"

DB_HOST="localhost"
DB_PORT="5433"
DB_NAME="booklibrary"
DB_USER="postgres"
PGPASSWORD="postgres"

COMPOSE_DIR="$(cd "$(dirname "$0")/.." && pwd)"
CONTAINER=$(docker compose -f "${COMPOSE_DIR}/compose.yaml" ps -q db 2>/dev/null || true)

mkdir -p "${BACKUP_DIR}"

echo "Backing up PostgreSQL database '${DB_NAME}' → ${BACKUP_DIR}"

if [[ -n "${CONTAINER}" ]]; then
    # Dump directly from the running container
    docker exec -e PGPASSWORD="${PGPASSWORD}" "${CONTAINER}" \
        pg_dump -U "${DB_USER}" -d "${DB_NAME}" -F c \
        > "${BACKUP_DIR}/${DB_NAME}.dump"
else
    # Fall back to local pg_dump via mapped port
    PGPASSWORD="${PGPASSWORD}" pg_dump \
        -h "${DB_HOST}" -p "${DB_PORT}" \
        -U "${DB_USER}" -d "${DB_NAME}" -F c \
        > "${BACKUP_DIR}/${DB_NAME}.dump"
fi

echo "Done. Backup saved to ${BACKUP_DIR}/${DB_NAME}.dump"
