using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NEventTrigger;
using NGame2.NUI.NComponent2.NPet;
using NGame2.NUI.NHelper.NPet;
using NGame2.NUI.NUIEffect;
using NShared;
using NShared.NEvent;
using NVespa;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.NPet;

public class PetFeedPopup : BaseWindow, IUserObserver<UserManager.UserStatInfo>, IUserObserver, PetFeedComponent.IListener
{
	public interface IListener
	{
		void RequestFeedingPet(PetInfo petInfo, int amount, Action<bool> callBack);

		void OnClickBackButton();
	}

	public static string PrefabPath = "Prefab2/Window/Pet/Popup/PetFeedPopup";

	public static PrefabAuto<PetFeedPopup> PrefabAuto = new PrefabAuto<PetFeedPopup>(PrefabPath);

	[Header("[Store Food]")]
	public UILabel FoodAmountLabel;

	public UILabel FoodRechargeTimelabel;

	[Header("[Pet List]")]
	public UIScrollView PetListScrollView;

	public UIGrid PetListGrid;

	public UILabel PetEmptyLabel;

	[Header("[Particle Reorder]")]
	public UIPanel PetFeedPanel;

	public ParticleResize ParticleResize;

	private IListener _listener;

	private List<PetFeedComponent> _petFeedComponents = new List<PetFeedComponent>();

	private long _feedAmount;

	private long _maxFoodAmount;

	private int _remainSec;

	private float _lastCheckedRemainTime;

	public void Open(IListener listener)
	{
		base.Open();
		_listener = listener;
		Init();
	}

	private void Init()
	{
		_feedAmount = GlobalAutoBehaviour<AccountManager>.instance.PetManager.RemainPetFoodCount;
		_maxFoodAmount = GlobalAutoBehaviour<AccountManager>.instance.PetManager.MaxPetFoodCount;
		SetCurrenctStoreFood();
		GenerateComponents();
		OpenRaiderFeedPopupUI triggerEvent = new OpenRaiderFeedPopupUI();
		SingletonAuto<EventTriggerManager>.instance.RaiseEvent(triggerEvent);
		if (ParticleResize != null)
		{
			ParticleResize.Resize(PetFeedPanel);
		}
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		if (Time.time - _lastCheckedRemainTime >= 1f)
		{
			UpdateFoodRechargeTime();
		}
	}

	public override void Close()
	{
		base.Close();
		if (_listener != null)
		{
			_listener.OnClickBackButton();
		}
		_listener = null;
		FactoryBase.ReleaseAndClearList(_petFeedComponents);
	}

	private void SetCurrenctStoreFood()
	{
		long maxFoodAmount = _maxFoodAmount;
		int num = 0;
		if (maxFoodAmount <= num)
		{
			UIHelper.SetLabelText(FoodAmountLabel, string.Empty);
			UIHelper.EnableGameObject(FoodAmountLabel, enable: false);
		}
		else
		{
			string text = $"{_feedAmount}/{_maxFoodAmount}";
			UIHelper.SetLabelText(FoodAmountLabel, text);
			UIHelper.EnableGameObject(FoodAmountLabel, enable: true);
		}
		_remainSec = 0;
		UpdateFoodRechargeTime();
	}

