using System;
using QuestPDF.Infrastructure;


namespace NERBABO.ApiService.Core.Reports.Composer;

public interface IComposeReport
{
    void ComposeHeader(IContainer container, string title);

    void ComposeFooter(IContainer container);
}
