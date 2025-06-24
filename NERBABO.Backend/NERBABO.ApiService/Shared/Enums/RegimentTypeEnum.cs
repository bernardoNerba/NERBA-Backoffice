using System.ComponentModel;

namespace NERBABO.ApiService.Shared.Enums;

/// <summary>
/// This enumeration defines the different types of regiment that a formation action can have.
/// It includes options such as "online", "presential" and "hybrid".
/// Use Humanizer to get the description of each enum value.
/// </summary>
public enum RegimentTypeEnum
{
    [Description("Online")]
    online,
    
    [Description("Presencial")]
    presential,
    
    [Description("Híbrido")]
    hybrid,
}
