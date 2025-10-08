using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Humanizer;
using NERBABO.ApiService.Shared.Dtos;
using NERBABO.ApiService.Shared.Enums;

namespace NERBABO.ApiService.Core.Sessions.Dtos;

public class UpdateSessionPresenceDto : EntityDto<long>
{
    [Required(ErrorMessage = "Presença do formador é um campo obrigatório.")]
    public string TeacherPresence { get; set; } = PresenceEnum.Unknown.Humanize();

    [JsonIgnore]
    public string UserId { get; set; } = string.Empty;
}
