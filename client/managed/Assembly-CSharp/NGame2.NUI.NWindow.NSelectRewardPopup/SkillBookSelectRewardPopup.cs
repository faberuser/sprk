using System;
using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow.NBattle;
using NShared;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow.NSelectRewardPopup;

public class SkillBookSelectRewardPopup : BaseSelectRewardPopup
{
	public interface IListener
	{
		void TryStartBattle(BattlePartySetting.ExtraOptionData extraOption);

		void GetReward(string[] itemCodes, Action enableButtotnAction);
	}

	[Serializable]
	public class SkillBookToggleInfo
	{
		public UIToggle Toggle;

		public SkillBookType Type;
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Battle/SkillBookSelectRewardPopup";

	public List<SkillBookToggleInfo> SkillBookToggleInfoList = new List<SkillBookToggleInfo>();

	private IListener _iListener;

	private Action _onMessagePopup;

	private BattleType _battleType;

	private SkillBookType _selectSkillBookType;

	public static void OpenGlobal(BattleType battleType, SelectRewardData RewardData, IListener iListener, Action onMessagePopup, SelectRewardButtonType buttonType)
	{
		if (RewardData != null && RewardData.IsSelectable())
		{
			WindowPrefab<SkillBookSelectRewardPopup> windowPrefab = new WindowPrefab<SkillBookSelectRewardPopup>("Resources/Prefab2/Window/Battle/SkillBookSelectRewardPopup", GameUIRoot.ELayerType.UI);
			windowPrefab.instance.OpenInternal(battleType, RewardData, iListener, onMessagePopup, buttonType);
			GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
		}
	}

	public void Open(BattleType battleType, SelectRewardData RewardData, IListener iListener, Action onMessagePopup, SelectRewardButtonType buttonType)
	{
		if (RewardData != null && RewardData.IsSelectable())
		{
			OpenInternal(battleType, RewardData, iListener, onMessagePopup, buttonType);
		}
	}

	private void OpenInternal(BattleType battleType, SelectRewardData rewardData, IListener iListener, Action onMessagePopup, SelectRewardButtonType buttonType)
	{
		base.Open();
		Init(battleType, rewardData, iListener, onMessagePopup, buttonType);
	}

	private void Init(BattleType battleType, SelectRewardData rewardData, IListener iListener, Action onMessagePopup, SelectRewardButtonType buttonType)
	{
		_iListener = iListener;
		_selectRewardData = rewardData;
		_battleType = battleType;
		_onMessagePopup = onMessagePopup;
		_selectSkillBookType = SkillBookType.Beginner;
		SetRewardList();
		SetButtonType(buttonType);
		SetToggle();
	}

	private void SetRewardList()
	{
		FactoryBase.ReleaseAndClearList(SelectRewardItemListInfo.RewardItemIconComponentList);
		if (_selectRewardData == null)
		{
			return;
		}
		List<SelectRewardData.SelectRewardItemInfo> selectRewardItemList = _selectRewardData.GetSelectRewardItemList();
		if (__Extension_Method_ICollection__.IsNullOrEmpty(selectRewardItemList))
		{
			return;
		}
		List<SelectRewardData.SelectRewardItemInfo> list = new List<SelectRewardData.SelectRewardItemInfo>();
		list.Clear();
		SelectRewardData.SelectRewardItemInfo selectRewardItemInfo = null;
		for (int i = 0; i < selectRewardItemList.Count; i++)
		{
			selectRewardItemInfo = selectRewardItemList[i];
			if (selectRewardItemInfo != null)
			{
				string code = selectRewardItemInfo.Code;
				int selectSkillBookType = (int)_selectSkillBookType;
				if (code.Contains(selectSkillBookType.ToString()))
				{
					list.Add(selectRewardItemInfo);
				}
			}
		}
		List<BoostedSelectRewardItemInfo> list2 = BaseSelectRewardPopup.ApplyBonusItemCount(_battleType, list);
		SetRewardList(list2, SelectRewardItemListInfo);
		EnableOKButtonState(enable: false);
	}

	private void SetToggle()
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(SkillBookToggleInfoList))
		{
			return;
		}
		SkillBookToggleInfoList.ForEach(delegate(SkillBookToggleInfo info)
		{
			if (info != null && info.Toggle != null && info.Type == _selectSkillBookType)
			{
				info.Toggle.Set(state: true);
			}
		});
	}

	public void OnChangeToggle(UIToggle toggle)
	{
		if (toggle == null || __Extension_Method_ICollection__.IsNullOrEmpty(SkillBookToggleInfoList))
		{
			return;
		}
		SkillBookToggleInfo skillBookToggleInfo = null;
		for (int num = SkillBookToggleInfoList.Count - 1; num >= 0; num--)
		{
			skillBookToggleInfo = SkillBookToggleInfoList[num];
			if (skillBookToggleInfo != null && !(skillBookToggleInfo.Toggle == null) && skillBookToggleInfo.Toggle.Equals(toggle))
			{
				_selectSkillBookType = skillBookToggleInfo.Type;
				break;
			}
		}
		SetRewardList();
	}

	public void OnClickCancel()
	{
		AutoRepeatBattle.ClearSelectedReward();
		Close();
	}

	public void OnClickConfirm()
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
		if (_onMessagePopup != null)
		{
			Vespa_System_Action_Extension_Method.fire(_onMessagePopup);
		}
	}

	public void OnClickGetReward()
	{
		if (_iListener == null || _selected != _selectRewardData.SelectCount || __Extension_Method_ICollection__.IsNullOrEmpty(SelectRewardItemListInfo.RewardItemIconComponentList))
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
		EnableOKButtonState(enable: false);
		_iListener.GetReward(selected.ToArray(), delegate
		{
			EnableOKButtonState(enable: true);
		});
	}
}
