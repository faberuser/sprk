using NShared.NJit;

namespace NShared;

public class KarmaDungeonDataContainer : JitDataContainerDefault<KarmaDungeonDataKey, KarmaDungeonData>
{
	public const string TABLENAME = "KarmaDungeonTable";

	private JitDictionary<ChapterDungeonKey2, KarmaDungeonData> _karmaDungeonDataByDungeon = new JitDictionary<ChapterDungeonKey2, KarmaDungeonData>();

	public override TablePathDefine PathDefine => new TablePathDefine("KarmaDungeonTable");

	public KarmaDungeonDataContainer()
	{
		base.Packer.RegisterAndBind(_karmaDungeonDataByDungeon);
	}

	protected override void OnEndLoadFromJsonData()
	{
		foreach (KarmaDungeonData item in base.BaseList.JsonLoad_GetOrNew())
		{
			_karmaDungeonDataByDungeon.JsonLoad_AddIfNotExist(new ChapterDungeonKey2(item.ChapterIndex, item.DungeonIndex), item);
		}
	}

	public KarmaDungeonData GetData(int punishGroupIndex, int stageLevel)
	{
		return GetData(new KarmaDungeonDataKey(punishGroupIndex, stageLevel));
	}

	public KarmaDungeonData GetDataByChapterDungeon(int chapterIndex, int dungeonIndex)
	{
		return _karmaDungeonDataByDungeon.GetOrDefault(new ChapterDungeonKey2(chapterIndex, dungeonIndex));
	}

	public int GetPunishmentGroupIndex(int chapterIndex, int dungeonIndex)
	{
		return GetDataByChapterDungeon(chapterIndex, dungeonIndex)?.PunishmentGroupIndex ?? (-1);
	}
}
