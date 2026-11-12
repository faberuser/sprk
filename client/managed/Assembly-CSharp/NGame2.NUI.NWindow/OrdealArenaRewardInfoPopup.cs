using System;
using System.Collections.Generic;
using NGame2.NContentsDefine;
using NGame2.NUI.NComponent2;
using NShared;
using NShared.NContentsDefine;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class OrdealArenaRewardInfoPopup : BaseWindow
{
	private enum EToggleType
	{
		Area,
		Rating
	}

	[Serializable]
	private class RewardGroup
	{
		public EToggleType ToggleType;

		public UIToggle Toggle;

		public GameObject Root;

		public UIScrollGrid ScrollGrid;
	}

	public const string PrefabPath = "Resources/Prefab2/Window/OrdealArena/OrdealArenaRewardInfoPopup";

	public UILocalizeEx Localize_GiveReward_Desc;

	[SerializeField]
	private RewardGroup[] RewardGroups;

	private EToggleType _curToggleType;

	private List<Component> _listComponent = new List<Component>();

	public void WindowOpen()
	{
		Open(Init, null);
	}

	private void Init()
	{
		UIToggle toggle = GetToggle(EToggleType.Area);
		if (!(toggle == null))
		{
			_curToggleType = EToggleType.Area;
			if (!toggle.value)
			{
				toggle.value = true;
			}
			else
			{
				OnAreaToggle();
			}
			InitDesc();
			SetAreaRewardComponets();
			SetRatingRewardComponents();
		}
	}

	public override void OnClosing()
	{
		base.OnClosing();
		FactoryBase.ReleaseAndClearList(_listComponent);
	}

	private void InitDesc()
	{
		NShared.NContentsDefine.MatchArena.TimeWithDayOfWeek ordealArena_GiveRewardTime = ContentsDefine.MatchArena.OrdealArena_GiveRewardTime;
		if (Localize_GiveReward_Desc != null && ordealArena_GiveRewardTime != null)
		{
			string string_of_TimeWithDayOfWeek = NGame2.NContentsDefine.MatchArena.GetString_of_TimeWithDayOfWeek(ordealArena_GiveRewardTime);
			Localize_GiveReward_Desc.SetLocalizeArg(string_of_TimeWithDayOfWeek);
		}
	}

	private UIToggle GetToggle(EToggleType toggleType)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(RewardGroups); i++)
		{
			RewardGroup rewardGroup = RewardGroups[i];
			if (rewardGroup.ToggleType == toggleType)
			{
				return rewardGroup.Toggle;
			}
		}
		return null;
	}

	private RewardGroup GetRewardGroup(EToggleType toggleType)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(RewardGroups); i++)
		{
			RewardGroup rewardGroup = RewardGroups[i];
			if (rewardGroup.ToggleType == toggleType)
			{
				return rewardGroup;
			}
		}
		return null;
	}

	private void SetAreaRewardComponets()
	{
		RewardGroup rewardGroup = GetRewardGroup(EToggleType.Area);
		if (rewardGroup == null)
		{
			return;
		}
		UIScrollGrid scrollGrid = rewardGroup.ScrollGrid;
		if (scrollGrid == null)
		{
			return;
		}
		foreach (KeyValuePair<MatchTierType, List<OrdealArenaRewardAreaData>> item in GameRoot.Instance.OrdealArenaRewardAreaDataContainer.GetAllDataByTierType())
		{
			OrdealArenaRewardAreaComponent ordealArenaRewardAreaComponent = UIHelper.AddGridItem(scrollGrid, OrdealArenaRewardAreaComponent.PrefabAuto);
			if (ordealArenaRewardAreaComponent == null)
			{
				break;
			}
			MatchTierType key = item.Key;
			List<OrdealArenaRewardAreaData> value = item.Value;
			ordealArenaRewardAreaComponent.Open(key, value);
			_listComponent.Add(ordealArenaRewardAreaComponent);
		}
		UIHelper.RepositionGrid(scrollGrid);
	}

	private void SetRatingRewardComponents()
	{
		RewardGroup rewardGroup = GetRewardGroup(EToggleType.Rating);
		if (rewardGroup == null)
		{
			return;
		}
		UIScrollGrid scrollGrid = rewardGroup.ScrollGrid;
		if (scrollGrid == null)
		{
			return;
		}
		IList<OrdealArenaRewardRatingData> allData = GameRoot.Instance.OrdealArenaRewardRatingDataContainer.GetAllData();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(allData); i++)
		{
			OrdealArenaRewardRatingComponent ordealArenaRewardRatingComponent = UIHelper.AddGridItem(scrollGrid, OrdealArenaRewardRatingComponent.PrefabAuto);
			if (ordealArenaRewardRatingComponent == null)
			{
				break;
			}
			OrdealArenaRewardRatingData data = allData[i];
			_listComponent.Add(ordealArenaRewardRatingComponent);
			ordealArenaRewardRatingComponent.Open(data);
		}
		UIHelper.RepositionGrid(scrollGrid);
	}

	public void OnAreaToggle()
	{
		UIToggle toggle = GetToggle(EToggleType.Area);
		if (!(toggle == null) && toggle.value)
		{
			OnOffGroup(EToggleType.Area);
		}
	}

	public void OnRatingToggle()
	{
		UIToggle toggle = GetToggle(EToggleType.Rating);
		if (!(toggle == null) && toggle.value)
		{
			OnOffGroup(EToggleType.Rating);
		}
	}

	private void OnOffGroup(EToggleType toggleType)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(RewardGroups); i++)
		{
			RewardGroup rewardGroup = RewardGroups[i];
			if (rewardGroup != null)
			{
				bool enable = rewardGroup.ToggleType == toggleType;
				UIHelper.EnableGameObject(rewardGroup.Root, enable);
			}
		}
	}
}
