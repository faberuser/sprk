using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class WorldMapPropDataContainer : JitDataContainerListOnly<WorldMapPropData>
{
	public const string TABLENAME = "WorldMapPropTable";

	private JitDictionary<int, List<WorldMapPropData>> _dataListByChapter = new JitDictionary<int, List<WorldMapPropData>>();

	public override TablePathDefine PathDefine => new TablePathDefine("WorldMapPropTable");

	public WorldMapPropDataContainer()
	{
		base.Packer.RegisterAndBind(_dataListByChapter);
	}

	protected override void OnEndLoadFromJsonData()
	{
		foreach (WorldMapPropData item in base.BaseList.JsonLoad_GetOrNew())
		{
			_dataListByChapter.JsonLoad_AddToListValue(item.ChapterIndex, item);
		}
	}

	public IList<WorldMapPropData> GetPropDataOfChapter(int chapterIndex)
	{
		return __Extension_Method_NShared_ReadonlyEmpty__.GetEmptyIfNull(_dataListByChapter.GetOrDefault(chapterIndex));
	}
}
