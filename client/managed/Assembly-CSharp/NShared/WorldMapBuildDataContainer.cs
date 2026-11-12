using System.Collections.Generic;
using System.Linq;
using NShared.NJit;

namespace NShared;

public class WorldMapBuildDataContainer : JitDataContainerListOnly<WorldMapBuildData>
{
	public const string TABLENAME = "WorldMapBuildTable";

	private JitDictionary<int, List<WorldMapBuildData>> _dataListByChapter = new JitDictionary<int, List<WorldMapBuildData>>();

	public override TablePathDefine PathDefine => new TablePathDefine("WorldMapBuildTable");

	public WorldMapBuildDataContainer()
	{
		base.Packer.RegisterAndBind(_dataListByChapter);
	}

	protected override void OnEndLoadFromJsonData()
	{
		foreach (WorldMapBuildData item in base.BaseList.JsonLoad_GetOrNew())
		{
			_dataListByChapter.JsonLoad_AddToListValue(item.ChapterIndex, item);
		}
	}

	public WorldMapBuildData GetData(int buildingIndex)
	{
		return (from v in GetAllData()
			where v.BuildingIndex == buildingIndex
			select v).FirstOrDefault();
	}

	public IList<WorldMapBuildData> GetBuildDataOfChapter(int chapterIndex)
	{
		return __Extension_Method_NShared_ReadonlyEmpty__.GetEmptyIfNull(_dataListByChapter.GetOrDefault(chapterIndex));
	}

	public bool IsBuildingContainsByChatper(int chapterIndex)
	{
		List<WorldMapBuildData> value;
		return _dataListByChapter.TryGetValue(chapterIndex, out value);
	}
}
