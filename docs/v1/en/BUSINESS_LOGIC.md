# Business Rules and System Logic

This document describes all business rules, entity relationships, allowed states, and deletion restrictions for each entity in the NERBA Backoffice system.

## Table of Contents

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

## User

### Relationships

| Relationship | Type | Required | Description |
|--------------|------|----------|-------------|
| Person | N:1 | Required | Each user must be associated with a person |
| Actions (Coordinator) | 1:N | Optional | List of actions where the user is coordinator |

### Business Rules

| Rule | Description | Layer |
|------|-------------|-------|
| Person required | A user must always be associated with a person | Service |
| Active state | User can be deactivated (IsActive = false) without being deleted | Model |
| Last login | System automatically records the date of last login | Model |

### Calculated Properties

| Property | Logic | Description |
|----------|-------|-------------|
| LastLogin | Humanizer | Humanized text of last login in pt-PT |

### Deletion Restrictions

| Condition | Behavior |
|-----------|----------|
| Active user | User can be deactivated instead of deleted |

---

## Person

### Relationships

| Relationship | Type | Required | Description |
|--------------|------|----------|-------------|
| User | 1:1 | Optional | Person can be a system user |
| Teacher | 1:1 | Optional | Person can be a trainer |
| Student | 1:1 | Optional | Person can be a trainee |
| HabilitationComprovativePdf | N:1 | Optional | Qualification proof PDF |
| IbanComprovativePdf | N:1 | Optional | IBAN proof PDF |
| IdentificationDocumentPdf | N:1 | Optional | Identification document PDF |

### Business Rules

| Rule | Description | Layer |
|------|-------------|-------|
| Unique NIF | NIF must be unique in the system | Service |
| Unique NISS | NISS, if provided, must be unique | Service |
| Unique Identification Number | Identification number, if provided, must be unique | Service |
| Unique Email | Email, if provided, must be unique | Service |
| Qualification validation | Person can verify if they meet minimum qualification level | Model |

### Calculated Properties

| Property | Logic | Description |
|----------|-------|-------------|
| IsTeacher | Teacher != null | Indicates if person is a trainer |
| IsStudent | Student != null | Indicates if person is a trainee |
| IsColaborator | User != null | Indicates if person is a collaborator/user |
| FullName | FirstName + LastName | Person's full name |

### Deletion Restrictions

| Condition | Behavior |
|-----------|----------|
| Is User | Cannot delete person who is a user |
| Is Teacher with ModuleTeachings | Cannot delete person who has taught modules |
| Is Student with ActionEnrollments | Cannot delete person enrolled in actions |

---

## Company

### Relationships

| Relationship | Type | Required | Description |
|--------------|------|----------|-------------|
| Students | 1:N | Optional | List of trainees associated with the company |

### Calculated Properties

| Property | Logic | Description |
|----------|-------|-------------|
| StudentsCount | Students.Count | Number of associated trainees |

### Deletion Restrictions

No specific restrictions implemented.

---

## Course

### Relationships

| Relationship | Type | Required | Description |
|--------------|------|----------|-------------|
| Frame | N:1 | Required | Course framework |
| Modules | N:N | Optional | List of course modules |
| Actions | 1:N | Optional | List of training actions for the course |

### States and Transitions

| State | Description | Allowed Transitions |
|-------|-------------|---------------------|
| NotStarted | Course not started | InProgress, Cancelled |
| InProgress | Course in progress | Completed, Cancelled |
| Completed | Course completed | - |
| Cancelled | Course cancelled | - |

### Business Rules

| Rule | Description | Layer |
|------|-------------|-------|
| Unique title | Course title must be unique | Service |
| Module duration | Sum of module hours cannot exceed TotalDuration | Service |
| Active module | Only active modules can be assigned to the course | Service |
| Active course for modules | Can only assign modules to active courses (NotStarted or InProgress) | Service |
| Module already assigned | A module cannot be assigned twice to the same course | Service |
| Complete course | Course can only be completed if all actions are completed | Service |
| Cancel course | Course can only be cancelled if all actions are cancelled | Service |
| Remove modules | Cannot remove modules from active courses | Service |

