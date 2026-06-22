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

    // Workout Location: 0 = Gym, 1 = Home, 2 = Both
    public int PreferredWorkoutLocation { get; set; } = 0;

    public bool IsPersonalTrainer { get; set; } = false;
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
    string Token,
    string? Role = null,
    string? ProfilePictureUrl = null
);

public record ChangePasswordRequest
{
    [Required(ErrorMessage = "A senha atual é obrigatória")]
    public string CurrentPassword { get; set; } = "";

    [Required(ErrorMessage = "A nova senha é obrigatória")]
    [MinLength(6, ErrorMessage = "A nova senha deve ter pelo menos 6 caracteres")]
    public string NewPassword { get; set; } = "";

    [Required(ErrorMessage = "A confirmação de senha é obrigatória")]
    [Compare(nameof(NewPassword), ErrorMessage = "As senhas não coincidem")]
    public string ConfirmPassword { get; set; } = "";
}

public record ForgotPasswordRequest
{
    [Required(ErrorMessage = "O email é obrigatório")]
    [EmailAddress(ErrorMessage = "Por favor, insira um email válido")]
    public string Email { get; set; } = "";
}

public record ResetPasswordRequest
{
    [Required(ErrorMessage = "O código de recuperação é obrigatório")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "O código deve ter 6 dígitos")]
    public string Token { get; set; } = "";

    [Required(ErrorMessage = "A nova senha é obrigatória")]
    [MinLength(6, ErrorMessage = "A nova senha deve ter pelo menos 6 caracteres")]
    public string NewPassword { get; set; } = "";

    [Required(ErrorMessage = "A confirmação de senha é obrigatória")]
    [Compare(nameof(NewPassword), ErrorMessage = "As senhas não coincidem")]
    public string ConfirmPassword { get; set; } = "";
}

public record ActivateAccountRequest
{
    [Required(ErrorMessage = "O token de ativação é obrigatório")]
    public string Token { get; set; } = "";

    [Required(ErrorMessage = "O nome é obrigatório")]
    public string Name { get; set; } = "";

    [Required(ErrorMessage = "A senha é obrigatória")]
    [MinLength(6, ErrorMessage = "A senha deve ter pelo menos 6 caracteres")]
    public string Password { get; set; } = "";

    [Required(ErrorMessage = "A confirmação de senha é obrigatória")]
    [Compare(nameof(Password), ErrorMessage = "As senhas não coincidem")]
    public string ConfirmPassword { get; set; } = "";

    // Workout Location: 0 = Gym, 1 = Home, 2 = Both
    public int PreferredWorkoutLocation { get; set; } = 0;
}