	private void UpdateFoodRechargeTime()
	{
		if (GlobalAutoBehaviour<AccountManager>.instance.PetManager.NotHavePet)
		{
			UIHelper.EnableGameObject(FoodRechargeTimelabel, enable: false);
			return;
		}
		if (_feedAmount >= _maxFoodAmount)
		{
			long maxFoodAmount = _maxFoodAmount;
			int num = 0;
			if (maxFoodAmount > num)
			{
				UIHelper.EnableGameObject(FoodRechargeTimelabel, enable: false);
				return;
			}
		}
		TimeSpan? foodRechargeRemainTime = PetHelper.GetFoodRechargeRemainTime();
		if (!foodRechargeRemainTime.HasValue || foodRechargeRemainTime.Value.TotalSeconds <= 0.0)
		{
			_remainSec = 0;
		}
		else
		{
			_lastCheckedRemainTime = Time.time;
			_remainSec = (int)foodRechargeRemainTime.Value.TotalSeconds;
		}
		int remainSec = _remainSec;
		int showSec = 1;
		int showMin = 1;
		int showHour = 1;
		int showDay = 1;
		int num2 = remainSec;
		string remainTimeTextAll = UIHelper.GetRemainTimeTextAll(num2, (byte)showDay != 0, (byte)showHour != 0, (byte)showMin != 0, (byte)showSec != 0);
		string stringDirect = LocalizationManager.GetStringDirect("PET_FOOD_CHARGE_TIME_REMAIN", remainTimeTextAll);
		UIHelper.SetLabelText(FoodRechargeTimelabel, stringDirect);
		UIHelper.EnableGameObject(FoodRechargeTimelabel, enable: true);
	}

	private void GenerateComponents()
	{
		List<int> placePetIndexList = GlobalAutoBehaviour<AccountManager>.instance.PetManager.PlacePetIndexList;
		int num = 0;
		List<PetInfo> list = new List<PetInfo>();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(placePetIndexList); i++)
		{
			int key = placePetIndexList[i];
			PetInfo petInfo = GlobalAutoBehaviour<AccountManager>.instance.PetManager.Get(key);
			if (petInfo != null)
			{
				list.Add(petInfo);
			}
		}
		list.Sort(PetFeedComparer.Default);
		for (int j = 0; j < __Extension_Method_ICollection__.COUNT(list); j++)
		{
			PetInfo petInfo2 = list[j];
			if (petInfo2 != null)
			{
				PetFeedComponent petFeedComponent = PetFeedComponent.PrefabAuto.Factory.Create(PetListGrid.transform);
				if (!(petFeedComponent == null))
				{
					petFeedComponent.Open(petInfo2, this);
					num++;
					_petFeedComponents.Add(petFeedComponent);
				}
			}
		}
		UIHelper.EnableGameObject(PetEmptyLabel, num <= 0);
		PetListGrid.Reposition();
		PetListScrollView.ResetPosition();
	}

	private void RefreshPetFeedAmountPopup()
	{
		_feedAmount = GlobalAutoBehaviour<AccountManager>.instance.PetManager.RemainPetFoodCount;
		SetCurrenctStoreFood();
		foreach (PetFeedComponent petFeedComponent in _petFeedComponents)
		{
			petFeedComponent.RefreshPetInfo(_feedAmount);
			petFeedComponent.UpdatePetState();
			petFeedComponent.UpdateButtonState();
		}
	}

	void PetFeedComponent.IListener.OnClickPetFeed(PetInfo petInfo, long amount, Action<bool> callBack)
	{
		if (_listener != null && amount > 0)
		{
			_listener.RequestFeedingPet(petInfo, checked((int)amount), callBack);
		}
	}

	void PetFeedComponent.IListener.UpdatePetFeedAmountPopup()
	{
		RefreshPetFeedAmountPopup();
	}

	void PetFeedComponent.IListener.SetPetFeedAmount(long usedAmount)
	{
		_feedAmount = checked(_feedAmount - usedAmount);
		if (usedAmount == 0)
		{
			long feedAmount = _feedAmount;
			int num = 0;
			if (feedAmount < num)
			{
				_feedAmount = GlobalAutoBehaviour<AccountManager>.instance.PetManager.RemainPetFoodCount;
			}
		}
		SetCurrenctStoreFood();
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}

	void IUserObserver<UserManager.UserStatInfo>.OnChanged(UserManager.UserStatInfo changedInfo)
	{
		if (changedInfo.StaminaType == StaminaType.PetFeedKey)
		{
			RefreshPetFeedAmountPopup();
		}
	}
}
