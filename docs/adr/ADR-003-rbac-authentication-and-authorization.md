# ADR 003: Autenticação JWT e Controle de Acesso Baseado em Papéis (RBAC)

## Status
**Aprovado e Implementado**

## Contexto
A plataforma atende diferentes públicos internos com necessidades e privilégios distintos: colaboradores individuais, lideranças diretas e Recursos Humanos.

## Decisão de Arquitetura
1. **Autenticação:** Baseada em padrão da indústria JSON Web Tokens (JWT) com assinatura HMAC-SHA256 e tempo de expiração configurável (12 horas).
2. **Perfis de Acesso (RBAC):**
   * **`Employee` (Colaborador):**
     * Registro de ponto em tempo real (com captura automática de timezone do navegador).
     * Consulta ao próprio espelho de ponto diário e mensal.
     * Envio de justificativas/solicitações de ajuste de batidas passadas.
   * **`Manager` (Gestor):**
     * Todos os privilégios de colaborador.
     * Visualização dos colaboradores subordinados à sua liderança.
     * Aprovação ou recusa de solicitações de ajuste com parecer.
   * **`HR` (Recursos Humanos):**
     * Acesso irrestrito a todos os colaboradores de todas as filiais (Brasil e Europa).
     * Gestão global de pendências e visão analítica para fechamento de folha.

## Consequências
* **Positivas:**
  * Endpoints protegidos no backend via anotações `[Authorize(Roles = "...")]`.
  * Guards de rota no Angular (`authGuard`, `managerOrHrGuard`) impedindo acesso indevido na interface.
  * Senhas persistidas com salt e custo elevado via algoritmo BCrypt (work factor 11).

