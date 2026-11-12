using System.Collections.Generic;
using NShared;

namespace NGame2.NUI.NWindow.NEntryMenu.NAction;

public class OpenGuild : IAction
{
	void IAction.Run(CampaignNodeData nodeData, NShared.EntryMenu menu, List<object> values)
	{
	}

	void IAction.Run(NShared.EntryMenu menu, List<object> values)
	{
		Vespa_Common_Class_Extension_Method.Interface((IAction)this).Run(null, menu, values);
	}
}
