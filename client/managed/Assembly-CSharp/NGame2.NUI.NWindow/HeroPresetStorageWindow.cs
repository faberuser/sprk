using System;
using System.Collections.Generic;
using System.Linq;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class HeroPresetStorageWindow : BaseWindow, HeroPresetStorageSlotComponent.IListener, HeroPresetNameChangePopup.IListener
{
	public interface IListener
	{
		void OnBuyHeroStorageSlot(string storageKey);

		void OnAddHeroStorageSlot(string storageKey);

		void OnRemoveHeroStoraeSlot(string storageKey);

		void OnChangeHeroStorageSlot(string staorageKey, string name);

		void OnSetHeroStorageSlot(string storageKey);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Hero/HeroPresetStorageWindow";

	public const int SlotSize = 5;

	public UILabel LabelPage;

	public GameObject GoEmptyRoot;

	public UILabel LabelEmptyDesc;

	public UIButton ButtonNameChange;

	public UIButton ButtonSave;

	public UIButton ButtonLoad;

	public UISimpleGrid SimpleGridSlot;

	public UIScrollGrid ScrollGridHeroPreset;

	private List<HeroPresetStorageSlotComponent> _listSlotComponent = new List<HeroPresetStorageSlotComponent>();

	private List<HeroPresetStorageComponent> _listHeroPresetComponent = new List<HeroPresetStorageComponent>();

	private int _totalSlotCount;

	private int _totalPageCount;

	private int _freeSlotCount;

	private int _paySlotCount;

	private float _loadDelaySec;

	private DateTime _lastLoadTime;

	private IListener _listener;

	public int CurrentPage;

	public int CurrentSlotNo;

	public void Open(IListener listener)
	{
		_listener = listener;
		Open(Init, delegate
		{
		});
	}

	public override void Close()
	{
		ReleaseHeroPresetComponent();
		ReleaseSlotComponent();
		base.Close();
	}

	private void Init()
	{
		InitValues();
		SetSlotComponents();
		SetSelectedSlot(resetHeroCompoenent: true);
		UpdateMenuButton();
	}

	private void InitValues()
	{
		_freeSlotCount = GameRoot.Instance.HeroPresetStorageSlotDataContainer.GetFreeSlotCount();
		_paySlotCount = GameRoot.Instance.HeroPresetStorageSlotDataContainer.GetPaySlotCount();
		_loadDelaySec = GameRoot.Instance.ConstantDataContainer.GetValueInt("HeroPresetRetryWaitTime", 1);
		_lastLoadTime = DateTime.Now.AddSeconds(0f - _loadDelaySec);
		_totalSlotCount = _freeSlotCount + _paySlotCount;
		_totalPageCount = _totalSlotCount / 5;
		SetSlotNo(1);
		SetPage(1);
	}

	public void SetHeroComponents(bool isPaidSlot, HeroPresetStorageInfo heroPresetStorageInfo)
	{
		ReleaseHeroPresetComponent();
		if (heroPresetStorageInfo == null || __Extension_Method_ICollection__.IsNullOrEmpty(heroPresetStorageInfo.HeroPresetInfos))
		{
			bool isOn = !isPaidSlot || heroPresetStorageInfo != null;
			SetEmptyOrUnSelectSlot(isOn, isEmpty: true);
			return;
		}
		SetEmptyOrUnSelectSlot(isOn: false, isEmpty: false);
		List<HeroPresetInfo> list = heroPresetStorageInfo.HeroPresetInfos.ToList();
		if (__Extension_Method_ICollection__.COUNT(list) > 1)
		{
			list.Sort(delegate(HeroPresetInfo lhs, HeroPresetInfo rhs)
			{
				CachedHeroInfo heroInfo = GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfo(lhs.HeroIndex);
				CachedHeroInfo heroInfo2 = GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfo(rhs.HeroIndex);
				if (heroInfo == null || heroInfo2 == null)
				{
					VespaDebug.LogError("[HeroPresetStorageWindow] HeroPresetCompare Error HeroInfo IsNull");
					return 0;
				}
				return HeroComparer.Default.Compare(heroInfo, heroInfo2);
			});
		}
		for (int num = 0; num < __Extension_Method_ICollection__.COUNT(list); num++)
		{
			HeroPresetInfo heroPresetInfo = list[num];
			if (heroPresetInfo != null)
			{
				HeroPresetStorageComponent heroPresetStorageComponent = UIHelper.AddGridItem(ScrollGridHeroPreset, HeroPresetStorageComponent.PrefabAuto);
				if (heroPresetStorageComponent == null)
				{
					break;
				}
				heroPresetStorageComponent.Open(heroPresetInfo);
				_listHeroPresetComponent.Add(heroPresetStorageComponent);
			}
		}
		UIHelper.RepositionGrid(ScrollGridHeroPreset);
	}

	private void ReleaseHeroPresetComponent()
	{
		FactoryBase.ReleaseAndClearList(_listHeroPresetComponent);
	}

	public void SetSlotComponents()
	{
		if (__Extension_Method_ICollection__.COUNT(_listSlotComponent) == 0)
		{
			for (int i = 0; i < 5; i++)
			{
				HeroPresetStorageSlotComponent heroPresetStorageSlotComponent = UIHelper.AddGridItem(SimpleGridSlot, HeroPresetStorageSlotComponent.PrefabAuto);
				if (heroPresetStorageSlotComponent == null)
				{
					break;
				}
				_listSlotComponent.Add(heroPresetStorageSlotComponent);
			}
		}
		for (int j = 0; j < __Extension_Method_ICollection__.COUNT(_listSlotComponent); j++)
		{
			HeroPresetStorageSlotComponent heroPresetStorageSlotComponent2 = _listSlotComponent[j];
			if (!(heroPresetStorageSlotComponent2 == null))
			{
				int key = GetSlotStartIndex(CurrentPage) + j + 1;
				HeroPresetStorageSlotData slotData = GetSlotData(key);
				if (slotData != null)
				{
					string storageKey = slotData.StorageKey;
					HeroPresetStorageInfo info = GlobalAutoBehaviour<AccountManager>.instance.HeroPresetStorageManager.Get(storageKey);
					heroPresetStorageSlotComponent2.Open(slotData, info, j, this);
				}
			}
		}
		UIHelper.RepositionGrid(SimpleGridSlot);
	}

	private void ReleaseSlotComponent()
	{
		FactoryBase.ReleaseAndClearList(_listSlotComponent);
	}

	private void SetSelectedSlot(bool resetHeroCompoenent)
	{
		GameRoot.Instance.HeroPresetStorageSlotDataContainer.GetPaySlotDataList();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_listSlotComponent); i++)
		{
			HeroPresetStorageSlotComponent heroPresetStorageSlotComponent = _listSlotComponent[i];
			if (heroPresetStorageSlotComponent == null)
			{
				continue;
			}
			int key = GetSlotStartIndex(CurrentPage) + i + 1;
			HeroPresetStorageSlotData slotData = GetSlotData(key);
			if (slotData != null)
			{
				string storageKey = slotData.StorageKey;
				HeroPresetStorageInfo heroPresetStorageInfo = GlobalAutoBehaviour<AccountManager>.instance.HeroPresetStorageManager.Get(storageKey);
				bool flag = CurrentSlotNo - 1 == i;
				heroPresetStorageSlotComponent.SetSlot(slotData, heroPresetStorageInfo);
				heroPresetStorageSlotComponent.SetSelected(flag);
				if (flag && resetHeroCompoenent)
				{
					bool isPaid = slotData.IsPaid;
					SetHeroComponents(isPaid, heroPresetStorageInfo);
				}
			}
		}
	}

	private void SetSlotNo(int slotNo)
	{
		CurrentSlotNo = slotNo;
	}

	private void UnselectSlot()
	{
		SetSlotNo(-1);
		SetSelectedSlot(resetHeroCompoenent: true);
		SetMenuButtonState(isAvailableSlot: false, canLoadSlot: false);
		SetEmptyOrUnSelectSlot(isOn: true, isEmpty: false);
	}

	private bool IsValidPage(int page)
	{
		if (page > 0)
		{
			return _totalPageCount >= page;
		}
		return false;
	}

	private void SetPage(int page)
	{
		CurrentPage = page;
		SetLabelPage(CurrentPage, _totalPageCount);
	}

	private void SetLabelPage(int currentPage, int totalPage)
	{
		UIHelper.SetLabelText(LabelPage, $"{currentPage}/{totalPage}");
	}

	private void ChangePage(int page)
	{
		if (IsValidPage(page))
		{
			ReleaseHeroPresetComponent();
			SetPage(page);
			UnselectSlot();
		}
	}

	private void SetEmptyOrUnSelectSlot(bool isOn, bool isEmpty)
	{
		UIHelper.EnableGameObject(GoEmptyRoot, isOn);
		if (isOn)
		{
			string stringDirect = LocalizationManager.GetStringDirect(isEmpty ? "EQUIP_PRESET_SLOT_EMPTY" : "EQUIP_PRESET_NOT_SELECTED");
			UIHelper.SetLabelText(LabelEmptyDesc, stringDirect);
		}
	}

	private void UpdateMenuButton()
	{
		int key = GetSlotStartIndex(CurrentPage) + CurrentSlotNo;
		HeroPresetStorageSlotData slotData = GetSlotData(key);
		if (slotData == null)
		{
			SetMenuButtonState(isAvailableSlot: false, canLoadSlot: false);
			return;
		}
		GetSlotState(slotData, out var isAvailableSlot, out var canLoadSlot);
		SetMenuButtonState(isAvailableSlot, canLoadSlot);
	}

	private void SetMenuButtonState(bool isAvailableSlot, bool canLoadSlot)
	{
		UIHelper.EnableButton(ButtonNameChange, isAvailableSlot && canLoadSlot, grayscaleIfDisable: true);
		UIHelper.EnableButton(ButtonSave, isAvailableSlot, grayscaleIfDisable: true);
		UIHelper.EnableButton(ButtonLoad, canLoadSlot, grayscaleIfDisable: true);
	}

	private void GetSlotState(HeroPresetStorageSlotData slotData, out bool isAvailableSlot, out bool canLoadSlot)
	{
		bool isPaid = slotData.IsPaid;
		string storageKey = slotData.StorageKey;
		HeroPresetStorageInfo heroPresetStorageInfo = GlobalAutoBehaviour<AccountManager>.instance.HeroPresetStorageManager.Get(storageKey);
		canLoadSlot = heroPresetStorageInfo != null && !__Extension_Method_ICollection__.IsNullOrEmpty(heroPresetStorageInfo.HeroPresetInfos);
		isAvailableSlot = !isPaid || (isPaid && heroPresetStorageInfo != null);
	}

	private string GetName(HeroPresetStorageSlotData data, HeroPresetStorageInfo info)
	{
		if (info != null && !String_Extension_Method.IsNullOrEmpty(info.Name))
		{
			return info.Name;
		}
		return LocalizationManager.GetStringDirect(data.NameKey) + data.Index;
	}

	private HeroPresetStorageSlotData GetSlotData(int key)
	{
		return GameRoot.Instance.HeroPresetStorageSlotDataContainer.GetData(key);
	}

	private string GetSlotStorageKey(int page, int index)
	{
		int slotStartIndex = GetSlotStartIndex(page);
		index += slotStartIndex;
		HeroPresetStorageSlotData slotData = GetSlotData(index);
		if (slotData == null)
		{
			return string.Empty;
		}
		return slotData.StorageKey;
	}

	private int GetSlotStartIndex(int page)
	{
		return Mathf.Max(0, page - 1) * 5;
	}

	public void OnAddSlot()
	{
		SetSelectedSlot(resetHeroCompoenent: true);
		UpdateMenuButton();
	}

	public void OnChangeSlotName()
	{
		SetSelectedSlot(resetHeroCompoenent: false);
	}

	void HeroPresetStorageSlotComponent.IListener.OnclickSlot(HeroPresetStorageSlotComponent caller)
	{
		if (caller == null || CurrentSlotNo == caller.SlotIndex + 1)
		{
			return;
		}
		SetSlotNo(caller.SlotIndex + 1);
		SetSelectedSlot(resetHeroCompoenent: true);
		int key = GetSlotStartIndex(CurrentPage) + CurrentSlotNo;
		HeroPresetStorageSlotData slotData = GetSlotData(key);
		if (slotData != null)
		{
			string storageKey = slotData.StorageKey;
			int buyGem = slotData.BuyGem;
			GetSlotState(slotData, out var isAvailableSlot, out var canLoadSlot);
			SetMenuButtonState(isAvailableSlot, canLoadSlot);
			if (!isAvailableSlot)
			{
				OpenBuyStorageSlotPopup(buyGem, storageKey);
			}
		}
	}

	void HeroPresetNameChangePopup.IListener.OnClickButton(string storageKey, string name)
	{
		if (_listener != null)
		{
			_listener.OnChangeHeroStorageSlot(storageKey, name);
		}
	}

	private void OpenMoveToPayShopPopup()
	{
		string text = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
		string message = LocalizationManager.GetString(NShared.NLocalization.System.MOVE_GEM_SHOP_CONFIRM);
		MessagePopup.Open(text, message, delegate
		{
			Close();
			SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop(NewPayShopCategoryType.Ruby.ToString());
		}, delegate
		{
			UnselectSlot();
		});
	}

	private void OpenBuyStorageSlotPopup(int buyGem, string storageKey)
	{
		PreserveMessagePopup2.OpenByHeroPresetSlot(PreserveMessagePopup2.EPriceType.Gem, buyGem, delegate
		{
			long totalGem = GlobalAutoBehaviour<AccountManager>.instance.UserManager.TotalGem;
			int num = buyGem;
			if (totalGem < num)
			{
				OpenMoveToPayShopPopup();
			}
			else if (_listener != null)
			{
				_listener.OnBuyHeroStorageSlot(storageKey);
			}
		}, delegate
		{
			UnselectSlot();
		});
	}

	public void OnClickPrevPage()
	{
		ChangePage(CurrentPage - 1);
	}

	public void OnClickNextPage()
	{
		ChangePage(CurrentPage + 1);
	}

	public void OnClickChangeSlotName()
	{
		if (_listener == null || CurrentSlotNo == -1)
		{
			return;
		}
		int key = GetSlotStartIndex(CurrentPage) + CurrentSlotNo;
		HeroPresetStorageSlotData slotData = GetSlotData(key);
		if (slotData != null)
		{
			string storageKey = slotData.StorageKey;
			HeroPresetStorageInfo heroPresetStorageInfo = GlobalAutoBehaviour<AccountManager>.instance.HeroPresetStorageManager.Get(storageKey);
			if (heroPresetStorageInfo != null)
			{
				string originName = GetName(slotData, heroPresetStorageInfo);
				HeroPresetNameChangePopup.Open(this, storageKey, originName);
			}
		}
	}

	public void OnClickSaveSlot()
	{
		if (_listener == null || CurrentSlotNo == -1)
		{
			return;
		}
		string storageKey = GetSlotStorageKey(CurrentPage, CurrentSlotNo);
		HeroPresetStorageInfo heroPresetStorageInfo = GlobalAutoBehaviour<AccountManager>.instance.HeroPresetStorageManager.Get(storageKey);
		if (heroPresetStorageInfo != null && !__Extension_Method_ICollection__.IsNullOrEmpty(heroPresetStorageInfo.HeroPresetInfos))
		{
			string text = LocalizationManager.GetString(NShared.NLocalization.Common.COMMON_CONFIRM);
			string stringDirect = LocalizationManager.GetStringDirect("EQUIP_PRESET_OVERWRITE_CONFIRM");
			MessagePopup3Button.Open(text, stringDirect, delegate
			{
				_listener.OnAddHeroStorageSlot(storageKey);
			}, null, NShared.NLocalization.Common.COMMON_CONFIRM, NShared.NLocalization.Common.COMMON_CANCEL);
		}
		else
		{
			_listener.OnAddHeroStorageSlot(storageKey);
		}
	}

	public void OnclickLoadSlot()
	{
		if (_listener != null && CurrentSlotNo != -1)
		{
			string slotStorageKey = GetSlotStorageKey(CurrentPage, CurrentSlotNo);
			if (GlobalAutoBehaviour<AccountManager>.instance.HeroPresetStorageManager.Get(slotStorageKey) == null)
			{
				string text = LocalizationManager.GetString(NShared.NLocalization.Common.COMMON_CONFIRM);
				string stringDirect = LocalizationManager.GetStringDirect("EQUIP_PRESET_NOT_SELECTED");
				MessagePopup.Open(text, stringDirect);
			}
			else if ((DateTime.Now - _lastLoadTime).TotalSeconds < (double)_loadDelaySec)
			{
				string text2 = LocalizationManager.GetString(NShared.NLocalization.Common.COMMON_CONFIRM);
				string stringDirect2 = LocalizationManager.GetStringDirect("EQUIP_PRESET_LOAD_UNAVAILABLE");
				MessagePopup.Open(text2, stringDirect2);
			}
			else
			{
				_lastLoadTime = DateTime.Now;
				_listener.OnSetHeroStorageSlot(slotStorageKey);
			}
		}
	}
}
