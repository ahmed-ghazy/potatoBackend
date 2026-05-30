using PotatoAccounting.Domain.Entities;

namespace PotatoAccounting.Application.PersonalAccounts.Dtos;

public class PersonalAccountEntryDto
{
    public int Id { get; set; }
    public int PersonalAccountId { get; set; }
    public DateTime Date { get; set; }
    public PersonalAccountEntryDirection Direction { get; set; }
    public decimal Amount { get; set; }
    public decimal Fee { get; set; }
    public decimal NetAmount => Amount - Fee;
    public PersonalAccountPaymentMethod? PaymentMethod { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }

    public static PersonalAccountEntryDto From(PersonalAccountEntry e) => new()
    {
        Id = e.Id,
        PersonalAccountId = e.PersonalAccountId,
        Date = e.Date,
        Direction = e.Direction,
        Amount = e.Amount,
        Fee = e.Fee,
        PaymentMethod = e.PaymentMethod,
        Description = e.Description,
        CreatedAt = e.CreatedAt,
    };
}
