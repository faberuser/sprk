using System.Collections.Generic;
using NGame2.NUI.NEventWorldTree;
using NGame2.NUI.NHelper.NEventGrowWorldTree;
using NGame2.NUI.NManager;
using NShared;
using NVespa;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class EventRewardComponent : LinkFactoryComponent, ItemIconComponent.Listener, CurrencyIconComponent.Listener
{
	public Transform IconRoot;

	public UISprite SpriteAlreadyRewarded;

	private int _depth;

	private bool _isCurrencyComponent;

	private bool _isButtonEnabled;

	private ItemIconComponent _itemIconComponent;

	private CurrencyIconComponent _currencyIconComponent;

	private EventStepType _eventStepType;

	private EventGrowWorldTreeWindow.IListener _listener;

	public EventStepType EventStepType
	{
		get
		{
			return _eventStepType;
		}
		set
		{
			_eventStepType = value;
		}
	}

	public EventGrowWorldTreeWindow.IListener Listener
	{
		get
		{
			return _listener;
		}
		set
		{
			_listener = value;
		}
	}

	public bool IsButtonEnabled
	{
		get
		{
			return _isButtonEnabled;
		}
		set
		{
			_isButtonEnabled = value;
		}
	}

	public bool IsCurrencyComponent
	{
		get
		{
			return _isCurrencyComponent;
		}
		set
		{
			_isCurrencyComponent = value;
		}
	}

	public void Open(EventStepType eventStepType, int rewardIndex, int depth, EventGrowWorldTreeWindow.IListener listener = null)
	{
		_eventStepType = eventStepType;
		_depth = depth;
		Listener = listener;
		SetRewardData(rewardIndex);
	}

	public void SetRewardData(int rewardIndex)
	{
		RewardData data = GameRoot.Instance.RewardDataContainer.GetData(rewardIndex);
		if (data == null)
		{
			VespaDebug.LogError($"[{GetType()}] RewardData is Null!");
			return;
		}
		List<RewardElementData> rewardItemList = data.GetRewardItemList();
		if (rewardItemList == null)
		{
			VespaDebug.LogError($"[{GetType()}] Reward List Data is Null!");
		}
		SetItemData(rewardItemList[0]);
	}

	public void SetEnabled(bool isEnabled, bool isAlready)
	{
		if (IsCurrencyComponent)
		{
			_currencyIconComponent.SetIconGrayScale(!isEnabled);
		}
		else
		{
			_itemIconComponent.SetIconGrayScale(!isEnabled);
		}
		_isButtonEnabled = isEnabled && isAlready;
		UIHelper.EnableGameObject(SpriteAlreadyRewarded, _isButtonEnabled);
	}

	private void SetItemData(RewardElementData rewardData)
	{
		if (rewardData == null)
		{
			VespaDebug.LogError($"[{GetType()}] RewardData is Null!");
		}
		else if (GameRoot.Instance.ItemDataContainer != null)
		{
			bool flag = rewardData.GetCurrencyType() != CurrencyType.None;
			SetActiveIconComponent(flag);
			if (flag)
			{
				OpenCurrencyIconComponent(rewardData);
			}
			else
			{
				OpenItemIconComponent(rewardData);
			}
		}
	}

	public void Release()
	{
		FactoryBase.SafeReleaseComponent(ref _currencyIconComponent);
		FactoryBase.SafeReleaseComponent(ref _itemIconComponent);
	}

	private void OpenItemIconComponent(RewardElementData rewardData)
	{
		ItemDataContainer itemDataContainer = GameRoot.Instance.ItemDataContainer;
		if (itemDataContainer != null)
		{
			if (_itemIconComponent == null)
			{
				_itemIconComponent = ItemIconComponent.PrefabAuto.Factory.Create(IconRoot);
			}
			ItemData dataByCode = itemDataContainer.GetDataByCode(rewardData.ItemCode);
			_itemIconComponent.Open(dataByCode, this);
			ItemIconComponent itemIconComponent = _itemIconComponent;
			int itemMax = rewardData.ItemMax;
			Color? color = null;
			int num = itemMax;
			itemIconComponent.SetItemCount(num, color);
			_itemIconComponent.WidgetThis.depth = _depth;
			Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_itemIconComponent.transform);
		}
	}

	void ItemIconComponent.Listener.OnClick(ItemIconComponent comp)
	{
		if (_isButtonEnabled)
		{
			OnClickMissionReward();
		}
	}

	private void OpenCurrencyIconComponent(RewardElementData rewardData)
	{
		if (_currencyIconComponent == null)
		{
			_currencyIconComponent = CurrencyIconComponent.PrefabAuto.Factory.Create(IconRoot);
		}
		CurrencyType currencyType = rewardData.GetCurrencyType();
		CurrencyIconComponent currencyIconComponent = _currencyIconComponent;
		int itemMax = rewardData.ItemMax;
		currencyIconComponent.Open(currencyType, itemMax, this);
		_currencyIconComponent.WidgetThis.depth = _depth;
		Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_currencyIconComponent.transform);
	}

	void CurrencyIconComponent.Listener.OnClick(CurrencyIconComponent component)
	{
		if (_isButtonEnabled)
		{
			OnClickMissionReward();
		}
	}

	private void OnClickMissionReward()
	{
		EventGrowWorldTreeManagement instance = SceneAutoBehaviour<EventGrowWorldTreeManagement>.instance;
		if (instance == null)
		{
			return;
		}
		EventGrowWorldTreeWindow window = instance.GetInstance<EventGrowWorldTreeWindow>();
		if (window == null)
		{
			return;
		}
		if (!EventGrowWorldTreeHelper.IsOpenContents())
		{
			EventGrowWorldTreeHelper.ForceCloseEventWindow("", delegate
			{
				window.OnSystemBackButton();
			});
		}
		else if (Listener != null)
		{
			Listener.OnClickMissionReward(_eventStepType, delegate
			{
				window.Refresh();
			});
		}
	}

	private void SetActiveIconComponent(bool isCurrency)
	{
		IsCurrencyComponent = isCurrency;
		if (_itemIconComponent != null)
		{
			UIHelper.EnableGameObject(_itemIconComponent, !isCurrency);
		}
		if (_currencyIconComponent != null)
		{
			UIHelper.EnableGameObject(_currencyIconComponent, isCurrency);
		}
	}
}
