# Dashboard e KPIs

Este documento descreve os indicadores chave de desempenho (KPIs) apresentados no dashboard e em outras áreas do sistema NERBA Backoffice, detalhando sua origem de dados, regras de cálculo, filtros aplicados e formato de exibição.

## Índice

- [Dashboard Principal](#dashboard-principal)
  - [Indicadores Numéricos](#indicadores-numéricos)
  - [Gráficos](#gráficos)
  - [Listagens Top 5](#listagens-top-5)
- [KPIs de Curso](#kpis-de-curso)
- [KPIs de Ação](#kpis-de-ação)

---

## Dashboard Principal

O dashboard principal agrega métricas de várias áreas do sistema para fornecer uma visão geral rápida do estado da formação.

### Indicadores Numéricos

Estes indicadores são apresentados como cartões simples no topo do dashboard.

| Título | Descrição | Fonte de Dados | Filtro Temporal | Cálculo | Formato Visual |
|--------|-----------|----------------|-----------------|---------|----------------|
| **Pagamentos a Formadores** | Total pago a formadores | Tabela `ModuleTeachings` | Mês Atual | Soma de `PaymentTotal` onde `PaymentDate` não é nulo | Valor numérico + "€" (ex: 1500€) |
| **Pagamentos a Formandos** | Total pago a formandos | Tabela `ActionEnrollments` | Mês Atual | Soma de `PaymentTotal` onde `PaymentDate` não é nulo | Valor numérico + "€" (ex: 850€) |
| **Quantidade de Empresas** | Novas empresas registadas | Tabela `Companies` | Ano Atual | Contagem de registos criados (`CreatedAt`) | Número Inteiro |

### Gráficos

Os gráficos fornecem visualizações analíticas sobre formandos e ações.

#### Formandos por Nível de Habilitação
- **Tipo:** Gráfico de Linha
- **Fonte:** Tabela `People` (apenas registos ligados a `Student`)
- **Filtro:** Criados no Ano Atual (últimos 12 meses)
- **Logica:** Agrupa formandos pelo seu nível de habilitação e conta as ocorrências.
- **Eixos:** 
  - X: Níveis de Habilitação (humanizado)
  - Y: Quantidade de Formandos

#### Resultados de Formandos
- **Tipo:** Gráfico Donut (Pie Chart)
- **Fonte:** Tabela `ActionEnrollments`
- **Filtro:** Criados no Ano Atual
- **Logica:** Agrupa inscrições pelo estado de aprovação (`ApprovalStatus`).
- **Estados:** Aprovado, Reprovado, Desistiu, Não Especificado.

#### Ações por Nível de Habilitação Mínimo
- **Tipo:** Gráfico Donut
- **Fonte:** Tabela `Actions` -> `Course`
- **Filtro:** Todo o histórico ("Sempre")
- **Logica:** Agrupa ações baseando-se no `MinHabilitationLevel` do curso associado.
- **Categorias:**
  - **Nível 1:** Até ao 9º ano (inclusive) e "Sem Habilitação"
  - **Nível 2:** 10º, 11º e 12º ano
  - **Nível 3:** Ensino Superior (Pós-Secundário até Doutoramento)

#### Formandos por Género ao Longo do Tempo
- **Tipo:** Gráfico de Barras
- **Fonte:** Tabela `People` (Student)
- **Filtro:** Criados no Ano Atual (para gráfico anual) ou Histórico (para "Sempre")
- **Logica:** Agrupa formandos por género e mês de criação.
- **Visualização:** Séries temporais mensais para cada género.

### Listagens Top 5

Apresentadas em abas (tabs) para consulta rápida das categorias mais ativas.

| Categoria | Fonte | Critério de Ordenação | Descrição |
|-----------|-------|-----------------------|-----------|
| **Top Localidades** | Tabela `Actions` | Contagem Descendente | As 5 localidades com maior número de ações criadas no ano atual. |
| **Top Regimes** | Tabela `Actions` | Contagem Descendente | Os 5 regimes (ex: Laboral, Pós-Laboral) mais frequentes no ano atual. |
| **Top Estados** | Tabela `Actions` | Contagem Descendente | Os 5 estados (ex: Em Progresso, Concluída) com mais ações no ano atual. |

---

## KPIs de Curso

Estes indicadores são calculados ao visualizar os detalhes de um curso específico (`GetCourseKpisAsync`). Eles agregam dados de **todas** as ações associadas a esse curso.

| Indicador | Fonte de Dados | Cálculo | Descrição |
|-----------|----------------|---------|-----------|
| **Total Formandos** | `Course.Actions` | Soma de `Action.TotalStudents` | Número total de formandos inscritos em todas as edições deste curso. |
| **Total Aprovados** | `Course.Actions` | Soma de `Action.TotalApproved` | Número total de formandos que concluíram com aproveitamento. |
| **Volume Total (Horas)** | `Course.Actions` | Soma de `Action.TotalVolumeHours` | Volume total de horas de formação ministradas (Carga Horária x Participantes). |
| **Volume Total (Dias)** | `Course.Actions` | Soma de `Action.TotalVolumeDays` | Total de dias de formação acumulados. |

---

## KPIs de Ação

Estes indicadores são específicos de uma única ação de formação (`GetActionKpisAsync`).

| Indicador | Fonte de Dados | Cálculo | Descrição |
|-----------|----------------|---------|-----------|
| **Total Formandos** | `ActionEnrollments` | Contagem (`Count`) | Número de formandos inscritos na ação. |
| **Total Aprovados** | `ActionEnrollments` | Contagem onde `ApprovalStatus == Approved` | Número de formandos aprovados. |
| **Volume Total (Horas)** | `SessionParticipations` | Soma de `Attendance` (horas de presença) | Soma de todas as horas de presença efectiva de todos os formandos. |
| **Volume Total (Dias)** | `SessionParticipations` | Contagem onde `Presence == Present` | Número total de presenças registadas (cada registo conta como 1 dia/sessão). |

---

## Referências Técnicas

- **Controlador API:** `NERBABO.Backend/NERBABO.ApiService/Core/Dashboard/Controllers/DashboardController.cs`
- **Serviço de Cálculo:** `NERBABO.Backend/NERBABO.ApiService/Core/Kpis/Services/KpisService.cs`
- **Frontend Component:** `NERBABO.Frontend/src/app/features/dashboard/dashboard.component.ts`
