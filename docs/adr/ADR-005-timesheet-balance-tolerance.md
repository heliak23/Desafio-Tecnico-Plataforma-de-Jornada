# ADR 005: Motor de Pareamento de Batidas e Cálculo de Banco de Horas

## Status
**Aprovado e Implementado**

## Contexto
O registro diário pode conter múltiplas batidas (`ClockIn`, `LunchStart`, `LunchEnd`, `ClockOut`). É necessário um algoritmo determinístico para calcular períodos trabalhados, períodos de descanso e saldo de horas em relação à jornada diária contratada (8 horas).

## Decisão de Arquitetura
1. **Algoritmo de Pareamento Sequencial:**
   * Ordenação rigorosa por `TimestampUtc`.
   * Período de Trabalho 1: `LunchStart - ClockIn`.
   * Período de Intervalo: `LunchEnd - LunchStart`.
   * Período de Trabalho 2: `ClockOut - LunchEnd`.
   * Suporte a jornadas simples (apenas Entrada e Saída) e horas extras após o expediente.
2. **Identificação de Irregularidades:**
   * Marcações ímpares em dias passados (ex: colaborador bateu entrada mas esqueceu a saída).
   * Saída registrada sem entrada correspondente.
   * O sistema sinaliza visualmente na interface com badge de alerta e botão direto para "Solicitar Ajuste".
3. **Saldo Diário e Mensal:**
   * $\text{Saldo Diário} = \text{Total Trabalhado} - \text{Carga Diária Esperada}$.
   * $\text{Saldo Mensal} = \sum \text{Saldos Diários}$.
   * Exibição formatada com indicativo visual de crédito (`+01h 30m`) ou débito (`-00h 45m`).

