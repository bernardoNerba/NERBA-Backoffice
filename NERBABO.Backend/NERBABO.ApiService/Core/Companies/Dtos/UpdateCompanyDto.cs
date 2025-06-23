using Humanizer;
using NerbaApp.Api.Validators;
using NERBABO.ApiService.Helper.Validators;
using NERBABO.ApiService.Shared.Dtos;
using NERBABO.ApiService.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace NERBABO.ApiService.Core.Companies.Dtos
{
    public class UpdateCompanyDto: EntityDto<long>
    {
        [Required(ErrorMessage = "Designação é um campo obrigatório.")]
        [ValidateLengthIfNotEmpty(155, MinimumLength = 3,
            ErrorMessage = "Designação deve conter pelo menos {2} caracteres e um máximo de {1} caracteres")]
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }

        [ValidateLengthIfNotEmpty(9, MinimumLength = 9, ErrorMessage = "Numero de Telefóne deve conter extamente 9 caractéres.")]
        [AllNumbers(ErrorMessage = "Número de Telefóne todos os caractéres devem ser números.")]
        public string? PhoneNumber { get; set; }

        [ValidateLengthIfNotEmpty(55, MinimumLength = 3,
        ErrorMessage = "Localidade deve conter pelo menos {2} caracteres e um máximo de {1} caracteres")]
        public string? Locality { get; set; }

        [ZipCode]
        public string? ZipCode { get; set; }

        [RegularExpression(@"^(?:[^@\s]+@[^@\s]+\.[^@\s]+)?$", ErrorMessage = "Email tem formato inválido.")]
        public string? Email { get; set; }
        public string? AtivitySector { get; set; }
            = AtivitySectorEnum.Unknown.Humanize().Transform(To.TitleCase);
        public string? Size { get; set; }
            = CompanySizeEnum.Micro.Humanize().Transform(To.TitleCase);

    }
}
