using MessagePack;

namespace NShared;

[MessagePackObject(false)]
[JsonSerializable(Javascript = true)]
public class MissionCategoryData : IDataKeySelector<MissionCategoryKey>
{
	[Key(0)]
	public int MainCategoryIndex { get; set; }

	[Key(1)]
	public int SubCategoryIndex { get; set; }

	[Key(2)]
	public EMissionType MissionType { get; set; }

	[Key(3)]
	public string MainCategoryLocalKey { get; set; }

	[Key(4)]
	public string SubCategoryLocalKey { get; set; }

	[Key(5)]
	public string RewardQuestType { get; set; }

	[Key(6)]
	public int RewardQuestIndex { get; set; }

	[Key(7)]
	public string UIPropName { get; set; }

	[Key(8)]
	public string SubCategoryButtonPath { get; set; }

	[Key(9)]
	public string UIBackgroundPath { get; set; }

	[Key(10)]
	public string OpenConditionType { get; set; }

	[Key(11)]
	public string[] OpenConditionValue { get; set; }

	[Key(12)]
	public string OpenConditionLocalKey { get; set; }

	[Key(13)]
	public int MissionComponentValue { get; set; }

	[Key(14)]
	public string ButtonPrefabName { get; set; }

	[Key(15)]
	public string SubCategoryDescLocalKey { get; set; }

	[Key(16)]
	public string IconLocalKey { get; set; }

	[Key(17)]
	public bool IsReservePopup { get; set; }

	[Key(18)]
	public string CtrAdid { get; set; }

	[Key(19)]
	public string PopupTitleLocalKey { get; set; }

	[Key(20)]
	public string PopupDescLocalKey { get; set; }

	[Key(21)]
	public string PopupImageAssetPath { get; set; }

	[Key(22)]
	public EMissionTopViewLayout TopViewLayoutType { get; set; }

	[Key(23)]
	public EMissionTopViewButtonGroup TopViewButtonGroupType { get; set; }

	[Key(24)]
	public EMissionTopViewRewardType TopViewRewardType { get; set; }

	MissionCategoryKey IDataKeySelector<MissionCategoryKey>.Key => new MissionCategoryKey(MainCategoryIndex, SubCategoryIndex);

	public MissionCategoryData GetNextData()
	{
		return GameRoot.Instance.MissionCategoryDataContainer.GetData(MainCategoryIndex, SubCategoryIndex + 1);
	}

	public override string ToString()
	{
		return $"MissionCategory MainIndex:{MainCategoryIndex} SubIndex:{SubCategoryIndex}";
	}

	public bool EqualMissionType(EMissionType missionType)
	{
		return MissionType == missionType;
	}

	public bool IsAnySpecialMission()
	{
		if (MissionType != EMissionType.Special && MissionType != EMissionType.Special2)
		{
			return MissionType == EMissionType.Special3;
		}
		return true;
	}

	public bool IsSpecialMission()
	{
		return MissionType == EMissionType.Special;
	}

	public bool IsSpecial2Mission()
	{
		return MissionType == EMissionType.Special2;
	}

	public bool IsSpecial3Mission()
	{
		return MissionType == EMissionType.Special3;
	}

	public bool IsWebEvent()
	{
		return MissionType == EMissionType.Event;
	}

	public bool IsDailyMission()
	{
		return MissionType == EMissionType.Daily;
	}

	public bool IsWeeklyMission()
	{
		return MissionType == EMissionType.Weekly;
	}

	public bool IsHeroMission()
	{
		return MissionType == EMissionType.Hero;
	}

	public bool IsMainMission()
	{
		return MissionType == EMissionType.Main;
	}

	public bool IsNodeMission()
	{
		return MissionType == EMissionType.Node;
	}

	public bool IsSpecialMissionType()
	{
		if (MissionType != EMissionType.Special && MissionType != EMissionType.Special2)
		{
			return MissionType == EMissionType.Special3;
		}
		return true;
	}
}
