using Humanizer;
using NERBABO.ApiService.Core.Actions.Models;
using NERBABO.ApiService.Core.Global.Models;
using NERBABO.ApiService.Core.ModuleTeachings.Models;
using NERBABO.ApiService.Core.Teachers.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace NERBABO.ApiService.Core.Reports.Composers;

public class TeacherFormComposer(HelperComposer helperComposer)
{
    private readonly HelperComposer _helperComposer = helperComposer;

    public async Task<Document> ComposeAsync(Teacher teacher, CourseAction action, List<ModuleTeaching> moduleTeachings, GeneralInfo infos)
    {
        // Pre-load images asynchronously
        var (generalLogoBytes, programLogoBytes, _) = await _helperComposer
            .LoadLogosAsync(infos.Logo, action.Course.Frame.ProgramLogo);

        // Generate PDF Trainer Form (Ficha de Formador)
        return Document.Create(container =>
        {
            // First page - Teacher identification form
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1.5f, Unit.Centimetre);

                page.Header().Element(container => HelperComposer.ComposeHeader(container, generalLogoBytes, programLogoBytes));
                page.Content().PaddingTop(5).Element(container => ComposeContent(container, teacher, moduleTeachings));
                page.Footer().Element(container => HelperComposer.ComposeFooter(container, null, $"{infos.Slug} é Entidade Certificada pela DGERT, C61"));
            });

