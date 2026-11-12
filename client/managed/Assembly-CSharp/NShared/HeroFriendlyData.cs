using System.Collections.Generic;
using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class HeroFriendlyData : IDataKeySelector<int>
{
	private List<HeroFriendlySubData> _data = new List<HeroFriendlySubData>();

	int IDataKeySelector<int>.Key => HeroIndex;

	[Key(0)]
	public int HeroIndex { get; set; }

	[Key(1)]
	public int NeedFriendlyPoint { get; set; }

	[Key(2)]
	public FriendlyActionType Action1 { get; set; }

	[Key(3)]
	public int GetMinPoint1 { get; set; }

	[Key(4)]
	public int GetMaxPoint1 { get; set; }

	[Key(5)]
	public ShopCostType ActionCostType1 { get; set; }

	[Key(6)]
	public int ActionCostValue1 { get; set; }

	[Key(7)]
	public FriendlyActionType Action2 { get; set; }

	[Key(8)]
	public int GetMinPoint2 { get; set; }

	[Key(9)]
	public int GetMaxPoint2 { get; set; }

	[Key(10)]
	public ShopCostType ActionCostType2 { get; set; }

	[Key(11)]
	public int ActionCostValue2 { get; set; }

	[Key(12)]
	public FriendlyActionType Action3 { get; set; }

	[Key(13)]
	public int GetMinPoint3 { get; set; }

	[Key(14)]
	public int GetMaxPoint3 { get; set; }

	[Key(15)]
	public ShopCostType ActionCostType3 { get; set; }

	[Key(16)]
	public int ActionCostValue3 { get; set; }

	[Key(17)]
	public FriendlyActionType Action4 { get; set; }

	[Key(18)]
	public int GetMinPoint4 { get; set; }

	[Key(19)]
	public int GetMaxPoint4 { get; set; }

	[Key(20)]
	public ShopCostType ActionCostType4 { get; set; }

	[Key(21)]
	public int ActionCostValue4 { get; set; }

	[Key(22)]
	public FriendlyActionType Action5 { get; set; }

	[Key(23)]
	public int GetMinPoint5 { get; set; }

	[Key(24)]
	public int GetMaxPoint5 { get; set; }

	[Key(25)]
	public ShopCostType ActionCostType5 { get; set; }

	[Key(26)]
	public int ActionCostValue5 { get; set; }

	[Key(27)]
	public FriendlyActionType Action6 { get; set; }

	[Key(28)]
	public int GetMinPoint6 { get; set; }

	[Key(29)]
	public int GetMaxPoint6 { get; set; }

	[Key(30)]
	public ShopCostType ActionCostType6 { get; set; }

	[Key(31)]
	public int ActionCostValue6 { get; set; }

	[Key(32)]
	public int HeroSelectChance { get; set; }

	[Key(33)]
	public byte NPC_Check { get; set; }

	public override string ToString()
	{
		return "HeroFriendlyData HeroIndex=" + HeroIndex;
	}

	public HeroFriendlySubData GetDataByFriendlyActionType(FriendlyActionType type)
	{
		if (_data.Count == 0)
		{
			SetData();
		}
		for (int i = 0; i < _data.Count; i++)
		{
			HeroFriendlySubData heroFriendlySubData = _data[i];
			if (heroFriendlySubData.Action == type)
			{
				return heroFriendlySubData;
			}
		}
		return new HeroFriendlySubData();
	}

	private void SetData()
	{
		SetSubData(Action1, GetMaxPoint1, GetMinPoint1, ActionCostType1, ActionCostValue1);
		SetSubData(Action2, GetMaxPoint2, GetMinPoint2, ActionCostType2, ActionCostValue2);
		SetSubData(Action3, GetMaxPoint3, GetMinPoint3, ActionCostType3, ActionCostValue3);
		SetSubData(Action4, GetMaxPoint4, GetMinPoint4, ActionCostType4, ActionCostValue4);
		SetSubData(Action5, GetMaxPoint5, GetMinPoint5, ActionCostType5, ActionCostValue5);
		SetSubData(Action6, GetMaxPoint6, GetMinPoint6, ActionCostType6, ActionCostValue6);
	}

	private void SetSubData(FriendlyActionType action, int max, int min, ShopCostType costType, int cost)
	{
		HeroFriendlySubData item = new HeroFriendlySubData
		{
			Action = action,
			GetMaxPoint = max,
			GetMinPoint = min,
			CostType = costType,
			ActionCost = cost
		};
		_data.Add(item);
	}
}
