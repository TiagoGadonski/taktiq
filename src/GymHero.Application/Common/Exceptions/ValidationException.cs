namespace GymHero.Application.Common.Exceptions;

public class ValidationException : Exception
{
    public ValidationException(string message) : base(message)
    {
    }

    // Opcional: Um construtor que pode receber uma coleção de erros,
    // útil se você integrar com a validação do FluentValidation mais a fundo.
    public ValidationException(IEnumerable<string> errors)
        : base(string.Join(", ", errors))
    {
    }
}