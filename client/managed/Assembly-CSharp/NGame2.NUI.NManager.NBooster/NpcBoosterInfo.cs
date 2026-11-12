namespace NGame2.NUI.NManager.NBooster;

public class NpcBoosterInfo : BoosterInfo
{
	public NpcBoosterInfo()
	{
		base.Type = BoosterInfoType.NpcBuff;
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
