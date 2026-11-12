using System.Collections.Generic;
using NGame2.NContents;
using NGame2.NContentsInfo;
using NGame2.NUI.NManager;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow.NEntryMenu.NAction;

public class OpenArenaBanPick : IAction
{
	void IAction.Run(CampaignNodeData nodeData, NShared.EntryMenu menu, List<object> values)
	{
		ContentsStatus contentsStatus = SingletonAuto<ContentsStatusManager>.instance.GetContentsStatus(ContentsStatusType.BanPick);
		if (contentsStatus == ContentsStatus.Disable || contentsStatus == ContentsStatus.Hide)
		{
			ErrorMessage();
			return;
		}
		SingletonAuto<MatchArena>.instance.RequestSeasonInfo(MatchPvpType.BanPick, delegate(MatchArena.MatchSeasonInfo seasonData)
		{
			if (seasonData == null)
			{
				ErrorMessage();
			}
			else
			{
				OpenBanpickArena();
			}
		});
	}

	private void ErrorMessage()
	{
		CenterMessage.Open(LocalizationManager.GetStringDirect("MATCH_PICKBAN_SEASON_INFO_REAL_END"));
	}

	private void OpenBanpickArena()
	{
		SceneAutoBehaviour<MatchManagement>.instance.OpenMatchInfo(MatchPvpType.BanPick, delegate
		{
			SceneAutoBehaviour<ContentsNoticePopupManagement>.instance.OpenContentsResultPopup(ContentsNoticeType.BanPick);
		});
	}

	void IAction.Run(NShared.EntryMenu menu, List<object> values)
	{
		Vespa_Common_Class_Extension_Method.Interface((IAction)this).Run(null, menu, values);
	}
}
