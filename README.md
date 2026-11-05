# D&D Group — Plataforma de Gestão de Jornada de Trabalho

Solução desenvolvida para centralização, registro e controle de jornada de trabalho para colaboradores distribuídos internacionalmente (Brasil e Europa), com suporte a múltiplos fusos horários e trabalho remoto.

---

## 🛠️ Stack Tecnológica

* **Backend:** C# / .NET 8 (ASP.NET Core Web API, Entity Framework Core)
* **Frontend:** Angular 18/19 (Standalone Components, Signals, Reactive Forms)
* **Banco de Dados:** PostgreSQL 16
* **Infraestrutura Local & Conteinerização:** Docker & Docker Compose
* **Autenticação:** JWT (JSON Web Tokens) com controle de acesso baseado em papéis (RBAC)

---

## 🧭 Decisões de Arquitetura e Negócio (ADRs)

1. **Tratamento de Fuso Horário (Multi-timezone):**
   * Todos os timestamps são obrigatoriamente persistidos em **UTC**.
   * Cada batida armazena o identificador IANA do fuso local (`timezone_id`, ex: `America/Sao_Paulo`, `Europe/Lisbon`) e o deslocamento (*offset*) no momento do registro.
   * O colaborador visualiza suas marcações no horário local em que foram feitas. O RH e Gestores possuem visão unificada e contextualizada.

2. **Tipos de Registro de Ponto:**
   * `ClockIn` (Entrada)
   * `LunchStart` (Início de Intervalo)
   * `LunchEnd` (Fim de Intervalo)
   * `ClockOut` (Saída)

3. **Perfis de Usuário (RBAC):**
   * `Employee` (Colaborador): registro de ponto diário, consulta do próprio histórico e extrato mensal.
   * `Manager` (Gestor): consulta da equipe, monitoramento de horas e aprovação de ajustes.
   * `HR` (Recursos Humanos): visão global da empresa, cadastro de colaboradores e fechamento mensal.

---

## 🚀 Como Executar o Ambiente de Banco de Dados (Docker)

1. Certifique-se de que o **Docker Desktop** esteja aberto e em execução.
2. No terminal, acesse a pasta raiz do projeto:
   ```bash
   cd dnd-jornada
   ```
3. Suba os containers do PostgreSQL e do pgAdmin:
   ```bash
   docker compose up -d
   ```
4. Verifique se os containers estão rodando com sucesso:
   ```bash
   docker compose ps
   ```

* **PostgreSQL:** Disponível na porta `5432`
* **pgAdmin (Web UI):** Acesse `http://localhost:5050` (Login: `admin@dndgroup.com`, Senha: `admin`)

