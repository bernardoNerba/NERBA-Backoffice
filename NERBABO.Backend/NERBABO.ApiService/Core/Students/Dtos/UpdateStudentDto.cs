using Microsoft.AspNetCore.Mvc;
using NerbaApp.Api.Validators;
using NERBABO.ApiService.Shared.Dtos;
using System.ComponentModel.DataAnnotations;

namespace NERBABO.ApiService.Core.Students.Dtos
{
    public class UpdateStudentDto : EntityDto<long>
    {
        [Required(ErrorMessage = "O Formando deve estar associado a uma pessoa.")]
        public long PersonId { get; set; }
        public long? CompanyId { get; set; }
        public bool IsEmployeed { get; set; }
        public bool IsRegisteredWithJobCenter { get; set; }

        [ValidateLengthIfNotEmpty(55, MinimumLength = 3,
        ErrorMessage = "Competências deve conter pelo menos {2} caracteres e um máximo de {1} caracteres")]
        public string CompanyRole { get; set; } = string.Empty;
    }
}