### Calculated Properties

| Property | Logic | Description |
|----------|-------|-------------|
| CurrentDuration | Sum(Modules.Hours) | Current assigned duration in hours |
| RemainingDuration | TotalDuration - CurrentDuration | Remaining available hours |
| IsCourseActive | Status == NotStarted \|\| InProgress | Indicates if course is active |
| ActionsQnt | Actions.Count | Number of actions |
| ModulesQnt | Modules.Count | Number of modules |
| FormattedModuleNames | Formatted list | Module names with hours |

### Deletion Restrictions

| Condition | Behavior |
|-----------|----------|
| Has associated actions | Cannot delete course with actions |
| Course completed | Cannot delete completed courses |

---

## CourseAction (Training Action)

### Relationships

| Relationship | Type | Required | Description |
|--------------|------|----------|-------------|
| Course | N:1 | Required | Course associated with the action |
| Coordinator (User) | N:1 | Required | User coordinating the action |
| ModuleTeachings | 1:N | Optional | Module teachings in this action |
| ActionEnrollments | 1:N | Optional | Trainee enrollments |

### States and Transitions

| State | Description | Allowed Transitions |
|-------|-------------|---------------------|
| NotStarted | Action not started | InProgress, Cancelled |
| InProgress | Action in progress | Completed, Cancelled |
| Completed | Action completed | - |
| Cancelled | Action cancelled | - |

### Business Rules

| Rule | Description | Layer |
|------|-------------|-------|
| Unique administration code | Administration code must be unique | Service |
| Start date < End date | Start date must be before end date | Service |
| Automatic number | ActionNumber is automatically generated (Actions.Count + 1) | Model |
| Change status | Only the coordinator can change action status | Service |

### Calculated Properties

| Property | Logic | Description |
|----------|-------|-------------|
| AllPaymentsProcessed | All(ModuleTeachings.PaymentProcessed) | All payments processed |
| AllModulesOfActionHaveTeacher | All modules have trainer | Trainer validation |
| IsActionActive | Status == NotStarted \|\| InProgress | Action active |
| AllSessionsScheduled | All(ModuleTeachings.ScheduledPercent == 100) | All sessions scheduled |
| Title | ActionNumber + Locality | Formatted title |
| TotalStudents | ActionEnrollments.Count | Total trainees |
| TotalApproved | Count(ApprovalStatus == Approved) | Total approved |
| TotalVolumeHours | Sum(Participations.Attendance) | Total volume hours |
| TotalVolumeDays | Count(Presence == Present) | Training days |

### Deletion Restrictions

Actions can be deleted, cascading deletion of associated reports.

---

## Frame

### Relationships

| Relationship | Type | Required | Description |
|--------------|------|----------|-------------|
| Courses | 1:N | Optional | Courses associated with the framework |

### Business Rules

| Rule | Description | Layer |
|------|-------------|-------|
| Unique program | Program must be unique | Service |
| Unique operation | Operation must be unique | Service |
| Financing logo required | Financing logo is required | Service |
| Financing logo not removable | Cannot remove financing logo | Service |
| Image validation | Logos must be valid images (JPG, PNG, GIF, BMP) < 5MB | Service |

### Deletion Restrictions

| Condition | Behavior |
|-----------|----------|
| Has associated courses | Cannot delete framework with courses |

---

## Module

### Relationships

| Relationship | Type | Required | Description |
|--------------|------|----------|-------------|
| Category | N:1 | Required | Module category |
| Courses | N:N | Optional | Courses that include this module |
| ModuleTeachings | 1:N | Optional | Teachings of this module |

### Business Rules

