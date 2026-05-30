namespace PotatoAccounting.Application.PersonalAccounts.Dtos;

public class PersonalAccountsSummaryDto
{
    public int AccountsCount { get; set; }
    public decimal TotalGiven { get; set; }
    public decimal TotalReceived { get; set; }
    public decimal NetBalance => TotalGiven - TotalReceived;
}
