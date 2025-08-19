using System.Text.Json;

namespace Check.Services;

public interface IProgramService
{
    (string? programCode, JsonDocument? doc) GetCurriculumByStudentId(string studentId);
}