| Rule | Description | Layer |
|------|-------------|-------|
| Unique combination | Name + Hours + Category must be unique | Service |
| Active module | Inactive modules cannot be assigned to courses | Service |
| Change hours | Cannot change hours if module is associated with courses | Service |
| Toggle activation | Cannot change activation if associated with active courses | Service |

### Calculated Properties

| Property | Logic | Description |
|----------|-------|-------------|
| CoursesQnt | Courses.Count | Number of courses using the module |

### Deletion Restrictions

| Condition | Behavior |
|-----------|----------|
| Associated active courses | Cannot delete module with active courses |

---

## Category (Module Category)

### Relationships

| Relationship | Type | Required | Description |
|--------------|------|----------|-------------|
| Modules | 1:N | Optional | Modules in this category |

### Business Rules

| Rule | Description | Layer |
|------|-------------|-------|
| Equality by name | Two categories are equal if Name and ShortenName match | Model |

---

## ModuleTeaching

### Relationships

| Relationship | Type | Required | Description |
|--------------|------|----------|-------------|
| Teacher | N:1 | Required | Teaching trainer |
| Action | N:1 | Required | Training action |
| Module | N:1 | Required | Module being taught |
| Sessions | 1:N | Optional | Teaching sessions |
| Avaliations | 1:N | Optional | Trainee evaluations |

### Business Rules

| Rule | Description | Layer |
|------|-------------|-------|
| Module belongs to action | Module must belong to the action's course | Service |
| Trainer meets qualification | Trainer must have qualification above course minimum | Service |
| One trainer per module/action | Each module in an action can only have one trainer | Service |

### Calculated Properties

| Property | Logic | Description |
|----------|-------|-------------|
| AvaliationAvg | (AvaliationCoordinator + AvaliationStudents) / 2 | Evaluation average |
| PaymentProcessed | PaymentDate.HasValue | Payment processed |
| ScheduledSessionsTime | Sum(Sessions.DurationHours) | Scheduled hours |
| IsModuleHoursScheduled | ScheduledSessionsTime == Module.Hours | Complete hours |
| IsPayed | PaymentDate != null | Payment made |
| ScheduledPercent | (ScheduledSessionsTime * 100) / Module.Hours | Scheduled percentage |
| CalculatedTotal | Sum(Sessions where Present) * hourRate | Calculated total |

---

## ModuleAvaliation (Module Evaluation)

### Relationships

| Relationship | Type | Required | Description |
|--------------|------|----------|-------------|
| ModuleTeaching | N:1 | Required | Evaluated teaching |
| ActionEnrollment | N:1 | Required | Trainee enrollment |

### Business Rules

| Rule | Description | Layer |
|------|-------------|-------|
| Grade 0-5 | Grade must be between 0 and 5 | DTO Validation |
| Automatic creation | Created automatically when enrolling trainee | Service |

### Calculated Properties

| Property | Logic | Description |
|----------|-------|-------------|
| Evaluated | Grade != 0 | Indicates if evaluated |

---

## Session

### Relationships

| Relationship | Type | Required | Description |
|--------------|------|----------|-------------|
| ModuleTeaching | N:1 | Required | Associated teaching |
| Participants | 1:N | Optional | Trainee participations |

### Business Rules

| Rule | Description | Layer |
|------|-------------|-------|
| Active action | Can only create session if action is active | Service |
| Course duration complete | Course must have total duration filled | Service |
| Valid weekday | Day must be in action's allowed days | Service |
| Date in period | Date must be between action start and end | Service |
| Module hours | Sum of sessions cannot exceed module hours | Service |

### Calculated Properties

| Property | Logic | Description |
|----------|-------|-------------|
| End | Start.AddHours(DurationHours) | End time |
| Time | Start - End formatted | Time interval |

### Deletion Restrictions

| Condition | Behavior |
|-----------|----------|
| Session taught | Cannot delete session with TeacherPresence = Present |

---

