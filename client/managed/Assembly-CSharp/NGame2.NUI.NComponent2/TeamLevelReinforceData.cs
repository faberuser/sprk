using NShared;

namespace NGame2.NUI.NComponent2;

public class TeamLevelReinforceData
{
	public int TeamLevel { get; private set; }

	public int Reinforce { get; private set; }

	public int CurrencyPrice { get; private set; }

	public CurrencyType CurrencyPriceType { get; private set; }

	public TeamLevelReinforceData(int teamLevel, int reinforce, int currencyPrice, CurrencyType currencyType)
	{
		TeamLevel = teamLevel;
		Reinforce = reinforce;
		CurrencyPrice = currencyPrice;
		CurrencyPriceType = currencyType;
	}
}
