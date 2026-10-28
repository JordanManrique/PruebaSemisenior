namespace back_prueba.Dtos
{
    public record TestResponse(string Code, string Name);

    public record SampleResponse(
        int Id,
        string DocumentNumber,
        string PatientName,
        string SampleType,
        DateTime CollectedAt,
        string Status,
        string? RejectionReason,
        DateTime CreatedAt,
        IReadOnlyList<TestResponse> Tests);

    public record SampleListItemResponse(
        int Id,
        string DocumentNumber,
        string PatientName,
        string SampleType,
        DateTime CollectedAt,
        string Status,
        int TestCount);
    public record PagedResponse<T>(
        IReadOnlyList<T> Items,
        int TotalCount,
        int Page,
        int PageSize,
        int TotalPages);
    public record ErrorResponse(
        int Status,
        string Message,
        IDictionary<string, string[]>? Errors = null);
}
