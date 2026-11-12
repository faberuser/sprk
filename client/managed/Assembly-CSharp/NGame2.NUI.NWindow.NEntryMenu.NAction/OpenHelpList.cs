using System.Collections.Generic;
using NShared;

namespace NGame2.NUI.NWindow.NEntryMenu.NAction;

internal class OpenHelpList : IAction
{
	void IAction.Run(CampaignNodeData nodeData, NShared.EntryMenu menu, List<object> values)
	{
		EventGuideListPopup.Open(ArgParser.GetValueString(menu.EntryMenuButton, 2));
	}

	void IAction.Run(NShared.EntryMenu menu, List<object> values)
	{
		Vespa_Common_Class_Extension_Method.Interface((IAction)this).Run(null, menu, values);
	}
}
