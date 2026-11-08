# ADR 002: Adoção de Clean Architecture no Backend (.NET 8)

## Status
**Aprovado e Implementado**

## Contexto
O desafio exige uma solução com alto padrão de qualidade de código, facilidade de manutenção, desacoplamento e testabilidade das regras de negócio críticas da jornada de trabalho.

## Decisão de Arquitetura
Estruturação da solução backend em 4 camadas bem delimitadas com regras de dependência unidirecionais:

1. **`DndJornada.Domain`:**
   * Entidades de domínio puras (`User`, `TimeRecord`, `TimeRecordAdjustment`).
   * Enums de negócio (`UserRole`, `TimeRecordType`, `AdjustmentStatus`, `RecordSource`).
   * Interfaces de repositórios (`IUserRepository`, `ITimeRecordRepository`, etc.).
   * Totalmente agnóstico de frameworks, banco de dados ou bibliotecas externas.

2. **`DndJornada.Application`:**
   * Casos de uso e orquestração de regras de negócio.
   * DTOs e validações de regras de negócio.
   * Motor de cálculo de jornada e banco de horas (`TimeRecordCalculator`).
   * Interfaces de serviços externos (JWT, hashing de senhas).

3. **`DndJornada.Infrastructure`:**
   * Implementação de acesso a dados com Entity Framework Core e Npgsql (PostgreSQL).
   * Mapeamento relacional via Fluent API com índices de performance.
   * Criptografia BCrypt para senhas.
   * Geração de tokens JWT e inicializador de seed de dados (`DbInitializer`).

4. **`DndJornada.Api`:**
   * Controllers RESTful documentados com OpenAPI/Swagger.
   * Injeção de dependências e configuração de segurança (JWT Bearer, CORS).

5. **`DndJornada.Tests`:**
   * Suíte de testes unitários automatizados com xUnit e FluentAssertions cobrindo cenários reais e transições de fusos horários.

## Consequências
* **Positivas:**
  * Facilidade de testes: o motor de cálculo de jornada pode ser testado isoladamente sem subir banco de dados.
  * Código legível, altamente profissional e aderente aos padrões corporativos do ecossistema .NET.

