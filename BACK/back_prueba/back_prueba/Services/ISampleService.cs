using back_prueba.Dtos;

namespace back_prueba.Services
{
    public interface ISampleService
    {
        Task<SampleResponse> CreateAsync(CreateSampleRequest request, CancellationToken ct);
        Task<PagedResponse<SampleListItemResponse>> GetPageAsync(PaginationQuery query, CancellationToken ct);
        Task<SampleResponse> UpdateStatusAsync(int id, UpdateSampleStatusRequest request, CancellationToken ct);
    }
}
