using System;
using NGame2.NAccount;
using NGame2.NUI.NWindow;
using NShared;
using NShared.NLocalization;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class MatchReplayComponent : LinkFactoryComponent
{
	public interface Listener
	{
		void OnView(MatchReplayComponent caller);
	}

	public const string PrefabPath = "Prefab2/Component/Match/MatchReplayComponent";

	public static PrefabAuto<MatchReplayComponent> PrefabAuto = new PrefabAuto<MatchReplayComponent>("Prefab2/Component/Match/MatchReplayComponent");

	public GameObject WinMarkObject;

	public GameObject LoseMarkObject;

	public UILabel OpponentNameLabel;

	public UILabel TimeLabel;

	public GameObject MatchScoreUpObject;

	public GameObject MatchScoreDownObject;

	public UILabel ResultLabel;

	private Listener _listener;

	public long ReplayUid { get; private set; }

	public ArenaReplayInfo ReplayInfo { get; private set; }

	public bool Open(ArenaReplayInfo replayInfo, MatchPvpType matchType, long replayUid, Listener listener)
	{
		ReplayUid = replayUid;
		ReplayInfo = replayInfo;
		_listener = listener;
		ReplayTeamInfo leftTeam = replayInfo.LeftTeam;
		ReplayTeamInfo rightTeam = replayInfo.RightTeam;
		if (leftTeam == null || rightTeam == null)
		{
			return false;
		}
		long id = GlobalAutoBehaviour<AccountManager>.instance.UserManager.Id;
		ReplayTeamInfo replayTeamInfo = ((((leftTeam.AccountId == id) ? leftTeam : rightTeam) == leftTeam) ? rightTeam : leftTeam);
		bool flag = ((leftTeam.AccountId != id) ? 1 : 0) == (int)replayInfo.WinTeam;
		UIHelper.EnableGameObject(WinMarkObject, flag);
		UIHelper.EnableGameObject(LoseMarkObject, !flag);
		UIHelper.EnableGameObject(MatchScoreUpObject, flag);
		UIHelper.EnableGameObject(MatchScoreDownObject, !flag);
		string text = "Lv " + replayTeamInfo.Level + " " + replayTeamInfo.NickName;
		UIHelper.SetLabelText(OpponentNameLabel, text);
		if (Vespa_String_Extension_Method2.TryParseNetworkTime2(replayInfo.RecordedTime, out var o_dateTime))
		{
			UIHelper.EnableGameObject(TimeLabel, enable: true);
			TimeSpan timeSpan = NGame2.NUI.NWindow.Common.GetUTCNow() - o_dateTime;
			if ((int)timeSpan.TotalDays >= 1)
			{
				UIHelper.SetLabelText(TimeLabel, LocalizationManager.GetString(Match.MATCH_HISTORY_TIME_DAY, (int)timeSpan.TotalDays));
			}
			else if ((int)timeSpan.TotalHours >= 1)
			{
				UIHelper.SetLabelText(TimeLabel, LocalizationManager.GetString(Match.MATCH_HISTORY_TIME_HOUR, (int)timeSpan.TotalHours));
			}
			else
			{
				UIHelper.SetLabelText(TimeLabel, LocalizationManager.GetString(Match.MATCH_HISTORY_TIME_MINUTE, (int)timeSpan.TotalMinutes));
			}
		}
		else
		{
			UIHelper.EnableGameObject(TimeLabel, enable: false);
		}
		if (flag)
		{
			if (replayInfo.WinMatchScore > 0 && matchType == MatchPvpType.Normal)
			{
				UIHelper.SetLabelText(ResultLabel, LocalizationManager.GetString(Match.MATCH_HISTORY_WIN_SCORE, replayInfo.WinMatchScore));
			}
			else
			{
				UIHelper.SetLabelText(ResultLabel, LocalizationManager.GetString(Match.MATCH_HISTORY_WIN));
			}
		}
		else if (replayInfo.LoseMatchScore < 0 && matchType == MatchPvpType.Normal)
		{
			UIHelper.SetLabelText(ResultLabel, LocalizationManager.GetString(Match.MATCH_HISTORY_LOSE_SCORE, -replayInfo.LoseMatchScore));
		}
		else
		{
			UIHelper.SetLabelText(ResultLabel, LocalizationManager.GetString(Match.MATCH_HISTORY_LOSE));
		}
		return true;
	}

	public void OnClickView()
	{
		if (_listener != null)
		{
			_listener.OnView(this);
		}
	}
}
