using NShared;

namespace NGame2.NUI.NWindow.NEntryMenu.NCondition;

public class EventNotExist : ICondition
{
	bool ICondition.Check(CampaignNodeData nodeData, string[] value)
	{
		return !EventExist.Check(nodeData, value);
	}
}
