namespace back_prueba.Entities
{
    public class Patient
    {
        public int Id { get; set; }
        public string DocumentNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public ICollection<Sample> Samples { get; set; } = new List<Sample>();
    }
}
