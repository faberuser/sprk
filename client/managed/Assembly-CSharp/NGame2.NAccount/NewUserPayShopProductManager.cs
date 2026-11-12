using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using MiniJSON;
using NGame2.NUI.NHelper.NPayShop;
using NShared;
using NVespa;

namespace NGame2.NAccount;

public class NewUserPayShopProductManager : NShared.NewUserPayShopProductManager
{
	public List<string> GoogleProductIds = new List<string>();

	public List<string> AppleProductIds = new List<string>();

	public List<string> PlatformProductIds => AppleProductIds;

	public void SetProductDatas(NewPayShopProductInfo[] values)
	{
		foreach (NewPayShopProductInfo newPayShopProductInfo in values)
		{
			OnAddData(newPayShopProductInfo.Index, newPayShopProductInfo);
		}
	}

	protected override void OnAddData(int index, NewPayShopProductInfo value)
	{
		AddPurchaseProduct(value);
		InitCardTitle(value);
		base.OnAddData(index, value);
	}

	public override bool Remove(int key)
	{
		RemovePurchaseProduct(Get(key));
		return base.Remove(key);
	}

	public override bool Remove(NewPayShopProductInfo value)
	{
		RemovePurchaseProduct(value);
		return base.Remove(value);
	}

	public override void RemoveAll()
	{
		GoogleProductIds.Clear();
		AppleProductIds.Clear();
		base.RemoveAll();
	}

	private void AddPurchaseProduct(NewPayShopProductInfo value)
	{
		if (!String_Extension_Method.IsNullOrEmpty(value.GoogleId))
		{
			value.GoogleId = value.GoogleId.Trim();
			if (!GoogleProductIds.Contains(value.GoogleId))
			{
				GoogleProductIds.Add(value.GoogleId);
			}
		}
		if (!String_Extension_Method.IsNullOrEmpty(value.AppleId))
		{
			value.AppleId = value.AppleId.Trim();
			if (!AppleProductIds.Contains(value.AppleId))
			{
				AppleProductIds.Add(value.AppleId);
			}
		}
	}

	private void RemovePurchaseProduct(NewPayShopProductInfo value)
	{
		if (!String_Extension_Method.IsNullOrEmpty(value.GoogleId))
		{
			value.GoogleId = value.GoogleId.Trim();
			if (GoogleProductIds.Contains(value.GoogleId))
			{
				GoogleProductIds.Remove(value.GoogleId);
			}
		}
		if (!String_Extension_Method.IsNullOrEmpty(value.AppleId))
		{
			value.AppleId = value.AppleId.Trim();
			if (AppleProductIds.Contains(value.AppleId))
			{
				AppleProductIds.Remove(value.AppleId);
			}
		}
	}

	public static void InitCardTitle(NewPayShopProductInfo value)
	{
		if (value != null && String_Extension_Method.IsNullOrEmpty(value.CardTitle))
		{
			value.CardTitle = value.ProductName;
		}
	}

	public bool HasNewItem(string category)
	{
		return Array.Exists(GetValues(), (NewPayShopProductInfo v) => v.Category == category && v.IsNew == 1);
	}

	public NewPayShopProductInfo[] GetProducts(string category)
	{
		return Array.FindAll(GetValues(), (NewPayShopProductInfo v) => v.Category == category);
	}

	public NewPayShopProductInfo[] GetProducts(int buyGroup)
	{
		return Array.FindAll(GetValues(), (NewPayShopProductInfo v) => v.BuyGroup == buyGroup);
	}

	public NewPayShopProductInfo[] GetProducts(ProductType type)
	{
		return Array.FindAll(GetValues(), (NewPayShopProductInfo v) => v.ProductType == type);
	}

	public NewPayShopProductInfo[] GetStepProducts(string category, int stepGroupIndex)
	{
		return Array.FindAll(GetProducts(category), delegate(NewPayShopProductInfo v)
		{
			if (v.StepProductGroup == null)
			{
				return false;
			}
			int valueInt = ArgParser.GetValueInt(v.StepProductGroup, 0);
			return stepGroupIndex == valueInt;
		});
	}

	public NewPayShopProductInfo GetStepProduct(string category, int stepGroupIndex, int stepValue)
	{
		return Array.Find(GetProducts(category), delegate(NewPayShopProductInfo v)
		{
			if (v.StepProductGroup == null)
			{
				return false;
			}
			int valueInt = ArgParser.GetValueInt(v.StepProductGroup, 0);
			int valueInt2 = ArgParser.GetValueInt(v.StepProductGroup, 1);
			return stepGroupIndex == valueInt && valueInt2 == stepValue;
		});
	}

