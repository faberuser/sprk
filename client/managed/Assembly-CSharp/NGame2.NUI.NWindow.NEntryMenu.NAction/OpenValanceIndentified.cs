using System.Collections.Generic;
using NShared;

namespace NGame2.NUI.NWindow.NEntryMenu.NAction;

public class OpenValanceIndentified : IAction
{
	void IAction.Run(CampaignNodeData nodeData, NShared.EntryMenu menu, List<object> values)
	{
		RestoredPortal.OpenValanceIdentified(nodeData);
	}

	void IAction.Run(NShared.EntryMenu menu, List<object> values)
	{
		Vespa_Common_Class_Extension_Method.Interface((IAction)this).Run(null, menu, values);
	}
}
