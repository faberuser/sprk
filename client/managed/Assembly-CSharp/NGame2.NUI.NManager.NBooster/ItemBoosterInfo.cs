namespace NGame2.NUI.NManager.NBooster;

public class ItemBoosterInfo : BoosterInfo
{
	public int ItemIndex { get; set; }

	public bool ShowAtBattle { get; set; }

	public ItemBoosterInfo()
	{
		ItemIndex = 0;
	}

	public override int GetSortScore()
	{
		if (ItemIndex != 0)
		{
			return (int)base.Type * 1000000 + (int)base.BonusType * 100 + ItemIndex;
		}
		return base.GetSortScore();
	}

	public override bool IsShowAtBattle()
	{
		return ShowAtBattle;
	}
}
