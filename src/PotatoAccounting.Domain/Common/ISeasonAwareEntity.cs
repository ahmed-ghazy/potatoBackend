namespace PotatoAccounting.Domain.Common;

public interface ISeasonAwareEntity
{
    int SeasonId { get; }
    void SetSeason(int seasonId);
}
