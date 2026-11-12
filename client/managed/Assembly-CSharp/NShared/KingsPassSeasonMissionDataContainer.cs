using MessagePack;
using NShared.NJit;

namespace NShared;

public class KingsPassSeasonMissionDataContainer : JitDataContainerDefault<KingsPassMissionKey2, KingsPassSeasonMissionData>
{
	[MessagePackObject(false)]
	[JitKey]
	public struct RewardKey2
	{
		[Key(0)]
		public int SeasonIndex;

		[Key(1)]
		public KingsPassType PassType;

		[Key(2)]
		public int Level;

		public RewardKey2(int seasonIndex, KingsPassType passType, int level)
		{
			SeasonIndex = seasonIndex;
			PassType = passType;
			Level = level;
		}

		public bool Equals(RewardKey2 other)
		{
			if (SeasonIndex == other.SeasonIndex && PassType == other.PassType)
			{
				return Level == other.Level;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return JitOption.BuildHash(SeasonIndex, PassType, Level);
		}

		public override string ToString()
		{
			return $"SeasonIndex={SeasonIndex} PassType={PassType} Level={Level}";
		}
	}

	public const string TABLENAME = "KingsPassSeasonMissionTable";

	private JitDictionary<RewardKey2, KingsPassSeasonMissionData> _dicByRewardKey = new JitDictionary<RewardKey2, KingsPassSeasonMissionData>();

	public override TablePathDefine PathDefine => new TablePathDefine("KingsPassSeasonMissionTable");

	public KingsPassSeasonMissionDataContainer()
	{
		base.Packer.RegisterAndBind(_dicByRewardKey);
	}

	protected override void OnEndLoadFromJsonData()
	{
		foreach (KingsPassSeasonMissionData item in base.BaseList.JsonLoad_GetOrNew())
		{
			RewardKey2 key = new RewardKey2(item.SeasonIndex, item.Type, item.Level);
			_dicByRewardKey.JsonLoad_Get()[key] = item;
		}
	}

	public KingsPassSeasonMissionData GetDataByRewardKey(int sessionIndex, KingsPassType kingsPassType, int level)
	{
		return ___Shared_Jit_JitDictionary___.GetOrDefault(key: new RewardKey2(sessionIndex, kingsPassType, level), This: _dicByRewardKey);
	}

	public KingsPassSeasonMissionData GetData(int sessionIndex, int missionIndex)
	{
		return GetData(new KingsPassMissionKey2(sessionIndex, missionIndex));
	}
}
