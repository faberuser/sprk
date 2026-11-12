using NShared.NJit;

namespace NShared;

public class ShopItemFilterDataContainer : JitDataContainerDefault<int, ShopItemFilterData>
{
	public const string TABLENAME = "ShopItemFilterTable";

	private JitDictionary<string, ShopItemFilterData> _itemDataByCodeDictionary = new JitDictionary<string, ShopItemFilterData>();

	public override TablePathDefine PathDefine => new TablePathDefine("ShopItemFilterTable");

	public ShopItemFilterDataContainer()
	{
		base.Packer.RegisterAndBind(_itemDataByCodeDictionary);
	}

	protected override void OnEndLoadFromJsonData()
	{
		foreach (ShopItemFilterData item in base.BaseList.JsonLoad_GetOrNew())
		{
			string itemCode = item.ItemCode;
			if (!string.IsNullOrEmpty(itemCode))
			{
				_itemDataByCodeDictionary.JsonLoad_AddIfNotExist(itemCode, item);
			}
		}
	}

	public ShopItemFilterData GetDataByCode(string code)
	{
		if (string.IsNullOrEmpty(code))
		{
			return null;
		}
		return _itemDataByCodeDictionary.GetOrDefault(code);
	}
}
