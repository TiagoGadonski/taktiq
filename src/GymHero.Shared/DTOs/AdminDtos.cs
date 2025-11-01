namespace GymHero.Shared.DTOs;

/// <summary>
/// DTO para exibir os dados de um utilizador na lista do painel de administração.
/// </summary>
// Mudar de 'record' para 'class' é a solução mais direta.
public class UserDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }

    // Construtor para facilitar a criação a partir da consulta LINQ
    public UserDto(Guid id, string name, string email, string role)
    {
        Id = id;
        Name = name;
        Email = email;
        Role = role;
    }
}

/// <summary>
/// DTO para enviar a requisição de alteração de cargo de um utilizador.
/// </summary>
public record UpdateUserRoleRequest(
    string NewRole
);

/// <summary>
/// DTO para atualizar informações do usuário (incluindo role).
/// </summary>
public record UpdateUserRequest(
    string Role
);

/// <summary>
/// DTO para criar um usuário admin customizado.
/// </summary>
public record CreateAdminRequest(
    string Name,
    string Email,
    string Password
);

/// <summary>
/// DTO para criar um novo usuário (qualquer role) - apenas admins podem usar.
/// </summary>
public record CreateUserRequest(
    string Name,
    string Email,
    string Password,
    string Role
);

/// <summary>
/// DTO para admin alterar senha de um usuário.
/// </summary>
public record AdminChangePasswordRequest(
    string NewPassword
);