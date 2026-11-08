# ⚔️ D&D Group — Plataforma de Gestão de Jornada de Trabalho

> Solução corporativa desenvolvida para atender ao desafio técnico da **Dungeons & Dragons Group**, centralizando o registro, cálculo e gestão de jornada para colaboradores distribuídos internacionalmente (**Brasil e Europa**), com suporte nativo a **multi-fuso horário**, viagens a trabalho e controle hierárquico por papéis (**RBAC**).

---

## 🧭 Diferenciais Técnicos e Arquiteturais

1. **Estratégia de Multi-Fuso Horário (Multi-Timezone):**
   * **Banco e API 100% em UTC:** impossibilita furos ou horas negativas decorrentes de viagens ou horário de verão.
   * **Identificador IANA & Offset:** cada batida salva o fuso de origem (ex: `America/Sao_Paulo`, `Europe/Lisbon`) e seu deslocamento no instante exato.
   * **Detecção Automática & Simulador de Deslocamento:** o frontend detecta o fuso do dispositivo do colaborador em tempo real via JavaScript `Intl` e conta com um seletor visual na tela de registro para simular e testar deslocamentos internacionais entre São Paulo (UTC-3), Lisboa (UTC+0/+1), Madrid (UTC+1/+2) ou Londres (UTC+0/+1).
   * **Perspectiva Flexível no Espelho:** o usuário e o RH podem alternar a visualização das marcações entre o Fuso Local, Fuso da Europa e UTC Absoluto.

2. **Simulador de Horário / Máquina do Tempo (Time Travel para Avaliação):**
   * **Demonstração Instantânea de 8 Horas:** o recrutador/avaliador não precisa esperar 8 horas reais para validar os cálculos.
   * Na tela de **Registrar Ponto**, há um painel de simulação que avança o relógio virtual gradualmente (`+15m`, `+30m`, `+1h`, `+2h`, `+4h`) ou realiza saltos diretos de rotina (`08:00 Entrada`, `12:00 Almoço`, `13:00 Volta`, `17:00 Saída`).
   * O relógio e a máquina de estados reagem imediatamente na interface e registram os pontos com timestamps simulados preservando integridade UTC.

3. **Clean Architecture no Backend (.NET 8):**
   * Separação estrita em camadas: `Domain`, `Application`, `Infrastructure`, `Api` e `Tests`.
   * Regras de cálculo de jornada e banco de horas isoladas e testadas com **100% de aprovação em testes unitários automatizados**.

4. **Frontend Moderno em Angular (v18/19):**
   * Standalone Components, Signals reativos para controle de estado, interceptors HTTP para injeção de JWT.
   * Design System corporativo e responsivo (Desktop e Mobile).
   * **Seletor de Perfis Demo com 1-Click:** botão na tela de login para o avaliador testar instantaneamente cada papel da empresa sem precisar digitar credenciais.
   * **Botão de Limpeza do Banco (Reset):** permite limpar batidas com 1 clique para reiniciar os testes do zero a qualquer instante.

5. **Ciclo Completo de Ajustes e Ocorrências:**
   * Solicitação de inclusão de batida esquecida com justificativa obrigatória.
   * Fluxo de aprovação pelo Gestor/RH que insere automaticamente a marcação no espelho mensal (`RecordSource.ManualAdjustment`).

6. **Fechamento Mensal da Equipe & Exportação em PDF / Impressão:**
   * **Balanço Consolidado da Empresa:** Painel de fechamento do mês para RH e gestores com total de horas trabalhadas, saldo geral de banco de horas e total de inconsistências.
   * **Exportação Oficial em PDF:** Layout A4 de impressão (`@media print`) tanto do **Espelho Individual de Ponto** quanto do **Relatório Consolidado de Fechamento da Equipe**, acompanhado de cabeçalho da *Dungeons & Dragons Group* e campos formais de assinatura do colaborador e visto do RH para homologação trabalhista.

---

## 🛠️ Stack Tecnológica

* **Backend:** C# / .NET 8 (ASP.NET Core Web API, Entity Framework Core 8, FluentValidation, BCrypt.Net)
* **Frontend:** Angular (Standalone Components, Signals, Reactive Forms, CSS Custom Properties)
* **Banco de Dados:** PostgreSQL 16 (persiste em UTC com healthcheck e volumes persistentes)
* **Infraestrutura Local:** Docker & Docker Compose
* **Testes Automatizados:** xUnit, FluentAssertions, Moq
* **Documentação de API:** OpenAPI / Swagger com suporte a Bearer Token

---

## 🏛️ Decisões de Arquitetura Documentadas (ADRs)

