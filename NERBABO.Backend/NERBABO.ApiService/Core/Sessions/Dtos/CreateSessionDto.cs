using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using NERBABO.ApiService.Core.Account.Models;
using NERBABO.ApiService.Helper.Validators;

namespace NERBABO.ApiService.Core.Sessions.Dtos
{
    public class CreateSessionDto
    {
        
        [Required(ErrorMessage = "ModuleTeaching é um campo obrigatório.")]
        public long ModuleTeachingId { get; set; }

        [Required(ErrorMessage = "Dia da semana é um campo obrigatório.")]
        public string Weekday { get; set; } = string.Empty;

        [Required(ErrorMessage = "Data agendada é um campo obrigatório.")]
        [FutureDate(ErrorMessage = "Data agendada deve ser no futuro.")]
        public string ScheduledDate { get; set; } = string.Empty;

        [Required(ErrorMessage = "Hora de início é um campo obrigatório.")]
        public string Start { get; set; } = string.Empty;

        [Required(ErrorMessage = "Duração em horas é um campo obrigatório.")]
        [Range(1.0, 12.0, ErrorMessage = "Duração deve estar entre 1 e 12 horas.")]
        public double DurationHours { get; set; }

        [JsonIgnore]
        public User User { get; set; } = new User();
    }
}