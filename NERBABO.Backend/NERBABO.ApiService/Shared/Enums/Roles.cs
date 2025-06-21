namespace NERBABO.ApiService.Shared.Enums;

/// <summary>
/// A enumeration representing different user roles within the application.
/// This enum is used to define the roles that users can have, such as Admin, User, CQ, and FM.
/// Each role corresponds to a specific set of permissions and access levels within the system.
/// CQ stands for "Centro Qualifica" (Qualification Center);
/// FM stands for "Formação Modular" (Modular Training).
/// </summary>
public enum Roles
{
    Admin,
    User,
    CQ,
    FM,
}
