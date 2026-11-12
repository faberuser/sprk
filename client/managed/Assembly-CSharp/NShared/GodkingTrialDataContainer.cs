using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class GodkingTrialDataContainer : JitDataContainerDefault<ChapterDungeonKey2, GodkingTrialData>
{
	public const string TABLENAME = "GodkingTrialTable";

	private JitDictionary<int, List<int>> _difficultyToDataTable = new JitDictionary<int, List<int>>();

	public override TablePathDefine PathDefine => new TablePathDefine("GodkingTrialTable");

	public GodkingTrialDataContainer()
	{
		base.Packer.RegisterAndBind(_difficultyToDataTable);
	}

	protected override void OnEndLoadFromJsonData()
	{
		foreach (GodkingTrialData item in base.BaseList.JsonLoad_GetOrNew())
		{
			_difficultyToDataTable.JsonLoad_AddToListValue(item.ChapterIndex, item.DungeonIndex);
		}
		_difficultyToDataTable.JsonLoad_SortListValue((int lhs, int rhs) => lhs.CompareTo(rhs));
	}

	public GodkingTrialData GetData(int chapterIndex, int dungeonIndex)
	{
		return GetData(new ChapterDungeonKey2(chapterIndex, dungeonIndex));
	}

	public IList<int> GetDifficultyList(int chapterIndex)
	{
		return __Extension_Method_NShared_ReadonlyEmpty__.GetEmptyIfNull(_difficultyToDataTable.GetOrDefault(chapterIndex));
	}

	public int GetMinDifficulty(int chapterIndex)
	{
		IList<int> difficultyList = GetDifficultyList(chapterIndex);
		if (difficultyList == null)
		{
			return 1;
		}
		if (1 > difficultyList.Count)
		{
			return 1;
		}
		return difficultyList[0];
	}

	public int GetMaxDifficulty(int chapterIndex)
	{
		return GetDifficultyList(chapterIndex)[^1];
	}
}
