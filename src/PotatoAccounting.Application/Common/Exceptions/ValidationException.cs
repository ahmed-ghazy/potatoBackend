using FluentValidation.Results;

namespace PotatoAccounting.Application.Common.Exceptions;

public class ValidationException : Exception
{
    public ValidationException()
        : base("حدث خطأ أو أكثر فى التحقق من البيانات.")
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(IEnumerable<ValidationFailure> failures) : this()
    {
        Errors = failures
            .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
            .ToDictionary(g => g.Key, g => g.ToArray());
    }

    public IDictionary<string, string[]> Errors { get; }
}
