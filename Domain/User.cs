namespace PruebaTecnica.Domain;

public class User : IAuditable
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    // Default en C# y no en la BD: en un bool EF no distingue "false" de "sin asignar".
    public bool IsActive { get; set; } = true;

    public string? PasswordHash { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public List<Address> Addresses { get; set; } = [];

    public static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