## SessionParticipation

### Relationships

| Relationship | Type | Required | Description |
|--------------|------|----------|-------------|
| Session | N:1 | Required | Participation session |
| ActionEnrollment | N:1 | Required | Trainee enrollment |

### Presence States

| State | Description |
|-------|-------------|
| Unknown | Not specified |
| Present | Present |
| Absent | Absent |

### Business Rules

| Rule | Description | Layer |
|------|-------------|-------|
| Automatic creation | Created automatically when enrolling trainee | Service |
| Attendance 0-24 | Attendance must be between 0 and 24 hours | DTO Validation |

### Calculated Properties

| Property | Logic | Description |
|----------|-------|-------------|
| AttendanceHours | Floor(Attendance) | Whole hours |
| AttendanceMinutes | (Attendance - Floor) * 60 | Remaining minutes |

---

## ActionEnrollment

### Relationships

| Relationship | Type | Required | Description |
|--------------|------|----------|-------------|
| Action | N:1 | Required | Enrollment action |
| Student | N:1 | Required | Enrolled trainee |
| Participations | 1:N | Automatic | Session participations |
| Avaliations | 1:N | Automatic | Module evaluations |

### Approval States

| State | Description | Condition |
|-------|-------------|-----------|
| NotSpecified | Not specified | Not evaluated |
| Approved | Approved | Average >= 3 |
| Rejected | Rejected | Average < 3 |
| Dropped | Dropped out | - |

### Business Rules

| Rule | Description | Layer |
|------|-------------|-------|
| All modules with trainer | All action modules must have a trainer | Service |
| Unique enrollment | Trainee cannot be enrolled twice in the same action | Service |
| Sessions scheduled | All sessions must be scheduled | Service |
| Minimum qualification | Trainee must meet course minimum qualification | Service |
| Relationship creation | When enrolling, creates SessionParticipation and ModuleAvaliation | Service |

### Calculated Properties

| Property | Logic | Description |
|----------|-------|-------------|
| AvgEvaluation | Sum(Avaliations.Grade) / Count | Evaluation average |
| StudentAvaliated | All(Avaliations.Evaluated) | All modules evaluated |
| IsPayed | PaymentDate != null | Payment made |
| ApprovalStatus | Calculated based on evaluations | Approval status |
| CalculatedTotal | Sum(Participations where Present) * hourRate | Calculated total |

### Deletion Restrictions

Cascade deletion removes associated SessionParticipations and ModuleAvaliations.

---

## Student (Trainee)

### Relationships

| Relationship | Type | Required | Description |
|--------------|------|----------|-------------|
| Person | N:1 | Required | Associated person |
| Company | N:1 | Optional | Trainee's company |
| ActionEnrollments | 1:N | Optional | Action enrollments |

### Business Rules

| Rule | Description | Layer |
|------|-------------|-------|
| Person exists | Associated person must exist | Service |
| Unique person | A person can only be one trainee | Service |

### Calculated Properties

| Property | Logic | Description |
|----------|-------|-------------|
| CanDelete | ActionEnrollments.Count == 0 | Can be deleted |

### Deletion Restrictions

| Condition | Behavior |
|-----------|----------|
| Has enrollments | Cannot delete trainee with enrollments |

---

## Teacher (Trainer)

### Relationships

| Relationship | Type | Required | Description |
|--------------|------|----------|-------------|
| Person | N:1 | Required | Associated person |
| IvaRegime (Tax) | N:1 | Required | VAT regime |
| IrsRegime (Tax) | N:1 | Required | IRS regime |
| ModuleTeachings | 1:N | Optional | Taught modules |

### Business Rules

| Rule | Description | Layer |
|------|-------------|-------|
| Person exists | Associated person must exist | Service |
| Unique person | A person can only be one trainer | Service |
| Unique CCP | Pedagogical Competence Certificate must be unique | Service |
| Valid VAT regime | VAT regime must be of type VAT | Service |
| Valid IRS regime | IRS regime must be of type IRS | Service |

