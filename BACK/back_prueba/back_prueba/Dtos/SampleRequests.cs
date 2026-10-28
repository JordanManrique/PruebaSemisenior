using System.ComponentModel.DataAnnotations;
using back_prueba.Validation;

namespace back_prueba.Dtos
{
    public class CreateSampleRequest
    {
        [Required(ErrorMessage = "El número de documento es obligatorio.")]
        [RegularExpression(@"^\d{6,12}$", ErrorMessage = "El número de documento debe tener solo dígitos y entre 6 y 12 caracteres.")]
        public string DocumentNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre completo es obligatorio.")]
        [MinLength(3, ErrorMessage = "El nombre completo debe tener mínimo 3 caracteres.")]
        [MaxLength(150, ErrorMessage = "El nombre completo debe tener máximo 150 caracteres.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "El tipo de muestra es obligatorio.")]
        [AllowedValues("Sangre", "Orina", "Heces", "Hisopado", ErrorMessage = "El tipo de muestra debe ser Sangre, Orina, Heces o Hisopado.")]
        public string SampleType { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha y hora de toma es obligatoria.")]
        [NotFutureDate(ErrorMessage = "La fecha y hora de toma no puede ser futura.")]
        public DateTime? CollectedAt { get; set; }


        [Required(ErrorMessage = "Debe indicar al menos un código de examen.")]
        [MinLength(1, ErrorMessage = "Debe indicar al menos un código de examen.")]
        [UniqueItems(ErrorMessage = "La lista de exámenes tiene códigos repetidos.")]
        public List<string> TestCodes { get; set; } = [];
    }

    public class UpdateSampleStatusRequest : IValidatableObject
    {

        [Required(ErrorMessage = "El estado es obligatorio.")]
        [AllowedValues("Registrada", "Recibida", "Procesada", "Rechazada", ErrorMessage = "El estado debe ser Registrada, Recibida, Procesada o Rechazada.")]
        public string Status { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "El motivo de rechazo debe tener máximo 500 caracteres.")]
        public string? RejectionReason { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Status == "Rechazada" && string.IsNullOrWhiteSpace(RejectionReason))
            {
                yield return new ValidationResult(
                    "El motivo de rechazo es obligatorio cuando el estado es Rechazada.",
                    [nameof(RejectionReason)]);
            }
        }
    }

    public class PaginationQuery
    {
        public const int DefaultPage = 1;
        public const int DefaultPageSize = 10;

        [Range(1, int.MaxValue, ErrorMessage = "page debe ser mayor o igual a 1.")]
        public int Page { get; set; } = DefaultPage;

        [Range(1, 50, ErrorMessage = "pageSize debe estar entre 1 y 50.")]
        public int PageSize { get; set; } = DefaultPageSize;
    }
}
