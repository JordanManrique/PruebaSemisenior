namespace back_prueba.Entities
{
    public class LabTest
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

        public ICollection<SampleTest> SampleTests { get; set; } = new List<SampleTest>();
    }
}
