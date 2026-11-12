using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class EquipFilterOptionGroupDataContainer : JitDataContainerListOnly<EquipFilterOptionGroupData>
{
	public const string TABLENAME = "EquipFilterOptionGroupTable";

	private Dictionary<int, EquipFilterOptionGroupData> _datas_by_Index = new Dictionary<int, EquipFilterOptionGroupData>();

	private Dictionary<string, EquipFilterOptionGroupData> _datas_by_Key = new Dictionary<string, EquipFilterOptionGroupData>();

	public override TablePathDefine PathDefine => new TablePathDefine("EquipFilterOptionGroupTable");

	protected override void OnEndLoad_Final()
	{
		foreach (EquipFilterOptionGroupData allDatum in GetAllData())
		{
			_datas_by_Index[allDatum.Index] = allDatum;
			_datas_by_Key[allDatum.Key] = allDatum;
			allDatum.InitTypeData();
		}
	}

	public EquipFilterOptionGroupData GetData(int index)
	{
		if (_datas_by_Index.TryGetValue(index, out var value))
		{
			return value;
		}
		return null;
	}

	public EquipFilterOptionGroupData GetData(string groupKey)
	{
		return _datas_by_Key[groupKey];
	}
}
