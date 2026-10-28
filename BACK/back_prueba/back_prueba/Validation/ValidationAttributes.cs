using System.ComponentModel.DataAnnotations;

namespace back_prueba.Validation
{
    public class NotFutureDateAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value) =>
            value is not DateTime date || date <= DateTime.Now;
    }

    public class UniqueItemsAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value) =>
            value is not IEnumerable<string> items
            || items.Select(i => i?.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).Count() == items.Count();
    }
}
