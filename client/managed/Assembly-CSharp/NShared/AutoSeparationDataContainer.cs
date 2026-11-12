using System.Linq;
using NShared.NJit;

namespace NShared;

public class AutoSeparationDataContainer : JitDataContainerDefault<int, AutoSeparationData>
{
	public const string TABLENAME = "AutoSeparationTable";

	public override TablePathDefine PathDefine => new TablePathDefine("AutoSeparationTable");

	public AutoSeparationData GetData(BattleType battleType, int chapterIndex = 0, int dungeonIndex = 0)
	{
		AutoSeparationData rData = null;
		if (chapterIndex > 0 && dungeonIndex > 0)
		{
			ForEachData(delegate(AutoSeparationData data)
			{
				if (data.BattleType.Contains(battleType) && data.ChapterIndex == chapterIndex && data.DungeonIndex == dungeonIndex)
				{
					rData = data;
				}
			});
		}
		if (rData == null)
		{
			ForEachData(delegate(AutoSeparationData data)
			{
				if (data.BattleType.Contains(battleType))
				{
					rData = data;
				}
			});
		}
		return rData;
	}
}
