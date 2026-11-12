using NGame2.NUI.NWindow;

namespace NGame2.NUI.NManager.NBooster;

public class GuildSkillBoosterInfo : BoosterInfo
{
	public GuildSkillBoosterInfo()
	{
		base.Type = BoosterInfoType.BoosterGuildSkill;
	}

	public override bool IsEffective()
	{
		return true;
	}

	public override int GetRemainSeconds()
	{
		return 0;
	}

	public override bool DisplayRemainTime()
	{
		return false;
	}

	public override void SetupBoosterTooltip(TooltipTextInfo tooltipTextInfo)
	{
		tooltipTextInfo.Name.Value = base.Name;
		tooltipTextInfo.Desc.Value = base.Desc;
		tooltipTextInfo.TypeText.Reset();
	}
}
