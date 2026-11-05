#!/usr/bin/env bash
DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
cd "$DIR/src/frontend" || exit

echo "Iniciando D&D Jornada Frontend Angular..."
npm start
