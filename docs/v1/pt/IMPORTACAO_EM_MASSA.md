# Importacao em Massa

Este documento descreve a funcionalidade de importacao em massa de entidades no sistema NERBA Backoffice.

## Indice

- [Visao Geral](#visao-geral)
- [Arquitetura](#arquitetura)
- [Implementacao Atual: Pessoas](#implementacao-atual-pessoas)
- [Fluxo de Importacao](#fluxo-de-importacao)
- [Validacoes e Tratamento de Erros](#validacoes-e-tratamento-de-erros)
- [Estrutura dos Templates](#estrutura-dos-templates)
- [Expandir para Novas Entidades](#expandir-para-novas-entidades)
- [Boas Praticas](#boas-praticas)
- [Referencias](#referencias)

---

## Visao Geral

A funcionalidade de importacao em massa permite carregar multiplos registos de uma entidade atraves de ficheiros CSV ou Excel. O sistema processa cada linha individualmente, validando dados e reportando erros detalhados por linha.

### Capacidades

| Caracteristica | Descricao |
|----------------|-----------|
| Formatos suportados | CSV (separador `;`) e Excel (`.xlsx`, `.xls`) |
| Limite de ficheiro | 10MB |
| Processamento em lotes | Configuravel (default: 100 registos) |
| Modo de paragem | Opcao para parar no primeiro erro |
| Validacao dry-run | Validar ficheiro sem guardar dados |
| Templates | Download de templates pre-formatados |

---

## Arquitetura

A funcionalidade segue uma arquitetura modular com separacao de responsabilidades.

### Componentes Backend

```
Shared/BulkImport/
├── Models/
│   └── BulkImportResult.cs      # Modelos de resultado
└── Services/
    ├── IBulkImportService.cs    # Interface base generica
    ├── IFileParserService.cs    # Interface de parsing
    ├── CsvParserService.cs      # Parser CSV
    └── ExcelParserService.cs    # Parser Excel

Core/People/BulkImport/
└── Services/
    ├── IPeopleBulkImportService.cs   # Interface especifica
    └── PeopleBulkImportService.cs    # Implementacao
```

### Componentes Frontend

```
features/people/import-people/
├── import-people.component.ts       # Componente principal
├── import-people.component.html     # Template do modal
├── import-people.component.css      # Estilos
└── import-result-modal.component.ts # Modal de resultados
```

### Fluxo de Dados

```
Frontend                    Backend
   │                           │
   ├── Upload ficheiro ──────► Controller
   │                           │
   │                      ┌────┴────┐
   │                      │ Validar │
   │                      │estrutura│
   │                      └────┬────┘
   │                           │
   │                      ┌────┴────┐
   │                      │ Parsing │
   │                      │ ficheiro│
   │                      └────┬────┘
   │                           │
   │                      ┌────┴────┐
   │                      │ Validar │
   │                      │cada linha│
   │                      └────┬────┘
   │                           │
   │                      ┌────┴────┐
   │                      │ Guardar │
   │                      │em lotes │
   │                      └────┬────┘
   │                           │
   ◄── BulkImportResult ───────┘
```

---

## Implementacao Atual: Pessoas

### Endpoints API

| Metodo | Endpoint | Descricao |
|--------|----------|-----------|
| `POST` | `/api/people/import/csv` | Importar de CSV |
| `POST` | `/api/people/import/excel` | Importar de Excel |
| `POST` | `/api/people/import/validate/csv` | Validar CSV (dry-run) |
| `POST` | `/api/people/import/validate/excel` | Validar Excel (dry-run) |
| `GET` | `/api/people/import/template/csv` | Download template CSV |
| `GET` | `/api/people/import/template/excel` | Download template Excel |

### Parametros de Query

| Parametro | Tipo | Default | Descricao |
|-----------|------|---------|-----------|
| `stopOnFirstError` | boolean | `false` | Parar no primeiro erro |
| `batchSize` | int | `100` | Registos por lote |

### Campos do Template

| Campo | Obrigatorio | Descricao |
|-------|-------------|-----------|
| `NomeCompleto` | Sim | Nome proprio e apelido (minimo 2 palavras) |
| `NIF` | Sim | 9 digitos, unico no sistema |
| `NumeroIdentificacao` | Nao | 5-10 caracteres |
| `DataValidacaoIdentificacao` | Nao | Data futura (formato: `dd/MM/yyyy`) |
| `NISS` | Nao | 11 digitos |
| `IBAN` | Nao | 25 caracteres |
| `DataNascimento` | Nao | Data passada (formato: `dd/MM/yyyy`) |
| `Morada` | Nao | Texto livre |
| `CodigoPostal` | Nao | Formato `NNNN-NNN` |
| `Telefone` | Nao | 9 digitos |
| `Email` | Nao | Formato email valido, unico no sistema |
| `Naturalidade` | Nao | 3-100 caracteres |
| `Nacionalidade` | Nao | 3-100 caracteres |
| `Genero` | Nao | Ver valores permitidos |
| `Habilitacao` | Nao | Ver valores permitidos |
| `TipoIdentificacao` | Nao | Ver valores permitidos |

### Valores Permitidos para Enums

**Genero:**
- `Nao Especificado`
- `Masculino`
- `Feminino`
- `Outro`

**Tipo de Identificacao:**
- `Nao Especificado`
- `Autorizacao de Residencia`
- `Identificacao Civil (CC/BI)`
- `Militar`
- `Passaporte`

**Habilitacao:**
- `Sem Comprovativo`
- `Sem escolaridade`
- `1º Ano` ate `12º Ano`
- `Pos-Secundario`
- `Bacharelato`
- `Licenciatura`
- `Mestrado`
- `Doutoramento`

---

## Fluxo de Importacao

### 1. Validacao de Estrutura

O sistema valida primeiro a estrutura do ficheiro:
- Extensao correta (`.csv`, `.xlsx`, `.xls`)
- Presenca de cabecalhos obrigatorios
- Ficheiro nao vazio

### 2. Parsing do Ficheiro

Cada parser (CSV ou Excel) converte as linhas em dicionarios `<header, value>`.

**Especificidades CSV:**
- Delimitador: `;` (formato europeu)
- Cultura: `pt-PT`
- Datas convertidas para `dd/MM/yyyy`

**Especificidades Excel:**
- Primeira folha processada
- Primeira linha como cabecalhos
- Datas convertidas para `dd/MM/yyyy`

### 3. Mapeamento para DTO

A funcao `MapRowToDto` converte cada linha no DTO correspondente:

```csharp
// Exemplo de mapeamento (PeopleBulkImportService.cs)
var dto = new CreatePersonDto
{
    FirstName = firstName,  // Extraido de NomeCompleto
    LastName = lastName,    // Ultima palavra de NomeCompleto
    NIF = GetValue(row, "NIF"),
    // ...
};
```

### 4. Validacao de Dados

Dois niveis de validacao:

**Data Annotations:**
- Campos obrigatorios
- Comprimentos minimos/maximos
- Formatos especificos

**Regras de Negocio:**
- NIF unico (sistema + ficheiro)
- Email unico (sistema + ficheiro)
- Enums validos

### 5. Persistencia em Lotes

Registos validos sao guardados em lotes para optimizar performance:

```csharp
if (successfulPeople.Count >= options.BatchSize)
{
    await CommitBatchAsync(successfulPeople, result, batchResultIndices);
    successfulPeople.Clear();
}
```

### 6. Pos-Processamento

- Invalidacao da cache
- Geracao de notificacoes para documentos em falta

---

## Validacoes e Tratamento de Erros

### Estrutura do Resultado

```csharp
public class BulkImportResult<T>
{
    public bool Success { get; set; }
    public int TotalRows { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<ImportRowResult<T>> Results { get; set; }
    public List<string> GlobalErrors { get; set; }
}
```

### Tipos de Erro

| Tipo | Nivel | Comportamento |
|------|-------|---------------|
| Erro global | Ficheiro | Interrompe processamento |
| Erro de linha | Linha | Linha ignorada, processamento continua |
| Aviso | Linha | Linha importada com aviso |

### Exemplo de Resposta

```json
{
  "success": true,
  "totalRows": 50,
  "successCount": 48,
  "failureCount": 2,
  "results": [
    {
      "rowNumber": 15,
      "success": false,
      "errors": [
        {
          "field": "NIF",
          "errorMessage": "NIF ja existe no sistema",
          "attemptedValue": "123456789",
          "severity": "Error"
        }
      ]
    }
  ],
  "summary": "Importacao concluida: 48 com sucesso, 2 falhadas"
}
```

---

## Estrutura dos Templates

### Localizacao

```
NERBABO.Backend/NERBABO.ApiService/wwwroot/templates/
├── People_Import_Template.csv
└── People_Import_Template.xlsx
```

### Exemplo CSV

```csv
NomeCompleto;NIF;NumeroIdentificacao;DataValidacaoIdentificacao;NISS;IBAN;DataNascimento;Morada;CodigoPostal;Telefone;Email;Naturalidade;Nacionalidade;Genero;Habilitacao;TipoIdentificacao
Joao Silva;123456789;AB123456;31/12/2030;12345678901;PT50000201231234567890154;15/01/1990;Rua Example 123;1000-001;912345678;joao.silva@example.com;Lisboa;Portuguesa;Masculino;Licenciatura;Identificacao Civil (CC/BI)
```

---

## Expandir para Novas Entidades

Para implementar importacao em massa para uma nova entidade, seguir estes passos:

### 1. Criar Interface Especifica

```csharp
// Core/[Entity]/BulkImport/Services/I[Entity]BulkImportService.cs
public interface ICompanyBulkImportService :
    IBulkImportService<CreateCompanyDto, Company, RetrieveCompanyDto>
{
}
```

### 2. Implementar Servico

```csharp
// Core/[Entity]/BulkImport/Services/[Entity]BulkImportService.cs
public class CompanyBulkImportService : ICompanyBulkImportService
{
    // Definir cabecalhos obrigatorios
    private readonly List<string> RequiredHeaders = new()
    {
        "Designacao", "NIF"
    };

    // Definir cabecalhos opcionais
    private readonly List<string> OptionalHeaders = new()
    {
        "Morada", "Telefone", "Localidade", "CodigoPostal", "Email",
        "SetorAtividade", "Dimensao"
    };

    // Implementar ImportFromFileAsync
    // Implementar MapRowToDto
    // Implementar ValidateDto
    // Implementar GetTemplateFileAsync
    // Implementar ValidateImportAsync
}
```

### 3. Criar Controller

```csharp
// Core/[Entity]/Controllers/[Entity]ImportController.cs
[Route("api/companies/import")]
[ApiController]
public class CompanyImportController : ControllerBase
{
    // Injetar ICompanyBulkImportService
    // Implementar endpoints CSV/Excel
    // Implementar endpoints validate
    // Implementar endpoints template
}
```

### 4. Registar Servico

```csharp
// Program.cs
builder.Services.AddScoped<ICompanyBulkImportService, CompanyBulkImportService>();
```

### 5. Criar Templates

Criar ficheiros em `wwwroot/templates/`:
- `Company_Import_Template.csv`
- `Company_Import_Template.xlsx`

### 6. Implementar Frontend

```typescript
// core/services/companies.service.ts
importCompaniesFromCsv(file: File): Observable<any> {
  const formData = new FormData();
  formData.append('file', file);
  return this.http.post<any>(
    `${API_ENDPOINTS.companies}import/csv`,
    formData
  );
}
```

### Exemplo Completo: Importacao de Empresas

Estrutura de ficheiros a criar:

```
Core/Companies/
├── BulkImport/
│   └── Services/
│       ├── ICompanyBulkImportService.cs
│       └── CompanyBulkImportService.cs
└── Controllers/
    └── CompanyImportController.cs

wwwroot/templates/
├── Company_Import_Template.csv
└── Company_Import_Template.xlsx
```

---

## Boas Praticas

### Performance

1. **Pre-carregar dados existentes** - Antes de processar, carregar NIFs/emails existentes para validacao eficiente
2. **Processamento em lotes** - Usar `BatchSize` adequado (50-200 registos)
3. **HashSets para unicidade** - Usar HashSet para verificar duplicados rapidamente

### Validacao

1. **Validar estrutura primeiro** - Falhar cedo se cabecalhos estao incorretos
2. **Validar duplicados no ficheiro** - Adicionar ao HashSet apos cada linha valida
3. **Mensagens de erro claras** - Incluir campo, valor tentado e mensagem descritiva

### Seguranca

1. **Limite de tamanho** - Usar `[RequestSizeLimit]` (10MB recomendado)
2. **Autorizacao** - Proteger endpoints com `[Authorize]`
3. **Validar extensoes** - Verificar extensao de ficheiro antes de processar

### UX

1. **Templates pre-preenchidos** - Incluir exemplos nos templates
2. **Instrucoes no modal** - Documentar valores permitidos
3. **Resultados detalhados** - Mostrar erros por linha com contexto

---

## Referencias

### Codigo Fonte

| Componente | Localizacao |
|------------|-------------|
| Interface base | `Shared/BulkImport/Services/IBulkImportService.cs` |
| Modelos de resultado | `Shared/BulkImport/Models/BulkImportResult.cs` |
| Parser CSV | `Shared/BulkImport/Services/CsvParserService.cs` |
| Parser Excel | `Shared/BulkImport/Services/ExcelParserService.cs` |
| Servico People | `Core/People/BulkImport/Services/PeopleBulkImportService.cs` |
| Controller People | `Core/People/Controllers/PeopleImportController.cs` |
| Frontend People | `features/people/import-people/` |
| Templates | `wwwroot/templates/` |

### Bibliotecas Utilizadas

| Biblioteca | Finalidade |
|------------|------------|
| CsvHelper | Parsing de ficheiros CSV |
| ClosedXML | Parsing de ficheiros Excel |
| Humanizer | Conversao de enums para texto legivel |

### Documentacao Relacionada

> Ver: [Validacoes de Entidades](./VALIDACOES.md)

> Ver: [Logica de Negocio](./LOGICA_DE_NEGOCIO.md)

> Ref: [CsvHelper Documentation](https://joshclose.github.io/CsvHelper/)

> Ref: [ClosedXML Documentation](https://closedxml.github.io/ClosedXML/)
