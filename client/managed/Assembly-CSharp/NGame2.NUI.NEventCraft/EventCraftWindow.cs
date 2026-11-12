using System;
using System.Collections;
using System.Collections.Generic;
using LitJson;
using NGame2.NAccount;
using NGame2.NDropBonusEvent;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NWindow;
using NGame2.NUtil;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NEventCraft;

public class EventCraftWindow : BaseCinemaWindow, ItemIconComponent.Listener, IUserObserver<ItemInfo>, IUserObserver
{
	public interface IListener
	{
		void OnCraft(EventCraftData eventCraftData, int craftCount);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/EventCraft/EventCraftWindow";

	private List<EventCraftData> _sortedEventCraftDataList = new List<EventCraftData>();

	private List<ItemIconComponent> _itemIconComponentList = new List<ItemIconComponent>();

	private List<EventCraftMaterialComponent> _materialItemIconComponentList = new List<EventCraftMaterialComponent>();

	public UIScrollGrid RewardItemGrid;

	public UIScrollGrid MaterialGrid;

	public UIButton ButtonPlus;

	public UIButton ButtonMinus;

	public UIButton ButtonMax;

	public UIButton ButtonMin;

	public UIButton ButtonCraft;

	public UIButton ButtonPrev;

	public UIButton ButtonNext;

	public UIInput CountInput;

	private int _currentCount;

	public UILabel LabelCraftCount;

	public UIWidget TooltipRootWIdget1;

	public UIWidget TooltipRootWIdget2;

	public float OpenRewardPopupWaitTime = 1f;

	private EventCraftData _selectedEventCraftData;

	private ItemIconComponent _selectedItemIconComponent;

	private int _canMaxCount;

	private int _canMinCount;

	private int _remainCraftCount;

	private int _craftMaxCount;

	private int _constantMaxCount;

	private readonly int _maxPerPage = 8;

	private int _currPage;

	private int _maxPage;

	private bool _playingSuccessAction;

	public static LocalStorageString _lastEventCraftCondition = new LocalStorageString("_lastEventCraftCondition");

	private IListener _listener;

	public void Open(IListener listener)
	{
		base.OpenWithSceneChanger(delegate
		{
			_listener = listener;
			Init();
		}, CheckRefreshProductMessage);
	}

	private void Init()
	{
		GlobalAutoBehaviour<AccountManager>.instance.AddUserObserver(this);
		_constantMaxCount = GameRoot.Instance.ConstantDataContainer.GetValueInt("EventCraftMaxCount");
		List<DropBonusEvent> allEffectiveEvent = GlobalAutoBehaviour<DropBonusEventManager>.instance.GetAllEffectiveEvent(EventBonusType.EventCraft);
		if (__Extension_Method_ICollection__.IsNullOrEmpty(allEffectiveEvent))
		{
			VespaDebug.LogError("DropBonusEvent : EventCraft is turned off.");
			return;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(allEffectiveEvent); i++)
		{
			JsonData jsonData = JsonMapper.ToObject(allEffectiveEvent[i].Condition);
			if (jsonData == null)
			{
				VespaDebug.LogError("DropBonusEvent Condition is null.");
				continue;
			}
			JsonData jsonData2 = jsonData["ShowGroups"];
			if (jsonData2 == null)
			{
				VespaDebug.LogError("DropBonusEvent Condition ShowGroups is null.");
				continue;
			}
			for (int j = 0; j < jsonData2.Count; j++)
			{
				JsonData fromArray = jsonData2.GetFromArray(j);
				if (fromArray != null)
				{
					IList<EventCraftData> datasByGroup = GameRoot.Instance.EventCraftDataContainer.GetDatasByGroup(fromArray.valueStr);
					for (int k = 0; k < __Extension_Method_ICollection__.COUNT(datasByGroup); k++)
					{
						_sortedEventCraftDataList.Add(datasByGroup[k]);
					}
				}
			}
		}
		_sortedEventCraftDataList.Sort(delegate(EventCraftData lhs, EventCraftData rhs)
		{
			int num = lhs.ShowIndex.CompareTo(rhs.ShowIndex);
			return (num == 0) ? lhs.Index.CompareTo(rhs.Index) : num;
		});
		InitPage();
		UpdatePage();
		_playingSuccessAction = false;
	}

