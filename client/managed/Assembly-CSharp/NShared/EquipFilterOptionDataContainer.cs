using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class EquipFilterOptionDataContainer : JitDataContainerListOnly<EquipFilterOptionData>
{
	public const string TABLENAME = "EquipFilterOptionTable";

	private Dictionary<string, List<EquipFilterOptionData>> _groupTable = new Dictionary<string, List<EquipFilterOptionData>>();

	private Dictionary<string, EquipFilterOptionData> _keyTable = new Dictionary<string, EquipFilterOptionData>();

	public override TablePathDefine PathDefine => new TablePathDefine("EquipFilterOptionTable");

	protected override void OnEndLoad_Final()
	{
		foreach (EquipFilterOptionData allDatum in GetAllData())
		{
			_keyTable[allDatum.Key] = allDatum;
			if (!_groupTable.ContainsKey(allDatum.Group))
			{
				_groupTable[allDatum.Group] = new List<EquipFilterOptionData>();
			}
			List<EquipFilterOptionData> list = _groupTable[allDatum.Group];
			if (list == null)
			{
				break;
			}
			allDatum.ConvNullToZero();
			list.Add(allDatum);
		}
	}

	public EquipFilterOptionData GetData(string optionKey)
	{
		if (!_keyTable.ContainsKey(optionKey))
		{
			return null;
		}
		return _keyTable[optionKey];
	}

	public EquipFilterOptionData[] GetEquipFilterOptionDataListByGroup(string groupKey)
	{
		if (!_groupTable.ContainsKey(groupKey))
		{
			return new EquipFilterOptionData[0];
		}
		return _groupTable[groupKey].ToArray();
	}
}
