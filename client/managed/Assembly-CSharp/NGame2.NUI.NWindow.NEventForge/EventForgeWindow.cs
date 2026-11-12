using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NComponent2.NEventForge;
using NShared;
using NShared.EventForge;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.NEventForge;

public class EventForgeWindow : BaseWindow, ItemIconComponent.Listener
{
	public interface IListener
	{
		void ProgressReinforceItem(int slotIndex, EventForgeData eventForgeData, Action<NShared.EventForge.Response> resultAction);

		void ProgressCompleteSuccessReinforceItem(int slotIndex, EventForgeData eventForgeData, Action<NShared.EventForge.Response> resultAction);

		void ExchangeRandomReward(int slotIndex, EventForgeData eventForgeData, Action resultAction);

		void ShowReinforcementPercentageInfo();
	}

	public struct Info
	{
		public IListener IListener { get; set; }

		public Func<int, int, bool> CheckEnoughCompleteSuccessMaterialCountFunc { get; set; }
	}

	public const string PrefabPath = "Resources/Prefab2/Window/EventForge/EventForgeWindow";

	private const int NextReinforcementLevelOffset = 1;

	[Header("[강화 아이템]")]
	public Transform EventItemIconCompRootTransform;

	public UILabel SuccessPercentageLabel;

	public UILabel FailPercentageLabel;

	public UILabel BreakPercentageLabel;

	public UIGame3DWidget ReinforcementEffectWidget;

	public UIGame3DWidget SuccessReinforcementEffectWidget;

	public UIGame3DWidget FailReinforcementEffectWidget;

	[Header("[강화 재료]")]
	public ReinforcementMaterialItemInfo ReinforcementMaterialItemInfo;

	[Header("[확정 강화 재료]")]
	public ReinforcementMaterialItemInfo CompleteSuccessReinforcementMaterialItemInfo;

	[Header("[강화 가능한 내 목록]")]
	public UIScrollGrid PossibleEventItemScrollGrid;

	public Transform NoEventItemInfoTransform;

	[Header("[강화 성공 획득 가능한 보상 목록]")]
	public GetRewardListInfo FirstGetRewardListInfo;

	public GetRewardListInfo SecondGetRewardListInfo;

	public UIButton PrevButton;

	public UIButton NextButton;

	public UISimpleGrid GetPossibleRewardGuideGrid;

	public Transform GetPossibleRewardRootTranform;

	public Transform NoSelectEventItemInformRootTransform;

	[Header("[강화 시도 필요 가격]")]
	public RequirePrice EventItemReinforcementRequirePrice;

	[Header("강화소 버튼")]
	public UIButton CompleteSuccessReinforcementButton;

	public UIButton ExchangeButton;

	public UIButton ReinforcementButton;

	private Info? _info;

	private IListener _iListener;

	private int _currentGroupIndex;

	private int _currentReinforcementLevel = -1;

	private int _currentSlotIndex;

	private int _getPossibleReinforcementLevel;

	private ItemIconComponent _eventItemIconComp;

	private List<ItemIconComponent> _possibleEventItemIconCompList = new List<ItemIconComponent>();

	private List<EventForgeGetRewardGuideComponent> _getPossibleRewardGuideCompList = new List<EventForgeGetRewardGuideComponent>();

	private bool CheckIsEffect
	{
		get
		{
			EventForgeData eventForgeData = GetEventForgeData(NextReinforcementLevel(_currentReinforcementLevel));
			if (eventForgeData == null)
			{
				return false;
			}
			if (eventForgeData.IsHighEnchant)
			{
				return false;
			}
			return true;
		}
	}

	private List<EquipItemInfo> PossibleEventItemInfoList
	{
		get
		{
			if (!GlobalAutoBehaviour<AccountManager>.isValidInstance)
			{
				return null;
			}
			NGame2.NAccount.UserEquipItemManager equipItemManager = GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager;
			if (equipItemManager == null)
			{
				return null;
			}
			List<EquipItemInfo> list = new List<EquipItemInfo>();
			list.Clear();
			foreach (EquipItemInfo item in equipItemManager)
			{
				if (item != null && item.ItemData != null && item.ItemData.Type == ItemType.EventEnchantWeapon)
				{
					list.Add(item);
				}
			}
			return list;
		}
	}

