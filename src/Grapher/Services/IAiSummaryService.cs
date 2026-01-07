using Grapher.Models;

namespace Grapher.Services
{
    public interface IAiSummaryService
    {
        Task<string> GenerateProjectSummaryAsync(Project project, IEnumerable<TaskItem> tasks);
    }
}
