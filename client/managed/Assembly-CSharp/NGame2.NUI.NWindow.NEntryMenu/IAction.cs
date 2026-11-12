using System.Collections.Generic;
using NShared;

namespace NGame2.NUI.NWindow.NEntryMenu;

public interface IAction
{
	void Run(CampaignNodeData nodeData, NShared.EntryMenu menu, List<object> values = null);

	void Run(NShared.EntryMenu menu, List<object> values = null);
}
