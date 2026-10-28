using back_prueba.Dtos;
using back_prueba.Entities;

namespace back_prueba.Repositories
{
    public interface ISampleRepository
    {
        Task<Patient?> GetPatientByDocumentAsync(string documentNumber, CancellationToken ct);
        Task<List<LabTest>> GetTestsByCodesAsync(IEnumerable<string> codes, CancellationToken ct);
        Task<Sample?> GetByIdAsync(int id, CancellationToken ct);
        Task<SampleResponse?> GetDetailAsync(int id, CancellationToken ct);
        Task<(List<SampleListItemResponse> Items, int TotalCount)> GetPageAsync(int page, int pageSize, CancellationToken ct);
        Task AddAsync(Sample sample, CancellationToken ct);
        Task<bool> UpdateStatusAsync(int id, string expectedStatus, string newStatus, string? rejectionReason, CancellationToken ct);
    }
}
