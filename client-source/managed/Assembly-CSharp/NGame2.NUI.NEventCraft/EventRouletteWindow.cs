using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

public class EventRouletteWindow : BaseCinemaWindow, EventRouletteComponent.IListener, IUserObserver<ItemInfo>, IUserObserver, IUserObserver<UserManager.UserStatInfo>, IUserObserver<NGame2.NAccount.UserPlayerCurrencyManager.ChangedCurrencyInfo>
{
	public interface IListener
	{
		void OnClickRun(EventRouletteData eventRouletteData, long rouletteCount);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/EventCraft/EventRouletteWindow";

	public UIBaseGrid ItemGrid;

	public UIInput CountInput;

	public UIButton MinButton;

	public UIButton MaxButton;

	public UIButton PlusButton;

	public UIButton MinusButton;

	public UIWidget TooltipRootWIdget;

	public UIButton ChangeButton;

	public float OpenRewardPopupWaitTime = 1f;

	private IListener _listener;

	private List<int> _compIndexList = new List<int>();

	private List<EventRouletteComponent> _compList = new List<EventRouletteComponent>();

	private EventRouletteComponent _selectedComp;

	private int _min = 1;

	private long _max = 99L;

	private long _currentCount = 1L;

	private bool _playingSuccessAction;

	bool IUserObserver.IsGlobal()
	{
		return false;
	}

	void IUserObserver<ItemInfo>.OnChanged(ItemInfo changedInfo)
	{
		if (changedInfo == null || _compList == null)
		{
			return;
		}
		ItemData changedData = GameRoot.Instance.ItemDataContainer.GetData(changedInfo.ItemIndex);
		if (changedData == null)
		{
			return;
		}
		_compList.ForEach(delegate(EventRouletteComponent e)
		{
			if (e.ItemData != null && e.ItemData == changedData)
			{
				e.Refresh();
				if (_selectedComp == e)
				{
					OnChangedComp();
				}
			}
		});
	}

	void IUserObserver<UserManager.UserStatInfo>.OnChanged(UserManager.UserStatInfo changedInfo)
	{
		if (changedInfo != null)
		{
			string statType = changedInfo.UserStatType.ToString();
			ChangeComponent(statType);
		}
	}

	void IUserObserver<NGame2.NAccount.UserPlayerCurrencyManager.ChangedCurrencyInfo>.OnChanged(NGame2.NAccount.UserPlayerCurrencyManager.ChangedCurrencyInfo changedInfo)
	{
		if (changedInfo != null)
		{
			string statType = changedInfo.currencyType.ToString();
			ChangeComponent(statType);
		}
	}

	public void Open(IListener listener)
	{
		base.OpenWithSceneChanger(delegate
		{
			_listener = listener;
			Init();
			GlobalAutoBehaviour<AccountManager>.instance.AddUserObserver(this);
		}, CheckRefreshProductMessage);
	}

	private void Init()
	{
		CreateCompList();
		SetSelectedComp(_compList.FirstOrDefault());
		_playingSuccessAction = false;
	}

	private void CreateCompList()
	{
		List<DropBonusEvent> effectiveEvents = GlobalAutoBehaviour<DropBonusEventManager>.instance.GetEffectiveEvents(EventBonusType.EventRoulette);
		if (__Extension_Method_ICollection__.IsNullOrEmpty(effectiveEvents))
		{
			VespaDebug.LogError("GetEffectiveEvent == null");
			return;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(effectiveEvents); i++)
		{
			DropBonusEvent dropBonusEvent = effectiveEvents[i];
			if (dropBonusEvent == null)
			{
				continue;
			}
			_ = dropBonusEvent.Condition;
			int[] eventContionValue = DropBonusEventManager.GetEventContionValue<int>(dropBonusEvent, "RouletteIndices");
			for (int j = 0; j < __Extension_Method_ICollection__.COUNT(eventContionValue); j++)
			{
				int key = eventContionValue[j];
				EventRouletteData data = GameRoot.Instance.EventRouletteDataContainer.GetData(key);
				if (data != null)
				{
					EventRouletteComponent eventRouletteComponent = UIHelper.AddGridItem(ItemGrid, EventRouletteComponent.PrefabAuto);
					eventRouletteComponent.Open(data, this);
					_compIndexList.Add(data.Index);
					_compList.Add(eventRouletteComponent);
				}
			}
		}
		UIHelper.RepositionGrid(ItemGrid);
	}

