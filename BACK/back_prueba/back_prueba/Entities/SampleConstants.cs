namespace back_prueba.Entities
{
    public static class SampleStatus
    {
        public const string Registered = "Registrada";
        public const string Received = "Recibida";
        public const string Processed = "Procesada";
        public const string Rejected = "Rechazada";

        public static readonly string[] All = [Registered, Received, Processed, Rejected];

        private static readonly Dictionary<string, string[]> Transitions = new()
        {
            [Registered] = [Received, Rejected],
            [Received] = [Processed, Rejected],
            [Processed] = [],
            [Rejected] = []
        };

        public static bool CanTransition(string from, string to) =>
            Transitions.TryGetValue(from, out var allowed) && allowed.Contains(to);
    }
    public static class SampleTypes
    {
        public static readonly string[] All = ["Sangre", "Orina", "Heces", "Hisopado"];
    }
}
