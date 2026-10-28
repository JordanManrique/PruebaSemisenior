namespace back_prueba.Entities
{
    public class Sample
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public string SampleType { get; set; } = string.Empty;
        public DateTime CollectedAt { get; set; }
        public string Status { get; set; } = SampleStatus.Registered;
        public string? RejectionReason { get; set; }
        public DateTime CreatedAt { get; set; }

        public Patient Patient { get; set; } = null!;
        public ICollection<SampleTest> SampleTests { get; set; } = new List<SampleTest>();
    }
}