	private void CheckRefreshProductMessage()
	{
		string value = Vespa_String_Extension_Method.ToStringJoin(_sortedEventCraftDataList, ", ");
		if (!String_Extension_Method.IsNullOrEmpty(_lastEventCraftCondition.Value) && !_lastEventCraftCondition.Value.Equals(value))
		{
			CenterMessage.Open(LocalizationManager.GetStringDirect("EVENT_OPHELIA_WORKSHOP_EVENT_ITEM_RENEWAL"));
		}
		_lastEventCraftCondition.Value = value;
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}

	void IUserObserver<ItemInfo>.OnChanged(ItemInfo changedInfo)
	{
		if (changedInfo == null || _materialItemIconComponentList == null)
		{
			return;
		}
		_materialItemIconComponentList.ForEach(delegate(EventCraftMaterialComponent e)
		{
			int currentItemIndex = e.GetCurrentItemIndex();
			string currentItemCode = e.GetCurrentItemCode();
			if (currentItemIndex == changedInfo.ItemIndex)
			{
				int reqCount = 0;
				if (currentItemCode.Equals(_selectedEventCraftData.MaterialItemCode1))
				{
					reqCount = _selectedEventCraftData.MaterialItemCount1;
				}
				else if (currentItemCode.Equals(_selectedEventCraftData.MaterialItemCode2))
				{
					reqCount = _selectedEventCraftData.MaterialItemCount2;
				}
				else if (currentItemCode.Equals(_selectedEventCraftData.MaterialItemCode3))
				{
					reqCount = _selectedEventCraftData.MaterialItemCount3;
				}
				else if (currentItemCode.Equals(_selectedEventCraftData.MaterialItemCode4))
				{
					reqCount = _selectedEventCraftData.MaterialItemCount4;
				}
				else if (currentItemCode.Equals(_selectedEventCraftData.MaterialItemCode5))
				{
					reqCount = _selectedEventCraftData.MaterialItemCount5;
				}
				else if (currentItemCode.Equals(_selectedEventCraftData.MaterialItemCode6))
				{
					reqCount = _selectedEventCraftData.MaterialItemCount6;
				}
				else if (currentItemCode.Equals(_selectedEventCraftData.MaterialItemCode7))
				{
					reqCount = _selectedEventCraftData.MaterialItemCount7;
				}
				else if (currentItemCode.Equals(_selectedEventCraftData.MaterialItemCode8))
				{
					reqCount = _selectedEventCraftData.MaterialItemCount8;
				}
				e.RefreshItemCount(reqCount, changedInfo.Count);
			}
		});
		UpdateMaterial(_selectedEventCraftData);
	}

	void ItemIconComponent.Listener.OnClick(ItemIconComponent itemIconComponent)
	{
		_selectedEventCraftData = UpdateSelect(itemIconComponent);
		UpdateMaterial(_selectedEventCraftData);
	}

	private void UpdateMaterial(EventCraftData selectedEventCraftData)
	{
		if (selectedEventCraftData != null)
		{
			FactoryBase.ReleaseAndClearList(_materialItemIconComponentList);
			_canMaxCount = 0;
			_currentCount = 0;
			SetCraftCount(selectedEventCraftData);
			CreateMaterialItemIconComponent(selectedEventCraftData.MaterialItemCode1, selectedEventCraftData.MaterialItemCount1, isFirstMaterial: true);
			CreateMaterialItemIconComponent(selectedEventCraftData.MaterialItemCode2, selectedEventCraftData.MaterialItemCount2);
			CreateMaterialItemIconComponent(selectedEventCraftData.MaterialItemCode3, selectedEventCraftData.MaterialItemCount3);
			CreateMaterialItemIconComponent(selectedEventCraftData.MaterialItemCode4, selectedEventCraftData.MaterialItemCount4);
			CreateMaterialItemIconComponent(selectedEventCraftData.MaterialItemCode5, selectedEventCraftData.MaterialItemCount5);
			CreateMaterialItemIconComponent(selectedEventCraftData.MaterialItemCode6, selectedEventCraftData.MaterialItemCount6);
			CreateMaterialItemIconComponent(selectedEventCraftData.MaterialItemCode7, selectedEventCraftData.MaterialItemCount7);
			CreateMaterialItemIconComponent(selectedEventCraftData.MaterialItemCode8, selectedEventCraftData.MaterialItemCount8);
			UIHelper.RepositionGrid(MaterialGrid);
			UpdateButtonState();
			UpdateMin();
			ValidateNumber();
		}
	}

