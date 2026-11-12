namespace NShared;

public class RewardElementData : DateElementField
{
	public HashString ItemCode { get; set; }

	public int ItemRate { get; set; }

	public int ItemMin { get; set; }

	public int ItemMax { get; set; }

	public int ItemStar { get; set; }

	public int ItemStarMin { get; set; }

	public int ItemStarMax { get; set; }

	public int Free { get; set; }

	public int ItemCustomOptionIndex { get; set; }

	public RewardElementData()
	{
	}

	public RewardElementData(int rate, string code, int min, int max, int minStar, int maxStar, int customOptionIndex, int star = 0, int free = 0)
	{
		ItemRate = rate;
		ItemCode = code;
		ItemMin = min;
		ItemMax = max;
		ItemStarMin = minStar;
		ItemStarMax = maxStar;
		ItemStar = star;
		Free = free;
		ItemCustomOptionIndex = customOptionIndex;
	}

	bool DateElementField.IsValid()
	{
		if (!string.IsNullOrEmpty(ItemCode))
		{
			return ItemRate > 0;
		}
		return false;
	}

	public bool IsGold()
	{
		return string.Equals(ItemCode, RewardType.Gold.ToString());
	}

	public bool IsGem()
	{
		return string.Equals(ItemCode, RewardType.Gem.ToString());
	}

	public bool IsFree()
	{
		return Free == 1;
	}

	public CurrencyType GetCurrencyType()
	{
		CurrencyType currencyType = CurrencyType.None;
		EnumAttributeContainer<RewardType, RewardTypeAttribute>.ForEach(delegate(RewardType rewardType, RewardTypeAttribute attr)
		{
			if (string.Equals(rewardType.ToString(), ItemCode))
			{
				currencyType = attr.CurrencyType;
			}
		});
		return currencyType;
	}
}
