namespace Check.Models;

public record ParsedGrade(
    string CourseCode,
    string? CourseName,
    int? Credits,
    double? Score10,
    string? LetterGrade,
    double? Gpa,
    bool IsFailed = false  // Môn rớt nếu điểm hệ 10 < 4
);
