# Geracao de PDF

Este documento descreve o sistema de geracao de relatorios PDF implementado no NERBA Backoffice, utilizando a biblioteca **QuestPDF**.

## Indice

- [Visao Geral](#visao-geral)
- [Arquitetura](#arquitetura)
- [Componentes Principais](#componentes-principais)
- [Padrao Composer](#padrao-composer)
- [Fluxo de Geracao](#fluxo-de-geracao)
- [Como Adicionar um Novo Relatorio](#como-adicionar-um-novo-relatorio)
- [Referencias](#referencias)

---

## Visao Geral

O sistema utiliza a biblioteca **QuestPDF** para gerar documentos PDF de forma programatica. A arquitetura foi desenhada para separar a logica de recuperacao de dados (Services) da logica de layout visual (Composers).

### Tecnologias

- **QuestPDF**: Biblioteca de layout fluente para .NET.
- **ASP.NET Core DI**: Injecao de dependencia para gestao de servicos e composers.

---

## Arquitetura

O sistema segue uma arquitetura baseada em servicos e compositores visuais:

```
Core/
├── Reports/
│   ├── Controllers/
│   │   └── PdfController.cs       # Endpoints API
│   ├── Services/
│   │   ├── IPdfService.cs         # Interface do servico
│   │   └── PdfService.cs          # Orquestrador de dados e persistencia
│   ├── Composers/                 # Logica visual (Layouts)
│   │   ├── HelperComposer.cs      # Estilos e componentes partilhados
│   │   ├── SessionsTimelineComposer.cs
│   │   ├── TeacherFormComposer.cs
│   │   └── ...
│   └── Models/
│       └── SavedPdf.cs            # Modelo para persistencia de metadados
```

---

## Componentes Principais

### PdfController

Responsavel por expor os endpoints HTTP. Ele verifica permissoes, obtem o utilizador atual e chama o `PdfService`. Retorna o ficheiro PDF gerado (`FileResult`) ou um erro (`ProblemDetails`).

### PdfService

O servico central que:
1. **Recupera Dados**: Consulta o `AppDbContext` para obter todas as entidades necessarias (Acoes, Sessoes, Formadores, etc.).
2. **Invoca Composers**: Instancia o composer apropriado injetado via DI e passa os dados.
3. **Persistencia**: Guarda o PDF gerado em disco (`storage/pdfs`) e regista metadados na tabela `SavedPdfs`.
4. **Caching**: Verifica se o conteudo mudou (via hash SHA256) antes de substituir ficheiros existentes.

### Composers

Classes responsaveis exclusivamente pelo layout do documento. Cada relatorio tem o seu proprio composer.

**Exemplo de estrutura de um Composer:**

```csharp
public class MyReportComposer
{
    private readonly HelperComposer _helperComposer;

    public MyReportComposer(HelperComposer helperComposer)
    {
        _helperComposer = helperComposer;
    }

    public async Task<Document> ComposeAsync(MyEntity data, GeneralInfo infos)
    {
        // Carregar recursos (logos, imagens)
        // Definir estrutura do documento (Pagina, Cabecalho, Conteudo, Rodape)
        return Document.Create(container => { ... });
    }
}
```

### HelperComposer

Contem estilos e componentes reutilizaveis para manter a consistencia visual entre relatorios:
- Cabecalhos e Rodapes padrao.
- Carregamento de Logos.
- Estilos de texto e tabelas.

---

## Padrao Composer

O padrao Composer isola a complexidade do layout QuestPDF.

### Injecao de Dependencia

Todos os composers sao registados no container DI em `Program.cs`:

```csharp
// Register Pdf Composer Services
builder.Services.AddScoped<HelperComposer>();
builder.Services.AddScoped<SessionsTimelineComposer>();
builder.Services.AddScoped<CoverActionReportComposer>();
// ...
```

Isso permite que um composer utilize outros composers (como o `HelperComposer`) via construtor.

---

## Fluxo de Geracao

1. **Request**: O cliente solicita `GET /api/Pdf/action/{id}/report`.
2. **Service**: `PdfService.GenerateReportAsync` e chamado.
3. **Data Fetch**: Os dados sao carregados do `AppDbContext` com `Include()` para relacoes.
4. **Validacao**: Se a entidade nao existir, retorna `Result.Fail`.
5. **Composicao**: O metodo `ComposeAsync` do respectivo Composer e chamado.
6. **Geracao**: `document.GeneratePdf()` converte o layout em bytes.
7. **Salvamento**: `SavePdfAsync` guarda o ficheiro e cria registo na base de dados.
8. **Response**: O controller retorna o ficheiro PDF.

---

## Como Adicionar um Novo Relatorio

Para adicionar um novo relatorio ao sistema, siga estes passos:

### 1. Criar o Composer

Crie uma nova classe em `Core/Reports/Composers/` (ex: `NewReportComposer.cs`). Injete o `HelperComposer`.

```csharp
public class NewReportComposer(HelperComposer helperComposer)
{
    public async Task<Document> ComposeAsync(MyData data, GeneralInfo infos)
    {
        return Document.Create(container => 
        {
            // Definir layout
        });
    }
}
```

### 2. Registar no DI

Adicione o registo no `Program.cs`:

```csharp
builder.Services.AddScoped<NewReportComposer>();
```

### 3. Atualizar o PdfService

Adicione um novo metodo ao `IPdfService` e implemente no `PdfService`.

```csharp
public async Task<Result<byte[]>> GenerateNewReportAsync(long id, string userId)
{
    // 1. Obter dados
    // 2. Chamar _newReportComposer.ComposeAsync(...)
    // 3. Gerar e guardar PDF
}
```

Nao se esqueca de injetar o novo composer no construtor do `PdfService`.

### 4. Criar Endpoint no Controller

Adicione uma acao no `PdfController`:

```csharp
[HttpGet("new-report/{id}")]
public async Task<IActionResult> GenerateNewReportAsync(long id)
{
    // ... chamar service e retornar File
}
```

---

## Referencias

- [QuestPDF Documentation](https://www.questpdf.com/documentation/getting-started.html)
- [Padrao Result](./RESULT_PATTERN.md)
