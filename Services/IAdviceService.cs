using Check.Models;

namespace Check.Services;

public interface IAdviceService
{
    Task<string> GenerateAdviceAsync(string studentId, IEnumerable<ParsedGrade> grades, CancellationToken ct);
    Task<string> ChatAsync(string studentId, IEnumerable<ParsedGrade> grades, IEnumerable<(string role,string content)> messages, CancellationToken ct);
}
