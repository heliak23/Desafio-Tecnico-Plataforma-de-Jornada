# ADR 001: Estratégia de Persistência e Tratamento de Multi-Fuso Horário (Multi-Timezone)

## Status
**Aprovado e Implementado**

## Contexto
A empresa **D&D Group** atua em múltiplos países com colaboradores distribuídos entre o Brasil (fuso `America/Sao_Paulo` - UTC-3) e Europa (fusos `Europe/Lisbon` - UTC+0/+1, `Europe/Madrid` - UTC+1/+2). Além disso, colaboradores viajam frequentemente entre países a trabalho.

Sistemas de ponto tradicionais que armazenam data e hora no fuso local sofrem com:
1. Cálculos de jornada corrompidos ou com horas negativas durante viagens internacionais (ex: bater entrada no Brasil e saída em Portugal).
2. Quebra de integridade na transição de Horário de Verão (DST - Daylight Saving Time).
3. Dificuldade de auditoria unificada pelo RH e lideranças internacionais.

## Decisão de Arquitetura
1. **Regra de Ouro da Persistência:** Toda e qualquer marcação temporal no banco de dados PostgreSQL é gravada estritamente em **UTC absoluto** (`timestamp with time zone`).
2. **Metadados de Contexto Geográfico:** Cada batida (`TimeRecord`) armazena:
   * `TimestampUtc`: o instante real no relógio atômico global.
   * `TimezoneId`: identificador IANA padronizado (ex: `America/Sao_Paulo`, `Europe/Lisbon`).
   * `OffsetMinutes`: o deslocamento exato em minutos no momento do registro (ex: `-180` para UTC-3, `0` para UTC+0, `+60` para UTC+1).
   * `LocationNotes`: anotação opcional de localidade informada pelo colaborador (ex: "Hub Lisboa - Viagem a trabalho").
3. **Visão Local Contextualizada:** O cálculo das horas trabalhadas é computado pela diferença física em UTC, enquanto a exibição visual na tela do colaborador converte o timestamp para o horário local daquele registro.

## Consequências
* **Positivas:**
  * Impossibilidade matemática de horas negativas ou duplicadas decorrentes de fuso.
  * O RH possui transparência total: sabe onde a batida foi feita e qual era o horário local do colaborador.
  * Suporte nativo da plataforma .NET 8 (`TimeZoneInfo.FindSystemTimeZoneById`) e API `Intl` do navegador.
* **Compensações:**
  * Exige conversão explícita ao gerar relatórios diários agrupados pela data civil local do colaborador.

