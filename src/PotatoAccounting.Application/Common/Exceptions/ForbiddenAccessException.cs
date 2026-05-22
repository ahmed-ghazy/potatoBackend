namespace PotatoAccounting.Application.Common.Exceptions;

public class ForbiddenAccessException : Exception
{
    public ForbiddenAccessException() : base("غير مسموح بالوصول") { }
    public ForbiddenAccessException(string message) : base(message) { }
}