            // Second page - Data treatment and consent
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1.5f, Unit.Centimetre);

                page.Header().Element(container => HelperComposer.ComposeHeader(container, generalLogoBytes, programLogoBytes));
                page.Content().PaddingTop(5).Element(container => ComposeDataTreatmentContent(container, action, infos));
                page.Footer().Element(container => HelperComposer.ComposeFooter(container, null, $"{infos.Slug} é Entidade Certificada pela DGERT, C61"));
            });
        });
    }


    private static void ComposeContent(IContainer container, Teacher teacher, List<ModuleTeaching> moduleTeachings)
    {
        container.Column(column =>
        {
            // Title
            HelperComposer.Title(column, "Ficha de Identificação do/a Formando/a");

            // IDENTIFICAÇÃO Section
            column.Item().PaddingBottom(10).Column(section =>
            {
                HelperComposer.SectionTitle(section, "Identificação:");

                HelperComposer.AddFormField(section, "Nome", teacher.Person.FullName ?? "", 40);
                HelperComposer.AddFormField(section, "Morada", teacher.Person.Address ?? "", 40);

                // Código Postal, Localidade, Telemóvel
                HelperComposer.AddMultFormField(section,
                [
                    new() { Label = "Cód.Postal", Value = teacher.Person.ZipCode ?? "", Space = 70 },
                    new() { Label = "Localidade", Value = teacher.Person.Naturality ?? "", Space = 60 },
                    new() { Label = "Telemóvel", Value = teacher.Person.PhoneNumber ?? "", Space = 60 }
                ]);

                // Email, Data Nascimento
                HelperComposer.AddMultFormField(section,
                [
                    new() { Label = "E-mail", Value = teacher.Person.Email ?? "", Space = 40 },
                    new() { Label = "Data Nascimento", Value = teacher.Person.BirthDate?.ToString("dd/MM/yyyy") ?? "", Space = 100 }
                ]);

                // Nacionalidade (combining nationality and naturality)
                var nacionalidadeCompleta = string.IsNullOrEmpty(teacher.Person.Nationality) && string.IsNullOrEmpty(teacher.Person.Naturality)
                    ? ""
                    : $"{teacher.Person.Nationality ?? ""}{(!string.IsNullOrEmpty(teacher.Person.Nationality) && !string.IsNullOrEmpty(teacher.Person.Naturality) ? ", " : "")}{teacher.Person.Naturality ?? ""}";
                HelperComposer.AddFormField(section, "Nacionalidade (freguesia e concelho)", nacionalidadeCompleta, 120);

                // Cartão do Cidadão, Validade
                HelperComposer.AddMultFormField(section,
                [
                    new() { Label = "N.º Cartão do Cidadão", Value = teacher.Person.IdentificationNumber ?? "", Space = 110 },
                    new() { Label = "Validade do cc", Value = teacher.Person.IdentificationValidationDate?.ToString("dd/MM/yyyy") ?? "", Space = 85 }
                ]);

                // NISS, NIF
                HelperComposer.AddMultFormField(section,
                [
                    new() { Label = "NISS", Value = teacher.Person.NISS ?? "", Space = 40 },
                    new() { Label = "N.º Contribuinte", Value = teacher.Person.NIF ?? "", Space = 100 }
                ]);
            });

            // FORMAÇÃO ACADÉMICA E PROFISSIONAL Section
            column.Item().PaddingBottom(10).Column(section =>
            {
                HelperComposer.SectionTitle(section, "Formação Académica e Profissional:");

                HelperComposer.AddFormField(section, "Habilitações Literárias", teacher.Person.Habilitation.Humanize() ?? "", 120);
                HelperComposer.AddFormField(section, "Outros Cursos", "", 90);
            });

            // FORMAÇÃO PEDAGÓGICA Section
            column.Item().PaddingBottom(10).Column(section =>
            {
                HelperComposer.SectionTitle(section, "Formação Pedagógica:");

                var hasCcp = !string.IsNullOrEmpty(teacher.Ccp);

                HelperComposer.AddCheckboxFieldRow(section, hasCcp, "CCP  n.º:", hasCcp ? teacher.Ccp : "", 60);
                HelperComposer.AddCheckboxRow(section, false, "Isentos de CAP (docentes profissionalizados)");
                HelperComposer.AddCheckboxRow(section, false, "Cópia autenticada do documento comprovativo da posse de profissionalização ou declaração emitida pelo estabelecimento de ensino superior onde conste que se encontram a leccionar e a respectiva categoria profissional", 7);
            });

            // REGIME FISCAL Section
            column.Item().PaddingBottom(10).Column(section =>
            {
                HelperComposer.SectionTitle(section, "Regime Fiscal:");

                HelperComposer.AddInfoRow(section, "Regime de IVA:", teacher.IvaRegime.Name, 80);
                HelperComposer.AddInfoRow(section, "Regime de IRS:", teacher.IrsRegime.Name, 80);
            });

            // DOCUMENTAÇÃO OBRIGATÓRIA Section (as checklist)
            column.Item().PaddingBottom(10).Column(section =>
            {
                HelperComposer.SectionTitle(section, "Documentação Obrigatória:");

                HelperComposer.AddCheckboxRow(section, false, "CV");
                HelperComposer.AddCheckboxRow(section, false, "CCP");
                HelperComposer.AddCheckboxRow(section, false, "Certificado de habilitações");
                HelperComposer.AddCheckboxRow(section, false, "IBAN");
                HelperComposer.AddCheckboxRow(section, false, "Documento comprovativo da situação contributiva regularizada para a Segurança Social");
                HelperComposer.AddCheckboxRow(section, false, "Documento comprovativo da situação tributária regularizada");
                HelperComposer.AddCheckboxRow(section, false, "Documento comprovativo de seguro de acidentes de trabalho de trabalhadores independentes em vigor, no caso de pessoas singulares");
            });

            // NOTA DE SELECÇÃO Section
            column.Item().PaddingBottom(10).Column(section =>
            {
                HelperComposer.SectionTitle(section, "Nota de Selecção – a preencher pela entidade formadora");

                section.Item().PaddingBottom(2).Text("Este formador foi seleccionado/a de acordo com os critérios de selecção definidos pela entidade formadora:")
                    .FontSize(7).FontFamily("Arial");

                HelperComposer.AddCheckboxRow(section, false, "Este/a formador/a apresenta um Curriculum Vitae perfeitamente adequado para a formação em causa", 7);
                HelperComposer.AddCheckboxRow(section, false, "Este/a formador/a evidencia experiência devidamente comprovada, para a formação em causa", 7);
                HelperComposer.AddCheckboxRow(section, false, "Este/a formador/a revela, em serviços de formação anteriores, um bom resultado de avaliação desempenho", 7);
                HelperComposer.AddCheckboxRow(section, false, "Este/a formador/a cumpre o perfil adequado", 7);
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


    private static void ComposeDataTreatmentContent(IContainer container, CourseAction action, GeneralInfo infos)
    {
        container.Column(column =>
        {
            // Title
            HelperComposer.Title(column, "Tratamento de Dados e Consentimento");

            // Data treatment text
            column.Item().PaddingBottom(15).Text(text =>
            {
                text.DefaultTextStyle(x => x.FontSize(9).FontFamily("Arial").LineHeight(1.3f));

                text.Span($"Tratamento dos dados: O {infos.Slug} garante a estrita confidencialidade no tratamento dos dados. Com a apresentação desta ficha, está em condições de monitorizar a formação que se propõe ministrar autorizando que os dados constantes deste documento sejam facultados à DGERT, entidade acreditadora de entidades formadoras, sejam registados no sistema de informação do {action.Course.Frame.Program}, registados no sistema de informação (digital e suporte papel) e para o envio por e-mail, SMS, de comunicações promocionais e de marketing direto relativo aos serviços do {infos.Slug}, aceitando-se, também, ser contactado para confirmação dos elementos prestados bem como de outros que venham a revelar interesse geral, no âmbito dos processo de monitorização e avaliação dos serviços desta {infos.Designation}. ");

                text.Span("Sem estes dados não é possível a participação na formação.").Bold();

                text.Span($" A qualquer momento, pode retirar o consentimento, atualizar dados - não afetando o tratamento já realizado - contactando, para o efeito, o {infos.PhoneNumber} ou através do e-mail {infos.Email} ou por comunicação direta junto de qualquer colaborador/a do {infos.Slug}. O período de conservação dos dados atenderá à necessidade e interesse do {infos.Slug} e dos cofinanciamentos, comprometendo-se esta associação adotar as medidas de conservação e segurança adequadas. Sem prejuízo da possibilidade de reclamar junto do {infos.Slug}, os titulares dos dados têm o direito de apresentar reclamação perante a Comissão Nacional de Proteção de Dados (CNPD). Consultar a política de privacidade no sítio web do {infos.Slug} - {infos.Website}");
            });

            // Image and sound consent section
            column.Item().PaddingBottom(10).PaddingTop(10).Text(text =>
            {
                text.DefaultTextStyle(x => x.FontSize(9).FontFamily("Arial").LineHeight(1.3f));
                text.Span($"Solicita-se consentimento por parte de participante na formação profissional organizada pelo {infos.Slug} para captação e gravação de imagem e som. As imagens e sons captados serão de uso exclusivo para fins de verificação da elegibilidade dos projetos e ainda para alguma publicidade de promoção dos projetos.");
            });

            // Consent checkboxes
            column.Item().PaddingBottom(5).Row(row =>
            {
                row.ConstantItem(15).Text("☐").FontSize(10).FontFamily("Arial");
                row.RelativeItem().Text("Sim, autorizo.")
                    .FontSize(9).FontFamily("Arial");
            });

            column.Item().Row(row =>
            {
                row.ConstantItem(15).Text("☐").FontSize(10).FontFamily("Arial");
                row.RelativeItem().Text("Não autorizo.")
                    .FontSize(9).FontFamily("Arial");
            });

            // Signature section
            column.Item().PaddingTop(30).AlignCenter().Column(signatureColumn =>
            {
                signatureColumn.Item().Text("_____________________________________________________________")
                    .FontSize(8).FontFamily("Arial");
                signatureColumn.Item().PaddingTop(2).Text("(O/a Formador/a)")
                    .FontSize(8).FontFamily("Arial");
            });
        });
    }
}
