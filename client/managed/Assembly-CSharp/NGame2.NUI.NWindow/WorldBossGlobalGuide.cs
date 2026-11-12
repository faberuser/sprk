using System;
using NGame2.NAccount;
using NGame2.NContentsInfo;
using NShared;
using NShared.NWorldBoss;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow;

public class WorldBossGlobalGuide : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/WorldBoss/WorldBossGlobalGuide";

	public UILabel Label_GlobalWorldBossDesc;

	public override void Open()
	{
		base.Open();
		WorldBossInfo worldBossInfo = SingletonUpdater<WorldBoss>.instance.WorldBossInfo;
		SetGlobalWorldBossDesc(worldBossInfo);
	}

	private void SetGlobalWorldBossDesc(WorldBossInfo worldBossInfo)
	{
		int num = 0;
		WorldBossData data = GameRoot.Instance.WorldBossDataContainer.GetData(worldBossInfo.Index);
		if (data != null)
		{
			num = data.DealBattleDuration;
		}
		DateTime time = Convert.ToDateTime(worldBossInfo.CreatedTime);
		DateTime time2 = Convert.ToDateTime(worldBossInfo.CreatedTime);
		DateTime dateTime = GlobalAutoBehaviour<AccountManager>.instance.ServerUTCToLocal(time);
		DateTime dateTime2 = GlobalAutoBehaviour<AccountManager>.instance.ServerUTCToLocal(time2).AddSeconds(num);
		Enum dayOfWeekKey = NGame2.NUI.Common.GetDayOfWeekKey(dateTime.DayOfWeek);
		Enum dayOfWeekKey2 = NGame2.NUI.Common.GetDayOfWeekKey(dateTime2.DayOfWeek);
		string text = LocalizationManager.GetString(dayOfWeekKey);
		string text2 = LocalizationManager.GetString(dayOfWeekKey2);
		string text3 = text + dateTime.ToString("HH:mm");
		string text4 = dateTime2.ToString("HH:mm");
		string text5 = text2 + text4;
		string stringDirect = LocalizationManager.GetStringDirect("GLOBAL_WORLDBOSS_INFO_DESC", text3, text5, text4);
		UIHelper.SetLabelText(Label_GlobalWorldBossDesc, stringDirect);
	}
}
