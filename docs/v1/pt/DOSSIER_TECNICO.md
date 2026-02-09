# Dossier Técnico Pedagógico e Documentação de Ação

Este documento detalha os documentos que constituem o Dossier Técnico Pedagógico digital e outros relatórios que podem ser gerados no contexto de uma Ação de Formação.

## Visão Geral

O sistema permite a geração automática de documentos em formato PDF para cada ação de formação. Estes documentos são gerados com base nos dados inseridos no sistema (formadores, formandos, cronograma, avaliações, etc.) e ficam armazenados para consulta futura.

Os documentos podem ser gerados e consultados na secção **"Ficheiros"** dentro da página de detalhes de uma Ação de Formação.

## Documentos Disponíveis

Abaixo encontra-se a lista de documentos disponíveis para geração:

| Documento | Identificador Técnico | Descrição | Conteúdo Típico |
|-----------|-----------------------|-----------|-----------------|
| **Capa** | `cover` | Capa de identificação do Dossier Técnico Pedagógico. | Título da ação, código, datas, logótipos de financiamento e identificação da entidade. |
| **Cronograma** | `sessions` / `session-report` | Calendário detalhado de todas as sessões da ação. | Lista de todas as sessões com data, horário, módulo, formador e sala. |
| **Informação Geral da Formação** | `course-action-information-report` | Resumo administrativo e financeiro da ação. | Dados de aprovação, financiamento, estrutura curricular e equipa pedagógica. |
| **Ficha de Formador** | `teacher-form` | Documento individual para cada formador. | Dados do formador, módulos que leciona na ação, carga horária e cronograma específico do formador. |
| **Processamento de Pagamentos a Formandos** | `course-action-student-payments-report` | Relatório para processamento financeiro de bolsas/apoios. | Lista de formandos com valores a pagar (bolsas, subsídios), baseada na assiduidade registada. |

## Processo de Geração

1. **Geração On-Demand**: O utilizador solicita a geração do documento clicando no botão "Gerar PDF" (ícone de download).
2. **Processamento**: O backend compila os dados atuais e gera o ficheiro PDF usando a biblioteca `QuestPDF`.
3. **Armazenamento**: O PDF gerado é guardado no servidor (`wwwroot/storage/pdfs/`) e registado na base de dados (`SavedPdf`).
4. **Cache**: Se o documento já foi gerado e os dados não mudaram (ou se o utilizador apenas clicar em "Ver"), o sistema pode retornar a versão guardada para poupar recursos.

Ler mais em [GERACAO_PDF.md](GERACAO_PDF.md).

## Ações Disponíveis

Para cada tipo de documento, o utilizador tem as seguintes opções:

- **Gerar/Regerar**: Cria uma nova versão do documento com os dados mais recentes.
- **Visualizar**: Abre o PDF gerado num novo separador do navegador.
- **Imprimir**: Envia o comando de impressão para o navegador.

## Referências Técnicas

- **Controlador**: `PdfController.cs` (`Core/Reports/Controllers`)
- **Serviço**: `PdfService.cs` e `IPdfService.cs` (`Core/Reports/Services`)
- **Compositores de Relatório**: `Core/Reports/Composers/`
    - `CoverActionReportComposer.cs`
    - `SessionsTimelineComposer.cs` (usado no Cronograma)
    - `TeacherFormComposer.cs`
    - `CourseActionInformationReportComposer.cs`
    - `CourseActionProcessStudentPaymentsComposer.cs`
- **Frontend**: `pdf-actions.component.ts` (Componente reutilizável para botões de ação PDF)
