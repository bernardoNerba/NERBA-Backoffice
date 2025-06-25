using System.ComponentModel;

namespace NERBABO.ApiService.Shared.Enums;

public enum WeekDaysEnum
{
    [Description("Domingo")]
    Sunday,

    [Description("Segunda-feira")]
    Monday,

    [Description("Terça-feira")]
    Tuesday,

    [Description("Quarta-feira")]
    Wednesday,

    [Description("Quinta-feira")]
    Thursday,

    [Description("Sexta-feira")]
    Friday,

    [Description("Sábado")]
    Saturday
}
