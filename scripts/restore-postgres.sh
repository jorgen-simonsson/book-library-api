#!/usr/bin/env bash
set -euo pipefail

BACKUP_ROOT="/srv/backups/postgres"
DB_NAME="booklibrary"
DB_USER="postgres"
PGPASSWORD="postgres"

COMPOSE_DIR="$(cd "$(dirname "$0")/.." && pwd)"

# Find the most recent backup folder
LATEST_DIR=$(find "${BACKUP_ROOT}" -maxdepth 1 -type d -name "backup_*" | sort | tail -n 1)

if [[ -z "${LATEST_DIR}" ]]; then
    echo "ERROR: No backup folders found in ${BACKUP_ROOT}" >&2
    exit 1
fi

DUMP_FILE="${LATEST_DIR}/${DB_NAME}.dump"

if [[ ! -f "${DUMP_FILE}" ]]; then
    echo "ERROR: Dump file not found: ${DUMP_FILE}" >&2
    exit 1
fi

echo "Latest backup: ${LATEST_DIR}"
echo "Dump file:     ${DUMP_FILE}"
echo
read -r -p "This will DROP and recreate the '${DB_NAME}' database. Continue? [y/N] " CONFIRM
[[ "${CONFIRM}" =~ ^[Yy]$ ]] || { echo "Aborted."; exit 0; }

CONTAINER=$(docker compose -f "${COMPOSE_DIR}/compose.yaml" ps -q db 2>/dev/null || true)

if [[ -n "${CONTAINER}" ]]; then
    # Copy the dump into the container and restore from there
    docker cp "${DUMP_FILE}" "${CONTAINER}:/tmp/${DB_NAME}.dump"

    docker exec -e PGPASSWORD="${PGPASSWORD}" "${CONTAINER}" \
        dropdb -U "${DB_USER}" --if-exists "${DB_NAME}"

    docker exec -e PGPASSWORD="${PGPASSWORD}" "${CONTAINER}" \
        createdb -U "${DB_USER}" "${DB_NAME}"

    docker exec -e PGPASSWORD="${PGPASSWORD}" "${CONTAINER}" \
        pg_restore -U "${DB_USER}" -d "${DB_NAME}" --no-owner --role="${DB_USER}" \
        /tmp/"${DB_NAME}".dump

    docker exec "${CONTAINER}" rm /tmp/"${DB_NAME}".dump
else
    # Fall back to local pg_restore via mapped port
    DB_HOST="localhost"
    DB_PORT="5433"

    PGPASSWORD="${PGPASSWORD}" dropdb \
        -h "${DB_HOST}" -p "${DB_PORT}" -U "${DB_USER}" --if-exists "${DB_NAME}"

    PGPASSWORD="${PGPASSWORD}" createdb \
        -h "${DB_HOST}" -p "${DB_PORT}" -U "${DB_USER}" "${DB_NAME}"

    PGPASSWORD="${PGPASSWORD}" pg_restore \
        -h "${DB_HOST}" -p "${DB_PORT}" -U "${DB_USER}" \
        -d "${DB_NAME}" --no-owner --role="${DB_USER}" "${DUMP_FILE}"
fi

echo
echo "Restore complete from ${DUMP_FILE}"
