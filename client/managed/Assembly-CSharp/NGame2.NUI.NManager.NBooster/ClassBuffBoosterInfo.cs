namespace NGame2.NUI.NManager.NBooster;

public class ClassBuffBoosterInfo : BoosterInfo
{
	public ClassBuffBoosterInfo()
	{
		base.Type = BoosterInfoType.ClassBuff;
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
