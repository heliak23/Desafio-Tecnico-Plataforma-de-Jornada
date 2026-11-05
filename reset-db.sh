#!/usr/bin/env bash
DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
if [ -d "$DIR/Desafio-Tecnico-Plataforma-de-Jornada" ]; then
  cd "$DIR/Desafio-Tecnico-Plataforma-de-Jornada" || exit
fi

echo "Removendo containers e volumes do PostgreSQL..."
docker compose down -v

echo "Recriando banco de dados PostgreSQL zerado..."
docker compose up -d

echo "Banco de dados zerado com sucesso! Na próxima inicialização da API, os dados iniciais serão recriados."

