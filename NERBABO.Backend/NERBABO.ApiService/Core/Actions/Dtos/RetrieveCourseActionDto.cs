using Microsoft.Extensions.Diagnostics.HealthChecks;
using NERBABO.ApiService.Shared.Enums;
using System.Text.Json.Serialization;

namespace NERBABO.ApiService.Core.Actions.Dtos
{
    public class RetrieveCourseActionDto
    {
        public long Id { get; set; }

        // Course Information
        public long CourseId { get; set; }
        public string CourseTitle { get; set; } = string.Empty;
        public string CourseArea { get; set; } = string.Empty;
        public string CourseMinHabilitationLevel { get; set; } = string.Empty;
        public float CourseTotalDuration { get; set; }
        public List<string> CourseDestinators { get; set; } = [];
        public List<string> CourseModules { get; set; } = [];

        // Coordenator Information
        public string CoordenatorId { get; set; } = string.Empty;
        public string CoordenatorName { get; set; } = string.Empty;

        // Action Information
        public string Title { get; set; } = string.Empty;
        public string AdministrationCode { get; set; } = string.Empty;
        public string? Address { get; set; } = string.Empty;
        public string Locality { get; set; } = string.Empty;
        public List<string> WeekDays { get; set; } = [];
        public string StartDate { get; set; } = string.Empty;
        public string EndDate { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Regiment { get; set; } = string.Empty;
        public string PaymentsProcessed 
            => AllPaymentsProcessed 
            ? "Processado" 
            : "Não Processado";
        public string ModulesHaveTeacher 
            => AllModulesOfActionHaveTeacher 
            ? "Todos os modulos com Formador" 
            : "Faltam Formadores";



        [JsonIgnore]
        public bool AllPaymentsProcessed { get; set; }
        
        [JsonIgnore]
        public bool AllModulesOfActionHaveTeacher { get; set; }


    }
}
