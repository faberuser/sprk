using NShared.NJit;

namespace NShared;

public class HeroLimitBreakDataContainer : JitDataContainerDefault<HeroLimitBreakDataKey, HeroLimitBreakData>
{
	public const string TABLENAME = "HeroLimitBreakTable";

	public override TablePathDefine PathDefine => new TablePathDefine("HeroLimitBreakTable");

	public int GetAddTranscendSkillPoint(int heroIndex, int limitBreakLevel)
	{
		return GetData(new HeroLimitBreakDataKey(heroIndex, limitBreakLevel))?.GetTranscendSkillPoint ?? 0;
	}

	public int GetAddTotalTranscendSkillPoint(int heroIndex, int limitBreakLevel)
	{
		int num = 0;
		for (int i = 1; i <= limitBreakLevel; i++)
		{
			num += GetAddTranscendSkillPoint(heroIndex, i);
		}
		return num;
	}
}