	private bool CheckPrevGetPossibleRewardList => _getPossibleReinforcementLevel > 0;

	private bool CheckNextGetPossibleRewardList => _getPossibleReinforcementLevel < GetMaxReinforcementLevel(NextReinforcementLevel(_getPossibleReinforcementLevel));

	private int RequirePrice => GetEventForgeData(NextReinforcementLevel(_currentReinforcementLevel))?.RequireGoldCost ?? (-1);

	private bool CheckPossibleReinforcement
	{
		get
		{
			if (_currentGroupIndex <= 0)
			{
				return false;
			}
			if (_currentSlotIndex <= 0)
			{
				return false;
			}
			if (!CheckEnoughReinforcementMaterialCount)
			{
				return false;
			}
			if (!CheckEnoughGold)
			{
				return false;
			}
			_ = _currentReinforcementLevel;
			return true;
		}
	}

	private bool CheckPossibleCompleteSuccessReinforcement
	{
		get
		{
			if (!_info.HasValue || !_info.HasValue)
			{
				return false;
			}
			EventForgeData eventForgeData = GetEventForgeData(NextReinforcementLevel(_currentReinforcementLevel));
			if (eventForgeData == null)
			{
				return false;
			}
			if (eventForgeData.SuccessPercentage >= 100f)
			{
				return false;
			}
			if (CheckPossibleReinforcement)
			{
				return _info.Value.CheckEnoughCompleteSuccessMaterialCountFunc(NextReinforcementLevel(_currentReinforcementLevel), _currentGroupIndex);
			}
			return false;
		}
	}

	private bool CheckPossibleExchange
	{
		get
		{
			if (_currentGroupIndex > 0 && _currentReinforcementLevel > 0)
			{
				return _currentSlotIndex > 0;
			}
			return false;
		}
	}

	private bool CheckEnoughReinforcementMaterialCount
	{
		get
		{
			EventForgeData eventForgeData = GetEventForgeData(NextReinforcementLevel(_currentReinforcementLevel));
			if (eventForgeData == null)
			{
				return false;
			}
			if (!GlobalAutoBehaviour<AccountManager>.isValidInstance || GlobalAutoBehaviour<AccountManager>.instance.ItemManager == null)
			{
				return false;
			}
			ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(eventForgeData.RequireItem);
			if (dataByCode == null)
			{
				return false;
			}
			return eventForgeData.RequireItemCount <= GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemCount(dataByCode.Index);
		}
	}

	private bool CheckEnoughGold
	{
		get
		{
			EventForgeData eventForgeData = GetEventForgeData(NextReinforcementLevel(_currentReinforcementLevel));
			if (eventForgeData == null)
			{
				return false;
			}
			if (!GlobalAutoBehaviour<AccountManager>.isValidInstance || GlobalAutoBehaviour<AccountManager>.instance.UserInfo == null)
			{
				return false;
			}
			return eventForgeData.RequireGoldCost <= GlobalAutoBehaviour<AccountManager>.instance.UserInfo.Gold;
		}
	}

	public void Open(Info info)
	{
		Open(delegate
		{
			Init(info);
		}, null);
	}

	private void Init(Info? info)
	{
		if (!info.HasValue || !info.HasValue)
		{
			Close();
			return;
		}
		_info = info;
		_iListener = info.Value.IListener;
		InitCurrentInfo();
		SetPossibleEventItemList();
		SetInfo(isChangeItem: true);
	}

	private void SetInfo(bool isChangeItem)
	{
		InitGetRewardListInfo();
		SetEventItemIconComponent(isChangeItem);
		SetReinforcementPercentageLabel();
		InitReinforcementMaterialItemInfo(isChangeItem);
		InitCompleteSuccessReinforcementMaterialItemInfo(isChangeItem);
		InitEventItemReinforcementRequirePrice();
		SetEventForgeButtonState();
		SetGetPossibleRewardInformState();
	}

