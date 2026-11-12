using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class NewPayShopProductDataContainer : JitDataContainerListOnly<NewPayShopProductData>
{
	public enum EStoreType
	{
		Google,
		Apple,
		OneStore,
		UDP,
		Samsung,
		Steam
	}

	private class DataSet
	{
		public JitDictionary<NewPayShopProductDataKey2, NewPayShopProductData> Dic { get; private set; }

		public JitDictionary<ProductType, List<NewPayShopProductData>> Lists_by_ProductType { get; private set; }

		public JitDictionary<string, List<NewPayShopProductData>> Lists_by_ProductId { get; private set; }

		public JitRef<List<string>> ProductIdList { get; private set; }

		public DataSet()
		{
			Dic = new JitDictionary<NewPayShopProductDataKey2, NewPayShopProductData>();
			Lists_by_ProductType = new JitDictionary<ProductType, List<NewPayShopProductData>>();
			Lists_by_ProductId = new JitDictionary<string, List<NewPayShopProductData>>();
			ProductIdList = new JitRef<List<string>>();
		}

		public void RegisterAndBind(JitPacker packer)
		{
			packer.RegisterAndBind(Dic);
			packer.RegisterAndBind(Lists_by_ProductType);
			packer.RegisterAndBind(Lists_by_ProductId);
			packer.RegisterAndBind(ProductIdList);
		}

		public void AddValue(string productId, NewPayShopProductData value)
		{
			NewPayShopProductDataKey2 key = new NewPayShopProductDataKey2(productId, value.Index);
			if (!Dic.JsonLoad_Get().ContainsKey(key))
			{
				Dic.JsonLoad_AddIfNotExist(key, value);
				Lists_by_ProductType.JsonLoad_AddToListValue(value.ProductType, value);
				Lists_by_ProductId.JsonLoad_AddToListValue(productId, value);
				if (!ProductIdList.JsonLoad_GetOrNew().Contains(productId))
				{
					ProductIdList.JsonLoad_GetOrNew().Add(productId);
				}
			}
		}
	}

	public const string TABLENAME = "NewPayShopProductTable";

	private static EStoreType _storeType;

	private DataSet _dataSet_for_Apple = new DataSet();

	private DataSet _dataSet_for_OneStore = new DataSet();

	private DataSet _dataSet_for_Samsung = new DataSet();

	private DataSet _dataSet_for_Google = new DataSet();

	public override TablePathDefine PathDefine => new TablePathDefine("NewPayShopProductTable");

	public static EStoreType StoreType
	{
		get
		{
			return _storeType;
		}
		set
		{
			_storeType = value;
		}
	}

	public NewPayShopProductDataContainer()
	{
		_dataSet_for_Apple.RegisterAndBind(base.Packer);
		_dataSet_for_OneStore.RegisterAndBind(base.Packer);
		_dataSet_for_Samsung.RegisterAndBind(base.Packer);
		_dataSet_for_Google.RegisterAndBind(base.Packer);
	}

	protected override void OnEndLoadFromJsonData()
	{
		foreach (NewPayShopProductData item in base.BaseList.JsonLoad_GetOrNew())
		{
			if (!String_Extension_Method.IsNullOrEmpty(item.AppleId))
			{
				_dataSet_for_Apple.AddValue(item.AppleId, item);
			}
			if (!String_Extension_Method.IsNullOrEmpty(item.OneStoreId))
			{
				_dataSet_for_OneStore.AddValue(item.OneStoreId, item);
			}
			if (!String_Extension_Method.IsNullOrEmpty(item.UDPStoreId))
			{
				_dataSet_for_Samsung.AddValue(item.UDPStoreId, item);
			}
			if (!String_Extension_Method.IsNullOrEmpty(item.GoogleId))
			{
				_dataSet_for_Google.AddValue(item.GoogleId, item);
			}
		}
	}

	private DataSet GetDataSet_for_CurrentStoreType()
	{
		return StoreType switch
		{
			EStoreType.Apple => _dataSet_for_Apple, 
			EStoreType.OneStore => _dataSet_for_OneStore, 
			EStoreType.Samsung => _dataSet_for_Samsung, 
			_ => _dataSet_for_Google, 
		};
	}

	public IList<NewPayShopProductData> GetDatasByProductType(ProductType type)
	{
		return GetDataSet_for_CurrentStoreType()?.Lists_by_ProductType.GetOrDefault(type);
	}

	public IList<string> GetPlatformProductIds()
	{
		return GetDataSet_for_CurrentStoreType()?.ProductIdList.Eval();
	}

	public NewPayShopProductData GetData(string productId, int productIndex)
	{
		return GetDataSet_for_CurrentStoreType()?.Dic.GetOrDefault(new NewPayShopProductDataKey2(productId, productIndex));
	}

	public IList<NewPayShopProductData> GetDatas(string productId)
	{
		IList<NewPayShopProductData> list = null;
		DataSet dataSet_for_CurrentStoreType = GetDataSet_for_CurrentStoreType();
		if (dataSet_for_CurrentStoreType != null)
		{
			list = dataSet_for_CurrentStoreType.Lists_by_ProductId.GetOrDefault(productId);
		}
		return __Extension_Method_NShared_ReadonlyEmpty__.GetEmptyIfNull(list);
	}

	public bool IsExcludeConsoleRequest(string key)
	{
		if (key == null)
		{
			return true;
		}
		IList<NewPayShopProductData> datas = GetDatas(key);
		if (datas != null && datas.Count > 0)
		{
			return false;
		}
		return true;
	}
}
