namespace NGame2.NUI.NManager.NBooster;

public class CostumeBoosterInfo : BoosterInfo
{
	public CostumeBoosterInfo()
	{
		base.Type = BoosterInfoType.CostumeAbility;
	}

	public override bool IsEffective()
	{
		return true;
	}

	public override int GetRemainSeconds()
	{
		return 0;
	}
}