### Calculated Properties

| Property | Logic | Description |
|----------|-------|-------------|
| CanDelete | ModuleTeachings.Count == 0 | Can be deleted |
| CommaSeparatedCompetences | Join(Competences, ", ") | Formatted competences |

### Deletion Restrictions

| Condition | Behavior |
|-----------|----------|
| Has taught modules | Cannot delete trainer with ModuleTeachings |

---

## Tax

### Relationships

| Relationship | Type | Required | Description |
|--------------|------|----------|-------------|
| GeneralInfo | 1:1 | Optional | General information (VAT) |
| IvaTeachers | 1:N | Optional | Trainers with this VAT regime |
| IrsTeachers | 1:N | Optional | Trainers with this IRS regime |

### Tax Types

| Type | Description |
|------|-------------|
| IVA | Value Added Tax |
| IRS | Personal Income Tax |

### Business Rules

| Rule | Description | Layer |
|------|-------------|-------|
| Percentage 0-100 | Percentage value between 0 and 100 | DTO Validation |
| Valid type | Type must be VAT or IRS | Model |

### Calculated Properties

| Property | Logic | Description |
|----------|-------|-------------|
| ValueDecimal | ValuePercent / 100 | Decimal value |

---

## GeneralInfo

### Relationships

| Relationship | Type | Required | Description |
|--------------|------|----------|-------------|
| IvaTax | N:1 | Required | Organization's VAT rate |

### Business Rules

| Rule | Description | Layer |
|------|-------------|-------|
| Single record | Only one GeneralInfo record in the system | Initialization |
| No manual creation | Data is initialized in the system, only update allowed | Controller |

### Calculated Properties

| Property | Logic | Description |
|----------|-------|-------------|
| HourlySubsidy | HourValueAlimentation formatted | Hourly subsidy in EUR |

---

## Notification

### Relationships

| Relationship | Type | Required | Description |
|--------------|------|----------|-------------|
| RelatedPerson | N:1 | Optional | Related person |
| ReadByUser | N:1 | Optional | User who read |

### States

| State | Description |
|-------|-------------|
| Unread | Unread |
| Read | Read |
| Archived | Archived |

### Notification Types

Notifications are automatically generated by a background service that checks:
- Missing documents for persons (qualifications, IBAN, identification)

### Business Rules

| Rule | Description | Layer |
|------|-------------|-------|
| Automatic generation | Notifications are generated by background service | Background Service |
| Regeneration on doc delete | When deleting document, notifications are regenerated | Service |

---

## Main Workflow

The typical workflow for creating a training action follows these steps:

```
1. Create Framework (Frame)
   |
2. Create Course with Framework
   |
3. Assign Modules to Course
   |-- Validate: Sum hours <= TotalDuration
   |
4. Create Training Action (CourseAction)
   |-- Validate: Course exists, Coordinator exists
   |
5. Assign Trainers to Modules (ModuleTeaching)
   |-- Validate: Trainer meets qualification
   |-- Validate: Module belongs to course
   |
6. Schedule Sessions (Session)
   |-- Validate: Action active, Valid days, Module hours
   |
7. Enroll Trainees (ActionEnrollment)
   |-- Validate: All modules with trainer
   |-- Validate: All sessions scheduled
   |-- Validate: Trainee meets qualification
   |-- Automatically creates: SessionParticipation, ModuleAvaliation
   |
8. Record Attendance and Evaluations
   |
9. Process Payments
```

---

## References

- Models location: `NERBABO.Backend/NERBABO.ApiService/Core/*/Models/`
- Services location: `NERBABO.Backend/NERBABO.ApiService/Core/*/Services/`
- Enums location: `NERBABO.Backend/NERBABO.ApiService/Shared/Enums/`

> See: [Entity Validations](./VALIDATIONS.md)
