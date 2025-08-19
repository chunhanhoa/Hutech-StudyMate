namespace Check.Services;

public interface IAIService
{
    Task<string> GetStudyAdviceAsync(string studentMessage, object studyData, CancellationToken ct = default);
}
