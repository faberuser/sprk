using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class KingsPassLevelDataContainer : JitDataContainerDefault<KingsPassLevelKey, KingsPassLevelData>
{
	public const string TABLENAME = "KingsPassLevelTable";

	private JitDictionary<int, List<KingsPassLevelData>> _lists_by_LevelIndex = new JitDictionary<int, List<KingsPassLevelData>>();

	public override TablePathDefine PathDefine => new TablePathDefine("KingsPassLevelTable");

	public KingsPassLevelDataContainer()
	{
		base.Packer.RegisterAndBind(_lists_by_LevelIndex);
	}

	protected override void OnEndLoadFromJsonData()
	{
		foreach (KingsPassLevelData item in base.BaseList.JsonLoad_GetOrNew())
		{
			_lists_by_LevelIndex.JsonLoad_AddToListValue(item.LevelIndex, item);
		}
	}

	public IList<KingsPassLevelData> GetLevelDatas(int levelIndex)
	{
		return _lists_by_LevelIndex.GetOrDefault(levelIndex);
	}

	public KingsPassLevelData GetData(int levelIndex, int level)
	{
		return GetData(new KingsPassLevelKey(levelIndex, level));
	}
}
