using System.ComponentModel.DataAnnotations;

namespace GymHero.Shared.DTOs;

// Usamos propriedades em vez de parâmetros no construtor.
// Isto permite uma inicialização mais simples (new()) e funciona melhor com Blazor.
public record RegisterRequest
{
    [Required(ErrorMessage = "O nome é obrigatório")]
    public string Name { get; set; } = "";

    [Required(ErrorMessage = "O email é obrigatório")]
    [EmailAddress(ErrorMessage = "Por favor, insira um email válido")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "A senha é obrigatória")]
    [MinLength(6, ErrorMessage = "A senha deve ter pelo menos 6 caracteres")]
    public string Password { get; set; } = "";
}

public record LoginRequest
{
    [Required(ErrorMessage = "O email é obrigatório")]
    [EmailAddress]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "A senha é obrigatória")]
    public string Password { get; set; } = "";
}

public record AuthResponse(
    Guid Id, 
    string Name, 
    string Email, 
    string Token
);