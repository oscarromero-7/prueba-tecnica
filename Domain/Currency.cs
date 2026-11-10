namespace PruebaTecnica.Domain;

public class Currency : IAuditable
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal RateToBase { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // "usd" y "USD" son el mismo código: siempre se guarda y se busca en mayúsculas.
    public static string NormalizeCode(string code) => code.Trim().ToUpperInvariant();
}