	public override void Reset()
	{
		base.Reset();
		InitCurrentInfo();
	}

	public override void ReOpen()
	{
		base.ReOpen();
		SetPossibleEventItemList();
		InitReinforcementMaterialItemInfo(isChangeItem: false);
		InitCompleteSuccessReinforcementMaterialItemInfo(isChangeItem: false);
		InitEventItemReinforcementRequirePrice();
		SetEventForgeButtonState();
	}

	public override void Close()
	{
		base.Close();
		InitCurrentInfo();
		ReleaseEventItemIconComponent();
		EventItemReinforcementRequirePrice.Close();
		ReinforcementMaterialItemInfo.Release();
		CompleteSuccessReinforcementMaterialItemInfo.Release();
		FirstGetRewardListInfo.Release();
		SecondGetRewardListInfo.Release();
	}

	private void InitCurrentInfo()
	{
		_currentGroupIndex = 0;
		_currentReinforcementLevel = -1;
		_currentSlotIndex = 0;
	}

	private void SetPossibleEventItemList()
	{
		FactoryBase.ReleaseAndClearList(_possibleEventItemIconCompList);
		UIHelper.EnableGameObject(NoEventItemInfoTransform, enable: true);
		List<EquipItemInfo> possibleEventItemInfoList = PossibleEventItemInfoList;
		if (__Extension_Method_ICollection__.IsNullOrEmpty(possibleEventItemInfoList))
		{
			return;
		}
		UIHelper.EnableGameObject(NoEventItemInfoTransform, possibleEventItemInfoList.Count <= 0);
		EquipItemInfo equipItemInfo = null;
		ItemIconComponent itemIconComponent = null;
		for (int i = 0; i < possibleEventItemInfoList.Count; i++)
		{
			equipItemInfo = possibleEventItemInfoList[i];
			if (equipItemInfo != null)
			{
				itemIconComponent = UIHelper.AddGridItem(PossibleEventItemScrollGrid, ItemIconComponent.PrefabAuto);
				if (!(itemIconComponent == null))
				{
					itemIconComponent.Open(equipItemInfo, this);
					itemIconComponent.SetSelected(equipItemInfo.SlotIndex == _currentSlotIndex);
					_possibleEventItemIconCompList.Add(itemIconComponent);
				}
			}
		}
		UIHelper.RepositionGrid(PossibleEventItemScrollGrid);
	}

	private void InitGetRewardListInfo()
	{
		_getPossibleReinforcementLevel = _currentReinforcementLevel;
		FirstGetRewardListInfo.Init(new GetRewardListInfo.Info
		{
			Group = _currentGroupIndex,
			ReinforcementLevel = _currentReinforcementLevel
		});
		SecondGetRewardListInfo.Init(new GetRewardListInfo.Info
		{
			Group = _currentGroupIndex,
			ReinforcementLevel = _currentReinforcementLevel
		});
		SetGetPossibleRewardList();
		InitGetPossibleRewardGuideComponentList();
	}

	private void SetGetPossibleRewardList()
	{
		FirstGetRewardListInfo.SetInfo(_getPossibleReinforcementLevel, GetMaxReinforcementLevel(_getPossibleReinforcementLevel));
		SecondGetRewardListInfo.SetInfo(NextReinforcementLevel(_getPossibleReinforcementLevel), GetMaxReinforcementLevel(NextReinforcementLevel(_getPossibleReinforcementLevel)));
		SetButtonState();
	}

	private void ReleaseGetPossibleRewardComponentList()
	{
		FactoryBase.ReleaseAndClearList(_getPossibleRewardGuideCompList);
	}

