using System;
using System.Collections.Generic;
using System.Linq;
using NGame2.NAccount;
using NGame2.NClearMission;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NLobby;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class ClearMissionPopup : BaseWindow, ClearMissionTabComponent.IListener, ClearMissionListComponent.IListener
{
	public interface IListener
	{
		void OnBuyPayShopProduct(int payShopProductIndex);

		void OnGetMissionReward(int missionIndex, string rewardTitle, Action onReceived, Action onRefreshTab);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/ClearMission/ClearMissionPopup";

	public UIScrollGrid TabGrid;

	public UILabel LabelMissionInfo;

	public Transform BonusRubyRoot;

	public UILabel LabelBonusRubyCount;

	public UILabel LabelPurchase;

	public UIButton ButtonPurchase;

	public UIBundleLoader ProductImageBundleTex;

	public UIScrollGrid MissionListGrid;

	private IListener _iListener;

	private ClearMissionProductData _currentProductData;

	private NewPayShopProductInfo _productInfo;

	private List<ClearMissionTabComponent> _tabComponentList = new List<ClearMissionTabComponent>();

	private List<ClearMissionListComponent> _missionComponentList = new List<ClearMissionListComponent>();

	public void Open(IListener ilistener)
	{
		Open(delegate
		{
			Init();
		}, null);
		_iListener = ilistener;
	}

	public override void Reset()
	{
		base.Reset();
		_iListener = null;
		_currentProductData = null;
		_productInfo = null;
		Clear();
	}

	public void RefreshPopup(ClearMissionProductData productData)
	{
		RefreshTabCanReceiveRewardExist();
		_productInfo = GlobalAutoBehaviour<AccountManager>.instance.NewPayShopProductManager.Get(productData.PayShopProductIndex);
		bool isPurchased = SingletonUpdater<ClearMissionManager>.instance.IsPurchasedProduct(productData);
		SetPurchaseButton(productData, isPurchased);
		RefreshMissionComponentList(productData, isPurchased);
	}

	private void Init()
	{
		SetTabComponentList();
		if (_tabComponentList.Count == 0)
		{
			Close();
			return;
		}
		int defaultTabProductIndex = GetDefaultTabProductIndex();
		SelectDefaultTab(defaultTabProductIndex);
	}

	private void SetTabComponentList()
	{
		FactoryBase.ReleaseAndClearList(_tabComponentList);
		ClearMissionProductData[] array = GameRoot.Instance.ClearMissionProductDataContainer.GetAllData().ToArray();
		Array.Sort(array, (ClearMissionProductData a, ClearMissionProductData b) => a.ShowIndex.CompareTo(b.ShowIndex));
		for (int num = 0; num < __Extension_Method_ICollection__.COUNT(array); num++)
		{
			ClearMissionProductData clearMissionProductData = array[num];
			if (clearMissionProductData == null)
			{
				continue;
			}
			bool flag = SingletonUpdater<ClearMissionManager>.instance.IsPurchasedProduct(clearMissionProductData);
			if (!flag)
			{
				continue;
			}
			bool flag2 = SingletonUpdater<ClearMissionManager>.instance.IsRemainMissionReward(clearMissionProductData.ProductGroupIndex);
			if (!flag || flag2)
			{
				bool flag3 = SingletonUpdater<ClearMissionManager>.instance.CanReceiveReward(checkAllMission: false, clearMissionProductData.ProductGroupIndex);
				if (flag3 && (_currentProductData == null || _currentProductData.Index > clearMissionProductData.Index))
				{
					_currentProductData = clearMissionProductData;
				}
				bool show = SingletonUpdater<ClearMissionManager>.instance.IsNewPromotion(clearMissionProductData.PayShopProductIndex);
				ClearMissionTabComponent clearMissionTabComponent = UIHelper.AddGridItem(TabGrid, ClearMissionTabComponent.PrefabAuto);
				if (clearMissionTabComponent != null)
				{
					clearMissionTabComponent.Init(this, clearMissionProductData);
					clearMissionTabComponent.ShowNew(show);
					clearMissionTabComponent.ShowCanReceiveRewardExist(flag3);
					_tabComponentList.Add(clearMissionTabComponent);
				}
			}
		}
		UIHelper.RepositionGrid(TabGrid);
	}

	private void SelectDefaultTab(int productIndex)
	{
		ClearMissionTabComponent clearMissionTabComponent = null;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_tabComponentList); i++)
		{
			ClearMissionTabComponent clearMissionTabComponent2 = _tabComponentList[i];
			if (clearMissionTabComponent2 != null)
			{
				clearMissionTabComponent2.SelectToggle(productIndex);
				if (clearMissionTabComponent2.IsSelectedTab())
				{
					clearMissionTabComponent = clearMissionTabComponent2;
				}
			}
		}
		if (clearMissionTabComponent != null && CanGridScrollMove(TabGrid))
		{
			TabGrid.ScrollTo(clearMissionTabComponent, 0f, 0f);
		}
	}

	private void RefreshTabCanReceiveRewardExist()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_tabComponentList); i++)
		{
			ClearMissionTabComponent clearMissionTabComponent = _tabComponentList[i];
			if (!(clearMissionTabComponent == null) && clearMissionTabComponent.ProductData != null)
			{
				bool show = SingletonUpdater<ClearMissionManager>.instance.IsNewPromotion(clearMissionTabComponent.ProductData.PayShopProductIndex);
				clearMissionTabComponent.ShowNew(show);
				bool show2 = SingletonUpdater<ClearMissionManager>.instance.CanReceiveReward(checkAllMission: false, clearMissionTabComponent.ProductData.ProductGroupIndex);
				clearMissionTabComponent.ShowCanReceiveRewardExist(show2);
			}
		}
	}

	private int GetDefaultTabProductIndex()
	{
		if (_currentProductData != null)
		{
			return _currentProductData.Index;
		}
		ClearMissionTabComponent valid = Vespa_Generic_IList_Extension_Method.GetValid(_tabComponentList, 0);
		if (valid != null && valid.ProductData != null)
		{
			return valid.ProductData.Index;
		}
		return 0;
	}

	private void SetMissionProduct(ClearMissionProductData productData)
	{
		FactoryBase.ReleaseAndClearList(_missionComponentList);
		_productInfo = GlobalAutoBehaviour<AccountManager>.instance.NewPayShopProductManager.Get(productData.PayShopProductIndex);
		bool isPurchased = SingletonUpdater<ClearMissionManager>.instance.IsPurchasedProduct(productData);
		SetMissionComponentList(productData, isPurchased);
		SetPurchaseButton(productData, isPurchased);
		SetProductImage(productData.ProductImagePath);
		SetProductBonus(_productInfo);
		SetMissionInfo(productData.MissionInfo);
		SetMissionListScrollPosition();
	}

	private void SetMissionComponentList(ClearMissionProductData productData, bool isPurchased)
	{
		if (productData == null)
		{
			return;
		}
		bool isPaidReward = productData.PurchaseType != ClearMissionPurchaseType.Free;
		ClearMissionProductMissionData[] missionList = GameRoot.Instance.ClearMissionProductMissionDataContainer.GetMissionList(productData.ProductGroupIndex);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(missionList); i++)
		{
			ClearMissionProductMissionData clearMissionProductMissionData = missionList[i];
			if (clearMissionProductMissionData != null)
			{
				ClearMissionListComponent clearMissionListComponent = UIHelper.AddGridItem(MissionListGrid, ClearMissionListComponent.PrefabAuto);
				if (clearMissionListComponent != null)
				{
					bool receivedReward = GlobalAutoBehaviour<AccountManager>.instance.ClearMissionManager.IsReceivedReward(clearMissionProductMissionData.Index);
					clearMissionListComponent.Open(this, clearMissionProductMissionData, receivedReward, isPaidReward, isPurchased);
					_missionComponentList.Add(clearMissionListComponent);
				}
			}
		}
		UIHelper.RepositionGrid(MissionListGrid);
	}

	private void RefreshMissionComponentList(ClearMissionProductData productData, bool isPurchased)
	{
		bool isPaidReward = productData.PurchaseType != ClearMissionPurchaseType.Free;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_missionComponentList); i++)
		{
			ClearMissionListComponent clearMissionListComponent = _missionComponentList[i];
			if (!(clearMissionListComponent == null) && clearMissionListComponent.MissionData != null)
			{
				bool receivedReward = GlobalAutoBehaviour<AccountManager>.instance.ClearMissionManager.IsReceivedReward(clearMissionListComponent.MissionData.Index);
				clearMissionListComponent.RefreshGetRewardButtonState(receivedReward, isPaidReward, isPurchased);
			}
		}
	}

	private void SetMissionListScrollPosition()
	{
		ClearMissionListComponent clearMissionListComponent = null;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_missionComponentList); i++)
		{
			ClearMissionListComponent clearMissionListComponent2 = _missionComponentList[i];
			if (!(clearMissionListComponent2 == null) && clearMissionListComponent2.MissionData != null)
			{
				if (GlobalAutoBehaviour<AccountManager>.instance.ClearMissionManager.IsReceivedReward(clearMissionListComponent2.MissionData.Index))
				{
					clearMissionListComponent = clearMissionListComponent2;
				}
				else if (SingletonUpdater<ClearMissionManager>.instance.CanReceiveRewardMission(clearMissionListComponent2.MissionData))
				{
					clearMissionListComponent = clearMissionListComponent2;
					break;
				}
			}
		}
		if (clearMissionListComponent != null && CanGridScrollMove(MissionListGrid))
		{
			MissionListGrid.ScrollTo(clearMissionListComponent, 0f, 0f);
		}
	}

	private void SetPurchaseButton(ClearMissionProductData productData, bool isPurchased)
	{
		bool enable = productData.PurchaseType != ClearMissionPurchaseType.Free;
		UIHelper.EnableGameObject(ButtonPurchase, enable);
		string stringDirect = LocalizationManager.GetStringDirect(isPurchased ? "BENEFIT_NEWBIE_BUY" : "SHOP_BUY");
		UIHelper.SetLabelText(LabelPurchase, stringDirect);
		UIHelper.EnableButton(ButtonPurchase, !isPurchased, grayscaleIfDisable: true);
		UIHelper.SetWidgetGrayScale(LabelPurchase, isPurchased);
	}

	private void SetProductImage(string path)
	{
		if (!String_Extension_Method.IsNullOrEmpty(path))
		{
			UIHelper.SetBundleTexture(ProductImageBundleTex, path);
		}
	}

	private void SetProductBonus(NewPayShopProductInfo productInfo)
	{
		int num = productInfo?.Gem ?? 0;
		bool flag = num != 0;
		if (flag)
		{
			UIHelper.SetLabelText(LabelBonusRubyCount, num);
		}
		UIHelper.EnableGameObject(BonusRubyRoot, flag);
	}

	private void SetMissionInfo(string infoKey)
	{
		string stringDirect = LocalizationManager.GetStringDirect(infoKey);
		UIHelper.SetLabelText(LabelMissionInfo, stringDirect);
	}

	private bool CanGridScrollMove(UIScrollGrid scrollGrid)
	{
		if (scrollGrid == null)
		{
			return false;
		}
		float height = scrollGrid.ClippingPanel.height;
		float num = scrollGrid.Grid.cellHeight * (float)scrollGrid.Grid.GetGridChildCount();
		if (height > num)
		{
			return false;
		}
		return true;
	}

	public void OnBuyPayShopProduct()
	{
		if (_iListener != null && _currentProductData != null)
		{
			_iListener.OnBuyPayShopProduct(_currentProductData.PayShopProductIndex);
		}
	}

	void ClearMissionTabComponent.IListener.OnClickTab(ClearMissionProductData productData)
	{
		if (productData != null)
		{
			_currentProductData = productData;
			SetMissionProduct(productData);
		}
	}

	void ClearMissionListComponent.IListener.OnClickGetReward(int index, Action onReceived)
	{
		if (_iListener != null && _currentProductData != null)
		{
			string stringDirect = LocalizationManager.GetStringDirect(_currentProductData.MissionName);
			_iListener.OnGetMissionReward(index, stringDirect, onReceived, delegate
			{
				RefreshTabCanReceiveRewardExist();
			});
		}
	}

	private void Clear()
	{
		FactoryBase.ReleaseAndClearList(_tabComponentList);
		FactoryBase.ReleaseAndClearList(_missionComponentList);
	}

	public void OnClickGameGuide()
	{
		Close();
		Action onClose = null;
		IList<ClearMissionProductData> allData = GameRoot.Instance.ClearMissionProductDataContainer.GetAllData();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(allData); i++)
		{
			ClearMissionProductData clearMissionProductData = allData[i];
			if (clearMissionProductData == null)
			{
				continue;
			}
			bool flag = SingletonUpdater<ClearMissionManager>.instance.IsPurchasedProduct(clearMissionProductData);
			if (flag)
			{
				bool flag2 = SingletonUpdater<ClearMissionManager>.instance.IsRemainMissionReward(clearMissionProductData.ProductGroupIndex);
				if (!flag || flag2)
				{
					onClose = SceneAutoBehaviour<ClearMissionPopupManagement>.instance.OpenClearMissionPopup;
					break;
				}
			}
		}
		SceneAutoBehaviour<LobbyManagement>.instance.OpenGameGuideWindow(onClose);
	}
}
