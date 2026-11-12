using NShared.NJit;

namespace NShared;

public class PlayGuideDataContainer : JitDataContainerDefault<int, PlayGuideData>
{
	public const string TABLENAME = "PlayGuideTable";

	public override TablePathDefine PathDefine => new TablePathDefine("PlayGuideTable");

	public int GetTutorialIndexByCondition(string conditionCheck)
	{
		foreach (PlayGuideData allDatum in GetAllData())
		{
			if (allDatum != null)
			{
				if (allDatum.Condition1.Length != 0 && allDatum.Condition1 != null && allDatum.Condition1[0] == conditionCheck)
				{
					return allDatum.TutorialIndex;
				}
				if (allDatum.Condition2 != null && allDatum.Condition2.Length != 0 && allDatum.Condition2[0] == conditionCheck)
				{
					return allDatum.TutorialIndex;
				}
				if (allDatum.Condition3 != null && allDatum.Condition3.Length != 0 && allDatum.Condition3[0] == conditionCheck)
				{
					return allDatum.TutorialIndex;
				}
			}
		}
		return 0;
	}
}
