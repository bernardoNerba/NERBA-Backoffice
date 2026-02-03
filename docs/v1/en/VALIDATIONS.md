# Entity Validations

This document describes all validations applied in create and update operations for each entity in the NERBA Backoffice system.

## Table of Contents

- [Custom Validators](#custom-validators)
- [User](#user)
- [Person](#person)
- [Company](#company)
- [Course](#course)
- [CourseAction (Training Action)](#courseaction-training-action)
- [Frame](#frame)
- [Module](#module)
- [Category (Module Category)](#category-module-category)
- [ModuleTeaching](#moduleteaching)
- [ModuleAvaliation (Module Evaluation)](#moduleavaliation-module-evaluation)
- [Session](#session)
- [SessionParticipation](#sessionparticipation)
- [ActionEnrollment](#actionenrollment)
- [Student (Trainee)](#student-trainee)
- [Teacher (Trainer)](#teacher-trainer)
- [Tax](#tax)
- [GeneralInfo](#generalinfo)
- [Notification](#notification)

---

## Custom Validators

The project uses custom validators for specific business rules.

| Validator | Description | Behavior |
|-----------|-------------|----------|
| `ValidateLengthIfNotEmpty` | Validates length only if value is not empty | Accepts null/empty values; applies length validation otherwise |
| `ValidateHours` | Validates if a value represents valid hours | Accepts values between min and max hours; can reject zero |
| `AllNumbers` | Ensures string contains only digits | Accepts empty/null strings for optional fields |
| `ZipCode` | Validates Portuguese postal code format | Format: `NNNN-NNN` (e.g., `1234-567`) |
| `FutureDate` | Ensures date is in the future | Accepted formats: `dd/MM/yyyy`, `yyyy-MM-dd` |
| `PastDate` | Ensures date is in the past | Accepted formats: `dd/MM/yyyy`, `yyyy-MM-dd` |

---

## User

### Create (RegisterDto)

| Field | Validations | Error Message |
|-------|-------------|---------------|
| `UserName` | Required, 3-30 characters | "Username is a required field." / "Username must contain at least 3 characters and a maximum of 30 characters" |
| `Email` | Required, valid email format | "Email is a required field" / "Invalid email format." |
| `Password` | Required, 8-30 characters | "Password is a required field" / "Password must contain at least 8 characters and a maximum of 30 characters" |
| `PersonId` | Required | "A person must be associated with the user." |

### Update (UpdateUserDto)

| Field | Validations | Error Message |
|-------|-------------|---------------|
| `Email` | Required, valid email format | "Email is a required field" / "Invalid email format." |
| `UserName` | Required, 3-30 characters | "Username is a required field." / "Username must contain at least 3 characters and a maximum of 30 characters" |
| `NewPassword` | Required, 8-30 characters | "Password is a required field" / "Password must contain at least 8 characters and a maximum of 30 characters" |

---

## Person

### Create (CreatePersonDto)

| Field | Validations | Error Message |
|-------|-------------|---------------|
| `FirstName` | Required, 3-100 characters | "First Name is a required field." |
| `LastName` | Required, 3-100 characters | "Last Name is a required field." |
| `NIF` | Required, exactly 9 digits, numbers only | "NIF is a required field." / "NIF all characters must be numbers" |
| `IdentificationNumber` | Optional, 5-10 characters | "Identification Number must contain up to 10 characters." |
| `IdentificationValidationDate` | Optional, future date | "Identification Validation Date has expired." |
| `NISS` | Optional, exactly 11 digits, numbers only | "NISS must contain exactly 11 characters." |
| `IBAN` | Optional, exactly 25 characters | "IBAN must contain exactly 25 characters." |
| `BirthDate` | Optional, past date | "Birth date must be in the past." |
| `Address` | Optional | - |
| `ZipCode` | Optional, format `NNNN-NNN` | "Postal Code must contain exactly 8 characters in format '1234-567'." |
| `PhoneNumber` | Optional, exactly 9 digits | "Phone Number must contain exactly 9 characters." |
| `Email` | Optional, valid email format | "Email has invalid format." |
| `Naturality` | Optional, 3-100 characters | "Naturality must contain at least 3 characters and a maximum of 100 characters" |
| `Nationality` | Optional, 3-100 characters | "Nationality must contain at least 3 characters and a maximum of 100 characters" |
| `IdentificationType` | Optional | - |
| `Gender` | Optional | - |
| `Habilitation` | Optional | - |

### Update (UpdatePersonDto)

Validations identical to create.

---

## Company

### Create (CreateCompanyDto)

| Field | Validations | Error Message |
|-------|-------------|---------------|
| `Name` | Required, 3-155 characters | "Designation is a required field." |
| `Address` | Optional | - |
| `PhoneNumber` | Optional, exactly 9 digits | "Phone Number must contain exactly 9 characters." |
| `Locality` | Optional, 3-55 characters | "Locality must contain at least 3 characters and a maximum of 55 characters" |
| `ZipCode` | Optional, format `NNNN-NNN` | "Postal Code must contain exactly 8 characters in format '1234-567'." |
| `Email` | Optional, valid email format | "Email has invalid format." |
| `AtivitySector` | Optional (default: "Unknown") | - |
| `Size` | Optional (default: "Micro") | - |

### Update (UpdateCompanyDto)

Validations identical to create.

---

## Course

### Create (CreateCourseDto)

| Field | Validations | Error Message |
|-------|-------------|---------------|
| `FrameId` | Required | "Framework is a required field." |
| `Title` | Required, 3-255 characters | "Title / Course Name is a required field." |
| `Objectives` | Optional, 3-510 characters | "Course objectives must contain at least 3 characters and a maximum of 510 characters" |
| `Destinators` | Optional (list) | - |
| `Area` | Optional, 3-55 characters | "Course area must contain at least 3 characters and a maximum of 55 characters" |
| `TotalDuration` | 0-1000 hours, greater than zero | "Field must be a valid numeric value between 0 and 1000 hours." |
| `Status` | Optional | - |
| `MinHabilitationLevel` | Optional | - |
| `Modules` | List of module IDs | - |

### Update (UpdateCourseDto)

Validations identical to create.

---

## CourseAction (Training Action)

### Create (CreateCourseActionDto)

| Field | Validations | Error Message |
|-------|-------------|---------------|
| `CourseId` | Required | "Course is a required field." |
| `AdministrationCode` | Required, 5-10 digits, numbers only | "Administration Code is a required field." / "Administration Code all characters must be numbers" |
| `Address` | Optional | - |
| `Locality` | Required | "Locality is a required field." |
| `WeekDays` | Optional (list) | - |
| `StartDate` | Required | "Start Date is a required field." |
| `EndDate` | Required | "End Date is a required field." |
| `Regiment` | Required | "Status is a required field." |
| `Status` | Optional (default: "NotStarted") | - |

> **Note**: `FutureDate` validations for `StartDate` and `EndDate` are commented in source code but can be activated.

### Update (UpdateCourseActionDto)

Validations identical to create (without default values).

---

## Frame

### Create (CreateFrameDto)

| Field | Validations | Error Message |
|-------|-------------|---------------|
| `Program` | Required, 3-150 characters | "Program is a required field." |
| `Intervention` | Required, 3-55 characters | "Intervention is a required field." |
| `InterventionType` | Required, 3-150 characters | "Intervention Type is a required field." |
| `Operation` | Required, 3-150 characters | "Operation is a required field." |
| `OperationType` | Required, 3-150 characters | "Operation Type is a required field." |
| `ProgramLogoFile` | Optional (file) | - |
| `FinancementLogoFile` | Required (file) | "Financing Logo is a required field." |

### Update (UpdateFrameDto)

| Field | Validations | Error Message |
|-------|-------------|---------------|
| `Program` | Required, 3-150 characters | "Program is a required field." |
| `Intervention` | Required, 3-55 characters | "Intervention is a required field." |
| `InterventionType` | Required, 3-150 characters | "Intervention Type is a required field." |
| `Operation` | Required, 3-150 characters | "Operation is a required field." |
| `OperationType` | Required, 3-150 characters | "Operation Type is a required field." |
| `ProgramLogoFile` | Optional (file) | - |
| `FinancementLogoFile` | Optional (file) | - |
| `RemoveProgramLogo` | Optional (boolean) | - |
| `RemoveFinancementLogo` | Optional (boolean, validated in service layer) | - |

---

## Module

### Create (CreateModuleDto)

| Field | Validations | Error Message |
|-------|-------------|---------------|
| `Name` | Required, 3-255 characters | "Module Name is a required field." |
| `Hours` | 0-1000 hours, greater than zero | "Field must be a valid numeric value between 0 and 1000 hours." |
| `Category` | Required | "Category is a required field." |

### Update (UpdateModuleDto)

| Field | Validations | Error Message |
|-------|-------------|---------------|
| `Name` | Required, 3-255 characters | "Module Name is a required field." |
| `Hours` | 0-1000 hours, greater than zero | "Field must be a valid numeric value between 0 and 1000 hours." |
| `IsActive` | Required | "Active is a required field." |

---

## Category (Module Category)

### Create (CreateCategoryDto)

| Field | Validations | Error Message |
|-------|-------------|---------------|
| `Name` | Required, 3-155 characters | "Module Category Name is a required field." |
| `ShortenName` | Required, 1-15 characters | "Module Category Abbreviation is a required field." |

### Update (UpdateCategoryDto)

| Field | Validations | Error Message |
|-------|-------------|---------------|
| `Id` | Required | "Id is a required field." |
| `Name` | Required, 3-155 characters | "Module Category Name is a required field." |
| `ShortenName` | Required, 1-15 characters | "Module Category Abbreviation is a required field." |

---

## ModuleTeaching

### Create (CreateModuleTeachingDto)

| Field | Validations | Error Message |
|-------|-------------|---------------|
| `TeacherId` | Required | "Trainer is a required field." |
| `ActionId` | Required | "Training Action is a required field." |
| `ModuleId` | Required | "Module is a required field." |

### Update (UpdateModuleTeachingDto)

Validations identical to create.

---

## ModuleAvaliation (Module Evaluation)

### Create (CreateModuleAvaliationDto)

| Field | Validations | Error Message |
|-------|-------------|---------------|
| `ModuleTeachingId` | Required | "ModuleTeachingId is required" |
| `ActionEnrollmentId` | Required | "ActionEnrollmentId is required" |
| `Grade` | Required, 0-5 | "Grade is required" / "Grade must be between 0 and 5" |

### Update (UpdateModuleAvaliationDto)

| Field | Validations | Error Message |
|-------|-------------|---------------|
| `Grade` | Required, 0-5 | "Grade is required" / "Grade must be between 0 and 5" |

---

## Session

### Create (CreateSessionDto)

| Field | Validations | Error Message |
|-------|-------------|---------------|
| `ModuleTeachingId` | Required | "ModuleTeaching is a required field." |
| `Weekday` | Required | "Weekday is a required field." |
| `ScheduledDate` | Required | "Scheduled date is a required field." |
| `Start` | Required | "Start time is a required field." |
| `DurationHours` | Required, 1-12 hours | "Duration must be between 1 and 12 hours." |
| `Note` | Optional, 3-255 characters | "Note must contain at least 3 characters and a maximum of 255 characters" |

> **Note**: `FutureDate` validation for `ScheduledDate` is commented in source code but can be activated.

### Update (UpdateSessionDto)

| Field | Validations | Error Message |
|-------|-------------|---------------|
| `Weekday` | Required | "Weekday is a required field." |
| `ScheduledDate` | Required | "Scheduled date is a required field." |
| `Start` | Required | "Start time is a required field." |
| `DurationHours` | Required, 0.1-24 hours | "Duration must be between 0.1 and 24 hours." |
| `TeacherPresence` | Optional (default: "Unknown") | - |
| `Note` | Optional, 3-255 characters | "Note must contain at least 3 characters and a maximum of 255 characters" |

---

## SessionParticipation

### Create (CreateSessionParticipationDto)

| Field | Validations | Error Message |
|-------|-------------|---------------|
| `SessionId` | Required | "Session is a required field." |
| `ActionEnrollmentId` | Required | "Action Enrollment is a required field" |
| `Presence` | Required | "Presence is a required field." |
| `Attendance` | Required, 0-24 | "Attendance is a required field." |

### Update (UpdateSessionParticipationDto)

| Field | Validations | Error Message |
|-------|-------------|---------------|
| `SessionParticipationId` | Required | "Participation Id is a required field" |
| `SessionId` | Required | "Session is a required field." |
| `ActionEnrollmentId` | Required | "Action Enrollment is a required field" |
| `Presence` | Required | "Presence is a required field." |
| `Attendance` | Required, 0-24 | "Attendance is a required field." |

---

## ActionEnrollment

### Create (CreateActionEnrollmentDto)

| Field | Validations | Error Message |
|-------|-------------|---------------|
| `ActionId` | Required | "Action is a required field." |
| `StudentId` | Required | "Trainee is a required field." |

### Update (UpdateActionEnrollmentDto)

Validations identical to create.

---

## Student (Trainee)

### Create (CreateStudentDto)

| Field | Validations | Error Message |
|-------|-------------|---------------|
| `PersonId` | Required | "The Trainee must be associated with a person." |
| `CompanyId` | Optional | - |
| `IsEmployeed` | Optional (boolean) | - |
| `IsRegisteredWithJobCenter` | Optional (boolean) | - |
| `CompanyRole` | Optional, 3-55 characters | "Competences must contain at least 3 characters and a maximum of 55 characters" |

### Update (UpdateStudentDto)

Validations identical to create.

---

## Teacher (Trainer)

### Create (CreateTeacherDto)

| Field | Validations | Error Message |
|-------|-------------|---------------|
| `IvaRegimeId` | Required | "VAT Regime is a required field." |
| `IrsRegimeId` | Required | "IRS Regime is a required field." |
| `PersonId` | Required | "A person must be associated with the user." |
| `Ccp` | Required, 3-55 characters | "CCP is a required field." |
| `Competences` | Optional, 3-55 characters | "Competences must contain at least 3 characters and a maximum of 55 characters" |

### Update (UpdateTeacherDto)

| Field | Validations | Error Message |
|-------|-------------|---------------|
| `IvaRegimeId` | Required | "VAT Regime is a required field." |
| `IrsRegimeId` | Required | "IRS Regime is a required field." |
| `PersonId` | Required | "A person must be associated with the user." |
| `Ccp` | Required, 3-55 characters | "CCP is a required field." |
| `Competences` | Optional, 3-55 characters | "Competences must contain at least 3 characters and a maximum of 55 characters" |
| `IsLecturingFM` | Optional (boolean) | - |
| `IsLecturingCQ` | Optional (boolean) | - |

---

## Tax

### Create (CreateTaxDto)

| Field | Validations | Error Message |
|-------|-------------|---------------|
| `Name` | Required, 3-50 characters | "Regime is a required field" |
| `ValuePercent` | Required, 0-100 | "Percentage value is a required field" / "Percentage value must be between 0 and 100" |
| `Type` | Required | "Type is a required field" |

### Update (UpdateTaxDto)

| Field | Validations | Error Message |
|-------|-------------|---------------|
| `Name` | Required, 3-50 characters | "Regime is a required field" |
| `ValuePercent` | Required, 0-100 | "Percentage value is a required field" / "Percentage value must be between 0 and 100" |
| `IsActive` | Optional (boolean) | - |
| `Type` | Required | "Type is a required field" |

---

## GeneralInfo

### Update (UpdateGeneralInfoDto)

| Field | Validations | Error Message |
|-------|-------------|---------------|
| `Designation` | Required, 3-255 characters | "Designation is a required field." |
| `IvaId` | Required | "VAT Rate is a required field" |
| `Site` | Required, 3-500 characters | "Headquarters is a required field." |
| `HourValueTeacher` | Required, >= 0 | "Trainer Hourly Value is a required field" |
| `HourValueAlimentation` | Required, >= 0 | "Food Hourly Value is a required field" |
| `BankEntity` | Required, 3-50 characters | "Bank Entity is a required field." |
| `Iban` | Required, exactly 25 characters | "IBAN is a required field." / "IBAN must contain exactly 25 characters" |
| `Nipc` | Required, exactly 9 digits | "NIPC is a required field." / "NIPC must contain only numbers." |
| `Logo` | Optional (file) | - |
| `Email` | Required, email format, 3-100 characters | "Email is a required field." / "Email must be a valid email address." |
| `Slug` | Required, 2-50 characters | "Slug is a required field." |
| `PhoneNumber` | Required, 9-20 characters | "Phone number is a required field." |
| `Website` | Required, 3-100 characters | "Website is a required field." |
| `InsurancePolicy` | Required, 3-200 characters | "Insurance Policy is a required field." |
| `FacilitiesCharacterization` | Required, 10-500 characters | "Facilities Characterization is a required field." |

> **Note**: There is no create DTO for GeneralInfo - data is initialized in the system.

---

## Notification

### Create (CreateNotificationDto)

| Field | Validations | Error Message |
|-------|-------------|---------------|
| `Title` | Required, max 200 characters | "Title is a required field." / "Title must contain up to 200 characters." |
| `Message` | Required, max 1000 characters | "Message is a required field." / "Message must contain up to 1000 characters." |
| `Type` | Required (enum NotificationTypeEnum) | "Type is a required field." |
| `RelatedPersonId` | Optional | - |
| `RelatedEntityType` | Optional, max 100 characters | "Entity Type must contain up to 100 characters." |
| `RelatedEntityId` | Optional | - |
| `ActionUrl` | Optional, max 500 characters | "Action URL must contain up to 500 characters." |

### Update (UpdateNotificationDto)

| Field | Validations | Error Message |
|-------|-------------|---------------|
| `Id` | Required | "Id is a required field." |
| `Status` | Optional (enum NotificationStatusEnum) | - |

---

## References

- DTOs location: `NERBABO.Backend/NERBABO.ApiService/Core/*/Dtos/`
- Custom validators location: `NERBABO.Backend/NERBABO.ApiService/Helper/Validators/`

> Ref: [Data Annotations - Microsoft Docs](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.dataannotations)
