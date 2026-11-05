#!/usr/bin/env bash
export DOTNET_ROOT="$LOCALAPPDATA/Microsoft/dotnet"
export PATH="/c/Users/User/AppData/Local/Microsoft/dotnet:$LOCALAPPDATA/Microsoft/dotnet:$PATH"

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
cd "$DIR/src/backend/DndJornada.Api" || exit

echo "Iniciando D&D Jornada Web API na porta 5000..."

if [ -f "$LOCALAPPDATA/Microsoft/dotnet/dotnet.exe" ]; then
    "$LOCALAPPDATA/Microsoft/dotnet/dotnet.exe" run --urls "http://localhost:5000"
elif [ -f "/c/Users/User/AppData/Local/Microsoft/dotnet/dotnet.exe" ]; then
    "/c/Users/User/AppData/Local/Microsoft/dotnet/dotnet.exe" run --urls "http://localhost:5000"
else
    dotnet run --urls "http://localhost:5000"
fi
