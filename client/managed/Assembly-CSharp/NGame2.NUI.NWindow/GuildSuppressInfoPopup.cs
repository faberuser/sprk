using System;
using System.Collections;
using NGame2.NAccount;
using NGame2.NGuild;
using NShared;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow;

public class GuildSuppressInfoPopup : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/GuildSuppress/GuildSuppressInfoPopup";

	public UILabel Label_GuildSuppressTitle;

	public UILabel Label_ApplyPeriodDesc;

	public UILabel Label_ProgressPeriodDesc;

	public UITable Table_Root;

	public new void Open()
	{
		Open(Init, null);
	}

	private void Init()
	{
		ExtensionMethod_VespaCoroutine.StartCoroutineThis(this, RepostionNextUpdate());
		GuildSuppressSessionInfo guildSuppressSessionInfo = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildSuppressManager.GetGuildSuppressSessionInfo();
		int index = (int)((guildSuppressSessionInfo != null) ? guildSuppressSessionInfo.SessionIndex : 0);
		int num = 0;
		GuildSuppressSessionData guildSuppressSessionData = RestoredPortal.ConquestSession(GameRoot.Instance.GuildSuppressSessionDataContainer, index);
		if (guildSuppressSessionData != null)
		{
			num = guildSuppressSessionData.SeasonIndex;
		}
		string text = "";
		if (num != 0)
		{
			text = LocalizationManager.GetStringDirect("RANK_BOARD_SEASON_INFO", num);
		}
		string stringDirect = LocalizationManager.GetStringDirect("PUNITIVE_EXPEDITION_INFO");
		UIHelper.SetLabelText(Label_GuildSuppressTitle, text + stringDirect);
		SetDescLabel();
	}

	private void SetDescLabel()
	{
		UIHelper.SetLabelText(Label_ApplyPeriodDesc, string.Empty);
		UIHelper.SetLabelText(Label_ProgressPeriodDesc, string.Empty);
		string[] valueArray = GameRoot.Instance.ConstantDataContainer.GetValueArray<string>("GlobalGuildSuppressApplyTime");
		string[] valueArray2 = GameRoot.Instance.ConstantDataContainer.GetValueArray<string>("GlobalGuildSuppressProgressTime");
		if (valueArray.Length == 4)
		{
			string localTimeRange = GetLocalTimeRange(valueArray[0], valueArray[1], valueArray[2], valueArray[3]);
			string stringDirect = LocalizationManager.GetStringDirect("PUNITIVE_EXPEDITION_INFO_APPLY_DESC", localTimeRange);
			UIHelper.SetLabelText(Label_ApplyPeriodDesc, stringDirect);
		}
		if (valueArray2.Length == 4)
		{
			string localTimeRange2 = GetLocalTimeRange(valueArray2[0], valueArray2[1], valueArray2[2], valueArray2[3]);
			string stringDirect2 = LocalizationManager.GetStringDirect("PUNITIVE_EXPEDITION_INFO_PLAY_DESC", localTimeRange2);
			UIHelper.SetLabelText(Label_ProgressPeriodDesc, stringDirect2);
		}
	}

	private string GetLocalTimeRange(string startDowUtc, string startUtc, string endDowUtc, string endUtc)
	{
		DateTime dateTime = MakeUtcTime(startUtc);
		DateTime dateTime2 = MakeUtcTime(endUtc);
		DateTime clientTime = UIHelper.GetClientTime(dateTime);
		DateTime clientTime2 = UIHelper.GetClientTime(dateTime2);
		DayOfWeek localDayOfWeek = GetLocalDayOfWeek(startDowUtc, clientTime, dateTime);
		DayOfWeek localDayOfWeek2 = GetLocalDayOfWeek(endDowUtc, clientTime2, dateTime2);
		string text = LocalizationManager.GetString(NGame2.NUI.Common.GetDayOfWeekKey(localDayOfWeek));
		string text2 = LocalizationManager.GetString(NGame2.NUI.Common.GetDayOfWeekKey(localDayOfWeek2));
		return text + " " + clientTime.ToString("HH:mm:ss") + " ~ " + text2 + " " + clientTime2.ToString("HH:mm:ss");
	}

	private DateTime MakeUtcTime(string utcTime)
	{
		DateTime currentServerUTCTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime();
		DateTime dateTime = Convert.ToDateTime(utcTime);
		return new DateTime(currentServerUTCTime.Year, currentServerUTCTime.Month, currentServerUTCTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second);
	}

	private DayOfWeek GetLocalDayOfWeek(string baseDoW, DateTime localTime, DateTime utcTime)
	{
		DayOfWeek dayOfWeek = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), baseDoW);
		int diffDay = GetDiffDay(localTime, utcTime);
		if (diffDay < 0)
		{
			dayOfWeek--;
		}
		else if (diffDay > 0)
		{
			dayOfWeek++;
		}
		return dayOfWeek;
	}

	private int GetDiffDay(DateTime t1, DateTime t2)
	{
		DateTime t3 = new DateTime(t1.Year, t1.Month, t1.Day);
		DateTime t4 = new DateTime(t2.Year, t2.Month, t2.Day);
		return DateTime.Compare(t3, t4);
	}

	private IEnumerator RepostionNextUpdate()
	{
		yield return null;
		if ((bool)Table_Root)
		{
			Table_Root.repositionNow = true;
		}
	}
}
