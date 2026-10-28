namespace back_prueba.Entities
{
    public class SampleTest
    {
        public int SampleId { get; set; }
        public int TestId { get; set; }

        public Sample Sample { get; set; } = null!;
        public LabTest Test { get; set; } = null!;
    }
}
