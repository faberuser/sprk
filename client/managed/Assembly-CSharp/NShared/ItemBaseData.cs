using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class ItemBaseData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public string Code { get; set; }

	[Key(3)]
	public string ItemStory { get; set; }

	[Key(4)]
	public bool IsArchive { get; set; }

	[Key(5)]
	public string Item3DPath { get; set; }

	[Key(6)]
	public float Item3DScale { get; set; }

	[Key(7)]
	public bool Item3DFlip { get; set; }

	[Key(8)]
	public ItemType Type { get; set; }

	[Key(9)]
	public string[] Value { get; set; }

	[Key(10)]
	public byte Grade { get; set; }

	[Key(11)]
	public bool Tutorial { get; set; }

	[Key(12)]
	public int BuyGold { get; set; }

	[Key(13)]
	public int BuyGem { get; set; }

	[Key(14)]
	public int BuyPvpCoin { get; set; }

	[Key(15)]
	public int BuyGuildPoint { get; set; }

	[Key(16)]
	public int BuyGuildArenaPoint { get; set; }

	[Key(17)]
	public int BuyRoyalPoint { get; set; }

	[Key(18)]
	public int BuyFriendshipPoint { get; set; }

	[Key(19)]
	public int BuyRaidPoint { get; set; }

	[Key(20)]
	public int BuyWorldBossPoint { get; set; }

	[Key(21)]
	public int BuyEventDungeonPoint { get; set; }

	[Key(22)]
	public int BuyEventDungeonPoint2 { get; set; }

	[Key(23)]
	public int BuyEventDungeonPoint3 { get; set; }

	[Key(24)]
	public int BuyEventGiftPoint { get; set; }

	[Key(25)]
	public int BuyGloryPoint { get; set; }

	[Key(26)]
	public int BuyRankingPoint { get; set; }

	[Key(27)]
	public int SellGold { get; set; }

	[Key(28)]
	public string AssetGroup { get; set; }

	[Key(29)]
	public bool NotForSale { get; set; }

	[Key(30)]
	public bool Breakable { get; set; }

	[Key(31)]
	public string[] Item3dModelNodeTree { get; set; }

	[Key(32)]
	public byte OpenWithdrawal { get; set; }

	[Key(33)]
	public bool GlobalNotify { get; set; }

	[Key(34)]
	public short Tier { get; set; }

	[Key(35)]
	public bool UseLog { get; set; }

	[Key(36)]
	public ItemData.GetEffectType GetEffect { get; set; }

	[Key(37)]
	public string DropItemNodeTreeCode { get; set; }

	[Key(38)]
	public int BoxDivision { get; set; }

	[Key(39)]
	public string ReplaceNameKey { get; set; }

	[Key(40)]
	public string[] NameParam { get; set; }

	[Key(41)]
	public string ReplaceDesc2Key { get; set; }

	[Key(42)]
	public string[] Desc2Param { get; set; }

	[Key(43)]
	public ClientActivityType MoveTaskType { get; set; }

	[Key(44)]
	public string[] MoveTaskValue { get; set; }

	[Key(45)]
	public int AwakenStar { get; set; }

	[Key(46)]
	public int BuyOrdealArenaPoint { get; set; }

	[Key(47)]
	public int BuyEventOrvelPoint { get; set; }

	[Key(48)]
	public int BuyChallengeRaidPoint { get; set; }

	[Key(49)]
	public int ItemMaxCap { get; set; }

	[IgnoreMember]
	public string Name => GameRoot.LocalizeProxy.GetCache_or_Build(this, "Name", BuildLocalize_Name);

	[IgnoreMember]
	public string Description => GameRoot.LocalizeProxy.GetCache_or_Build(this, "Description", BuildLocalize_Description);

	[IgnoreMember]
	public string Description2 => GameRoot.LocalizeProxy.GetCache_or_Build(this, "Description2", BuildLocalize_Description2);

	[IgnoreMember]
	public string ReplaceDescKey { get; set; }

	[IgnoreMember]
	public string[] DescParam { get; set; }

	[IgnoreMember]
	public int BuyLuaPoint { get; set; }

	[IgnoreMember]
	public int BuyShopEventPoint { get; set; }

	int IDataKeySelector<int>.Key => Index;

	private string GetCreatureNameForItem()
	{
		if (Type == ItemType.AwakeMaterial)
		{
			HeroAwakeData byMaterialItemCode = GameRoot.Instance.HeroAwakeDataContainer.GetByMaterialItemCode(Code);
			if (byMaterialItemCode == null)
			{
				return string.Empty;
			}
			CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(byMaterialItemCode.HeroIndex);
			if (data == null)
			{
				return string.Empty;
			}
			return data.Name;
		}
		return string.Empty;
	}

	private string BuildLocalize_Name()
	{
		string origKey = "ITEM_NAME_" + Index;
		return GameRoot.LocalizeProxy.GetStringWithReplaceKey(origKey, ReplaceNameKey, GetCreatureNameForItem());
	}

	private string BuildLocalize_Description()
	{
		string origKey = "ITEM_DESC_" + Index;
		return GameRoot.LocalizeProxy.GetStringWithReplaceKey(origKey, ReplaceDescKey);
	}

	private string BuildLocalize_Description2()
	{
		string origKey = "ITEM_DESC2_" + Index;
		return GameRoot.LocalizeProxy.GetStringWithReplaceKey(origKey, ReplaceDesc2Key);
	}

	public override string ToString()
	{
		return "Index=" + Index + " (" + Name + ")";
	}
}
