namespace Check.Models;

public record UploadResult(
    string StudentId,
    string? ProgramCode,
    bool CurriculumFound,
    string? Department,
    string? AcademicYear,
    int? TotalCredits,
    IEnumerable<ParsedGrade> Grades
);
