# ADR 004: Fluxo de Ajuste e Justificativa de Ponto com Inclusão Automática

## Status
**Aprovado e Implementado**

## Contexto
Em cenários reais de trabalho remoto e viagens internacionais, imprevistos acontecem: falha de conectividade em aeroportos, esquecimento de batidas de almoço ou saídas, e consultas médicas.

## Decisão de Arquitetura
Implementação de um ciclo de vida formal para ocorrências:

1. **Abertura pelo Colaborador:**
   * O colaborador informa a data da ocorrência, tipo de batida desejada, horário local real e uma justificativa detalhada.
   * A solicitação nasce no estado `Pending`.
2. **Revisão pelo Gestor ou RH:**
   * O líder analisa a solicitação no seu painel de gestão e pode emitir um parecer textual.
   * Decisão: **Aprovar** (`Approved`) ou **Recusar** (`Rejected`).
3. **Efeito Colateral Automático na Aprovação:**
   * Ao aprovar, o sistema instancia e grava automaticamente um novo registro de ponto (`TimeRecord`) na data/hora informadas com a flag `RecordSource.ManualAdjustment` e anotações do revisor, recalculando instantaneamente o saldo do espelho mensal.

## Consequências
* **Positivas:**
  * Trilhas de auditoria completas: o sistema sabe quem bateu diretamente e quem teve o ponto inserido via aprovação do gestor.
  * Zero intervenção manual em bancos de dados por DBAs para consertar pontos esquecidos.

