using System.Collections.Generic;
using System.Linq;
using NShared.NJit;

namespace NShared;

public class PhaseStepRewardDataContainer : JitDataContainerDefault<PhaseStepRewardDataKey, PhaseStepRewardData>
{
	public const string TABLENAME = "PhaseStepRewardTable";

	public override TablePathDefine PathDefine => new TablePathDefine("PhaseStepRewardTable");

	public List<PhaseStepRewardData> GetPhaseStepDataList(int chapterIndex, int dungeonIndex)
	{
		return (from v in GetAllData()
			where v.ChapterIndex == chapterIndex && v.DungeonIndex == dungeonIndex
			select v).ToList();
	}

	public bool GetPhaseStepDataExist(int chapterIndex, int dungeonIndex)
	{
		return GetAllData().Any((PhaseStepRewardData v) => v.ChapterIndex == chapterIndex && v.DungeonIndex == dungeonIndex);
	}
}
