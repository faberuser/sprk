using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class ItemBaseDataContainer : JitDataContainerDefault<int, ItemBaseData>
{
	public const string TABLENAME = "ItemTable";

	private JitDictionary<string, ItemBaseData> _dicByCode = new JitDictionary<string, ItemBaseData>();

	private JitDictionary<ItemType, List<ItemBaseData>> _dicByType = new JitDictionary<ItemType, List<ItemBaseData>>();

	private JitRef<List<int>> _maxCapList = new JitRef<List<int>>();

	public override TablePathDefine PathDefine => new TablePathDefine("ItemTable");

	public ItemBaseDataContainer()
	{
		base.Packer.RegisterAndBind(_dicByCode);
		base.Packer.RegisterAndBind(_dicByType);
		base.Packer.RegisterAndBind(_maxCapList);
	}

	protected override void OnEndLoadFromJsonData()
	{
		base.OnEndLoadFromJsonData();
		foreach (KeyValuePair<int, ItemBaseData> item in base.BaseDic.JsonLoad_Get())
		{
			if (_dicByCode.JsonLoad_Get().TryGetValue(item.Value.Code, out var _))
			{
				_dicByCode.JsonLoad_Get()[item.Value.Code] = item.Value;
			}
			else
			{
				_dicByCode.JsonLoad_Get().Add(item.Value.Code, item.Value);
			}
			_dicByType.JsonLoad_AddToListValue(item.Value.Type, item.Value);
			if (item.Value.ItemMaxCap > 0)
			{
				_maxCapList.JsonLoad_GetOrNew().Add(item.Value.Index);
			}
		}
	}

	public ItemBaseData GetDataByCode(string code)
	{
		if (string.IsNullOrEmpty(code))
		{
			return null;
		}
		return _dicByCode.GetOrDefault(code);
	}

	public IList<ItemBaseData> GetItemDataListByType(ItemType type)
	{
		return __Extension_Method_NShared_ReadonlyEmpty__.GetEmptyIfNull(_dicByType.GetOrDefault(type));
	}

	public IList<int> GetMaxCapIndices()
	{
		return _maxCapList.Eval();
	}
}
