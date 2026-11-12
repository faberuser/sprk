using System.Collections.Generic;
using System.Linq;
using NShared.NJit;

namespace NShared;

public class GuildBuildingDataContainer : JitDataContainerDefault<GuildBuildingDataKey, GuildBuildingData>
{
	public const string TABLENAME = "GuildBuildingTable";

	private JitRef<List<int>> _allIndices = new JitRef<List<int>>();

	private JitDictionary<int, List<GuildBuildingData>> _dicByIndex = new JitDictionary<int, List<GuildBuildingData>>();

	public override TablePathDefine PathDefine => new TablePathDefine("GuildBuildingTable");

	public GuildBuildingDataContainer()
	{
		base.Packer.RegisterAndBind(_allIndices);
		base.Packer.RegisterAndBind(_dicByIndex);
	}

	protected override void OnEndLoadFromJsonData()
	{
		_allIndices.JsonLoad_Set((from v in base.BaseList.JsonLoad_GetOrNew()
			select v.Index).Distinct().ToList());
		foreach (GuildBuildingData item in base.BaseList.JsonLoad_GetOrNew())
		{
			_dicByIndex.JsonLoad_AddToListValue(item.Index, item);
		}
	}

	public IList<int> GetAllGuildBuildingIndices()
	{
		return _allIndices.Eval();
	}

	public GuildBuildingData GetData(int index, int level)
	{
		return GetData(new GuildBuildingDataKey(index, level));
	}

	public List<GuildBuildingData> GetDatasByIndex(int index)
	{
		_dicByIndex.TryGetValue(index, out var value);
		return value;
	}

	public List<GuildBuildingData> GetDatasByType(EGuildBuildingType buildingType)
	{
		return GetDatasByIndex((int)buildingType);
	}

	public bool CanUpgrade(int index, int level)
	{
		return GetData(new GuildBuildingDataKey(index, level))?.CanUpgrade ?? false;
	}

	public int GetNeedWoodPoint(int index, int level)
	{
		return GetData(new GuildBuildingDataKey(index, level))?.ReqWood ?? 0;
	}

	public int GetReqStone(int index, int level)
	{
		return GetData(new GuildBuildingDataKey(index, level))?.ReqStone ?? 0;
	}

	public int GetReqMetal(int index, int level)
	{
		return GetData(new GuildBuildingDataKey(index, level))?.ReqMetal ?? 0;
	}

	public int GetReqGuildLevel(int index, int level)
	{
		return GetData(new GuildBuildingDataKey(index, level))?.ReqGuildLevel ?? 0;
	}
}