	private void InitGetPossibleRewardGuideComponentList()
	{
		ReleaseGetPossibleRewardComponentList();
		if (GetPossibleRewardGuideGrid == null)
		{
			return;
		}
		List<EventForgeData> dataListByGroup = GameRoot.Instance.EventForgeDataContainer.GetDataListByGroup(_currentGroupIndex);
		if (__Extension_Method_ICollection__.IsNullOrEmpty(dataListByGroup))
		{
			return;
		}
		dataListByGroup.Sort((EventForgeData data, EventForgeData compData) => data.EnchantLevel.CompareTo(compData.EnchantLevel));
		AddEventForgeGetRewardGuideComponent(0, isLast: false);
		EventForgeData eventForgeData = null;
		for (int num = 0; num < dataListByGroup.Count; num++)
		{
			eventForgeData = dataListByGroup[num];
			if (eventForgeData != null)
			{
				AddEventForgeGetRewardGuideComponent(eventForgeData.EnchantLevel, eventForgeData.EnchantLevel >= eventForgeData.MaxEnchantLevel);
			}
		}
		UIHelper.RepositionGrid(GetPossibleRewardGuideGrid);
	}

	private void AddEventForgeGetRewardGuideComponent(int level, bool isLast)
	{
		if (!(GetPossibleRewardGuideGrid == null))
		{
			EventForgeGetRewardGuideComponent eventForgeGetRewardGuideComponent = UIHelper.AddGridItem(GetPossibleRewardGuideGrid, EventForgeGetRewardGuideComponent.PrefabAuto);
			if (!(eventForgeGetRewardGuideComponent == null))
			{
				eventForgeGetRewardGuideComponent.Open(new EventForgeGetRewardGuideComponent.Info
				{
					ReinforcementLevel = level,
					CurrentReinforcementLevel = _currentReinforcementLevel,
					IsLast = isLast
				});
				_getPossibleRewardGuideCompList.Add(eventForgeGetRewardGuideComponent);
			}
		}
	}

