# Validacoes de Entidades

Este documento descreve todas as validacoes aplicadas nas operacoes de criacao e atualizacao de cada entidade do sistema NERBA Backoffice.

## Indice

- [Validadores Customizados](#validadores-customizados)
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

## Validadores Customizados

O projeto utiliza validadores customizados para regras de negocio especificas.

| Validador | Descricao | Comportamento |
|-----------|-----------|---------------|
| `ValidateLengthIfNotEmpty` | Valida comprimento apenas se o valor nao for vazio | Aceita valores nulos/vazios; aplica validacao de comprimento caso contrario |
| `ValidateHours` | Valida se um valor representa horas validas | Aceita valores entre min e max horas; pode rejeitar zero |
| `AllNumbers` | Garante que a string contem apenas digitos | Aceita strings vazias/nulas para campos opcionais |
| `ZipCode` | Valida formato de codigo postal portugues | Formato: `NNNN-NNN` (ex: `1234-567`) |
| `FutureDate` | Garante que a data e futura | Formatos aceites: `dd/MM/yyyy`, `yyyy-MM-dd` |
| `PastDate` | Garante que a data e passada | Formatos aceites: `dd/MM/yyyy`, `yyyy-MM-dd` |

---

## User (Utilizador)

### Criacao (RegisterDto)

| Campo | Validacoes | Mensagem de Erro |
|-------|------------|------------------|
| `UserName` | Obrigatorio, 3-30 caracteres | "Username e um campo obrigatorio." / "Nome de Utilizador deve conter pelo menos 3 caracteres e um maximo de 30 caracteres" |
| `Email` | Obrigatorio, formato email valido | "Email e um campo obrigatorio" / "Formato do email invalido." |
| `Password` | Obrigatorio, 8-30 caracteres | "Password e um campo obrigatorio" / "A password deve conter pelo menos 8 caracteres e um maximo de 30 caracteres" |
| `PersonId` | Obrigatorio | "E obrigatorio associar uma pessoa ao utilizador." |

### Atualizacao (UpdateUserDto)

| Campo | Validacoes | Mensagem de Erro |
|-------|------------|------------------|
| `Email` | Obrigatorio, formato email valido | "Email e um campo obrigatorio" / "Formato do email invalido." |
| `UserName` | Obrigatorio, 3-30 caracteres | "Username e um campo obrigatorio." / "Nome de Utilizador deve conter pelo menos 3 caracteres e um maximo de 30 caracteres" |
| `NewPassword` | Obrigatorio, 8-30 caracteres | "Password e um campo obrigatorio" / "A password deve conter pelo menos 8 caracteres e um maximo de 30 caracteres" |

---

## Person (Pessoa)

### Criacao (CreatePersonDto)

| Campo | Validacoes | Mensagem de Erro |
|-------|------------|------------------|
| `FirstName` | Obrigatorio, 3-100 caracteres | "Primeiro Nome e um campo obrigatorio." |
| `LastName` | Obrigatorio, 3-100 caracteres | "Ultimo Nome e um campo obrigatorio." |
| `NIF` | Obrigatorio, exatamente 9 digitos, apenas numeros | "NIF e um campo obrigatorio." / "NIF todos os caracteres devem ser numeros" |
| `IdentificationNumber` | Opcional, 5-10 caracteres | "Numero de Identificacao deve conter ate 10 caracteres." |
| `IdentificationValidationDate` | Opcional, data futura | "Data de Validacao da Identificacao expirou." |
| `NISS` | Opcional, exatamente 11 digitos, apenas numeros | "NISS deve conter exatamente 11 caracteres." |
| `IBAN` | Opcional, exatamente 25 caracteres | "IBAN deve conter exatamente 25 caracteres." |
| `BirthDate` | Opcional, data passada | "A data de nascimento deve ser do passado." |
| `Address` | Opcional | - |
| `ZipCode` | Opcional, formato `NNNN-NNN` | "Codigo Postal deve conter exatamente 8 caracteres no formato '1234-567'." |
| `PhoneNumber` | Opcional, exatamente 9 digitos | "Numero de Telefone deve conter exatamente 9 caracteres." |
| `Email` | Opcional, formato email valido | "Email tem formato invalido." |
| `Naturality` | Opcional, 3-100 caracteres | "Naturalidade deve conter pelo menos 3 caracteres e um maximo de 100 caracteres" |
| `Nationality` | Opcional, 3-100 caracteres | "Nacionalidade deve conter pelo menos 3 caracteres e um maximo de 100 caracteres" |
| `IdentificationType` | Opcional | - |
| `Gender` | Opcional | - |
| `Habilitation` | Opcional | - |

### Atualizacao (UpdatePersonDto)

Validacoes identicas a criacao.

---

## Company (Empresa)

### Criacao (CreateCompanyDto)

| Campo | Validacoes | Mensagem de Erro |
|-------|------------|------------------|
| `Name` | Obrigatorio, 3-155 caracteres | "Designacao e um campo obrigatorio." |
| `Address` | Opcional | - |
| `PhoneNumber` | Opcional, exatamente 9 digitos | "Numero de Telefone deve conter exatamente 9 caracteres." |
| `Locality` | Opcional, 3-55 caracteres | "Localidade deve conter pelo menos 3 caracteres e um maximo de 55 caracteres" |
| `ZipCode` | Opcional, formato `NNNN-NNN` | "Codigo Postal deve conter exatamente 8 caracteres no formato '1234-567'." |
| `Email` | Opcional, formato email valido | "Email tem formato invalido." |
| `AtivitySector` | Opcional (default: "Unknown") | - |
| `Size` | Opcional (default: "Micro") | - |

### Atualizacao (UpdateCompanyDto)

Validacoes identicas a criacao.

---

## Course (Curso)

### Criacao (CreateCourseDto)

| Campo | Validacoes | Mensagem de Erro |
|-------|------------|------------------|
| `FrameId` | Obrigatorio | "Enquadramento e um campo obrigatorio." |
| `Title` | Obrigatorio, 3-255 caracteres | "Titulo / Nome do curso e um campo obrigatorio." |
| `Objectives` | Opcional, 3-510 caracteres | "Objetivos do curso deve conter pelo menos 3 caracteres e um maximo de 510 caracteres" |
| `Destinators` | Opcional (lista) | - |
| `Area` | Opcional, 3-55 caracteres | "Area do curso deve conter pelo menos 3 caracteres e um maximo de 55 caracteres" |
| `TotalDuration` | 0-1000 horas, maior que zero | "O campo deve ser um valor numerico valido entre 0 e 1000 horas." |
| `Status` | Opcional | - |
| `MinHabilitationLevel` | Opcional | - |
| `Modules` | Lista de IDs de modulos | - |

### Atualizacao (UpdateCourseDto)

Validacoes identicas a criacao.

---

## CourseAction (Acao de Formacao)

### Criacao (CreateCourseActionDto)

| Campo | Validacoes | Mensagem de Erro |
|-------|------------|------------------|
| `CourseId` | Obrigatorio | "Curso e um campo obrigatorio." |
| `AdministrationCode` | Obrigatorio, 5-10 digitos, apenas numeros | "Codigo Administrativo e um campo obrigatorio." / "Codigo Administrativo todos os caracteres devem ser numeros" |
| `Address` | Opcional | - |
| `Locality` | Obrigatorio | "Localidade e um campo obrigatorio." |
| `WeekDays` | Opcional (lista) | - |
| `StartDate` | Obrigatorio | "Data de Inicio e um campo obrigatorio." |
| `EndDate` | Obrigatorio | "Data de Fim e um campo obrigatorio." |
| `Regiment` | Obrigatorio | "Estado e um campo obrigatorio." |
| `Status` | Opcional (default: "NotStarted") | - |

> **Nota**: As validacoes `FutureDate` para `StartDate` e `EndDate` estao comentadas no codigo fonte mas podem ser ativadas.

### Atualizacao (UpdateCourseActionDto)

Validacoes identicas a criacao (sem valores default).

---

## Frame (Enquadramento)

### Criacao (CreateFrameDto)

| Campo | Validacoes | Mensagem de Erro |
|-------|------------|------------------|
| `Program` | Obrigatorio, 3-150 caracteres | "Programa e um campo obrigatorio." |
| `Intervention` | Obrigatorio, 3-55 caracteres | "Intervencao e um campo obrigatorio." |
| `InterventionType` | Obrigatorio, 3-150 caracteres | "Tipo de Intervecao e um campo obrigatorio." |
| `Operation` | Obrigatorio, 3-150 caracteres | "Operacao e um campo obrigatorio." |
| `OperationType` | Obrigatorio, 3-150 caracteres | "Operacao Tipo e um campo obrigatorio." |
| `ProgramLogoFile` | Opcional (ficheiro) | - |
| `FinancementLogoFile` | Obrigatorio (ficheiro) | "Logo de Financiamento e um campo obrigatorio." |

### Atualizacao (UpdateFrameDto)

| Campo | Validacoes | Mensagem de Erro |
|-------|------------|------------------|
| `Program` | Obrigatorio, 3-150 caracteres | "Programa e um campo obrigatorio." |
| `Intervention` | Obrigatorio, 3-55 caracteres | "Intervencao e um campo obrigatorio." |
| `InterventionType` | Obrigatorio, 3-150 caracteres | "Tipo de Intervecao e um campo obrigatorio." |
| `Operation` | Obrigatorio, 3-150 caracteres | "Operacao e um campo obrigatorio." |
| `OperationType` | Obrigatorio, 3-150 caracteres | "Operacao Tipo e um campo obrigatorio." |
| `ProgramLogoFile` | Opcional (ficheiro) | - |
| `FinancementLogoFile` | Opcional (ficheiro) | - |
| `RemoveProgramLogo` | Opcional (booleano) | - |
| `RemoveFinancementLogo` | Opcional (booleano, validado na camada de servico) | - |

---

## Module (Modulo)

### Criacao (CreateModuleDto)

| Campo | Validacoes | Mensagem de Erro |
|-------|------------|------------------|
| `Name` | Obrigatorio, 3-255 caracteres | "Nome do Modulo e um campo obrigatorio." |
| `Hours` | 0-1000 horas, maior que zero | "O campo deve ser um valor numerico valido entre 0 e 1000 horas." |
| `Category` | Obrigatorio | "Categoria e um campo obrigatorio." |

### Atualizacao (UpdateModuleDto)

| Campo | Validacoes | Mensagem de Erro |
|-------|------------|------------------|
| `Name` | Obrigatorio, 3-255 caracteres | "Nome do Modulo e um campo obrigatorio." |
| `Hours` | 0-1000 horas, maior que zero | "O campo deve ser um valor numerico valido entre 0 e 1000 horas." |
| `IsActive` | Obrigatorio | "Ativo e um campo obrigatorio." |

---

## Category (Categoria de Modulo)

### Criacao (CreateCategoryDto)

| Campo | Validacoes | Mensagem de Erro |
|-------|------------|------------------|
| `Name` | Obrigatorio, 3-155 caracteres | "Nome da Categoria de Modulo e um campo obrigatorio." |
| `ShortenName` | Obrigatorio, 1-15 caracteres | "Abreviatura da Categoria de Modulo e um campo obrigatorio." |

### Atualizacao (UpdateCategoryDto)

| Campo | Validacoes | Mensagem de Erro |
|-------|------------|------------------|
| `Id` | Obrigatorio | "Id e um campo obrigatorio." |
| `Name` | Obrigatorio, 3-155 caracteres | "Nome da Categoria de Modulo e um campo obrigatorio." |
| `ShortenName` | Obrigatorio, 1-15 caracteres | "Abreviatura da Categoria de Modulo e um campo obrigatorio." |

---

## ModuleTeaching (Lecionacao de Modulo)

### Criacao (CreateModuleTeachingDto)

| Campo | Validacoes | Mensagem de Erro |
|-------|------------|------------------|
| `TeacherId` | Obrigatorio | "Formador e um campo obrigatorio." |
| `ActionId` | Obrigatorio | "Acao Formacao e um campo obrigatorio." |
| `ModuleId` | Obrigatorio | "Modulo e um campo obrigatorio." |

### Atualizacao (UpdateModuleTeachingDto)

Validacoes identicas a criacao.

---

## ModuleAvaliation (Avaliacao de Modulo)

### Criacao (CreateModuleAvaliationDto)

| Campo | Validacoes | Mensagem de Erro |
|-------|------------|------------------|
| `ModuleTeachingId` | Obrigatorio | "ModuleTeachingId e obrigatorio" |
| `ActionEnrollmentId` | Obrigatorio | "ActionEnrollmentId e obrigatorio" |
| `Grade` | Obrigatorio, 0-5 | "Grade e obrigatorio" / "Grade deve estar entre 0 e 5" |

### Atualizacao (UpdateModuleAvaliationDto)

| Campo | Validacoes | Mensagem de Erro |
|-------|------------|------------------|
| `Grade` | Obrigatorio, 0-5 | "Grade e obrigatorio" / "Grade deve estar entre 0 e 5" |

---

## Session (Sessao)

### Criacao (CreateSessionDto)

| Campo | Validacoes | Mensagem de Erro |
|-------|------------|------------------|
| `ModuleTeachingId` | Obrigatorio | "ModuleTeaching e um campo obrigatorio." |
| `Weekday` | Obrigatorio | "Dia da semana e um campo obrigatorio." |
| `ScheduledDate` | Obrigatorio | "Data agendada e um campo obrigatorio." |
| `Start` | Obrigatorio | "Hora de inicio e um campo obrigatorio." |
| `DurationHours` | Obrigatorio, 1-12 horas | "Duracao deve estar entre 1 e 12 horas." |
| `Note` | Opcional, 3-255 caracteres | "Observacao deve conter pelo menos 3 caracteres e um maximo de 255 caracteres" |

> **Nota**: A validacao `FutureDate` para `ScheduledDate` esta comentada no codigo fonte mas pode ser ativada.

### Atualizacao (UpdateSessionDto)

| Campo | Validacoes | Mensagem de Erro |
|-------|------------|------------------|
| `Weekday` | Obrigatorio | "Dia da semana e um campo obrigatorio." |
| `ScheduledDate` | Obrigatorio | "Data agendada e um campo obrigatorio." |
| `Start` | Obrigatorio | "Hora de inicio e um campo obrigatorio." |
| `DurationHours` | Obrigatorio, 0.1-24 horas | "Duracao deve estar entre 0.1 e 24 horas." |
| `TeacherPresence` | Opcional (default: "Unknown") | - |
| `Note` | Opcional, 3-255 caracteres | "Observacao deve conter pelo menos 3 caracteres e um maximo de 255 caracteres" |

---

## SessionParticipation (Participacao em Sessao)

### Criacao (CreateSessionParticipationDto)

| Campo | Validacoes | Mensagem de Erro |
|-------|------------|------------------|
| `SessionId` | Obrigatorio | "Sessao e um campo obrigatorio." |
| `ActionEnrollmentId` | Obrigatorio | "Inscricao na Acao e um campo obrigatorio" |
| `Presence` | Obrigatorio | "Presenca e um campo obrigatorio." |
| `Attendance` | Obrigatorio, 0-24 | "Frequencia e um campo obrigatorio." |

### Atualizacao (UpdateSessionParticipationDto)

| Campo | Validacoes | Mensagem de Erro |
|-------|------------|------------------|
| `SessionParticipationId` | Obrigatorio | "Id da participacao e um campo obrigatorio" |
| `SessionId` | Obrigatorio | "Sessao e um campo obrigatorio." |
| `ActionEnrollmentId` | Obrigatorio | "Inscricao na Acao e um campo obrigatorio" |
| `Presence` | Obrigatorio | "Presenca e um campo obrigatorio." |
| `Attendance` | Obrigatorio, 0-24 | "Frequencia e um campo obrigatorio." |

---

## ActionEnrollment (Inscricao em Acao)

### Criacao (CreateActionEnrollmentDto)

| Campo | Validacoes | Mensagem de Erro |
|-------|------------|------------------|
| `ActionId` | Obrigatorio | "Acao e um campo obrigatorio." |
| `StudentId` | Obrigatorio | "Formando e um campo obrigatorio." |

### Atualizacao (UpdateActionEnrollmentDto)

Validacoes identicas a criacao.

---

## Student (Formando)

### Criacao (CreateStudentDto)

| Campo | Validacoes | Mensagem de Erro |
|-------|------------|------------------|
| `PersonId` | Obrigatorio | "O Formando deve estar associado a uma pessoa." |
| `CompanyId` | Opcional | - |
| `IsEmployeed` | Opcional (booleano) | - |
| `IsRegisteredWithJobCenter` | Opcional (booleano) | - |
| `CompanyRole` | Opcional, 3-55 caracteres | "Competencias deve conter pelo menos 3 caracteres e um maximo de 55 caracteres" |

### Atualizacao (UpdateStudentDto)

Validacoes identicas a criacao.

---

## Teacher (Formador)

### Criacao (CreateTeacherDto)

| Campo | Validacoes | Mensagem de Erro |
|-------|------------|------------------|
| `IvaRegimeId` | Obrigatorio | "Regime IVA e um campo obrigatorio." |
| `IrsRegimeId` | Obrigatorio | "Regime IRS e um campo obrigatorio." |
| `PersonId` | Obrigatorio | "E obrigatorio associar uma pessoa ao utilizador." |
| `Ccp` | Obrigatorio, 3-55 caracteres | "CCP e um campo obrigatorio." |
| `Competences` | Opcional, 3-55 caracteres | "Competencias deve conter pelo menos 3 caracteres e um maximo de 55 caracteres" |

### Atualizacao (UpdateTeacherDto)

| Campo | Validacoes | Mensagem de Erro |
|-------|------------|------------------|
| `IvaRegimeId` | Obrigatorio | "Regime IVA e um campo obrigatorio." |
| `IrsRegimeId` | Obrigatorio | "Regime IRS e um campo obrigatorio." |
| `PersonId` | Obrigatorio | "E obrigatorio associar uma pessoa ao utilizador." |
| `Ccp` | Obrigatorio, 3-55 caracteres | "CCP e um campo obrigatorio." |
| `Competences` | Opcional, 3-55 caracteres | "Competencias deve conter pelo menos 3 caracteres e um maximo de 55 caracteres" |
| `IsLecturingFM` | Opcional (booleano) | - |
| `IsLecturingCQ` | Opcional (booleano) | - |

---

## Tax (Imposto)

### Criacao (CreateTaxDto)

| Campo | Validacoes | Mensagem de Erro |
|-------|------------|------------------|
| `Name` | Obrigatorio, 3-50 caracteres | "Regime e um campo obrigatorio" |
| `ValuePercent` | Obrigatorio, 0-100 | "Valor percentual e um campo obrigatorio" / "Valor percentual deve estar entre 0 e 100" |
| `Type` | Obrigatorio | "Tipo e um campo obrigatorio" |

### Atualizacao (UpdateTaxDto)

| Campo | Validacoes | Mensagem de Erro |
|-------|------------|------------------|
| `Name` | Obrigatorio, 3-50 caracteres | "Regime e um campo obrigatorio" |
| `ValuePercent` | Obrigatorio, 0-100 | "Valor percentual e um campo obrigatorio" / "Valor percentual deve estar entre 0 e 100" |
| `IsActive` | Opcional (booleano) | - |
| `Type` | Obrigatorio | "Tipo e um campo obrigatorio" |

---

## GeneralInfo (Informacoes Gerais)

### Atualizacao (UpdateGeneralInfoDto)

| Campo | Validacoes | Mensagem de Erro |
|-------|------------|------------------|
| `Designation` | Obrigatorio, 3-255 caracteres | "Designacao e um campo obrigatorio." |
| `IvaId` | Obrigatorio | "Taxa Iva e um campo obrigatorio" |
| `Site` | Obrigatorio, 3-500 caracteres | "Sede e um campo obrigatorio." |
| `HourValueTeacher` | Obrigatorio, >= 0 | "Valor Hora do Formador e um campo obrigatorio" |
| `HourValueAlimentation` | Obrigatorio, >= 0 | "Valor Hora Alimentacao e um campo obrigatorio" |
| `BankEntity` | Obrigatorio, 3-50 caracteres | "Entidade Bancaria e um campo obrigatorio." |
| `Iban` | Obrigatorio, exatamente 25 caracteres | "IBAN e um campo obrigatorio." / "IBAN deve conter exatamente 25 caracteres" |
| `Nipc` | Obrigatorio, exatamente 9 digitos | "NIPC e um campo obrigatorio." / "NIPC deve conter somente numeros." |
| `Logo` | Opcional (ficheiro) | - |
| `Email` | Obrigatorio, formato email, 3-100 caracteres | "Email e um campo obrigatorio." / "Email deve ser um endereco de email valido." |
| `Slug` | Obrigatorio, 2-50 caracteres | "Slug e um campo obrigatorio." |
| `PhoneNumber` | Obrigatorio, 9-20 caracteres | "Numero de telefone e um campo obrigatorio." |
| `Website` | Obrigatorio, 3-100 caracteres | "Website e um campo obrigatorio." |
| `InsurancePolicy` | Obrigatorio, 3-200 caracteres | "Apolice de Seguro e um campo obrigatorio." |
| `FacilitiesCharacterization` | Obrigatorio, 10-500 caracteres | "Caracterizacao das Instalacoes e um campo obrigatorio." |

> **Nota**: Nao existe DTO de criacao para GeneralInfo - os dados sao inicializados no sistema.

---

## Notification (Notificacao)

### Criacao (CreateNotificationDto)

| Campo | Validacoes | Mensagem de Erro |
|-------|------------|------------------|
| `Title` | Obrigatorio, max 200 caracteres | "Titulo e um campo obrigatorio." / "Titulo deve conter ate 200 caracteres." |
| `Message` | Obrigatorio, max 1000 caracteres | "Mensagem e um campo obrigatorio." / "Mensagem deve conter ate 1000 caracteres." |
| `Type` | Obrigatorio (enum NotificationTypeEnum) | "Tipo e um campo obrigatorio." |
| `RelatedPersonId` | Opcional | - |
| `RelatedEntityType` | Opcional, max 100 caracteres | "Tipo de Entidade deve conter ate 100 caracteres." |
| `RelatedEntityId` | Opcional | - |
| `ActionUrl` | Opcional, max 500 caracteres | "URL de Acao deve conter ate 500 caracteres." |

### Atualizacao (UpdateNotificationDto)

| Campo | Validacoes | Mensagem de Erro |
|-------|------------|------------------|
| `Id` | Obrigatorio | "Id e um campo obrigatorio." |
| `Status` | Opcional (enum NotificationStatusEnum) | - |

---

## Referencias

- Localizacao dos DTOs: `NERBABO.Backend/NERBABO.ApiService/Core/*/Dtos/`
- Localizacao dos validadores customizados: `NERBABO.Backend/NERBABO.ApiService/Helper/Validators/`

> Ref: [Data Annotations - Microsoft Docs](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.dataannotations)