	public NewPayShopProductInfo GetPostFirstPurchaseProduct(string category, int firstPuchseProductIndex)
	{
		return Array.Find(GetProducts(category), (NewPayShopProductInfo v) => v.PrePurchased != 0 && v.PrePurchased == firstPuchseProductIndex);
	}

	public void ClearLIst(string category)
	{
		if (String_Extension_Method.IsNullOrEmpty(category))
		{
			VespaDebug.LogError("PayShopCategory is None");
			return;
		}
		NewPayShopProductInfo[] products = GetProducts(category);
		if (products != null)
		{
			NewPayShopProductInfo[] array = products;
			foreach (NewPayShopProductInfo value in array)
			{
				Remove(value);
			}
		}
	}

	public NewPayShopProductInfo GetAttendanceProduct(int attendanceIndex)
	{
		NewPayShopProductInfo[] products = GetProducts(ProductType.Attendance);
		if (products == null)
		{
			return null;
		}
		return Array.Find(products, (NewPayShopProductInfo v) => !__Extension_Method_ICollection__.IsNullOrEmpty(v.SelectAttendanceIndices) && TypeIterator.Contains(v.SelectAttendanceIndices, attendanceIndex));
	}

	public NewPayShopProductInfo GetProduct(int productIndex)
	{
		return Array.Find(GetValues(), (NewPayShopProductInfo v) => v.Index == productIndex);
	}

	public NewPayShopProductInfo GetProduct(string category, string itemCode)
	{
		NewPayShopProductInfo[] products = GetProducts(category);
		if (products == null)
		{
			return null;
		}
		NewPayShopProductInfo[] array = Array.FindAll(products, delegate(NewPayShopProductInfo product)
		{
			if (__Extension_Method_ICollection__.IsNullOrEmpty(product.UseItemCode))
			{
				return false;
			}
			return Array.Exists(product.UseItemCode, (string code) => code.Equals(itemCode)) ? true : false;
		});
		if (__Extension_Method_ICollection__.COUNT(array) <= 0)
		{
			return null;
		}
		return array[0];
	}

	public NewPayShopProductInfo GetByOneStoreId(string oneStoreId)
	{
		return Array.Find(GetValues(), (NewPayShopProductInfo v) => v.OneStoreId == oneStoreId);
	}

	public string ConvertOneStoreToGoole(string oneStoreId)
	{
		NewPayShopProductInfo byOneStoreId = GetByOneStoreId(oneStoreId);
		if (byOneStoreId == null)
		{
			return string.Empty;
		}
		return byOneStoreId.GoogleId;
	}

	public void LoadLocalTable(Action callback = null)
	{
		IJsonLoader jsonLoader = GameRoot.Instance.JsonLoader;
		Func<IDictionary, NewPayShopProductInfo2> parser = CustomJsonMarshaler.CustomParse<NewPayShopProductInfo2>;
		jsonLoader.Load("TableData/NewPayShopProductTable.json", delegate(object jsonData)
		{
			Path.GetFileNameWithoutExtension("TableData/NewPayShopProductTable.json");
			foreach (IDictionary item in jsonData as List<object>)
			{
				try
				{
					NewPayShopProductInfo2 newPayShopProductInfo = parser(item);
					if (newPayShopProductInfo != null)
					{
						OnAddData(newPayShopProductInfo.Key, newPayShopProductInfo);
					}
				}
				catch (Exception ex)
				{
					VespaDebug.LogError("Error while add data : " + ex);
				}
			}
			JsonPool.instance.Release(jsonData);
			Vespa_System_Action_Extension_Method.fire(callback);
		});
	}

	public bool HasAnyFreePackage()
	{
		bool result = false;
		NewPayShopProductInfo[] values = GetValues();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(values); i++)
		{
			NewPayShopProductInfo newPayShopProductInfo = values[i];
			if (newPayShopProductInfo != null && newPayShopProductInfo.ProductType == ProductType.Package && newPayShopProductInfo.Price == 0L && String_Extension_Method.IsNullOrEmpty(newPayShopProductInfo.KRW) && String_Extension_Method.IsNullOrEmpty(newPayShopProductInfo.USD) && String_Extension_Method.IsNullOrEmpty(newPayShopProductInfo.THB) && String_Extension_Method.IsNullOrEmpty(newPayShopProductInfo.VND) && PayShopHelper.IsBuyable(newPayShopProductInfo) && !PayShopHelper.IsNewbieTabProductPurchaseable(newPayShopProductInfo))
			{
				result = true;
				break;
			}
		}
		return result;
	}
}
