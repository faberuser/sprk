using MessagePack;

namespace NShared;

[MessagePackObject(false)]
[JsonSerializable(Javascript = true)]
public class MissionVisualData : IDataKeySelector<MissionVisualKey>
{
	MissionVisualKey IDataKeySelector<MissionVisualKey>.Key => new MissionVisualKey(MainCategoryIndex, SubCategoryIndex, MainStep, SubStep);

	[Key(0)]
	public int MainCategoryIndex { get; set; }

	[Key(1)]
	public int SubCategoryIndex { get; set; }

	[Key(2)]
	public string QuestType { get; set; }

	[Key(3)]
	public int QuestIndex { get; set; }

	[Key(4)]
	public int MissionGradeValue { get; set; }

	[Key(5)]
	public int MainStep { get; set; }

	[Key(6)]
	public int SubStep { get; set; }

	[Key(7)]
	public string NodeIconPath { get; set; }

	[Key(8)]
	public string NodeIconLocalKey { get; set; }

	[Key(9)]
	public string MilestonePopupNpcCode { get; set; }

	[Key(10)]
	public string[] DetailDisplayLocalize { get; set; }

	public override string ToString()
	{
		return $"Mission QuestType:{QuestType} Index:{QuestIndex}";
	}

	public MissionVisualData GetNextMainStepData()
	{
		MissionVisualKey key = new MissionVisualKey(MainCategoryIndex, SubCategoryIndex, MainStep + 1, 0);
		return GameRoot.Instance.MissionVisualDataContainer.GetData(key);
	}

	public MissionVisualData GetNextSubStepData()
	{
		MissionVisualKey key = new MissionVisualKey(MainCategoryIndex, SubCategoryIndex, MainStep, SubStep + 1);
		return GameRoot.Instance.MissionVisualDataContainer.GetData(key);
	}

	public MissionVisualData GetNextData()
	{
		MissionVisualData nextSubStepData = GetNextSubStepData();
		if (nextSubStepData != null)
		{
			return nextSubStepData;
		}
		return GetNextMainStepData();
	}
}
