using MessagePack;

namespace NShared;

[MessagePackObject(false)]
[JsonSerializable(Javascript = true)]
public class NewEquipGachaData : IDataKeySelector<int>, IPayShopCardData
{
	int IDataKeySelector<int>.Key => Index;

	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public EquipGachaType Type { get; set; }

	[Key(2)]
	public string Name { get; set; }

	[Key(3)]
	public string Desc { get; set; }

	[Key(4)]
	public int CardType { get; set; }

	[Key(5)]
	public string Category { get; set; }

	[Key(6)]
	public int Durational { get; set; }

	[Key(7)]
	public string BeginTime { get; set; }

	[Key(8)]
	public string EndTime { get; set; }

	[Key(9)]
	public int FreeCoolTime { get; set; }

	[Key(10)]
	public byte Bid { get; set; }

	[Key(11)]
	public byte BonusBid { get; set; }

	[Key(12)]
	public int OpenGold { get; set; }

	[Key(13)]
	public int OpenGem { get; set; }

	[Key(14)]
	public int Mileage { get; set; }

	[Key(15)]
	public int DailyMaxFree { get; set; }

	[Key(16)]
	public bool ShowProbability { get; set; }

	[Key(17)]
	public int[] PickupHeroIndices { get; set; }

	[Key(18)]
	public int PickupHeroCount { get; set; }

	[Key(19)]
	public EquipGachaSubType GachaSubType { get; set; }

	[Key(20)]
	public string GachaCategorySelectTitle { get; set; }

	[Key(21)]
	public string GachaSceneDesc { get; set; }

	[Key(22)]
	public int GachaShowIndex { get; set; }

	[Key(23)]
	public string PickupGachaExpiredMessage { get; set; }

	[Key(24)]
	public int GachaCategoryGroupIndex { get; set; }

	[Key(25)]
	public bool UniqueAwake { get; set; }

	[Key(26)]
	public int DailyMaxSale { get; set; }

	[Key(27)]
	public int[] StepUpHeroIndices { get; set; }

	[Key(28)]
	public int[] StepUpHeroCostumeIndices { get; set; }

	[Key(29)]
	public string IconPath { get; set; }

	[Key(30)]
	public bool TicketOnly { get; set; }

	[Key(31)]
	public int StarterMileage { get; set; }

	[Key(32)]
	public int[] RewardIndices { get; set; }

	[Key(33)]
	public string AppearNodeTree { get; set; }

	[Key(34)]
	public string ReactionNodeTree { get; set; }

	[Key(35)]
	public string[] NodeTree { get; set; }

	[Key(36)]
	public int OpenDate { get; set; }

	[Key(37)]
	public string[] OpenCondition { get; set; }

	[Key(38)]
	public int MileageAmountForReward { get; set; }

	[Key(39)]
	public int StepUpAccessoryIndex { get; set; }

	[Key(40)]
	public float[] StepUpAccessoryOffset { get; set; }

	[Key(41)]
	public float[] StepUpAccessoryRotation { get; set; }

	[Key(42)]
	public float StepUpAccessoryScale { get; set; }

	[Key(43)]
	public bool AllowExpiredTicket { get; set; }

	[Key(44)]
	public int PurchaseRestrictProductIndex { get; set; }

	public override string ToString()
	{
		return "EquipGachaData(index=" + Index + ")";
	}

	public int GetStepUpHeroIndex(int index)
	{
		return GetArrayValue(StepUpHeroIndices, index, 0);
	}

	public int GetStepUpHeroCostumeIndex(int index)
	{
		return GetArrayValue(StepUpHeroCostumeIndices, index, 0);
	}

	private static T GetArrayValue<T>(T[] arrays, int index, T defaultValue = default(T))
	{
		if (arrays == null)
		{
			return defaultValue;
		}
		if (arrays.Length <= index)
		{
			return defaultValue;
		}
		return arrays[index];
	}

	public string GetGachaBackGround()
	{
		return GetArrayValue(NodeTree, 0, "Low");
	}

	public string GetLuaWeaponPrefix()
	{
		return GetArrayValue(NodeTree, 1, "Blue");
	}

	public bool IsStarterPickupGacha()
	{
		if (Type == EquipGachaType.Pickup)
		{
			return GachaSubType == EquipGachaSubType.Starter;
		}
		return false;
	}
}
