using NGame2.NContents;
using NShared;
using NVespa;

namespace NGame2.NUI.NWindow.NEntryMenu.NCondition;

public class OpenContents : ICondition
{
	bool ICondition.Check(CampaignNodeData nodeData, string[] value)
	{
		if (__Extension_Method_ICollection__.COUNT(value) < 1)
		{
			return false;
		}
		if (SingletonAuto<ContentsOpenChecker>.instance == null)
		{
			return false;
		}
		ContentsType valueEnum = ArgParser.GetValueEnum(value, 1, ContentsType.None);
		return SingletonAuto<ContentsOpenChecker>.instance.CheckContentsOpen(valueEnum);
	}
}
