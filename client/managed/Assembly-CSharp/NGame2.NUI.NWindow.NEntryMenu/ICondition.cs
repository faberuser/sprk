using NShared;

namespace NGame2.NUI.NWindow.NEntryMenu;

public interface ICondition
{
	bool Check(CampaignNodeData nodeData, string[] value);
}
