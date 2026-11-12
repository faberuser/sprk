using System;
using System.Linq;
using MessagePack;
using NShared.NJit;

namespace NShared;

public class PunishmentRaidRewardDataContainer : JitDataContainerDefault<int, PunishmentRaidRewardData>
{
	[MessagePackObject(false)]
	[JitKey]
	public struct RewardAndIndexKey
	{
		[Key(0)]
		public PunishmentRaidRewardType RewardType;

		[Key(1)]
		public int RaidIndex;

		public RewardAndIndexKey(PunishmentRaidRewardType rewardType, int raidIndex)
		{
			RewardType = rewardType;
			RaidIndex = raidIndex;
		}

		public bool Equals(RewardAndIndexKey other)
		{
			if (RewardType == other.RewardType)
			{
				return RaidIndex == other.RaidIndex;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return JitOption.BuildHash(RewardType, RaidIndex);
		}

		public override string ToString()
		{
			return $"{GetType().Name}({RewardType}, {RaidIndex})";
		}
	}

	public const string TABLENAME = "PunishmentRaidRewardTable";

	private JitDictionary<RewardAndIndexKey, PunishmentRaidRewardData> _dicByRewardType = new JitDictionary<RewardAndIndexKey, PunishmentRaidRewardData>();

	public override TablePathDefine PathDefine => new TablePathDefine("PunishmentRaidRewardTable");

	public PunishmentRaidRewardDataContainer()
	{
		base.Packer.RegisterAndBind(_dicByRewardType);
	}

	protected override void OnEndLoadFromJsonData()
	{
		foreach (PunishmentRaidRewardData item in base.BaseList.JsonLoad_GetOrNew())
		{
			if (item.RewardType != PunishmentRaidRewardType.None)
			{
				_dicByRewardType.JsonLoad_AddIfNotExist(new RewardAndIndexKey(item.RewardType, item.RaidIndex), item);
			}
		}
	}

	public PunishmentRaidRewardData GetPunishmentRaidRewardData(PunishmentRaidRewardType rewardType, int raidIndex)
	{
		return _dicByRewardType.GetOrDefault(new RewardAndIndexKey(rewardType, raidIndex));
	}

	public PunishmentRaidRewardData GetDataByClearCheckIndices(int[] indices)
	{
		Array.Sort(indices);
		foreach (PunishmentRaidRewardData allDatum in GetAllData())
		{
			if (!allDatum.Active)
			{
				continue;
			}
			int[] clearCheckIndex = allDatum.ClearCheckIndex;
			if (clearCheckIndex.Length == indices.Length)
			{
				Array.Sort(clearCheckIndex);
				if (clearCheckIndex.SequenceEqual(indices))
				{
					return allDatum;
				}
			}
		}
		return null;
	}
}
