using back_prueba.Data;
using back_prueba.Dtos;
using back_prueba.Entities;
using Microsoft.EntityFrameworkCore;

namespace back_prueba.Repositories
{
    public class SampleRepository(LabDbContext db) : ISampleRepository
    {
        public Task<Patient?> GetPatientByDocumentAsync(string documentNumber, CancellationToken ct) =>
            db.Patients.FirstOrDefaultAsync(p => p.DocumentNumber == documentNumber, ct);

        public Task<List<LabTest>> GetTestsByCodesAsync(IEnumerable<string> codes, CancellationToken ct) =>
            db.Tests.Where(t => codes.Contains(t.Code)).ToListAsync(ct);

        public Task<Sample?> GetByIdAsync(int id, CancellationToken ct) =>
            db.Samples.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id, ct);

        public Task<SampleResponse?> GetDetailAsync(int id, CancellationToken ct) =>
            db.Samples
                .AsNoTracking()
                .Where(s => s.Id == id)
                .Select(s => new SampleResponse(
                    s.Id,
                    s.Patient.DocumentNumber,
                    s.Patient.FullName,
                    s.SampleType,
                    s.CollectedAt,
                    s.Status,
                    s.RejectionReason,
                    s.CreatedAt,
                    s.SampleTests
                        .OrderBy(st => st.Test.Code)
                        .Select(st => new TestResponse(st.Test.Code, st.Test.Name))
                        .ToList()))
                .FirstOrDefaultAsync(ct);

        public async Task<(List<SampleListItemResponse> Items, int TotalCount)> GetPageAsync(int page, int pageSize, CancellationToken ct)
        {
            var total = await db.Samples.CountAsync(ct);

            var items = await db.Samples
                .AsNoTracking()
                .OrderByDescending(s => s.CreatedAt)
                .ThenByDescending(s => s.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new SampleListItemResponse(
                    s.Id,
                    s.Patient.DocumentNumber,
                    s.Patient.FullName,
                    s.SampleType,
                    s.CollectedAt,
                    s.Status,
                    s.SampleTests.Count))
                .ToListAsync(ct);

            return (items, total);
        }

        public async Task AddAsync(Sample sample, CancellationToken ct)
        {
            await using var transaction = await db.Database.BeginTransactionAsync(ct);
            db.Samples.Add(sample);
            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }

        public async Task<bool> UpdateStatusAsync(int id, string expectedStatus, string newStatus, string? rejectionReason, CancellationToken ct)
        {
            var affected = await db.Samples
                .Where(s => s.Id == id && s.Status == expectedStatus)
                .ExecuteUpdateAsync(u => u
                    .SetProperty(s => s.Status, newStatus)
                    .SetProperty(s => s.RejectionReason, rejectionReason), ct);

            return affected == 1;
        }
    }
}
