using back_prueba.Dtos;
using back_prueba.Entities;
using back_prueba.Exceptions;
using back_prueba.Repositories;

namespace back_prueba.Services
{
    public class SampleService(ISampleRepository repository) : ISampleService
    {
        public async Task<SampleResponse> CreateAsync(CreateSampleRequest request, CancellationToken ct)
        {
            var codes = request.TestCodes.Select(c => c.Trim().ToUpperInvariant()).ToList();

            var tests = await repository.GetTestsByCodesAsync(codes, ct);
            var missing = codes.Except(tests.Select(t => t.Code), StringComparer.OrdinalIgnoreCase).ToArray();
            if (missing.Length > 0)
            {
                throw new ValidationException(
                    "Algunos códigos de examen no existen en el catálogo.",
                    new Dictionary<string, string[]>
                    {
                        [nameof(CreateSampleRequest.TestCodes)] = missing.Select(c => $"El examen '{c}' no existe.").ToArray()
                    });
            }
            var patient = await repository.GetPatientByDocumentAsync(request.DocumentNumber, ct)
                ?? new Patient
                {
                    DocumentNumber = request.DocumentNumber,
                    FullName = request.FullName.Trim()
                };

            var sample = new Sample
            {
                Patient = patient,
                SampleType = request.SampleType,
                CollectedAt = request.CollectedAt!.Value,
                Status = SampleStatus.Registered,
                SampleTests = tests.Select(t => new SampleTest { Test = t }).ToList()
            };

            await repository.AddAsync(sample, ct);

            return (await repository.GetDetailAsync(sample.Id, ct))!;
        }

        public async Task<PagedResponse<SampleListItemResponse>> GetPageAsync(PaginationQuery query, CancellationToken ct)
        {
            var (items, total) = await repository.GetPageAsync(query.Page, query.PageSize, ct);
            var totalPages = (int)Math.Ceiling(total / (double)query.PageSize);

            return new PagedResponse<SampleListItemResponse>(items, total, query.Page, query.PageSize, totalPages);
        }

        public async Task<SampleResponse> UpdateStatusAsync(int id, UpdateSampleStatusRequest request, CancellationToken ct)
        {
            var sample = await repository.GetByIdAsync(id, ct)
                ?? throw new NotFoundException($"No existe una muestra con Id {id}.");

            if (!SampleStatus.CanTransition(sample.Status, request.Status))
            {
                throw new ConflictException(
                    $"No se permite cambiar la muestra del estado '{sample.Status}' al estado '{request.Status}'.");
            }

            var reason = request.Status == SampleStatus.Rejected ? request.RejectionReason!.Trim() : null;

            var updated = await repository.UpdateStatusAsync(id, sample.Status, request.Status, reason, ct);
            if (!updated)
            {
                throw new ConflictException(
                    $"La muestra {id} fue modificada por otra solicitud. Consulte su estado actual e intente de nuevo.");
            }

            return (await repository.GetDetailAsync(id, ct))!;
        }
    }
}
