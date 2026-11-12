using System.Collections.Generic;
using NGame2.NUI.NManager;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow.NEntryMenu.NAction;

public class EnterPurifyRoomTranscend : IAction
{
	void IAction.Run(CampaignNodeData nodeData, NShared.EntryMenu menu, List<object> values)
	{
		CachedHeroInfo selectedHeroInfo = null;
		if (values != null && values.Count > 0)
		{
			selectedHeroInfo = (CachedHeroInfo)values[0];
		}
		SceneAutoBehaviour<HeroTrialDungeonManagement>.instance.OpenPurifyHeroSelectWindow(HeroAwakeType.Transcend, selectedHeroInfo);
	}

	void IAction.Run(NShared.EntryMenu menu, List<object> values)
	{
		Vespa_Common_Class_Extension_Method.Interface((IAction)this).Run(null, menu, values);
	}
}
