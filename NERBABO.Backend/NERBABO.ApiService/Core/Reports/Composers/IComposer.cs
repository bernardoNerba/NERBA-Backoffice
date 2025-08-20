using NERBABO.ApiService.Core.Actions.Models;
using NERBABO.ApiService.Core.Sessions.Models;
using QuestPDF.Fluent;

namespace NERBABO.ApiService.Core.Reports.Composers;

public interface IComposer
{
    Document ComposeSessionsTimeline(List<Session> sessions, CourseAction action);
}