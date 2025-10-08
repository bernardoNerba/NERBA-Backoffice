using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Humanizer;
using NerbaApp.Api.Validators;
using NERBABO.ApiService.Helper.Validators;
using NERBABO.ApiService.Shared.Dtos;
using NERBABO.ApiService.Shared.Enums;

namespace NERBABO.ApiService.Core.Sessions.Dtos;

public class UpdateSessionDto : EntityDto<long>
{
        [Required(ErrorMessage = "Dia da semana é um campo obrigatório.")]
        public string Weekday { get; set; } = string.Empty;

        [Required(ErrorMessage = "Data agendada é um campo obrigatório.")]
        [FutureDate(ErrorMessage = "Data agendada deve ser no futuro.")]
        public string ScheduledDate { get; set; } = string.Empty;

        [Required(ErrorMessage = "Hora de início é um campo obrigatório.")]
        public string Start { get; set; } = string.Empty;

        [Required(ErrorMessage = "Duração em horas é um campo obrigatório.")]
        [Range(0.1, 24.0, ErrorMessage = "Duração deve estar entre 0.1 e 24 horas.")]
        public double DurationHours { get; set; }
        public string TeacherPresence { get; set; } = PresenceEnum.Unknown.Humanize();

        [ValidateLengthIfNotEmpty(255, MinimumLength = 3,
        ErrorMessage = "Observação deve conter pelo menos {2} caracteres e um máximo de {1} caracteres")]
        public string Note { get; set; } = string.Empty;

        [JsonIgnore]
        public string UserId { get; set; } = string.Empty;

}