	private void SetCraftCount(EventCraftData eventCraftData)
	{
		if (eventCraftData == null)
		{
			_craftMaxCount = 0;
			UIHelper.SetLabelText(LabelCraftCount, "");
			return;
		}
		_craftMaxCount = eventCraftData.CraftCount;
		int num = GlobalAutoBehaviour<AccountManager>.instance.EventCraftManager.Get(eventCraftData.Index)?.CraftCount ?? 0;
		_remainCraftCount = _craftMaxCount - num;
		string stringDirect = LocalizationManager.GetStringDirect("CRAFT_AVAILABLE_COUNT", _remainCraftCount, _craftMaxCount);
		string text = ((_craftMaxCount == 0) ? "" : stringDirect);
		UIHelper.SetLabelText(LabelCraftCount, text);
	}

	private void UpdateMin()
	{
		_canMinCount = ((_canMaxCount > 0) ? 1 : 0);
		_currentCount = _canMinCount;
		SetNumber(_currentCount);
		UpdateMaterialCount();
	}

	private EventCraftData UpdateSelect(ItemIconComponent itemIconComponent)
	{
		if (_selectedItemIconComponent != null)
		{
			_selectedItemIconComponent.SetSelected(selected: false);
		}
		_selectedItemIconComponent = itemIconComponent;
		itemIconComponent.SetSelected(selected: true);
		string itemCode = itemIconComponent.ItemData.Code;
		int subKey = itemIconComponent.SubKey;
		if (itemIconComponent.SubKey != 0)
		{
			return _sortedEventCraftDataList.Find((EventCraftData e) => e.Index == subKey);
		}
		return _sortedEventCraftDataList.Find((EventCraftData e) => e.ItemCode == itemCode);
	}

	private void CreateMaterialItemIconComponent(string materialItemCode, int reqCount, bool isFirstMaterial = false)
	{
		if (String_Extension_Method.IsNullOrEmpty(materialItemCode))
		{
			return;
		}
		ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(materialItemCode);
		if (dataByCode != null)
		{
			int itemCount = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemCount(dataByCode.Code);
			EventCraftMaterialComponent eventCraftMaterialComponent = UIHelper.AddGridItem(MaterialGrid, EventCraftMaterialComponent.PrefabAuto);
			eventCraftMaterialComponent.Open(dataByCode, reqCount, itemCount);
			_materialItemIconComponentList.Add(eventCraftMaterialComponent);
			int num = itemCount / reqCount;
			if (isFirstMaterial)
			{
				_canMaxCount = num;
			}
			else if (num < _canMaxCount)
			{
				_canMaxCount = num;
			}
			int max = ((_craftMaxCount != 0) ? _remainCraftCount : _constantMaxCount);
			_canMaxCount = Mathf.Clamp(_canMaxCount, 0, max);
		}
	}

	public int GetNumber()
	{
		int result = 0;
		string value = CountInput.value;
		if (!string.IsNullOrEmpty(value))
		{
			int.TryParse(value, out result);
		}
		return result;
	}

	public void OnNumberChanged()
	{
		if (String_Extension_Method.IsNullOrEmpty(CountInput.value))
		{
			_currentCount = 0;
		}
		else
		{
			ValidateNumber();
		}
	}

	public void OnNumberSubmit()
	{
		ValidateNumber();
	}

	private void ValidateNumber()
	{
		int number = GetNumber();
		SetNumber(number);
		UpdateMaterialCount();
	}

	private void SetNumber(int number)
	{
		_currentCount = Mathf.Clamp(number, _canMinCount, _canMaxCount);
		int num = ((_currentCount <= 0) ? 1 : _currentCount);
		CountInput.value = num.ToString();
		UpdateButtonState();
	}

