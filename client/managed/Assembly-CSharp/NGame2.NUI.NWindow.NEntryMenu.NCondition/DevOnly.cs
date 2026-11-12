using NGameAsset;
using NShared;

namespace NGame2.NUI.NWindow.NEntryMenu.NCondition;

public class DevOnly : ICondition
{
	bool ICondition.Check(CampaignNodeData nodeData, string[] value)
	{
		return !Define.IsLiveBuild();
	}
}
