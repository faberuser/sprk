using System;
using NGame2.NBattleField;
using NShared;
using NVespa;
using NVespa.NGlobal;
using UnityEngine;

namespace NGame2.NUI.NWindow.NBattle;

public class TimeStatus : BaseWindowUpdater
{
	[Serializable]
	public class LabelTimeInfo
	{
		public int Time;

		public Color Color;
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Battle/TimeStatus";

	[SerializeField]
	private UILabel _timePrefixLabel;

	[SerializeField]
	private UILabel _remainTimeLabel;

	[SerializeField]
	private LabelTimeInfo[] _labelTimeInfo;

	public void Open(int chapterIndex, int dungeonIndex, bool isScenario)
	{
		base.Open();
		CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(chapterIndex, dungeonIndex);
		if (data == null)
		{
			UIHelper.EnableGameObject(_timePrefixLabel, enable: false);
			return;
		}
		BattleDefineData battleDefine = data.GetBattleDefine(isScenario);
		if (battleDefine == null)
		{
			UIHelper.EnableGameObject(_timePrefixLabel, enable: false);
			return;
		}
		if (data.BattleType == BattleType.Arena)
		{
			Close();
			return;
		}
		if (battleDefine == null || String_Extension_Method.IsNullOrEmpty(battleDefine.LabelPrefix))
		{
			UIHelper.EnableGameObject(_timePrefixLabel, enable: false);
			return;
		}
		UIHelper.EnableGameObject(_timePrefixLabel, enable: true);
		UIHelper.SetLabelText(_timePrefixLabel, LocalizationManager.GetStringDirect(battleDefine.LabelPrefix));
	}

	public override void UpdatePeriodically()
	{
		base.UpdatePeriodically();
		if (!SingletonAuto<BattleFacade>.instance.IsValidBattle())
		{
			Close();
			return;
		}
		BattleFacade.BattleCacheData battleCache = SingletonAuto<BattleFacade>.instance.GetBattleCache();
		if (battleCache.InfiniteBattleTime)
		{
			Close();
			return;
		}
		UIHelper.EnableGameObject(_remainTimeLabel, battleCache.MaxBattleTime > 0);
		if (battleCache.MaxBattleTime <= 0)
		{
			return;
		}
		int num = (battleCache.RemainedBattleTimeMs + 999) / 1000;
		int num2 = num / 60;
		int num3 = num % 60;
		UIHelper.SetLabelText(_remainTimeLabel, $"{num2}:{num3:D2}");
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_labelTimeInfo); i++)
		{
			LabelTimeInfo labelTimeInfo = _labelTimeInfo[i];
			if (num <= labelTimeInfo.Time)
			{
				UIHelper.SetWidgetColor(_remainTimeLabel, labelTimeInfo.Color);
				break;
			}
		}
	}
}