As decisões tomadas para cobrir cenários reais implícitos do negócio estão detalhadas na pasta [`docs/adr/`](file:///c:/Users/User/.gemini/antigravity/scratch/dnd-jornada/Desafio-Tecnico-Plataforma-de-Jornada/docs/adr/):

* [ADR 001 — Estratégia de Persistência e Multi-Fuso Horário](file:///c:/Users/User/.gemini/antigravity/scratch/dnd-jornada/Desafio-Tecnico-Plataforma-de-Jornada/docs/adr/ADR-001-multi-timezone-persistence-strategy.md)
* [ADR 002 — Adoção de Clean Architecture no Backend](file:///c:/Users/User/.gemini/antigravity/scratch/dnd-jornada/Desafio-Tecnico-Plataforma-de-Jornada/docs/adr/ADR-002-clean-architecture-layering.md)
* [ADR 003 — Autenticação JWT e Perfis RBAC](file:///c:/Users/User/.gemini/antigravity/scratch/dnd-jornada/Desafio-Tecnico-Plataforma-de-Jornada/docs/adr/ADR-003-rbac-authentication-and-authorization.md)
* [ADR 004 — Fluxo de Ajuste e Justificativa de Ponto](file:///c:/Users/User/.gemini/antigravity/scratch/dnd-jornada/Desafio-Tecnico-Plataforma-de-Jornada/docs/adr/ADR-004-point-adjustment-workflow.md)
* [ADR 005 — Pareamento de Batidas e Banco de Horas](file:///c:/Users/User/.gemini/antigravity/scratch/dnd-jornada/Desafio-Tecnico-Plataforma-de-Jornada/docs/adr/ADR-005-timesheet-balance-tolerance.md)

---

## 🚀 Como Executar o Projeto Passo a Passo

### Pré-requisitos
* Docker Desktop em execução
* .NET 8 SDK
* Node.js (v18+) e npm

---

### Passo 1: Subir o Banco de Dados (PostgreSQL)

Na raiz do repositório, execute:

```bash
docker compose up -d
```

* **PostgreSQL:** porta `5432`
* **pgAdmin (Painel Web):** [http://localhost:5050](http://localhost:5050) (Login: `admin@dndgroup.com` / Senha: `admin`)

---

### Passo 2: Executar o Backend (.NET 8 Web API)

**Opção 1 — Script Rápido (Raiz do Projeto):**
* **Git Bash / Linux / macOS:**
  ```bash
  ./run-backend.sh
  ```
* **Windows (CMD / PowerShell):**
  ```cmd
  run-backend.bat
  ```

**Opção 2 — Execução Manual:**
```bash
cd src/backend/DndJornada.Api
dotnet run
```

* A API iniciará em: `http://localhost:5000`
* **Swagger Interativo:** Acesse [http://localhost:5000/swagger](http://localhost:5000/swagger)
* *Nota:* Na primeira execução, a API executa automaticamente as migrações e popula o banco de dados com dados de teste.

#### Para rodar os Testes Automatizados:
```bash
cd src/backend
dotnet test
```

---

### Passo 3: Executar o Frontend (Angular)

**Opção 1 — Script Rápido (Raiz do Projeto):**
* **Git Bash / Linux / macOS:**
  ```bash
  ./run-frontend.sh
  ```
* **Windows (CMD / PowerShell):**
  ```cmd
  run-frontend.bat
  ```

**Opção 2 — Execução Manual:**
```bash
cd src/frontend
npm install
npm start
```

* Abra no navegador: [http://localhost:4200](http://localhost:4200)

---

### 🔄 Scripts Auxiliares (Raiz do Repositório)

Para conveniência durante a avaliação e desenvolvimento, estão disponíveis scripts prontos na raiz:

| Finalidade | Linux / macOS / Git Bash | Windows (CMD / PowerShell) |
| :--- | :--- | :--- |
| **Iniciar Backend** | `./run-backend.sh` | `run-backend.bat` |
| **Iniciar Frontend** | `./run-frontend.sh` | `run-frontend.bat` |
| **Resetar Banco de Dados** | `./reset-db.sh` | `reset-db.bat` |

---

## 👥 Contas de Teste Pré-Configuradas (Demo)

A aplicação conta com um **seletor rápido** na tela de login, mas caso queira autenticar manualmente, utilize qualquer uma das contas abaixo (todas com a senha padrão `Password123!`):

| Perfil / Papel | E-mail | Fuso de Origem | Cenário Demonstrado |
| :--- | :--- | :--- | :--- |
| **RH Geral** | `admin@dndgroup.com` | `America/Sao_Paulo` | Visão macro de todos os colaboradores, relatórios e fechamentos globais. |
| **Gestor (Brasil)** | `gestor.brasil@dndgroup.com` | `America/Sao_Paulo` | Gestão da equipe brasileira e aprovação de ajustes. |
| **Gestor (Europa)** | `gestor.europa@dndgroup.com` | `Europe/Lisbon` | Gestão da equipe europeia e aprovação de ajustes. |
| **Colaborador (Brasil)** | `colab.brasil@dndgroup.com` | `America/Sao_Paulo` | Colaborador padrão no horário de Brasília (UTC-3). |
| **Colaborador (Viajante)** | `colab.viajante@dndgroup.com` | `America/Sao_Paulo` | **Caso crítico do desafio:** possui batidas registradas tanto no Brasil quanto em Lisboa, comprovando a precisão do multi-fuso. |
| **Colaborador (Europa)** | `colab.europa@dndgroup.com` | `Europe/Lisbon` | Colaborador padrão no horário de Lisboa (UTC+0 / UTC+1). |

---

## 📬 Contato do Desenvolvedor
Repositório desenvolvido para entrega da avaliação técnica solicitada por:
* **E-mail:** [lucas.freitas@dreamerstudios.io](mailto:lucas.freitas@dreamerstudios.io)
