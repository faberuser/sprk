using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NShared;
using NShared.NLocalization;
using NShared.UsePotionItem;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class UseTpItemSelectPopup : BaseWindow, IUserObserver<NGame2.NAccount.UserHeroManager.ChangeHeroInfo>, IUserObserver, IUserObserver<ItemInfo>, UseTpItemSelectComponent.IListener
{
	public const string PrefabPath = "Prefab2/Window/Inventory/UseTpItemSelectPopup";

	public UISimpleGrid HeroSmallPortraitGrid;

	public UIScrollGrid TpItemGrid;

	public UILabel LabelHeroLevel;

	public UILabel LabelHeroName;

	public UILabel LabelTp;

	public UIButton ButtonUse;

	private HeroPortraitComponent _heroPortrait;

	private List<UseTpItemSelectComponent> _itemSelectComponents = new List<UseTpItemSelectComponent>();

	public CachedHeroInfo HeroInfo { get; private set; }

	public static void Open(CachedHeroInfo heroInfo)
	{
		WindowPrefab<UseTpItemSelectPopup> windowPrefab = new WindowPrefab<UseTpItemSelectPopup>("Prefab2/Window/Inventory/UseTpItemSelectPopup", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(heroInfo);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	public void OpenInternal(CachedHeroInfo heroInfo)
	{
		HeroInfo = heroInfo;
		Open(delegate
		{
			SetHeroInfo();
			SetHeroPortrait();
			SetHeroTp();
			SetTpItemList();
			SetUseItemButton();
		}, null);
	}

	public override void Close()
	{
		base.Close();
		BoxCollider component = _heroPortrait.GetComponent<BoxCollider>();
		if (component != null)
		{
			component.enabled = true;
		}
		FactoryBase.SafeReleaseComponent(ref _heroPortrait);
		ClearTpItemComponents();
	}

	private void SetHeroPortrait()
	{
		_heroPortrait = UIHelper.AddGridItem(HeroSmallPortraitGrid, HeroPortraitComponent.SmallPrefabAuto);
		_heroPortrait.Open(HeroInfo, null, null, HeroPortraitComponent.EPortraitMode.Icon);
		_heroPortrait.SetLevel(show: false);
		BoxCollider component = _heroPortrait.GetComponent<BoxCollider>();
		if (component != null)
		{
			component.enabled = false;
		}
		UIHelper.RepositionGrid(HeroSmallPortraitGrid);
	}

	public void SetHeroInfo()
	{
		UIHelper.SetLabelText(LabelHeroLevel, "Lv" + HeroInfo.Level);
		CreatureData creatureData = HeroInfo.CreatureData;
		UIHelper.SetLabelText(LabelHeroName, (creatureData != null) ? creatureData.Name : string.Empty);
	}

	private void SetTpItemList()
	{
		ClearTpItemComponents();
		List<ItemInfo> tpItemList = GetTpItemList();
		if (__Extension_Method_ICollection__.IsNullOrEmpty(tpItemList))
		{
			return;
		}
		tpItemList.Sort(delegate(ItemInfo lhf, ItemInfo rhf)
		{
			ItemData data = GameRoot.Instance.ItemDataContainer.GetData(lhf.ItemIndex);
			ItemData data2 = GameRoot.Instance.ItemDataContainer.GetData(rhf.ItemIndex);
			if (data == null || data2 == null)
			{
				return 0;
			}
			int valueInt = data.GetValueInt(0);
			int valueInt2 = data2.GetValueInt(0);
			return valueInt.CompareTo(valueInt2);
		});
		for (int num = 0; num < __Extension_Method_ICollection__.COUNT(tpItemList); num++)
		{
			ItemInfo itemInfo = tpItemList[num];
			if (itemInfo != null)
			{
				UseTpItemSelectComponent useTpItemSelectComponent = UIHelper.AddGridItem(TpItemGrid, UseTpItemSelectComponent.PrefabAuto);
				if (!(useTpItemSelectComponent == null))
				{
					useTpItemSelectComponent.Open(HeroInfo, itemInfo, this);
					_itemSelectComponents.Add(useTpItemSelectComponent);
				}
			}
		}
		UIHelper.RepositionGrid(TpItemGrid);
	}

	private void ClearTpItemComponents()
	{
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(_itemSelectComponents))
		{
			FactoryBase.ReleaseAndClearList(_itemSelectComponents);
		}
	}

	private List<ItemInfo> GetTpItemList()
	{
		if (!AccountManager.IsLogined())
		{
			return null;
		}
		return GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemsByType(ItemType.TranscendPoint);
	}

	public void SetHeroTp(int transcendPoint = 0)
	{
		int num = HeroInfo.GetCurrentTotalTrancendPoint() + transcendPoint;
		int num2 = HeroInfo.GetUsableTranscendSkillPoint(HeroInfo.SkillPage) + transcendPoint;
		string text = ((transcendPoint == 0) ? $"{num2:00}/{num:00}" : string.Format(LocalizationManager.GetString(HeroTranscend.TRANSCEND_POINT_COUNT), 0, num2, num));
		UIHelper.SetLabelText(LabelTp, text);
	}

	private void SetUseItemButton()
	{
		UseTpItemSelectComponent selectComponent = GetSelectComponent();
		bool flag = selectComponent != null;
		bool flag2 = false;
		if (flag)
		{
			flag2 = selectComponent.IsValidItemCount();
		}
		UIHelper.EnableButton(ButtonUse, flag && flag2, grayscaleIfDisable: true);
	}

	private void UseTpItem(int itemIndex, int itemCount, string creatureName, int useTpValue)
	{
		NShared.UsePotionItem.Request request = new NShared.UsePotionItem.Request();
		request.ItemIndex = itemIndex;
		request.HeroIndex = HeroInfo.HeroIndex;
		request.ItemCount = itemCount;
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.UsePotionItem.Response response)
		{
			if (response.Result != ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResult);
			GlobalAutoBehaviour<AccountManager>.instance.SetExtendTranscendPoint(response.HeroTranscendResult);
			CenterMessage.Open(LocalizationManager.GetString(HeroTranscend.TRANSCEND_POINT_CENTER_MESSAGE, creatureName, useTpValue));
			Close();
		});
	}

	private UseTpItemSelectComponent GetSelectComponent()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_itemSelectComponents); i++)
		{
			UseTpItemSelectComponent useTpItemSelectComponent = _itemSelectComponents[i];
			if (!(useTpItemSelectComponent == null) && useTpItemSelectComponent.IsSelect)
			{
				return useTpItemSelectComponent;
			}
		}
		return null;
	}

	public void OnclickUseTpItem()
	{
		if (HeroInfo == null)
		{
			return;
		}
		UseTpItemSelectComponent selectComponent = GetSelectComponent();
		if (selectComponent == null)
		{
			return;
		}
		int itemIndex = 0;
		int itemCount = 0;
		int useTpValue = 0;
		int overPoint = 0;
		selectComponent.GetUseTpItemInfo(out itemIndex, out itemCount, out useTpValue, out overPoint);
		ItemData data = GameRoot.Instance.ItemDataContainer.GetData(itemIndex);
		if (data == null)
		{
			return;
		}
		string stringDirect = LocalizationManager.GetStringDirect("COMMON_NOTIFICATION");
		string text = "";
		if (overPoint > 0)
		{
			int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("MaxTranscendPoint");
			text = LocalizationManager.GetString(HeroTranscend.TRANSCEND_POINT_EXCESSIVE_WARNING, data.Name, valueInt);
			string stringDirect2 = LocalizationManager.GetStringDirect("COMMON_YES");
			string stringDirect3 = LocalizationManager.GetStringDirect("COMMON_NO");
			MessagePopup3Button.Open(stringDirect, text, null, delegate
			{
				UseTpItem(itemIndex, itemCount, HeroInfo.CreatureData.Name, useTpValue);
			}, stringDirect3, stringDirect2);
		}
		else
		{
			text = LocalizationManager.GetString(HeroTranscend.TRANSCEND_POINT_CONFIRM, HeroInfo.CreatureData.Name, useTpValue);
			MessagePopup.Open(stringDirect, text, delegate
			{
				UseTpItem(itemIndex, itemCount, HeroInfo.CreatureData.Name, useTpValue);
			}, null);
		}
	}

	public void OnclickHelp()
	{
		EventGuidePopup.Open(new Enum[5]
		{
			HeroTranscend.TRANSCEND_POINT_APPLY,
			HeroTranscend.TRANSCEND_POINT_INFO_TITLE_1,
			HeroTranscend.TRANSCEND_POINT_INFO_DESC_1,
			HeroTranscend.TRANSCEND_POINT_INFO_TITLE_2,
			HeroTranscend.TRANSCEND_POINT_INFO_DESC_2
		});
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}

	void IUserObserver<NGame2.NAccount.UserHeroManager.ChangeHeroInfo>.OnChanged(NGame2.NAccount.UserHeroManager.ChangeHeroInfo changedInfo)
	{
		_ = changedInfo.ChangeType;
		_ = 22;
	}

	void IUserObserver<ItemInfo>.OnChanged(ItemInfo itemInfo)
	{
		ItemData data = GameRoot.Instance.ItemDataContainer.GetData(itemInfo.ItemIndex);
		if (data != null)
		{
			_ = data.Type;
			_ = 33;
		}
	}

	void UseTpItemSelectComponent.IListener.OnClickComponent(int itemIndex)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_itemSelectComponents); i++)
		{
			UseTpItemSelectComponent useTpItemSelectComponent = _itemSelectComponents[i];
			if (!(useTpItemSelectComponent == null))
			{
				useTpItemSelectComponent.SelectComponent(itemIndex);
			}
		}
		SetHeroTp();
		SetUseItemButton();
	}

	void UseTpItemSelectComponent.IListener.ChangeHeroTpInfo(int transcendPoint)
	{
		SetHeroTp(transcendPoint);
		SetUseItemButton();
	}
}
