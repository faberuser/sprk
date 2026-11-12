using System;
using System.Collections.Generic;
using NGame2.NEventTrigger;
using NShared;
using NShared.NEvent;
using NVespa;
using NVespa.NGlobal;

namespace NGame2.NUI.NWindow;

public class RecommendEquipmentContainer : BaseWindow
{
	public enum ToggleState
	{
		NONE = -1,
		COMBAT,
		BOSS,
		PVP,
		END
	}

	[Serializable]
	public class RecommendEquipData
	{
		public UIToggle RecommendToggle;

		public string[] RecommendString;
	}

	public const string PrefabPath = "Resources/Prefab2/Window/RecommendEquipment/RecommendEquipmentContainer";

	public const int RecommendMaxCount = 3;

	public UIGrid ToggleGrid;

	public List<RecommendEquipData> recommendEquipDatas;

	public UIGrid TextGrid;

	public List<UILabel> RecommendTextLabel;

	private HeroInfo _heroInfo;

	private ToggleState _currtoggleState = ToggleState.NONE;

	public void Open(HeroInfo HeroInfo)
	{
		if (HeroInfo == null)
		{
			return;
		}
		CreatureData creatureData = HeroInfo.CreatureData;
		if (creatureData == null || (__Extension_Method_ICollection__.IsNullOrEmpty(creatureData.EquipoptionRecommend1) && __Extension_Method_ICollection__.IsNullOrEmpty(creatureData.EquipoptionRecommend2) && __Extension_Method_ICollection__.IsNullOrEmpty(creatureData.EquipoptionRecommend3)))
		{
			return;
		}
		_heroInfo = HeroInfo;
		_currtoggleState = ToggleState.NONE;
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(creatureData.EquipoptionRecommend1))
		{
			recommendEquipDatas[0].RecommendString = creatureData.EquipoptionRecommend1;
		}
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(creatureData.EquipoptionRecommend2))
		{
			recommendEquipDatas[1].RecommendString = creatureData.EquipoptionRecommend2;
		}
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(creatureData.EquipoptionRecommend3))
		{
			recommendEquipDatas[2].RecommendString = creatureData.EquipoptionRecommend3;
		}
		base.Open();
		int? num = null;
		for (int num2 = __Extension_Method_ICollection__.COUNT(recommendEquipDatas) - 1; num2 >= 0; num2--)
		{
			UIHelper.EnableGameObject(recommendEquipDatas[num2].RecommendToggle, !__Extension_Method_ICollection__.IsNullOrEmpty(recommendEquipDatas[num2].RecommendString));
			recommendEquipDatas[num2].RecommendToggle.startsActive = false;
			recommendEquipDatas[num2].RecommendToggle.value = false;
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(recommendEquipDatas[num2].RecommendString))
			{
				num = num2;
			}
		}
		ToggleGrid.Reposition();
		if (num.HasValue)
		{
			recommendEquipDatas[num.Value].RecommendToggle.value = true;
			ToggleRecommend();
		}
		SingletonAuto<EventTriggerManager>.instance.RaiseEvent(new OpenEquipOptionPopup());
	}

	public void ToggleRecommend()
	{
		int num = recommendEquipDatas.FindIndex((RecommendEquipData x) => x.RecommendToggle.value && !__Extension_Method_ICollection__.IsNullOrEmpty(x.RecommendString));
		if (num < 0 || num == (int)_currtoggleState)
		{
			return;
		}
		_currtoggleState = (ToggleState)num;
		for (int num2 = 0; num2 < __Extension_Method_ICollection__.COUNT(RecommendTextLabel); num2++)
		{
			UIHelper.EnableGameObject(RecommendTextLabel[num2], enable: false);
		}
		string[] recommendString = recommendEquipDatas[num].RecommendString;
		int num3 = 1;
		for (int num4 = 0; num4 < recommendString.Length; num4++)
		{
			if (!String_Extension_Method.IsNullOrEmpty(recommendString[num4]))
			{
				string text = string.Format(LocalizationManager.GetStringDirect("HERO_EQUIPOPTION.RECOMMEND_INFO"), arg1: LocalizationManager.GetStringDirect(recommendString[num4]), arg0: num3);
				UIHelper.EnableGameObject(RecommendTextLabel[num4], enable: true);
				UIHelper.SetLabelText(RecommendTextLabel[num4], text);
				num3++;
			}
		}
		TextGrid.Reposition();
	}

	public override void Close()
	{
		base.Close();
		_heroInfo = null;
		for (int i = 0; i < 3; i++)
		{
			recommendEquipDatas[i].RecommendToggle.Set(state: false);
			recommendEquipDatas[i].RecommendString = null;
		}
	}
}
