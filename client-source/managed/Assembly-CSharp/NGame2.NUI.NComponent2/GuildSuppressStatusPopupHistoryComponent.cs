using System;
using NGame2.NAccount;
using NGame2.NUI.NHelper.NGuild;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NComponent2;

public class GuildSuppressStatusPopupHistoryComponent : GuildSuppressStatusBoardUserComponent
{
	public new const string PrefabPath = "Prefab2/Component/GuildSuppress/GuildSuppressStatusPopupHistoryComponent";

	public new static PrefabAuto<GuildSuppressStatusPopupHistoryComponent> PrefabAuto = new PrefabAuto<GuildSuppressStatusPopupHistoryComponent>("Prefab2/Component/GuildSuppress/GuildSuppressStatusPopupHistoryComponent");

	public UILabel TimeLabel;

	public UILabel BossNameLabel;

	public void Open(DateTime time, string name, int raidIndex, int level, long score)
	{
		SetTime(time);
		name = LocalizationManager.GetStringDirect("GUILD_SUPPRESS_STATUS_BOARD_PARTY_LEADER", name);
		SetName(name);
		SetBossName(raidIndex, level);
		SetScore(score);
	}

	private void SetTime(DateTime dateTime)
	{
		int serverUTCOffsetHour = GlobalAutoBehaviour<AccountManager>.instance.GetServerUTCOffsetHour();
		dateTime = dateTime.AddHours(serverUTCOffsetHour);
		string localMonthName = NGame2.NUI.Common.GetLocalMonthName(dateTime.Month);
		string ordinalizeDay = NGame2.NUI.Common.GetOrdinalizeDay(dateTime.Day);
		string stringDirect = LocalizationManager.GetStringDirect("TIME_MONTH_DAY_HOUR_MINUTE", localMonthName, ordinalizeDay, dateTime.Hour.ToString("D2"), dateTime.Minute.ToString("D2"));
		UIHelper.SetLabelText(TimeLabel, stringDirect);
	}

	private void SetBossName(int raidIndex, int level)
	{
		GuildSuppressHelper.SetBossNameLabel(raidIndex, level, BossNameLabel, null);
	}
}
