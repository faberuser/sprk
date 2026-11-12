using System;
using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow.NSelectRewardPopup;
using NShared;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.NBattle;

public class PreSelectRewardPopup : BaseSelectRewardPopup
{
	public interface Listener
	{
		void TryStartBattle(BattlePartySetting.ExtraOptionData extraOption);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Battle/PreSelectRewardPopup";

	private Listener _listener;

	private Action _onMessagePopup;

	private Action<List<string>> _onMessagePopup2;

	public static void Open(BattleType battleType, SelectRewardData RewardData, Listener listener, Action onMessagePopup)
	{
		if (RewardData != null && RewardData.IsSelectable())
		{
			WindowPrefab<PreSelectRewardPopup> windowPrefab = new WindowPrefab<PreSelectRewardPopup>("Resources/Prefab2/Window/Battle/PreSelectRewardPopup", GameUIRoot.ELayerType.UI);
			windowPrefab.instance.OpenInternal(battleType, RewardData, listener, onMessagePopup);
			GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
		}
	}

	public static void Open(BattleType battleType, SelectRewardData RewardData, Action<List<string>> onMessagePopup2)
	{
		if (RewardData != null && RewardData.IsSelectable())
		{
			WindowPrefab<PreSelectRewardPopup> windowPrefab = new WindowPrefab<PreSelectRewardPopup>("Resources/Prefab2/Window/Battle/PreSelectRewardPopup", GameUIRoot.ELayerType.UI);
			windowPrefab.instance.OpenInternal(battleType, RewardData, null, null, onMessagePopup2);
			GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
		}
	}

	private void OpenInternal(BattleType battleType, SelectRewardData RewardData, Listener listener, Action onMessagePopup, Action<List<string>> onMessagePopup2 = null)
	{
		base.Open();
		_listener = listener;
		_selectRewardData = RewardData;
		_onMessagePopup = onMessagePopup;
		_onMessagePopup2 = onMessagePopup2;
		List<BoostedSelectRewardItemInfo> list = BaseSelectRewardPopup.ApplyBonusItemCount(battleType, _selectRewardData.GetSelectRewardItemList());
		float num = 0f;
		SetRewardList(list, SelectRewardItemListInfo);
		if (SelectRewardItemListInfo.RewardItemGrid != null)
		{
			num = SelectRewardItemListInfo.RewardItemGrid.Grid.cellWidth;
		}
		int rewardItemListElementCount = _selectRewardData.GetRewardItemListElementCount();
		RootWidget.width = Mathf.Max(MinWidth, rewardItemListElementCount * (int)num + LeftRightMargin);
		EnableOKButtonState(enable: false);
	}

	public void OnClickSelectReward()
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(SelectRewardItemListInfo.RewardItemIconComponentList))
		{
			return;
		}
		List<string> selected = new List<string>();
		SelectRewardItemListInfo.RewardItemIconComponentList.ForEach(delegate(ItemIconComponent comp)
		{
			if (comp.IsSelected())
			{
				selected.Add(comp.ItemData.Code);
			}
		});
		Close();
		AutoRepeatBattle.SetSelectedReward(selected);
		Vespa_System_Action_Extension_Method.fire(_onMessagePopup);
		Vespa_System_Action_Extension_Method.fire(_onMessagePopup2, selected);
	}

	public void OnClickCancel()
	{
		AutoRepeatBattle.ClearSelectedReward();
		Close();
	}
}
