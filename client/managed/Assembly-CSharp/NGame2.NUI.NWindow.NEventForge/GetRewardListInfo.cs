using System;
using System.Collections.Generic;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NWindow.NEventForge;

[Serializable]
public class GetRewardListInfo
{
	public struct Info
	{
		public int Group { get; set; }

		public int ReinforcementLevel { get; set; }
	}

	public UIBaseGrid RewardGrid;

	public UILabel ReinforcementLevelLabel;

	public Transform NotEnoughReinforcementLevelNotiTransform;

	public Transform AlreadyMaxReinforceLevelNotiTransform;

	public Transform CurrentReinforcementLevelTransform;

	public UISprite LineSprite;

	public Color DefaultReinforcementLevelColor;

	public Color CurrentReinforcementLevelColor;

	public Color NextReinforcementLevelColor;

	private List<Component> _rewardCompList = new List<Component>();

	private Info? _info;

	public int RewardCount
	{
		get
		{
			if (__Extension_Method_ICollection__.IsNullOrEmpty(_rewardCompList))
			{
				return 0;
			}
			return _rewardCompList.Count;
		}
	}

	public void Init(Info info)
	{
		_info = info;
	}

	public void Release()
	{
		ReleaseRewardList();
	}

	public void SetInfo(int level, int maxLevel)
	{
		bool flag = level > 0 && maxLevel < level;
		SetReinforcementLevelInfo(level, flag);
		SetRewardList(level);
		UIHelper.EnableGameObject(NotEnoughReinforcementLevelNotiTransform, level == 0);
		UIHelper.EnableGameObject(AlreadyMaxReinforceLevelNotiTransform, level > 0 && flag);
		UIHelper.EnableGameObject(LineSprite, !flag);
		UIHelper.EnableGameObject(ReinforcementLevelLabel, !flag);
		UIHelper.EnableGameObject(CurrentReinforcementLevelTransform, enable: false);
		if (_info.HasValue && _info.HasValue)
		{
			UIHelper.EnableGameObject(CurrentReinforcementLevelTransform, _info.Value.ReinforcementLevel == level);
		}
	}

	private void SetReinforcementLevelInfo(int level, bool alreadyMaxLevel)
	{
		if (_info.HasValue && _info.HasValue)
		{
			Color color = GetColor(level);
			string text = ColorUtility.ToHtmlStringRGB(color);
			if (LineSprite != null)
			{
				LineSprite.color = color;
			}
			string text2 = "[c][" + text + "]" + (alreadyMaxLevel ? "-" : LocalizationManager.GetStringDirect("EVENT_FORGE_MSG_ENCHANTLEVEL", level)) + "[-][/c]";
			UIHelper.SetLabelText(ReinforcementLevelLabel, text2);
		}
	}

	private void ReleaseRewardList()
	{
		FactoryBase.ReleaseAndClearList(_rewardCompList);
	}

	private void SetRewardList(int level)
	{
		ReleaseRewardList();
		if (!(RewardGrid == null))
		{
			EventForgeData eventForgeData = GetEventForgeData(level);
			if (eventForgeData != null)
			{
				_rewardCompList = UIHelper.CreateRewardItemIconComponentList(eventForgeData.ExchangeRewardIndex, RewardGrid, includeCurrency: true);
				UIHelper.RepositionGrid(RewardGrid);
			}
		}
	}

	private Color GetColor(int level)
	{
		if (!_info.HasValue || !_info.HasValue)
		{
			return DefaultReinforcementLevelColor;
		}
		if (_info.Value.ReinforcementLevel == level)
		{
			return CurrentReinforcementLevelColor;
		}
		if (_info.Value.ReinforcementLevel + 1 == level)
		{
			return NextReinforcementLevelColor;
		}
		return DefaultReinforcementLevelColor;
	}

	private EventForgeData GetEventForgeData(int level)
	{
		if (!_info.HasValue || !_info.HasValue)
		{
			return null;
		}
		return GameRoot.Instance.EventForgeDataContainer.GetDataByLevel(_info.Value.Group, level);
	}
}