	private void SetSelectedComp(EventRouletteComponent comp)
	{
		if (!(comp == null) && !(comp == _selectedComp))
		{
			_selectedComp = comp;
			_compList.ForEach(delegate(EventRouletteComponent e)
			{
				e.SetSelected(_selectedComp == e);
			});
			OnChangedComp(resetCount: true);
		}
	}

	private void CheckRefreshProductMessage()
	{
		LocalAccountString localAccountString = new LocalAccountString(string.Empty, "_lastEventRouletteIndices");
		string value = Vespa_String_Extension_Method.ToStringJoin(_compIndexList, ", ");
		if (!String_Extension_Method.IsNullOrEmpty(localAccountString.Value) && !localAccountString.Value.Equals(value))
		{
			CenterMessage.Open(LocalizationManager.GetStringDirect("EVENT_OPHELIA_WORKSHOP_EVENT_ITEM_RENEWAL"));
		}
		localAccountString.Value = value;
	}

	private void ChangeComponent(string statType)
	{
		if (_compList == null)
		{
			return;
		}
		_compList.ForEach(delegate(EventRouletteComponent e)
		{
			if (e.CurrencyData != null && e.CurrencyData.UserStatType == statType)
			{
				e.Refresh();
				if (_selectedComp == e)
				{
					OnChangedComp();
				}
			}
		});
	}

	private void OnChangedComp(bool resetCount = false)
	{
		if (resetCount)
		{
			CountInput.value = _min.ToString();
		}
		EventRouletteData eventRouletteData = _selectedComp.EventRouletteData;
		RefreshMinMax(eventRouletteData);
		ValidateNumber();
	}

