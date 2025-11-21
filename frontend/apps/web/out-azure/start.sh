#!/bin/sh
set -e
cd frontend/apps/web
HOST=0.0.0.0 PORT=${PORT:-8080} NODE_ENV=production exec node server.js
