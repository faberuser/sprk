namespace NGame2.NUI.NManager.NBooster;

public class PetBuffBoosterInfo : BoosterInfo
{
	public PetBuffBoosterInfo()
	{
		base.Type = BoosterInfoType.PetBuff;
	}

	public override int GetRemainSeconds()
	{
		return 0;
	}

	public override bool DisplayRemainTime()
	{
		return false;
	}

	public override bool IsEffective()
	{
		return true;
	}
}