	private void RefreshMinMax(EventRouletteData rouletteData)
	{
		if (rouletteData == null)
		{
			return;
		}
		ResetCount();
		int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("EventRouletteMaxCount", 99);
		_max = valueInt;
		switch (rouletteData.Type)
		{
		case EventRouletteType.Currency:
		{
			CurrencyTypeData data = GameRoot.Instance.CurrencyTypeDataContainer.GetData(rouletteData.CurrencyType);
			if (data != null)
			{
				long num6 = GlobalAutoBehaviour<AccountManager>.instance.UserManager.GetUserStat(data.UserStatType) / rouletteData.CurrencyValue;
				_max = ((num6 >= _max) ? _max : num6);
				long max5 = _max;
				int num7 = 0;
				long max6;
				if (num7 < max5)
				{
					max6 = _max;
				}
				else
				{
					int num8 = 1;
					max6 = num8;
				}
				_max = max6;
			}
			break;
		}
		case EventRouletteType.Item:
		{
			ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(rouletteData.ItemCode);
			if (dataByCode != null)
			{
				int num = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemCount(dataByCode.Index) / rouletteData.ItemCount;
				long max = _max;
				int num2 = num;
				long max2;
				if (num2 < max)
				{
					int num3 = num;
					max2 = num3;
				}
				else
				{
					max2 = _max;
				}
				_max = max2;
				long max3 = _max;
				int num4 = 0;
				long max4;
				if (num4 < max3)
				{
					max4 = _max;
				}
				else
				{
					int num5 = 1;
					max4 = num5;
				}
				_max = max4;
			}
			break;
		}
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

	private void ValidateNumber()
	{
		int number = GetNumber();
		long number2 = number;
		SetNumber(number2);
	}

	private void SetNumber(long number)
	{
		int min = _min;
		long max = _max;
		int min2 = min;
		int num = checked(Mathf.Clamp((int)number, min2, (int)max));
		_currentCount = num;
		CountInput.value = _currentCount.ToString();
		UpdateButtonState();
		UpdateComp(_currentCount);
	}

	private void UpdateButtonState()
	{
		bool enable = _currentCount < _max;
		long currentCount = _currentCount;
		int min = _min;
		bool enable2 = currentCount > min;
		UIHelper.EnableButton(PlusButton, enable);
		UIHelper.EnableButton(MinusButton, enable2);
		UIButton minButton = MinButton;
		long currentCount2 = _currentCount;
		int min2 = _min;
		UIHelper.EnableButton(minButton, currentCount2 != min2);
		UIHelper.EnableButton(MaxButton, _currentCount != _max);
	}

	private void UpdateComp(long num)
	{
		_compList.ForEach(delegate(EventRouletteComponent e)
		{
			long number;
			if (!(_selectedComp == e))
			{
				int num2 = 1;
				number = num2;
			}
			else
			{
				number = num;
			}
			e.SetNumber(number);
		});
		bool enable = _selectedComp != null && _selectedComp.CanChange;
		UIHelper.EnableButton(ChangeButton, enable);
	}

	public override void Reset()
	{
		base.Reset();
		ResetCount();
		ReleaseComponent();
	}

	private void ResetCount()
	{
		_min = 1;
		int num = 99;
		_max = num;
		int num2 = 0;
		_currentCount = num2;
	}

	private void ReleaseComponent()
	{
		_selectedComp = null;
		if (_compIndexList != null)
		{
			_compIndexList.Clear();
		}
		FactoryBase.ReleaseAndClearList(_compList);
	}

	public override void OnSystemBackButton()
	{
		if (!_playingSuccessAction)
		{
			base.OnSystemBackButton();
		}
	}

	public override void OnClosing()
	{
		base.OnClosing();
		GlobalAutoBehaviour<AccountManager>.instance.RemoveUserObserver(this);
	}

	public void PlaySuccessAction(EventRouletteData eventRouletteData, Action onStart, Action onComplete)
	{
		AddTouchDisabler("PlaySuccessAction");
		_playingSuccessAction = true;
		PlayNodeTree("EventRouletteResultWindow", isOpen: true, delegate
		{
			Vespa_System_Action_Extension_Method.fire(onStart);
		}, null);
		Action onComplete2 = delegate
		{
			RemoveTouchDisabler("PlaySuccessAction");
			_playingSuccessAction = false;
			OnChangedComp(resetCount: true);
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

	public void OnClickPlus()
	{
		int number = GetNumber();
		int num = 1;
		int num2 = number;
		checked
		{
			SetNumber(unchecked((long)num2) + unchecked((long)num));
		}
	}

	public void OnClickMinus()
	{
		int number = GetNumber();
		int num = 1;
		int num2 = number;
		checked
		{
			SetNumber(unchecked((long)num2) - unchecked((long)num));
		}
	}

	public void OnClickMin()
	{
		int min = _min;
		SetNumber(min);
	}

	public void OnClickMax()
	{
		SetNumber(_max);
	}

	public void OnNumberSubmit()
	{
		ValidateNumber();
	}

	public void OnNumberChanged()
	{
		if (String_Extension_Method.IsNullOrEmpty(CountInput.value))
		{
			int num = 1;
			_currentCount = num;
			UpdateButtonState();
			UpdateComp(_currentCount);
		}
		else
		{
			ValidateNumber();
		}
	}

	public void OnClickChanger()
	{
		if (_listener != null)
		{
			EventRouletteData eventRouletteData = _selectedComp.EventRouletteData;
			if (eventRouletteData != null)
			{
				_listener.OnClickRun(eventRouletteData, _currentCount);
			}
		}
	}

	public void OnShowTooltip()
	{
		DescTooltip.Open(TooltipRootWIdget, new TooltipTextInfo
		{
			Desc = new StringProxy
			{
				Value = LocalizationManager.GetStringDirect("EVENT_WITCH_ROULETTE_DESC")
			},
			Name = new StringProxy
			{
				Value = string.Empty
			}
		});
	}

	public void OnHideTooltip()
	{
		DescTooltip.Hide(TooltipRootWIdget);
	}

	void EventRouletteComponent.IListener.OnClickEnter(EventRouletteComponent caller)
	{
		SetSelectedComp(caller);
	}
}
