using System.Collections.Generic;
using System.Linq;
using MessagePack;
using NShared.NJit;

namespace NShared;

public class StepUpGachaRewardDataContainer : JitDataContainerDefault<StepupGachaRewardDataKey, StepUpGachaRewardData>
{
	[MessagePackObject(false)]
	[JitKey]
	public struct GachaRoundKey
	{
		[Key(0)]
		public int Index;

		[Key(1)]
		public int Round;

		public GachaRoundKey(int index, int round)
		{
			Index = index;
			Round = round;
		}

		public bool Equals(GachaRoundKey other)
		{
			if (Index == other.Index)
			{
				return Round == other.Round;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return JitOption.BuildHash(Index, Round);
		}

		public override string ToString()
		{
			return $"Index={Index} Round={Round}";
		}
	}

	public const string TABLENAME = "StepUpGachaRewardTable";

	private JitDictionary<int, List<StepUpGachaRewardData>> _dicGachaRewardList = new JitDictionary<int, List<StepUpGachaRewardData>>();

	private JitDictionary<GachaRoundKey, List<StepUpGachaRewardData>> _dicGachaRoundList = new JitDictionary<GachaRoundKey, List<StepUpGachaRewardData>>();

	public override TablePathDefine PathDefine => new TablePathDefine("StepUpGachaRewardTable");

	public StepUpGachaRewardDataContainer()
	{
		base.Packer.RegisterAndBind(_dicGachaRewardList);
		base.Packer.RegisterAndBind(_dicGachaRoundList);
	}

	protected override void OnEndLoadFromJsonData()
	{
		foreach (StepUpGachaRewardData item in base.BaseList.JsonLoad_GetOrNew())
		{
			_dicGachaRewardList.JsonLoad_AddToListValue(item.Index, item);
			_dicGachaRoundList.JsonLoad_AddToListValue(new GachaRoundKey(item.Index, item.Round), item);
		}
	}

	public StepUpGachaRewardData GetData(int index, int step)
	{
		return GetData(new StepupGachaRewardDataKey(index, step));
	}

	public StepUpGachaRewardData GetNexData(StepUpGachaRewardData data)
	{
		if (data == null)
		{
			return null;
		}
		return GetData(new StepupGachaRewardDataKey(data.Index, data.NextStep));
	}

	public IList<StepUpGachaRewardData> GetStepupGachaListByIndex(int index)
	{
		return _dicGachaRewardList.GetOrDefault(index);
	}

	public IList<StepUpGachaRewardData> GetStepupGachaRoundList(int index, int round)
	{
		return _dicGachaRoundList.GetOrDefault(new GachaRoundKey(index, round));
	}

	public int GetStepupGachaCount(int index, int round)
	{
		int result = 10;
		IList<StepUpGachaRewardData> stepupGachaRoundList = GetStepupGachaRoundList(index, round);
		StepUpGachaRewardData stepUpGachaRewardData = stepupGachaRoundList.LastOrDefault();
		StepUpGachaRewardData stepUpGachaRewardData2 = stepupGachaRoundList.FirstOrDefault();
		if (stepUpGachaRewardData != null && stepUpGachaRewardData2 != null)
		{
			result = 1 + (stepUpGachaRewardData.Step - stepUpGachaRewardData2.Step);
		}
		return result;
	}
}
