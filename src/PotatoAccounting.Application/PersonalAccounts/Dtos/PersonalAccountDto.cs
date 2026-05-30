namespace PotatoAccounting.Application.PersonalAccounts.Dtos;

public class PersonalAccountDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }

    public decimal TotalGiven { get; set; }
    public decimal TotalReceived { get; set; }
    public int EntriesCount { get; set; }

    /// <summary>
    /// Net balance from the app user's perspective.
    /// Positive => the other person owes the user (user gave more than they received).
    /// Negative => the user owes the other person.
    /// </summary>
    public decimal Balance => TotalGiven - TotalReceived;
}
