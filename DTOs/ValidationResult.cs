namespace AuthenticationModule.DTOs
{
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string Message { get; set; }
        public int CantidadActual { get; set; }
        public int Minimo { get; set; }
        public int Maximo { get; set; }

        public static ValidationResult Success() => new() { IsValid = true };

        public static ValidationResult Error(string message) => new() { IsValid = false, Message = message };
    }
}