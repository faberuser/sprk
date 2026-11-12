using NShared.NJit;

namespace NShared;

public class ShakmehBossDataContainer : JitDataContainerDefault<ChapterDungeonKey2, ShakmehBossData>
{
	public const string TABLENAME = "ShakmehBossTable";

	private JitDictionary<ShakmehBossType, ShakmehBossData> _dataByType = new JitDictionary<ShakmehBossType, ShakmehBossData>();

	public override TablePathDefine PathDefine => new TablePathDefine("ShakmehBossTable");

	public ShakmehBossDataContainer()
	{
		base.Packer.RegisterAndBind(_dataByType);
	}

	protected override void OnEndLoadFromJsonData()
	{
		foreach (ShakmehBossData item in base.BaseList.JsonLoad_GetOrNew())
		{
			_dataByType.JsonLoad_AddIfNotExist(item.BossType, item);
		}
	}

	public ShakmehBossData GetData(int chapterIndex, int dungeonIndex)
	{
		return GetData(new ChapterDungeonKey2(chapterIndex, dungeonIndex));
	}

	public ShakmehBossData GetDataByBossType(ShakmehBossType bossType)
	{
		return _dataByType.GetOrDefault(bossType);
	}
}
