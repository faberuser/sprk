using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NManager;
using NGameAsset;
using NShared;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NComponent2.NPayShop;

public class PayShopHotAndNewList : PayShopBaseList<NewPayShopProductInfo, PayShopProductComponent, string>, PayShopRollingBanner.IListener, PayShopProductComponent.IListener, PayShopBaseComponent.IListener
{
	public new interface IListener : PayShopBaseList.IListener
	{
		void OnClickBanner(int bannerIndex);
	}

	public PayShopRollingBanner RollingBanner;

	public UITexture StaticBanner;

	private NewPayShopBannerData _staticBannerData;

	public override void Open(PayShopBaseList.IListener listener)
	{
		base.Open(listener);
		RollingBanner.Open(this);
	}

	public override void CreateList()
	{
		SelectOpenPlayerType(PayShopManagement.GetPayShopUserType());
	}

	public void SelectOpenPlayerType(PayShopUserGroup groupType)
	{
		string category = "Event";
		NewPayShopOpenConditionData data = GameRoot.Instance.NewPayShopOpenConditionDataContainer.GetData(groupType);
		if (data != null)
		{
			category = data.LinkCategory;
		}
		CreateList(category);
		SetBanner(groupType);
	}

	private void SetBanner(PayShopUserGroup groupType)
	{
		RollingBanner.Clear();
		List<NewPayShopBannerData> bannerDataByPlayerType = GameRoot.Instance.NewPayShopBannerDataContainer.GetBannerDataByPlayerType(groupType);
		bannerDataByPlayerType.Sort(delegate(NewPayShopBannerData a, NewPayShopBannerData b)
		{
			int num = 0;
			num = a.Sort.CompareTo(b.Sort);
			return (num != 0) ? num : a.Index.CompareTo(b.Index);
		});
		foreach (NewPayShopBannerData item in bannerDataByPlayerType)
		{
			if (!item.IsEnable)
			{
				continue;
			}
			AssetTexture newBannerAssetTexture = PayShopManagement.GetNewBannerAssetTexture(item.Index);
			if (newBannerAssetTexture != null)
			{
				if (item.Type == BannerPlayTypes.Static)
				{
					_staticBannerData = item;
					UIHelper.SetTextureInfo(StaticBanner, newBannerAssetTexture.GetTextureInfo());
				}
				else
				{
					RollingBanner.AddTexture(item.Index, newBannerAssetTexture);
				}
			}
		}
		RollingBanner.Play();
	}

	public override List<NewPayShopProductInfo> GetDataList(string category)
	{
		return GetDatas(category);
	}

	public static List<NewPayShopProductInfo> GetDatas(string category)
	{
		List<NewPayShopProductInfo> list = new List<NewPayShopProductInfo>();
		list.AddRange(GlobalAutoBehaviour<AccountManager>.instance.NewPayShopProductManager.GetProducts("HotNew"));
		return list;
	}

	protected override PrefabAuto<PayShopProductComponent> GetComponentPrefabAuto()
	{
		return PayShopProductComponent.Small_PrefabAuto;
	}

	protected override List<NewPayShopProductInfo> Filtering(List<NewPayShopProductInfo> datas)
	{
		return PayShopProductList.ProudctFilter(datas);
	}

	protected override void Sort(List<NewPayShopProductInfo> dataList, int[] sortIndices = null)
	{
		PayShopProductList.ProductSort(dataList);
	}

	protected override NewPayShopProductInfo GetData(int index)
	{
		return GlobalAutoBehaviour<AccountManager>.instance.NewPayShopProductManager.Get(index);
	}

	public override void ClearAll()
	{
		base.ClearAll();
		RollingBanner.Clear();
	}

	public override void ClearList()
	{
		base.ClearList();
		RollingBanner.Clear();
		RollingBanner.Stop();
		_staticBannerData = null;
	}

	void PayShopRollingBanner.IListener.OnClick(int index)
	{
		if (_listener != null)
		{
			((IListener)_listener).OnClickBanner(index);
		}
	}

	public void OnClickStaticBanner()
	{
		if (_staticBannerData != null)
		{
			((PayShopRollingBanner.IListener)this).OnClick(_staticBannerData.Index);
		}
	}

	public override void UpdateProduct(int index)
	{
		PayShopProductComponent payShopProductComponent = FindComponent(index);
		if (!(payShopProductComponent == null))
		{
			NewPayShopProductInfo productInfo = payShopProductComponent.ProductInfo;
			if (GlobalAutoBehaviour<AccountManager>.instance.NewPayShopProductManager.GetPostFirstPurchaseProduct(productInfo.Category, productInfo.Index) != null)
			{
				UpdateFirstPurchaseProduct(productInfo, payShopProductComponent);
			}
			else if (__Extension_Method_ICollection__.COUNT(productInfo.StepProductGroup) > 0)
			{
				UpdateComboProduct(productInfo, payShopProductComponent);
			}
			else
			{
				payShopProductComponent.Refresh();
			}
		}
	}

	public void UpdateComboProduct(NewPayShopProductInfo info, PayShopProductComponent comp)
	{
		if (__Extension_Method_ICollection__.COUNT(info.StepProductGroup) < 2)
		{
			return;
		}
		int valueInt = ArgParser.GetValueInt(info.StepProductGroup, 0);
		int valueInt2 = ArgParser.GetValueInt(info.StepProductGroup, 1);
		PlayerProductPurchaseInfo playerProductPurchaseInfo = GlobalAutoBehaviour<AccountManager>.instance.PlayerProductPurchaseManager.Get(info.Index);
		if (playerProductPurchaseInfo != null && playerProductPurchaseInfo.PurchasedCount >= info.PurchasableCount)
		{
			NewPayShopProductInfo stepProduct = GlobalAutoBehaviour<AccountManager>.instance.NewPayShopProductManager.GetStepProduct(info.Category, valueInt, valueInt2 + 1);
			if (stepProduct == null)
			{
				comp.Refresh();
				return;
			}
			comp.ChangeProductInfo(stepProduct);
			int dataIndex = GetDataIndex(info);
			ChagneData(dataIndex, stepProduct);
		}
		else
		{
			comp.Refresh();
		}
	}

	public void UpdateFirstPurchaseProduct(NewPayShopProductInfo info, PayShopProductComponent comp)
	{
		NewPayShopProductInfo postFirstPurchaseProduct = GlobalAutoBehaviour<AccountManager>.instance.NewPayShopProductManager.GetPostFirstPurchaseProduct(info.Category, info.Index);
		if (postFirstPurchaseProduct == null)
		{
			comp.Refresh();
			return;
		}
		comp.ChangeProductInfo(postFirstPurchaseProduct);
		int dataIndex = GetDataIndex(info);
		ChagneData(dataIndex, postFirstPurchaseProduct);
	}

	private PayShopProductComponent FindComponent(int index)
	{
		return _componentList.Find((PayShopProductComponent comp) => comp.ProductInfo.Index == index);
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		RollingBanner.UpdateDerived();
	}

	void PayShopProductComponent.IListener.OnClickProduct(int originalIndex, int comboIndex)
	{
	}

	void PayShopProductComponent.IListener.OnChangeComboProduct(int productIndex, int step)
	{
	}
}
