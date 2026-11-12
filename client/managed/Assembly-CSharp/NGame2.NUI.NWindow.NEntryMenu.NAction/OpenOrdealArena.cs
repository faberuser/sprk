using System.Collections.Generic;
using NGame2.NContentsInfo;
using NGame2.NOrdeal;
using NShared;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow.NEntryMenu.NAction;

public class OpenOrdealArena : IAction
{
	void IAction.Run(CampaignNodeData nodeData, NShared.EntryMenu menu, List<object> values)
	{
		SingletonAuto<MatchArena>.instance.RequestOrdealSeasonInfo(delegate
		{
			GlobalAutoBehaviour<OrdealArenaManager>.instance.RequestOrdealInfo();
		});
	}

	void IAction.Run(NShared.EntryMenu menu, List<object> values)
	{
		Vespa_Common_Class_Extension_Method.Interface((IAction)this).Run(null, menu, values);
	}
}
