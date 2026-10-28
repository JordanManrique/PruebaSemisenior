namespace back_prueba.Exceptions
{
    /// <summary>Base de las excepciones de negocio; cada una define su código HTTP.</summary>
    public abstract class AppException(string message, int statusCode, IDictionary<string, string[]>? errors = null)
        : Exception(message)
    {
        public int StatusCode { get; } = statusCode;
        public IDictionary<string, string[]>? Errors { get; } = errors;
    }

    /// <summary>400: regla de validación que depende de datos (p. ej. códigos inexistentes).</summary>
    public class ValidationException(string message, IDictionary<string, string[]> errors)
        : AppException(message, StatusCodes.Status400BadRequest, errors);

    /// <summary>404: recurso no encontrado.</summary>
    public class NotFoundException(string message)
        : AppException(message, StatusCodes.Status404NotFound);

    /// <summary>409: conflicto con una regla de negocio.</summary>
    public class ConflictException(string message)
        : AppException(message, StatusCodes.Status409Conflict);
}
