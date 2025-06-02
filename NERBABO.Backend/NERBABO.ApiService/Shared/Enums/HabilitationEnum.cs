using System.ComponentModel;

namespace NERBABO.ApiService.Shared.Enums;

public enum HabilitationEnum
{
    [Description("Sem Comprovativo")]
    WithoutProof,

    [Description("Sem escolaridade")]
    WithoutHabilitation,

    [Description("1º Ano")]
    FirstYear,

    [Description("2º Ano")]
    SecondYear,

    [Description("3º Ano")]
    ThirdYear,

    [Description("4º Ano")]
    FourthYear,

    [Description("5º Ano")]
    FifthYear,

    [Description("6º Ano")]
    SixthYear,

    [Description("7º Ano")]
    SeventhYear,

    [Description("8º Ano")]
    EighthYear,

    [Description("9º Ano")]
    NinthYear,

    [Description("10º Ano")]
    TenthYear,

    [Description("11º Ano")]
    EleventhYear,

    [Description("12º Ano")]
    TwelfthYear,

    [Description("Pós-Secundário")]
    PostSecondary,

    [Description("Bacharelato")]
    Bachelors,

    [Description("Licenciatura")]
    Undergraduate,

    [Description("Mestrado")]
    Masters,

    [Description("Doutoramento")]
    Doctorate
}
