# Regras de Negocio e Logica do Sistema

Este documento descreve todas as regras de negocio, relacoes entre entidades, estados permitidos e restricoes de eliminacao para cada entidade do sistema NERBA Backoffice.

## Indice

- [User (Utilizador)](#user-utilizador)
- [Person (Pessoa)](#person-pessoa)
- [Company (Empresa)](#company-empresa)
- [Course (Curso)](#course-curso)
- [CourseAction (Acao de Formacao)](#courseaction-acao-de-formacao)
- [Frame (Enquadramento)](#frame-enquadramento)
- [Module (Modulo)](#module-modulo)
- [Category (Categoria de Modulo)](#category-categoria-de-modulo)
- [ModuleTeaching (Lecionacao de Modulo)](#moduleteaching-lecionacao-de-modulo)
- [ModuleAvaliation (Avaliacao de Modulo)](#moduleavaliation-avaliacao-de-modulo)
- [Session (Sessao)](#session-sessao)
- [SessionParticipation (Participacao em Sessao)](#sessionparticipation-participacao-em-sessao)
- [ActionEnrollment (Inscricao em Acao)](#actionenrollment-inscricao-em-acao)
- [Student (Formando)](#student-formando)
- [Teacher (Formador)](#teacher-formador)
- [Tax (Imposto)](#tax-imposto)
- [GeneralInfo (Informacoes Gerais)](#generalinfo-informacoes-gerais)
- [Notification (Notificacao)](#notification-notificacao)

---

## User (Utilizador)

### Relacoes

| Relacao | Tipo | Obrigatoriedade | Descricao |
|---------|------|-----------------|-----------|
| Person | N:1 | Obrigatoria | Cada utilizador deve estar associado a uma pessoa |
| Actions (Coordenator) | 1:N | Opcional | Lista de acoes onde o utilizador e coordenador |

### Regras de Negocio

| Regra | Descricao | Camada |
|-------|-----------|--------|
| Pessoa obrigatoria | Um utilizador deve sempre estar associado a uma pessoa | Service |
| Estado ativo | Utilizador pode ser desativado (IsActive = false) sem ser eliminado | Model |
| Ultimo login | Sistema regista automaticamente a data do ultimo login | Model |

### Propriedades Calculadas

| Propriedade | Logica | Descricao |
|-------------|--------|-----------|
| LastLogin | Humanizer | Texto humanizado do ultimo login em pt-PT |

### Restricoes de Eliminacao

| Condicao | Comportamento |
|----------|---------------|
| Utilizador ativo | Utilizador pode ser desativado em vez de eliminado |

---

## Person (Pessoa)

### Relacoes

| Relacao | Tipo | Obrigatoriedade | Descricao |
|---------|------|-----------------|-----------|
| User | 1:1 | Opcional | Pessoa pode ser um utilizador do sistema |
| Teacher | 1:1 | Opcional | Pessoa pode ser um formador |
| Student | 1:1 | Opcional | Pessoa pode ser um formando |
| HabilitationComprovativePdf | N:1 | Opcional | PDF comprovativo de habilitacoes |
| IbanComprovativePdf | N:1 | Opcional | PDF comprovativo de IBAN |
| IdentificationDocumentPdf | N:1 | Opcional | PDF do documento de identificacao |

### Regras de Negocio

| Regra | Descricao | Camada |
|-------|-----------|--------|
| NIF unico | O NIF deve ser unico no sistema | Service |
| NISS unico | O NISS, se fornecido, deve ser unico | Service |
| Numero Identificacao unico | O numero de identificacao, se fornecido, deve ser unico | Service |
| Email unico | O email, se fornecido, deve ser unico | Service |
| Validacao de habilitacao | Pessoa pode verificar se cumpre nivel minimo de habilitacao | Model |

### Propriedades Calculadas

| Propriedade | Logica | Descricao |
|-------------|--------|-----------|
| IsTeacher | Teacher != null | Indica se pessoa e formador |
| IsStudent | Student != null | Indica se pessoa e formando |
| IsColaborator | User != null | Indica se pessoa e colaborador/utilizador |
| FullName | FirstName + LastName | Nome completo da pessoa |

### Restricoes de Eliminacao

| Condicao | Comportamento |
|----------|---------------|
| E Utilizador | Nao pode eliminar pessoa que e utilizador |
| E Formador com ModuleTeachings | Nao pode eliminar pessoa que lecionou modulos |
| E Formando com ActionEnrollments | Nao pode eliminar pessoa inscrita em acoes |

---

## Company (Empresa)

### Relacoes

| Relacao | Tipo | Obrigatoriedade | Descricao |
|---------|------|-----------------|-----------|
| Students | 1:N | Opcional | Lista de formandos associados a empresa |

### Propriedades Calculadas

| Propriedade | Logica | Descricao |
|-------------|--------|-----------|
| StudentsCount | Students.Count | Numero de formandos associados |

### Restricoes de Eliminacao

Sem restricoes especificas implementadas.

---

## Course (Curso)

### Relacoes

| Relacao | Tipo | Obrigatoriedade | Descricao |
|---------|------|-----------------|-----------|
| Frame | N:1 | Obrigatoria | Enquadramento do curso |
| Modules | N:N | Opcional | Lista de modulos do curso |
| Actions | 1:N | Opcional | Lista de acoes formativas do curso |

### Estados e Transicoes

| Estado | Descricao | Transicoes Permitidas |
|--------|-----------|----------------------|
| NotStarted | Curso nao iniciado | InProgress, Cancelled |
| InProgress | Curso em progresso | Completed, Cancelled |
| Completed | Curso concluido | - |
| Cancelled | Curso cancelado | - |

### Regras de Negocio

| Regra | Descricao | Camada |
|-------|-----------|--------|
| Titulo unico | O titulo do curso deve ser unico | Service |
| Duracao dos modulos | A soma das horas dos modulos nao pode exceder TotalDuration | Service |
| Modulo ativo | So modulos ativos podem ser atribuidos ao curso | Service |
| Curso ativo para modulos | So pode atribuir modulos a cursos ativos (NotStarted ou InProgress) | Service |
| Modulo ja atribuido | Um modulo nao pode ser atribuido duas vezes ao mesmo curso | Service |
| Concluir curso | Curso so pode ser concluido se todas as acoes estiverem concluidas | Service |
| Cancelar curso | Curso so pode ser cancelado se todas as acoes estiverem canceladas | Service |
| Remover modulos | Nao pode remover modulos de cursos ativos | Service |

### Propriedades Calculadas

| Propriedade | Logica | Descricao |
|-------------|--------|-----------|
| CurrentDuration | Sum(Modules.Hours) | Duracao atual atribuida em horas |
| RemainingDuration | TotalDuration - CurrentDuration | Horas restantes disponiveis |
| IsCourseActive | Status == NotStarted \|\| InProgress | Indica se curso esta ativo |
| ActionsQnt | Actions.Count | Numero de acoes |
| ModulesQnt | Modules.Count | Numero de modulos |
| FormattedModuleNames | Lista formatada | Nomes dos modulos com horas |

### Restricoes de Eliminacao

| Condicao | Comportamento |
|----------|---------------|
| Tem acoes associadas | Nao pode eliminar curso com acoes |
| Curso concluido | Nao pode eliminar cursos concluidos |

---

## CourseAction (Acao de Formacao)

### Relacoes

| Relacao | Tipo | Obrigatoriedade | Descricao |
|---------|------|-----------------|-----------|
| Course | N:1 | Obrigatoria | Curso associado a acao |
| Coordenator (User) | N:1 | Obrigatoria | Utilizador coordenador da acao |
| ModuleTeachings | 1:N | Opcional | Lecionacoes de modulos nesta acao |
| ActionEnrollments | 1:N | Opcional | Inscricoes de formandos |

### Estados e Transicoes

| Estado | Descricao | Transicoes Permitidas |
|--------|-----------|----------------------|
| NotStarted | Acao nao iniciada | InProgress, Cancelled |
| InProgress | Acao em progresso | Completed, Cancelled |
| Completed | Acao concluida | - |
| Cancelled | Acao cancelada | - |

### Regras de Negocio

| Regra | Descricao | Camada |
|-------|-----------|--------|
| Codigo administrativo unico | O codigo administrativo deve ser unico | Service |
| Data inicio < Data fim | Data de inicio deve ser anterior a data de fim | Service |
| Numero automatico | ActionNumber e gerado automaticamente (Actions.Count + 1) | Model |
| Alterar estado | Apenas o coordenador pode alterar o estado da acao | Service |

### Propriedades Calculadas

| Propriedade | Logica | Descricao |
|-------------|--------|-----------|
| AllPaymentsProcessed | All(ModuleTeachings.PaymentProcessed) | Todos pagamentos processados |
| AllModulesOfActionHaveTeacher | Todos modulos tem formador | Validacao de formadores |
| IsActionActive | Status == NotStarted \|\| InProgress | Acao ativa |
| AllSessionsScheduled | All(ModuleTeachings.ScheduledPercent == 100) | Todas sessoes agendadas |
| Title | ActionNumber + Locality | Titulo formatado |
| TotalStudents | ActionEnrollments.Count | Total de formandos |
| TotalApproved | Count(ApprovalStatus == Approved) | Total aprovados |
| TotalVolumeHours | Sum(Participations.Attendance) | Volume total de horas |
| TotalVolumeDays | Count(Presence == Present) | Dias de formacao |

### Restricoes de Eliminacao

Acoes podem ser eliminadas, eliminando em cascata os reports associados.

---

## Frame (Enquadramento)

### Relacoes

| Relacao | Tipo | Obrigatoriedade | Descricao |
|---------|------|-----------------|-----------|
| Courses | 1:N | Opcional | Cursos associados ao enquadramento |

### Regras de Negocio

| Regra | Descricao | Camada |
|-------|-----------|--------|
| Programa unico | O programa deve ser unico | Service |
| Operacao unica | A operacao deve ser unica | Service |
| Logo financiamento obrigatorio | O logo de financiamento e obrigatorio | Service |
| Logo financiamento nao removivel | Nao pode remover o logo de financiamento | Service |
| Validacao de imagem | Logos devem ser imagens validas (JPG, PNG, GIF, BMP) < 5MB | Service |

### Restricoes de Eliminacao

| Condicao | Comportamento |
|----------|---------------|
| Tem cursos associados | Nao pode eliminar enquadramento com cursos |

---

## Module (Modulo)

### Relacoes

| Relacao | Tipo | Obrigatoriedade | Descricao |
|---------|------|-----------------|-----------|
| Category | N:1 | Obrigatoria | Categoria do modulo |
| Courses | N:N | Opcional | Cursos que incluem este modulo |
| ModuleTeachings | 1:N | Opcional | Lecionacoes deste modulo |

### Regras de Negocio

| Regra | Descricao | Camada |
|-------|-----------|--------|
| Combinacao unica | Nome + Horas + Categoria deve ser unico | Service |
| Modulo ativo | Modulos inativos nao podem ser atribuidos a cursos | Service |
| Alterar horas | Nao pode alterar horas se modulo esta associado a cursos | Service |
| Toggle ativacao | Nao pode alterar ativacao se associado a cursos ativos | Service |

### Propriedades Calculadas

| Propriedade | Logica | Descricao |
|-------------|--------|-----------|
| CoursesQnt | Courses.Count | Numero de cursos que usam o modulo |

### Restricoes de Eliminacao

| Condicao | Comportamento |
|----------|---------------|
| Cursos ativos associados | Nao pode eliminar modulo com cursos ativos |

---

## Category (Categoria de Modulo)

### Relacoes

| Relacao | Tipo | Obrigatoriedade | Descricao |
|---------|------|-----------------|-----------|
| Modules | 1:N | Opcional | Modulos desta categoria |

### Regras de Negocio

| Regra | Descricao | Camada |
|-------|-----------|--------|
| Igualdade por nome | Duas categorias sao iguais se Nome e ShortenName coincidem | Model |

---

## ModuleTeaching (Lecionacao de Modulo)

### Relacoes

| Relacao | Tipo | Obrigatoriedade | Descricao |
|---------|------|-----------------|-----------|
| Teacher | N:1 | Obrigatoria | Formador que leciona |
| Action | N:1 | Obrigatoria | Acao formativa |
| Module | N:1 | Obrigatoria | Modulo lecionado |
| Sessions | 1:N | Opcional | Sessoes da lecionacao |
| Avaliations | 1:N | Opcional | Avaliacoes dos formandos |

### Regras de Negocio

| Regra | Descricao | Camada |
|-------|-----------|--------|
| Modulo pertence a acao | O modulo deve pertencer ao curso da acao | Service |
| Formador cumpre habilitacao | Formador deve ter habilitacao superior ao minimo do curso | Service |
| Um formador por modulo/acao | Cada modulo numa acao so pode ter um formador | Service |

### Propriedades Calculadas

| Propriedade | Logica | Descricao |
|-------------|--------|-----------|
| AvaliationAvg | (AvaliationCoordenator + AvaliationStudents) / 2 | Media de avaliacoes |
| PaymentProcessed | PaymentDate.HasValue | Pagamento processado |
| ScheduledSessionsTime | Sum(Sessions.DurationHours) | Horas agendadas |
| IsModuleHoursScheduled | ScheduledSessionsTime == Module.Hours | Horas completas |
| IsPayed | PaymentDate != null | Pagamento efetuado |
| ScheduledPercent | (ScheduledSessionsTime * 100) / Module.Hours | Percentagem agendada |
| CalculatedTotal | Sum(Sessions where Present) * hourRate | Total calculado |

---

## ModuleAvaliation (Avaliacao de Modulo)

### Relacoes

| Relacao | Tipo | Obrigatoriedade | Descricao |
|---------|------|-----------------|-----------|
| ModuleTeaching | N:1 | Obrigatoria | Lecionacao avaliada |
| ActionEnrollment | N:1 | Obrigatoria | Inscricao do formando |

### Regras de Negocio

| Regra | Descricao | Camada |
|-------|-----------|--------|
| Nota 0-5 | Nota deve estar entre 0 e 5 | DTO Validation |
| Criacao automatica | Criada automaticamente ao inscrever formando | Service |

### Propriedades Calculadas

| Propriedade | Logica | Descricao |
|-------------|--------|-----------|
| Evaluated | Grade != 0 | Indica se foi avaliado |

---

## Session (Sessao)

### Relacoes

| Relacao | Tipo | Obrigatoriedade | Descricao |
|---------|------|-----------------|-----------|
| ModuleTeaching | N:1 | Obrigatoria | Lecionacao associada |
| Participants | 1:N | Opcional | Participacoes de formandos |

### Regras de Negocio

| Regra | Descricao | Camada |
|-------|-----------|--------|
| Acao ativa | So pode criar sessao se acao esta ativa | Service |
| Duracao curso completa | Curso deve ter duracao total preenchida | Service |
| Dia semana valido | Dia deve estar nos dias permitidos da acao | Service |
| Data no periodo | Data deve estar entre inicio e fim da acao | Service |
| Horas do modulo | Soma das sessoes nao pode exceder horas do modulo | Service |

### Propriedades Calculadas

| Propriedade | Logica | Descricao |
|-------------|--------|-----------|
| End | Start.AddHours(DurationHours) | Hora de fim |
| Time | Start - End formatado | Intervalo de tempo |

### Restricoes de Eliminacao

| Condicao | Comportamento |
|----------|---------------|
| Sessao lecionada | Nao pode eliminar sessao com TeacherPresence = Present |

---

## SessionParticipation (Participacao em Sessao)

### Relacoes

| Relacao | Tipo | Obrigatoriedade | Descricao |
|---------|------|-----------------|-----------|
| Session | N:1 | Obrigatoria | Sessao da participacao |
| ActionEnrollment | N:1 | Obrigatoria | Inscricao do formando |

### Estados de Presenca

| Estado | Descricao |
|--------|-----------|
| Unknown | Nao especificado |
| Present | Presente |
| Absent | Faltou |

### Regras de Negocio

| Regra | Descricao | Camada |
|-------|-----------|--------|
| Criacao automatica | Criada automaticamente ao inscrever formando | Service |
| Assiduidade 0-24 | Attendance deve estar entre 0 e 24 horas | DTO Validation |

### Propriedades Calculadas

| Propriedade | Logica | Descricao |
|-------------|--------|-----------|
| AttendanceHours | Floor(Attendance) | Horas inteiras |
| AttendanceMinutes | (Attendance - Floor) * 60 | Minutos restantes |

---

## ActionEnrollment (Inscricao em Acao)

### Relacoes

| Relacao | Tipo | Obrigatoriedade | Descricao |
|---------|------|-----------------|-----------|
| Action | N:1 | Obrigatoria | Acao da inscricao |
| Student | N:1 | Obrigatoria | Formando inscrito |
| Participations | 1:N | Automatica | Participacoes nas sessoes |
| Avaliations | 1:N | Automatica | Avaliacoes dos modulos |

### Estados de Aprovacao

| Estado | Descricao | Condicao |
|--------|-----------|----------|
| NotSpecified | Nao especificado | Nao avaliado |
| Approved | Aprovado | Media >= 3 |
| Rejected | Reprovado | Media < 3 |
| Dropped | Desistiu | - |

### Regras de Negocio

| Regra | Descricao | Camada |
|-------|-----------|--------|
| Todos modulos com formador | Todos modulos da acao devem ter formador | Service |
| Inscricao unica | Formando nao pode estar inscrito duas vezes na mesma acao | Service |
| Sessoes agendadas | Todas sessoes devem estar agendadas | Service |
| Habilitacao minima | Formando deve cumprir habilitacao minima do curso | Service |
| Criacao de relacoes | Ao inscrever, cria SessionParticipation e ModuleAvaliation | Service |

### Propriedades Calculadas

| Propriedade | Logica | Descricao |
|-------------|--------|-----------|
| AvgEvaluation | Sum(Avaliations.Grade) / Count | Media de avaliacoes |
| StudentAvaliated | All(Avaliations.Evaluated) | Todos modulos avaliados |
| IsPayed | PaymentDate != null | Pagamento efetuado |
| ApprovalStatus | Calculado com base em avaliacoes | Estado de aprovacao |
| CalculatedTotal | Sum(Participations where Present) * hourRate | Total calculado |

### Restricoes de Eliminacao

Eliminacao em cascata remove SessionParticipations e ModuleAvaliations associadas.

---

## Student (Formando)

### Relacoes

| Relacao | Tipo | Obrigatoriedade | Descricao |
|---------|------|-----------------|-----------|
| Person | N:1 | Obrigatoria | Pessoa associada |
| Company | N:1 | Opcional | Empresa do formando |
| ActionEnrollments | 1:N | Opcional | Inscricoes em acoes |

### Regras de Negocio

| Regra | Descricao | Camada |
|-------|-----------|--------|
| Pessoa existente | Pessoa associada deve existir | Service |
| Pessoa unica | Uma pessoa so pode ser um formando | Service |

### Propriedades Calculadas

| Propriedade | Logica | Descricao |
|-------------|--------|-----------|
| CanDelete | ActionEnrollments.Count == 0 | Pode ser eliminado |

### Restricoes de Eliminacao

| Condicao | Comportamento |
|----------|---------------|
| Tem inscricoes | Nao pode eliminar formando com inscricoes |

---

## Teacher (Formador)

### Relacoes

| Relacao | Tipo | Obrigatoriedade | Descricao |
|---------|------|-----------------|-----------|
| Person | N:1 | Obrigatoria | Pessoa associada |
| IvaRegime (Tax) | N:1 | Obrigatoria | Regime de IVA |
| IrsRegime (Tax) | N:1 | Obrigatoria | Regime de IRS |
| ModuleTeachings | 1:N | Opcional | Modulos lecionados |

### Regras de Negocio

| Regra | Descricao | Camada |
|-------|-----------|--------|
| Pessoa existente | Pessoa associada deve existir | Service |
| Pessoa unica | Uma pessoa so pode ser um formador | Service |
| CCP unico | Certificado de Competencias Pedagogicas deve ser unico | Service |
| Regime IVA valido | Regime IVA deve ser do tipo IVA | Service |
| Regime IRS valido | Regime IRS deve ser do tipo IRS | Service |

### Propriedades Calculadas

| Propriedade | Logica | Descricao |
|-------------|--------|-----------|
| CanDelete | ModuleTeachings.Count == 0 | Pode ser eliminado |
| CommaSeparatedCompetences | Join(Competences, ", ") | Competencias formatadas |

### Restricoes de Eliminacao

| Condicao | Comportamento |
|----------|---------------|
| Tem modulos lecionados | Nao pode eliminar formador com ModuleTeachings |

---

## Tax (Imposto)

### Relacoes

| Relacao | Tipo | Obrigatoriedade | Descricao |
|---------|------|-----------------|-----------|
| GeneralInfo | 1:1 | Opcional | Informacoes gerais (IVA) |
| IvaTeachers | 1:N | Opcional | Formadores com este regime IVA |
| IrsTeachers | 1:N | Opcional | Formadores com este regime IRS |

### Tipos de Imposto

| Tipo | Descricao |
|------|-----------|
| IVA | Imposto sobre Valor Acrescentado |
| IRS | Imposto sobre Rendimento de Pessoas Singulares |

### Regras de Negocio

| Regra | Descricao | Camada |
|-------|-----------|--------|
| Percentagem 0-100 | Valor percentual entre 0 e 100 | DTO Validation |
| Tipo valido | Tipo deve ser IVA ou IRS | Model |

### Propriedades Calculadas

| Propriedade | Logica | Descricao |
|-------------|--------|-----------|
| ValueDecimal | ValuePercent / 100 | Valor em decimal |

---

## GeneralInfo (Informacoes Gerais)

### Relacoes

| Relacao | Tipo | Obrigatoriedade | Descricao |
|---------|------|-----------------|-----------|
| IvaTax | N:1 | Obrigatoria | Taxa IVA da organizacao |

### Regras de Negocio

| Regra | Descricao | Camada |
|-------|-----------|--------|
| Registo unico | Apenas um registo de GeneralInfo no sistema | Inicializacao |
| Sem criacao manual | Dados sao inicializados no sistema, so atualizacao permitida | Controller |

### Propriedades Calculadas

| Propriedade | Logica | Descricao |
|-------------|--------|-----------|
| HourlySubsidy | HourValueAlimentation formatado | Subsidio por hora em EUR |

---

## Notification (Notificacao)

### Relacoes

| Relacao | Tipo | Obrigatoriedade | Descricao |
|---------|------|-----------------|-----------|
| RelatedPerson | N:1 | Opcional | Pessoa relacionada |
| ReadByUser | N:1 | Opcional | Utilizador que leu |

### Estados

| Estado | Descricao |
|--------|-----------|
| Unread | Nao lida |
| Read | Lida |
| Archived | Arquivada |

### Tipos de Notificacao

Notificacoes sao geradas automaticamente por um servico em background que verifica:
- Documentos em falta para pessoas (habilitacoes, IBAN, identificacao)

### Regras de Negocio

| Regra | Descricao | Camada |
|-------|-----------|--------|
| Geracao automatica | Notificacoes sao geradas por background service | Background Service |
| Regeneracao ao apagar doc | Ao eliminar documento, notificacoes sao regeneradas | Service |

---

## Fluxo de Trabalho Principal

O fluxo tipico de criacao de uma acao formativa segue estes passos:

```
1. Criar Enquadramento (Frame)
   |
2. Criar Curso (Course) com Enquadramento
   |
3. Atribuir Modulos ao Curso
   |-- Validar: Soma horas <= TotalDuration
   |
4. Criar Acao Formativa (CourseAction)
   |-- Validar: Curso existe, Coordenador existe
   |
5. Atribuir Formadores aos Modulos (ModuleTeaching)
   |-- Validar: Formador cumpre habilitacao
   |-- Validar: Modulo pertence ao curso
   |
6. Agendar Sessoes (Session)
   |-- Validar: Acao ativa, Dias validos, Horas do modulo
   |
7. Inscrever Formandos (ActionEnrollment)
   |-- Validar: Todos modulos com formador
   |-- Validar: Todas sessoes agendadas
   |-- Validar: Formando cumpre habilitacao
   |-- Cria automaticamente: SessionParticipation, ModuleAvaliation
   |
8. Registar Presencas e Avaliacoes
   |
9. Processar Pagamentos
```

---

## Referencias

- Localizacao dos Models: `NERBABO.Backend/NERBABO.ApiService/Core/*/Models/`
- Localizacao dos Services: `NERBABO.Backend/NERBABO.ApiService/Core/*/Services/`
- Localizacao dos Enums: `NERBABO.Backend/NERBABO.ApiService/Shared/Enums/`

> Ver: [Validacoes de Entidades](./VALIDATIONS.md)
