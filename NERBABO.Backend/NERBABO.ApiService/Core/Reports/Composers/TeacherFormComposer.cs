using Humanizer;
using NERBABO.ApiService.Core.Actions.Models;
using NERBABO.ApiService.Core.Global.Models;
using NERBABO.ApiService.Core.ModuleTeachings.Models;
using NERBABO.ApiService.Core.Teachers.Models;
using NERBABO.ApiService.Shared.Services;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace NERBABO.ApiService.Core.Reports.Composers;

public class TeacherFormComposer(IImageService imageService)
{
    private readonly IImageService _imageService = imageService;

    public Document Compose(Teacher teacher, CourseAction action, List<ModuleTeaching> moduleTeachings, GeneralInfo infos)
    {
        // Generate PDF Trainer Form (Ficha de Formador)
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1.5f, Unit.Centimetre);

                page.Header().Element(container => ComposeHeader(container, action, infos));
                page.Content().PaddingTop(5).Element(container => ComposeContent(container, teacher, moduleTeachings));
                page.Footer().Element(container => ComposeFooter(container, infos));
            });
        });
    }

    private void ComposeHeader(IContainer container, CourseAction action, GeneralInfo infos)
    {
        container.Column(column =>
        {
            // Logos row
            column.Item().PaddingVertical(5).Row(row =>
            {
                // Left side - General Info logo
                if (!string.IsNullOrEmpty(infos.Logo))
                {
                    row.ConstantItem(80).Element(logoContainer =>
                    {
                        try
                        {
                            var generalLogoBytes = _imageService.GetImageAsync(infos.Logo).ConfigureAwait(false).GetAwaiter().GetResult();
                            if (generalLogoBytes != null)
                            {
                                logoContainer.Height(40).AlignLeft().AlignMiddle()
                                    .Image(generalLogoBytes).FitArea();
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Failed to load general info logo: {ex.Message}");
                        }
                    });
                }
                else
                {
                    row.ConstantItem(80);
                }

                row.RelativeItem();

                // Right side - Program logo
                if (!string.IsNullOrEmpty(action.Course.Frame.ProgramLogo))
                {
                    row.ConstantItem(80).Element(logoContainer =>
                    {
                        try
                        {
                            var programImageBytes = _imageService.GetImageAsync(action.Course.Frame.ProgramLogo).ConfigureAwait(false).GetAwaiter().GetResult();
                            if (programImageBytes != null)
                            {
                                logoContainer.Height(40).AlignRight().AlignMiddle()
                                    .Image(programImageBytes).FitArea();
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Failed to load program logo: {ex.Message}");
                        }
                    });
                }
                else
                {
                    row.ConstantItem(80);
                }
            });
        });
    }

    private void ComposeContent(IContainer container, Teacher teacher, List<ModuleTeaching> moduleTeachings)
    {
        container.Column(column =>
        {
            // Title
            column.Item().PaddingBottom(10).AlignCenter().Text("FICHA DE IDENTIFICAÇÃO DO/A FORMADOR/A")
                .FontSize(11).FontFamily("Arial").Bold();

            // IDENTIFICAÇÃO Section
            column.Item().PaddingBottom(10).Column(section =>
            {
                section.Item().PaddingBottom(5).Text("IDENTIFICAÇÃO:")
                    .FontSize(9).FontFamily("Arial").Bold();

                // Nome
                section.Item().PaddingBottom(3).Row(row =>
                {
                    row.ConstantItem(40).Text("Nome:").FontSize(8).FontFamily("Arial");
                    row.RelativeItem().BorderBottom(1).Padding(2).Text(teacher.Person.FullName ?? "").FontSize(8).FontFamily("Arial");
                });

                // Morada
                section.Item().PaddingBottom(3).Row(row =>
                {
                    row.ConstantItem(40).Text("Morada:").FontSize(8).FontFamily("Arial");
                    row.RelativeItem().BorderBottom(1).Padding(2).Text(teacher.Person.Address ?? "").FontSize(8).FontFamily("Arial");
                });

                // Código Postal, Localidade, Telemóvel
                section.Item().PaddingBottom(3).Row(row =>
                {
                    row.ConstantItem(70).Text("Cód.Postal:").FontSize(8).FontFamily("Arial");
                    row.ConstantItem(80).BorderBottom(1).Padding(2).Text(teacher.Person.ZipCode ?? "").FontSize(8).FontFamily("Arial");
                    row.ConstantItem(60).PaddingLeft(10).Text("Localidade:").FontSize(8).FontFamily("Arial");
                    row.ConstantItem(120).BorderBottom(1).Padding(2).Text(teacher.Person.Naturality ?? "").FontSize(8).FontFamily("Arial");
                    row.ConstantItem(60).PaddingLeft(10).Text("Telemóvel:").FontSize(8).FontFamily("Arial");
                    row.RelativeItem().BorderBottom(1).Padding(2).Text(teacher.Person.PhoneNumber ?? "").FontSize(8).FontFamily("Arial");
                });

                // Email, Data Nascimento
                section.Item().PaddingBottom(3).Row(row =>
                {
                    row.ConstantItem(40).Text("E-mail:").FontSize(8).FontFamily("Arial");
                    row.ConstantItem(200).BorderBottom(1).Padding(2).Text(teacher.Person.Email ?? "").FontSize(8).FontFamily("Arial");
                    row.ConstantItem(100).PaddingLeft(10).Text("Data Nascimento:").FontSize(8).FontFamily("Arial");
                    row.RelativeItem().BorderBottom(1).Padding(2).Text(teacher.Person.BirthDate?.ToString("dd/MM/yyyy") ?? "").FontSize(8).FontFamily("Arial");
                });

                // Nacionalidade (combining nationality and naturality)
                section.Item().PaddingBottom(3).Row(row =>
                {
                    row.ConstantItem(120).Text("Nacionalidade (freguesia e concelho):").FontSize(8).FontFamily("Arial");
                    var nacionalidadeCompleta = string.IsNullOrEmpty(teacher.Person.Nationality) && string.IsNullOrEmpty(teacher.Person.Naturality)
                        ? ""
                        : $"{teacher.Person.Nationality ?? ""}{(!string.IsNullOrEmpty(teacher.Person.Nationality) && !string.IsNullOrEmpty(teacher.Person.Naturality) ? ", " : "")}{teacher.Person.Naturality ?? ""}";
                    row.RelativeItem().BorderBottom(1).Padding(2).Text(nacionalidadeCompleta).FontSize(8).FontFamily("Arial");
                });

                // Cartão do Cidadão, Validade
                section.Item().PaddingBottom(3).Row(row =>
                {
                    row.ConstantItem(110).Text("N.º Cartão do Cidadão:").FontSize(8).FontFamily("Arial");
                    row.ConstantItem(150).BorderBottom(1).Padding(2).Text(teacher.Person.IdentificationNumber ?? "").FontSize(8).FontFamily("Arial");
                    row.ConstantItem(85).PaddingLeft(10).Text("Validade do cc:").FontSize(8).FontFamily("Arial");
                    row.RelativeItem().BorderBottom(1).Padding(2).Text(teacher.Person.IdentificationValidationDate?.ToString("dd/MM/yyyy") ?? "").FontSize(8).FontFamily("Arial");
                });

                // NISS, NIF
                section.Item().Row(row =>
                {
                    row.ConstantItem(40).Text("NISS:").FontSize(8).FontFamily("Arial");
                    row.ConstantItem(150).BorderBottom(1).Padding(2).Text(teacher.Person.NISS ?? "").FontSize(8).FontFamily("Arial");
                    row.ConstantItem(100).PaddingLeft(10).Text("N.º Contribuinte:").FontSize(8).FontFamily("Arial");
                    row.RelativeItem().BorderBottom(1).Padding(2).Text(teacher.Person.NIF ?? "").FontSize(8).FontFamily("Arial");
                });
            });

            // FORMAÇÃO ACADÉMICA E PROFISSIONAL Section
            column.Item().PaddingBottom(10).Column(section =>
            {
                section.Item().PaddingBottom(5).Text("FORMAÇÃO ACADÉMICA E PROFISSIONAL:")
                    .FontSize(9).FontFamily("Arial").Bold();

                section.Item().PaddingBottom(3).Row(row =>
                {
                    row.ConstantItem(120).Text("Habilitações Literárias:").FontSize(8).FontFamily("Arial");
                    row.RelativeItem().BorderBottom(1).Padding(2).Text(teacher.Person.Habilitation.Humanize() ?? "").FontSize(8).FontFamily("Arial");
                });

                section.Item().Row(row =>
                {
                    row.ConstantItem(90).Text("Outros Cursos:").FontSize(8).FontFamily("Arial");
                    row.RelativeItem().BorderBottom(1).Padding(2).Text("").FontSize(8).FontFamily("Arial");
                });
            });

            // FORMAÇÃO PEDAGÓGICA Section
            column.Item().PaddingBottom(10).Column(section =>
            {
                section.Item().PaddingBottom(5).Text("FORMAÇÃO PEDAGÓGICA:")
                    .FontSize(9).FontFamily("Arial").Bold();

                var hasCcp = !string.IsNullOrEmpty(teacher.Ccp);

                // Option 1: CCP with number
                section.Item().PaddingBottom(3).Row(row =>
                {
                    row.ConstantItem(15).Text(hasCcp ? "☑" : "☐").FontSize(8).FontFamily("Arial");
                    row.ConstantItem(60).Text("CCP  n.º:").FontSize(8).FontFamily("Arial");
                    row.RelativeItem().BorderBottom(1).Padding(2).Text(hasCcp ? teacher.Ccp : "").FontSize(8).FontFamily("Arial");
                });

                // Option 2: Isentos de CAP
                section.Item().PaddingBottom(3).Row(row =>
                {
                    row.ConstantItem(15).Text("☐").FontSize(8).FontFamily("Arial");
                    row.RelativeItem().Text("Isentos de CAP (docentes profissionalizados)")
                        .FontSize(8).FontFamily("Arial");
                });

                // Option 3: Document copy
                section.Item().Row(row =>
                {
                    row.ConstantItem(15).Text("☐").FontSize(8).FontFamily("Arial");
                    row.RelativeItem().Text("Cópia autenticada do documento comprovativo da posse de profissionalização ou declaração emitida pelo estabelecimento de ensino superior onde conste que se encontram a leccionar e a respectiva categoria profissional")
                        .FontSize(7).FontFamily("Arial");
                });
            });

            // REGIME FISCAL Section
            column.Item().PaddingBottom(10).Column(section =>
            {
                section.Item().PaddingBottom(5).Text("REGIME FISCAL:")
                    .FontSize(9).FontFamily("Arial").Bold();

                // IVA Regime - automatically set
                section.Item().PaddingBottom(3).Row(row =>
                {
                    row.ConstantItem(80).Text("Regime de IVA:").FontSize(8).FontFamily("Arial");
                    row.RelativeItem().Text($"{teacher.IvaRegime.Name}")
                        .FontSize(8).FontFamily("Arial");
                });

                // IRS Regime - automatically set
                section.Item().Row(row =>
                {
                    row.ConstantItem(80).Text("Regime de IRS:").FontSize(8).FontFamily("Arial");
                    row.RelativeItem().Text($"{teacher.IrsRegime.Name}")
                        .FontSize(8).FontFamily("Arial");
                });
            });

            // DOCUMENTAÇÃO OBRIGATÓRIA Section (as checklist)
            column.Item().PaddingBottom(10).Column(section =>
            {
                section.Item().PaddingBottom(5).Text("DOCUMENTAÇÃO OBRIGATÓRIA:")
                    .FontSize(9).FontFamily("Arial").Bold();

                section.Item().PaddingBottom(2).Row(row =>
                {
                    row.ConstantItem(15).Text("☐").FontSize(8).FontFamily("Arial");
                    row.RelativeItem().Text("CV").FontSize(8).FontFamily("Arial");
                });

                section.Item().PaddingBottom(2).Row(row =>
                {
                    row.ConstantItem(15).Text("☐").FontSize(8).FontFamily("Arial");
                    row.RelativeItem().Text("CCP").FontSize(8).FontFamily("Arial");
                });

                section.Item().PaddingBottom(2).Row(row =>
                {
                    row.ConstantItem(15).Text("☐").FontSize(8).FontFamily("Arial");
                    row.RelativeItem().Text("Certificado de habilitações").FontSize(8).FontFamily("Arial");
                });

                section.Item().PaddingBottom(2).Row(row =>
                {
                    row.ConstantItem(15).Text("☐").FontSize(8).FontFamily("Arial");
                    row.RelativeItem().Text("IBAN").FontSize(8).FontFamily("Arial");
                });

                section.Item().PaddingBottom(2).Row(row =>
                {
                    row.ConstantItem(15).Text("☐").FontSize(8).FontFamily("Arial");
                    row.RelativeItem().Text("Documento comprovativo da situação contributiva regularizada para a Segurança Social")
                        .FontSize(8).FontFamily("Arial");
                });

                section.Item().PaddingBottom(2).Row(row =>
                {
                    row.ConstantItem(15).Text("☐").FontSize(8).FontFamily("Arial");
                    row.RelativeItem().Text("Documento comprovativo da situação tributária regularizada")
                        .FontSize(8).FontFamily("Arial");
                });

                section.Item().Row(row =>
                {
                    row.ConstantItem(15).Text("☐").FontSize(8).FontFamily("Arial");
                    row.RelativeItem().Text("Documento comprovativo de seguro de acidentes de trabalho de trabalhadores independentes em vigor, no caso de pessoas singulares")
                        .FontSize(8).FontFamily("Arial");
                });
            });

            // NOTA DE SELECÇÃO Section
            column.Item().PaddingBottom(10).Column(section =>
            {
                section.Item().PaddingBottom(5).Text("NOTA DE SELECÇÃO – a preencher pela entidade formadora")
                    .FontSize(9).FontFamily("Arial").Bold();

                section.Item().PaddingBottom(2).Text("Este formador foi seleccionado/a de acordo com os critérios de selecção definidos pela entidade formadora:")
                    .FontSize(7).FontFamily("Arial");

                section.Item().PaddingBottom(2).Row(row =>
                {
                    row.ConstantItem(15).Text("☐").FontSize(8).FontFamily("Arial");
                    row.RelativeItem().Text("Este/a formador/a apresenta um Curriculum Vitae perfeitamente adequado para a formação em causa")
                        .FontSize(7).FontFamily("Arial");
                });

                section.Item().PaddingBottom(2).Row(row =>
                {
                    row.ConstantItem(15).Text("☐").FontSize(8).FontFamily("Arial");
                    row.RelativeItem().Text("Este/a formador/a evidencia experiência devidamente comprovada, para a formação em causa")
                        .FontSize(7).FontFamily("Arial");
                });

                section.Item().PaddingBottom(2).Row(row =>
                {
                    row.ConstantItem(15).Text("☐").FontSize(8).FontFamily("Arial");
                    row.RelativeItem().Text("Este/a formador/a revela, em serviços de formação anteriores, um bom resultado de avaliação desempenho")
                        .FontSize(7).FontFamily("Arial");
                });

                section.Item().Row(row =>
                {
                    row.ConstantItem(15).Text("☐").FontSize(8).FontFamily("Arial");
                    row.RelativeItem().Text("Este/a formador/a cumpre o perfil adequado")
                        .FontSize(7).FontFamily("Arial");
                });
            });

            // Signature Section
            column.Item().PaddingTop(15).Column(section =>
            {
                section.Item().PaddingBottom(2).Text("Responsabilizo-me pelas declarações prestadas e comprometo-me a comunicar eventuais alterações,")
                    .FontSize(8).FontFamily("Arial");

                section.Item().PaddingTop(25).AlignCenter().Column(signatureColumn =>
                {
                    signatureColumn.Item().Text("_____________________________________________________________")
                        .FontSize(8).FontFamily("Arial");
                    signatureColumn.Item().PaddingTop(2).Text("(O/a Formador/a)")
                        .FontSize(8).FontFamily("Arial");
                });
            });
        });
    }

    private void ComposeFooter(IContainer container, GeneralInfo infos)
    {
        container.Column(column =>
        {
            column.Item().PaddingTop(10).AlignCenter().Text("O NERBA é Entidade Certificada pela DGERT, C61")
                .FontSize(7).FontFamily("Arial").Italic();
        });
    }
}
