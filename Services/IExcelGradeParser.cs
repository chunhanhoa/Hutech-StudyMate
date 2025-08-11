using Check.Models;

namespace Check.Services;

public interface IExcelGradeParser
{
    Task<IReadOnlyList<ParsedGrade>> ParseAsync(Stream excelStream, CancellationToken ct = default);
}
