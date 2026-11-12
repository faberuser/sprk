using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NShared;
using NShared.NLocalization;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow;

public class UseTpItemHeroListPopup : BaseWindow, IUserObserver<NGame2.NAccount.UserHeroManager.ChangeHeroInfo>, IUserObserver, IUserObserver<ItemInfo>, UseTpItemHeroListComponent.IListener
{
	public const string PrefabPath = "Prefab2/Window/Inventory/UseTpItemHeroListPopup";

	public UILabel LabelEmpty;

	public UIScrollGrid GridHeroList;

	private List<UseTpItemHeroListComponent> _heroUseTpItemComponents = new List<UseTpItemHeroListComponent>();

	public static void OpenWindow()
	{
		WindowPrefab<UseTpItemHeroListPopup> windowPrefab = new WindowPrefab<UseTpItemHeroListPopup>("Prefab2/Window/Inventory/UseTpItemHeroListPopup", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal();
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	public void OpenInternal()
	{
		Open(SetHeroList, null);
	}

	private void SetLabelEmpty(bool isEmpty)
	{
		UIHelper.EnableGameObject(LabelEmpty, isEmpty);
	}

	public void SetHeroList()
	{
		ClearHeroUseTpItemComponent();
		List<CachedHeroInfo> maxTranscendHeroList = GetMaxTranscendHeroList();
		bool flag = __Extension_Method_ICollection__.IsNullOrEmpty(maxTranscendHeroList);
		SetLabelEmpty(flag);
		if (flag)
		{
			return;
		}
		GameRoot.Instance.ConstantDataContainer.GetValueInt("MaxTranscendPoint");
		maxTranscendHeroList.Sort(delegate(CachedHeroInfo lhf, CachedHeroInfo rhf)
		{
			int currentTotalTrancendPoint = lhf.GetCurrentTotalTrancendPoint();
			int currentTotalTrancendPoint2 = rhf.GetCurrentTotalTrancendPoint();
			return currentTotalTrancendPoint.CompareTo(currentTotalTrancendPoint2);
		});
		for (int num = 0; num < __Extension_Method_ICollection__.COUNT(maxTranscendHeroList); num++)
		{
			CachedHeroInfo cachedHeroInfo = maxTranscendHeroList[num];
			if (cachedHeroInfo != null)
			{
				UseTpItemHeroListComponent useTpItemHeroListComponent = UIHelper.AddGridItem(GridHeroList, UseTpItemHeroListComponent.PrefabAuto);
				if (!(useTpItemHeroListComponent == null))
				{
					useTpItemHeroListComponent.Open(cachedHeroInfo, this);
					_heroUseTpItemComponents.Add(useTpItemHeroListComponent);
				}
			}
		}
		UIHelper.RepositionGrid(GridHeroList);
	}

	private void ClearHeroUseTpItemComponent()
	{
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(_heroUseTpItemComponents))
		{
			FactoryBase.ReleaseAndClearList(_heroUseTpItemComponents);
		}
	}

	public List<CachedHeroInfo> GetMaxTranscendHeroList()
	{
		if (!AccountManager.IsLogined())
		{
			return null;
		}
		return GlobalAutoBehaviour<AccountManager>.instance.HeroManager.GetMaxTranscendHeroList();
	}

	public bool HasTranscendPointItem()
	{
		return !__Extension_Method_ICollection__.IsNullOrEmpty(GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemsByType(ItemType.TranscendPoint));
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

	void UseTpItemHeroListComponent.IListener.OnClickHero(CachedHeroInfo heroInfo)
	{
		string stringDirect = LocalizationManager.GetStringDirect("COMMON_NOTIFICATION");
		string message = LocalizationManager.GetString(HeroTranscend.TRANSCEND_POINT_SHOP_CONFIRM);
		if (!HasTranscendPointItem())
		{
			MessagePopup.Open(stringDirect, message, delegate
			{
				SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop(NewPayShopCategoryType.Convenience.ToString());
				Close();
			}, null);
		}
		else if (heroInfo != null)
		{
			UseTpItemSelectPopup.Open(heroInfo);
		}
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}

	void IUserObserver<NGame2.NAccount.UserHeroManager.ChangeHeroInfo>.OnChanged(NGame2.NAccount.UserHeroManager.ChangeHeroInfo changedInfo)
	{
		if (changedInfo.ChangeType == NGame2.NAccount.UserHeroManager.HeroChangeType.ExtendTranscendPoint)
		{
			SetHeroList();
		}
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
}