	private void UpdateMaterialCount()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_materialItemIconComponentList); i++)
		{
			EventCraftMaterialComponent eventCraftMaterialComponent = _materialItemIconComponentList[i];
			int num = ((_currentCount <= 0) ? 1 : _currentCount);
			eventCraftMaterialComponent.RefreshItemCount(eventCraftMaterialComponent.ReqCount * num, eventCraftMaterialComponent.CurCount);
		}
	}

	private void UpdateButtonState()
	{
		InternalUpdateButtonState(calPlus: true, canMinus: true);
		if (_canMaxCount <= 0)
		{
			InternalUpdateButtonState(calPlus: false, canMinus: false);
			return;
		}
		bool calPlus = _currentCount < _canMaxCount;
		bool canMinus = _currentCount > _canMinCount;
		InternalUpdateButtonState(calPlus, canMinus);
	}

	private void InternalUpdateButtonState(bool calPlus, bool canMinus)
	{
		UIHelper.EnableButton(ButtonPlus, calPlus);
		UIHelper.EnableButton(ButtonMax, calPlus);
		UIHelper.EnableButton(ButtonMinus, canMinus);
		UIHelper.EnableButton(ButtonMin, canMinus);
		UIHelper.EnableButton(ButtonCraft, _currentCount > 0, grayscaleIfDisable: true);
	}

	private void InitPage()
	{
		_currPage = 0;
		_maxPage = Mathf.Max((__Extension_Method_ICollection__.COUNT(_sortedEventCraftDataList) - 1) / _maxPerPage, 0);
		SetCraftCount(null);
	}

	private void UpdatePage()
	{
		FactoryBase.ReleaseAndClearList(_itemIconComponentList);
		int a = __Extension_Method_ICollection__.COUNT(_sortedEventCraftDataList);
		int num = _currPage * _maxPerPage;
		int num2 = Mathf.Min(a, num + _maxPerPage);
		for (int i = num; i < num2; i++)
		{
			EventCraftData eventCraftData = _sortedEventCraftDataList[i];
			ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(eventCraftData.ItemCode);
			if (dataByCode == null)
			{
				VespaDebug.LogError("itemData is null");
				return;
			}
			ItemIconComponent itemIconComponent = UIHelper.AddGridItem(RewardItemGrid, ItemIconComponent.PrefabAuto);
			itemIconComponent.Open(dataByCode, this);
			itemIconComponent.SetSubKey(eventCraftData.Index);
			if (eventCraftData.CraftCount != 0)
			{
				int num3 = GlobalAutoBehaviour<AccountManager>.instance.EventCraftManager.Get(eventCraftData.Index)?.CraftCount ?? 0;
				int num4 = eventCraftData.CraftCount - num3;
				itemIconComponent.SetIconGrayScale(num4 <= 0);
				itemIconComponent.SetItemCount_req(num4, eventCraftData.CraftCount);
			}
			_itemIconComponentList.Add(itemIconComponent);
		}
		UIHelper.RepositionGrid(RewardItemGrid);
		UpdatePageButtonState();
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(_itemIconComponentList))
		{
			_selectedEventCraftData = UpdateSelect(_itemIconComponentList[0]);
			_craftMaxCount = _selectedEventCraftData.CraftCount;
			UpdateMaterial(_selectedEventCraftData);
		}
	}

	private void UpdatePageButtonState()
	{
		bool enable = _currPage < _maxPage;
		bool enable2 = _currPage > 0;
		UIHelper.EnableButton(ButtonPrev, enable2);
		UIHelper.EnableButton(ButtonNext, enable);
	}

	public void UpdateCraftCount(EventCraftData eventCraftData)
	{
		if (eventCraftData != null && eventCraftData.CraftCount != 0)
		{
			ItemIconComponent itemIconComponent = _itemIconComponentList.Find((ItemIconComponent comp) => (comp.SubKey != 0 && comp.SubKey == eventCraftData.Index) ? ((bool)comp) : (comp.ItemData.Code == eventCraftData.ItemCode));
			if (!(itemIconComponent == null))
			{
				int num = GlobalAutoBehaviour<AccountManager>.instance.EventCraftManager.Get(eventCraftData.Index)?.CraftCount ?? 0;
				int num2 = eventCraftData.CraftCount - num;
				itemIconComponent.SetIconGrayScale(num2 <= 0);
				itemIconComponent.SetItemCount_req(num2, eventCraftData.CraftCount);
				itemIconComponent.OnClickIcon();
			}
		}
	}

	public void OnClickPlus()
	{
		if (_currentCount < _canMaxCount)
		{
			SetNumber(GetNumber() + 1);
			UpdateMaterialCount();
		}
	}

	public void OnClickMinus()
	{
		if (_currentCount > _canMinCount)
		{
			SetNumber(GetNumber() - 1);
			UpdateMaterialCount();
		}
	}

	public void OnClickMax()
	{
		SetNumber(_canMaxCount);
		UpdateMaterialCount();
	}

	public void OnClickMin()
	{
		UpdateMin();
	}

	public void OnClickPrev()
	{
		_currPage = Mathf.Clamp(--_currPage, 0, _maxPage);
		UpdatePage();
	}

	public void OnClickNext()
	{
		_currPage = Mathf.Clamp(++_currPage, 0, _maxPage);
		UpdatePage();
	}

	public void OnGo()
	{
		if (_currentCount > 0 && _selectedEventCraftData != null)
		{
			_listener.OnCraft(_selectedEventCraftData, _currentCount);
		}
	}

	public void OnShowRecipeTooltip()
	{
		DescTooltip.Open(TooltipRootWIdget1, new TooltipTextInfo
		{
			Desc = new StringProxy
			{
				Value = LocalizationManager.GetStringDirect("EVENT_WITCH_CRAFT_ITEM_DESC")
			},
			Name = new StringProxy
			{
				Value = string.Empty
			}
		});
	}

	public void OnHideRecipeTooltip()
	{
		DescTooltip.Hide(TooltipRootWIdget1);
	}

	public void OnShowMaterialTooltip()
	{
		DescTooltip.Open(TooltipRootWIdget2, new TooltipTextInfo
		{
			Desc = new StringProxy
			{
				Value = LocalizationManager.GetStringDirect("EVENT_WITCH_CRAFT_MATERIAL_DESC")
			},
			Name = new StringProxy
			{
				Value = string.Empty
			}
		});
	}

	public void OnHideMaterialTooltip()
	{
		DescTooltip.Hide(TooltipRootWIdget2);
	}

	public void PlaySuccessAction(bool isSpecialResult, Action onStart, Action onComplete)
	{
		AddTouchDisabler("PlaySuccessAction");
		_playingSuccessAction = true;
		string cinemaName = (isSpecialResult ? "EventCraftSpecialResultWindow" : "EventCraftNormalResultWindow");
		PlayNodeTree(cinemaName, isOpen: true, delegate
		{
			Vespa_System_Action_Extension_Method.fire(onStart);
		}, null);
		Action onComplete2 = delegate
		{
			RemoveTouchDisabler("PlaySuccessAction");
			_playingSuccessAction = false;
			Vespa_System_Action_Extension_Method.fire(onComplete);
		};
		VespaCoroutine.Start(this, coPlayCustomComplete(OpenRewardPopupWaitTime, onComplete2));
	}

	private IEnumerator coPlayCustomComplete(float customCompleteTime, Action onComplete)
	{
		if (customCompleteTime > 0f)
		{
			yield return new WaitForSeconds(customCompleteTime);
			Vespa_System_Action_Extension_Method.fire(onComplete);
		}
		else
		{
			Vespa_System_Action_Extension_Method.fire(onComplete);
		}
	}

	public override void OnSystemBackButton()
	{
		if (!_playingSuccessAction)
		{
			base.OnSystemBackButton();
		}
	}

	public override void Close()
	{
		base.Close();
		_sortedEventCraftDataList.Clear();
		FactoryBase.ReleaseAndClearList(_itemIconComponentList);
		FactoryBase.ReleaseAndClearList(_materialItemIconComponentList);
		_currentCount = 0;
		_selectedEventCraftData = null;
		_canMaxCount = 0;
		_canMinCount = 0;
		_currPage = 0;
		_maxPage = 0;
		_constantMaxCount = 0;
		_craftMaxCount = 0;
	}
}
