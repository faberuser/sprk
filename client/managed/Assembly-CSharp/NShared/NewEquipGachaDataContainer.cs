using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class NewEquipGachaDataContainer : JitDataContainerDefault<int, NewEquipGachaData>
{
	public const string TABLENAME = "NewEquipGachaTable";

	private JitDictionary<string, List<NewEquipGachaData>> _lists_by_Category = new JitDictionary<string, List<NewEquipGachaData>>();

	private JitDictionary<EquipGachaType, List<NewEquipGachaData>> _lists_by_Type = new JitDictionary<EquipGachaType, List<NewEquipGachaData>>();

	public override TablePathDefine PathDefine => new TablePathDefine("NewEquipGachaTable");

	public NewEquipGachaDataContainer()
	{
		base.Packer.RegisterAndBind(_lists_by_Category);
		base.Packer.RegisterAndBind(_lists_by_Type);
	}

	protected override void OnEndLoadFromJsonData()
	{
		foreach (NewEquipGachaData item in base.BaseList.JsonLoad_GetOrNew())
		{
			if (!String_Extension_Method.IsNullOrEmpty(item.Category))
			{
				_lists_by_Category.JsonLoad_AddToListValue(item.Category, item);
			}
			_lists_by_Type.JsonLoad_AddToListValue(item.Type, item);
		}
	}

	public IList<NewEquipGachaData> GetDatasByCategory(string category)
	{
		return _lists_by_Category.GetOrDefault(category);
	}

	public IList<NewEquipGachaData> GetDataListTypeOf(EquipGachaType gachaType)
	{
		return _lists_by_Type.GetOrDefault(gachaType);
	}
}