	private void EnableSelectGuidePoint()
	{
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(_getPossibleRewardGuideCompList))
		{
			_getPossibleRewardGuideCompList.ForEach(delegate(EventForgeGetRewardGuideComponent component)
			{
				component.EnableSelectPoint(_getPossibleReinforcementLevel);
			});
		}
	}

	private void SetGetPossibleRewardInformState()
	{
		UIHelper.EnableGameObject(GetPossibleRewardRootTranform, _currentSlotIndex > 0);
		UIHelper.EnableGameObject(NoSelectEventItemInformRootTransform, _currentSlotIndex <= 0);
	}

	private void ReleaseEventItemIconComponent()
	{
		FactoryBase.SafeReleaseComponent(ref _eventItemIconComp);
	}

	private void SetEventItemIconComponent(bool isChangeItem)
	{
		if (isChangeItem)
		{
			ReleaseEventItemIconComponent();
		}
		EventForgeData eventForgeData = GetEventForgeData((_currentReinforcementLevel <= 0) ? 1 : _currentReinforcementLevel);
		if (eventForgeData == null)
		{
			return;
		}
		ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(eventForgeData.ItemCode);
		if (dataByCode == null || EventItemIconCompRootTransform == null)
		{
			return;
		}
		if (isChangeItem)
		{
			_eventItemIconComp = ItemIconComponent.PrefabAuto.Factory.Create(EventItemIconCompRootTransform);
			if (_eventItemIconComp == null)
			{
				return;
			}
		}
		_eventItemIconComp.Open(dataByCode, this);
		_eventItemIconComp.ResetPosition();
	}

	private void SetReinforcementPercentageLabel()
	{
		UIHelper.SetLabelText(SuccessPercentageLabel, "-");
		UIHelper.SetLabelText(FailPercentageLabel, "-");
		UIHelper.SetLabelText(BreakPercentageLabel, "-");
		EventForgeData eventForgeData = GetEventForgeData(NextReinforcementLevel(_currentReinforcementLevel));
		if (eventForgeData != null)
		{
			UIHelper.SetLabelText(SuccessPercentageLabel, GetPercentageStr(eventForgeData.SuccessPercentage));
			UIHelper.SetLabelText(FailPercentageLabel, GetPercentageStr(eventForgeData.FailPercentage));
			UIHelper.SetLabelText(BreakPercentageLabel, GetPercentageStr(eventForgeData.BreakPercentage));
		}
	}

	private void InitReinforcementMaterialItemInfo(bool isChangeItem)
	{
		ReinforcementMaterialItemInfo.Init(new ReinforcementMaterialItemInfo.Info
		{
			GroupIndex = _currentGroupIndex,
			ReinforcementLevel = NextReinforcementLevel(_currentReinforcementLevel),
			IsCompleteSuccessReinforcement = false,
			IsChangeItem = isChangeItem
		});
	}

	private void InitCompleteSuccessReinforcementMaterialItemInfo(bool isChangeItem)
	{
		CompleteSuccessReinforcementMaterialItemInfo.Init(new ReinforcementMaterialItemInfo.Info
		{
			GroupIndex = _currentGroupIndex,
			ReinforcementLevel = NextReinforcementLevel(_currentReinforcementLevel),
			IsCompleteSuccessReinforcement = true,
			IsChangeItem = isChangeItem
		});
	}

	private void InitEventItemReinforcementRequirePrice()
	{
		if (EventItemReinforcementRequirePrice != null)
		{
			EventItemReinforcementRequirePrice.Open(0L, 0L);
			SetReinforceItemRequirePrice();
		}
	}

	private void SetReinforceItemRequirePrice()
	{
		if (GlobalAutoBehaviour<AccountManager>.isValidInstance && GlobalAutoBehaviour<AccountManager>.instance.UserInfo != null)
		{
			EventItemReinforcementRequirePrice.SetPrice(RequirePrice, GlobalAutoBehaviour<AccountManager>.instance.UserInfo.Gold);
		}
	}

	private void SetButtonState()
	{
		UIHelper.EnableButton(PrevButton, CheckPrevGetPossibleRewardList, grayscaleIfDisable: true);
		UIHelper.EnableButton(NextButton, CheckNextGetPossibleRewardList, grayscaleIfDisable: true);
	}

	private void SetEventForgeButtonState()
	{
		UIHelper.EnableButton(ReinforcementButton, CheckPossibleReinforcement, grayscaleIfDisable: true);
		UIHelper.EnableButton(CompleteSuccessReinforcementButton, CheckPossibleCompleteSuccessReinforcement, grayscaleIfDisable: true);
		UIHelper.EnableButton(ExchangeButton, CheckPossibleExchange, grayscaleIfDisable: true);
	}

	private void ResultReinforceEventItem(NShared.EventForge.Response response)
	{
		if (response == null || !CheckIsEffect || ReinforcementEffectWidget == null)
		{
			return;
		}
		ReinforcementEffectWidget.Play(delegate
		{
			ReinforcementEffectWidget.Stop(immediatelyClear: false);
			if (response.Success)
			{
				UIHelper.PlaySound("UI2.EquipLevelUp1");
				if (SuccessReinforcementEffectWidget != null)
				{
					SuccessReinforcementEffectWidget.Play(delegate
					{
						SuccessReinforcementEffectWidget.Stop(immediatelyClear: false);
					});
				}
			}
			else
			{
				UIHelper.PlaySound("BlackSmith.Fail");
				if (FailReinforcementEffectWidget != null)
				{
					FailReinforcementEffectWidget.Play(delegate
					{
						FailReinforcementEffectWidget.Stop(immediatelyClear: false);
					});
				}
				ResultFail();
			}
		});
	}

	private void ResultSuccess(NShared.EventForge.Response response)
	{
		if (response == null || response.ResultEquipItem == null)
		{
			return;
		}
		_currentReinforcementLevel = response.ResultEquipItem.Level;
		_currentSlotIndex = response.ResultEquipItem.SlotIndex;
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(_possibleEventItemIconCompList))
		{
			_possibleEventItemIconCompList.ForEach(delegate(ItemIconComponent component)
			{
				if (component != null && component.EquipItemInfo != null && component.EquipItemInfo.SlotIndex == _currentSlotIndex)
				{
					component.Open(response.ResultEquipItem, this);
				}
			});
		}
		SetInfo(isChangeItem: false);
	}

	private void ResultFail()
	{
		SetInfo(isChangeItem: false);
	}

	private bool CheckIsBreak(NShared.EventForge.Response response)
	{
		if (response == null)
		{
			return false;
		}
		EventForgeData eventForgeData = GetEventForgeData(NextReinforcementLevel(_currentReinforcementLevel));
		if (eventForgeData == null)
		{
			return false;
		}
		if (!eventForgeData.IsHighEnchant)
		{
			return false;
		}
		return !response.Success;
	}

	private EventForgeData GetEventForgeData(int reinforcementLevel)
	{
		return GameRoot.Instance.EventForgeDataContainer.GetDataByLevel(_currentGroupIndex, reinforcementLevel);
	}

	private int GetMaxReinforcementLevel(int reinforcementLevel)
	{
		return GetEventForgeData(reinforcementLevel)?.MaxEnchantLevel ?? (-1);
	}

	private string GetPercentageStr(float percentage)
	{
		if (percentage <= 0f)
		{
			return "-";
		}
		return percentage + "%";
	}

	private int NextReinforcementLevel(int reinforcementLevel)
	{
		return reinforcementLevel + 1;
	}

	void ItemIconComponent.Listener.OnClick(ItemIconComponent itemIconComponent)
	{
		if (itemIconComponent == null)
		{
			return;
		}
		EquipItemInfo equipItemInfo = itemIconComponent.EquipItemInfo;
		if (equipItemInfo == null || equipItemInfo.ItemData == null)
		{
			return;
		}
		int groupByItemCode = GameRoot.Instance.EventForgeDataContainer.GetGroupByItemCode(equipItemInfo.ItemData.Code);
		if (groupByItemCode <= 0)
		{
			return;
		}
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(_possibleEventItemIconCompList))
		{
			_possibleEventItemIconCompList.ForEach(delegate(ItemIconComponent component)
			{
				if (component != null)
				{
					component.SetSelected(selected: false);
				}
			});
		}
		itemIconComponent.SetSelected(selected: true);
		_currentGroupIndex = groupByItemCode;
		_currentSlotIndex = equipItemInfo.SlotIndex;
		_currentReinforcementLevel = equipItemInfo.Level;
		SetInfo(isChangeItem: true);
	}

	public void OnClickProgressReinforcement()
	{
		if (_iListener == null)
		{
			return;
		}
		_iListener.ProgressReinforceItem(_currentSlotIndex, GetEventForgeData(_currentReinforcementLevel), delegate(NShared.EventForge.Response response)
		{
			ResultReinforceEventItem(response);
			if (CheckIsBreak(response))
			{
				InitCurrentInfo();
				SetPossibleEventItemList();
				SetInfo(isChangeItem: true);
			}
			else
			{
				ResultSuccess(response);
			}
		});
	}

	public void OnClickCompleteSuccessReinforcement()
	{
		if (_iListener != null)
		{
			_iListener.ProgressCompleteSuccessReinforceItem(_currentSlotIndex, GetEventForgeData(_currentReinforcementLevel), delegate(NShared.EventForge.Response response)
			{
				ResultSuccess(response);
			});
		}
	}

	public void OnClickExchangeRandomReward()
	{
		if (_iListener != null)
		{
			_iListener.ExchangeRandomReward(_currentSlotIndex, GetEventForgeData(_currentReinforcementLevel), delegate
			{
				InitCurrentInfo();
				SetPossibleEventItemList();
				SetInfo(isChangeItem: true);
			});
		}
	}

	public void OnClickReinforcementPercentageInfo()
	{
		if (_iListener != null)
		{
			_iListener.ShowReinforcementPercentageInfo();
		}
	}

	public void OnClickPrevGetPossibleReward()
	{
		if (CheckPrevGetPossibleRewardList)
		{
			_getPossibleReinforcementLevel--;
			SetGetPossibleRewardList();
			EnableSelectGuidePoint();
		}
	}

	public void OnClickNextGetPossibleReward()
	{
		if (CheckNextGetPossibleRewardList)
		{
			_getPossibleReinforcementLevel++;
			SetGetPossibleRewardList();
			EnableSelectGuidePoint();
		}
	}
}
