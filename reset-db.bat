@echo off
echo Removendo containers e volumes do PostgreSQL...
docker compose down -v

echo Recriando banco de dados PostgreSQL zerado...
docker compose up -d

echo Banco de dados zerado com sucesso! Na proxima inicializacao da API, os dados iniciais serao recriados.

