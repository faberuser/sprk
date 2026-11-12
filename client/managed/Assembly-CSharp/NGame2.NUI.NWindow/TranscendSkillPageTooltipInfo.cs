namespace NGame2.NUI.NWindow;

public class TranscendSkillPageTooltipInfo : TooltipTextInfo
{
	public int HeroIndex;

	public int TranscnedSkillPage;

	public override void Reset()
	{
		base.Reset();
		HeroIndex = 0;
		TranscnedSkillPage = 0;
	}
}
