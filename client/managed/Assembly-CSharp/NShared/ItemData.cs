namespace NShared;

public class ItemData<T> : ItemData where T : class, IItemSubData
{
	public T SubData => GetSubData<T>();

	public ItemData(ItemBaseData itemBase, T itemSub)
		: base(itemBase, itemSub)
	{
	}
}
public abstract class ItemData
{
	[JsonSerializable(Javascript = true)]
	public enum GetEffectType
	{
		Normal,
		Rare,
		Unique,
		ClassUnique,
		SuperiorUnique,
		ExceptionalUnique
	}

	private ItemBaseData _itemBase;

	private IItemSubData _itemSub;

	public int Index => _itemBase.Index;

	public string Code => _itemBase.Code;

	public string Name => _itemBase.Name;

	public string ItemStory => _itemBase.ItemStory;

	public bool IsArchive => _itemBase.IsArchive;

	public string Item3DPath => _itemBase.Item3DPath;

	public float Item3DScale => _itemBase.Item3DScale;

	public bool Item3DFlip => _itemBase.Item3DFlip;

	public ItemType Type => _itemBase.Type;

	public string[] Value => _itemBase.Value;

	public byte Grade => _itemBase.Grade;

	public bool Tutorial => _itemBase.Tutorial;

	public int BuyGold => _itemBase.BuyGold;

	public int BuyGem => _itemBase.BuyGem;

	public int BuyPvpCoin => _itemBase.BuyPvpCoin;

	public int BuyGuildPoint => _itemBase.BuyGuildPoint;

	public int BuyGuildArenaPoint => _itemBase.BuyGuildArenaPoint;

	public int BuyRoyalPoint => _itemBase.BuyRoyalPoint;

	public int BuyFriendshipPoint => _itemBase.BuyFriendshipPoint;

	public int BuyRaidPoint => _itemBase.BuyRaidPoint;

	public int BuyWorldBossPoint => _itemBase.BuyWorldBossPoint;

	public int BuyEventDungeonPoint => _itemBase.BuyEventDungeonPoint;

	public int BuyEventDungeonPoint2 => _itemBase.BuyEventDungeonPoint2;

	public int BuyEventDungeonPoint3 => _itemBase.BuyEventDungeonPoint3;

	public int BuyEventGiftPoint => _itemBase.BuyEventGiftPoint;

	public int BuyGloryPoint => _itemBase.BuyGloryPoint;

	public int BuyRankingPoint => _itemBase.BuyRankingPoint;

	public int SellGold => _itemBase.SellGold;

	public string AssetGroup => _itemBase.AssetGroup;

	public bool NotForSale => _itemBase.NotForSale;

	public bool Breakable => _itemBase.Breakable;

	public string[] Item3dModelNodeTree => _itemBase.Item3dModelNodeTree;

	public byte OpenWithdrawal => _itemBase.OpenWithdrawal;

	public bool GlobalNotify => _itemBase.GlobalNotify;

	public short Tier => _itemBase.Tier;

	public bool UseLog => _itemBase.UseLog;

	public GetEffectType GetEffect => _itemBase.GetEffect;

	public string DropItemNodeTreeCode => _itemBase.DropItemNodeTreeCode;

	public int BoxDivision => _itemBase.BoxDivision;

	public string ReplaceNameKey => _itemBase.ReplaceNameKey;

	public string[] NameParam => _itemBase.NameParam;

	public string ReplaceDesc2Key => _itemBase.ReplaceDesc2Key;

	public string[] Desc2Param => _itemBase.Desc2Param;

	public ClientActivityType MoveTaskType => _itemBase.MoveTaskType;

	public string[] MoveTaskValue => _itemBase.MoveTaskValue;

	public int AwakenStar => _itemBase.AwakenStar;

	public int BuyOrdealArenaPoint => _itemBase.BuyOrdealArenaPoint;

	public int BuyEventOrvelPoint => _itemBase.BuyEventOrvelPoint;

	public int BuyChallengeRaidPoint => _itemBase.BuyChallengeRaidPoint;

	public int ItemMaxCap => _itemBase.ItemMaxCap;

	public string Description => _itemBase.Description;

	public string Description2 => _itemBase.Description2;

	public string ReplaceDescKey => _itemBase.ReplaceDescKey;

	public string[] DescParam => _itemBase.DescParam;

	public int BuyLuaPoint => _itemBase.BuyLuaPoint;

	public int BuyShopEventPoint => _itemBase.BuyShopEventPoint;

	protected ItemData(ItemBaseData itemBase, IItemSubData itemSub)
	{
		_itemBase = itemBase;
		_itemSub = itemSub;
	}

	public string GetValue(int index)
	{
		return ArgParser.GetValueString(Value, index);
	}

	public int GetValueInt(int index)
	{
		return ArgParser.GetValueInt(Value, index);
	}

	public T GetValueEnum<T>(int index) where T : struct
	{
		return ArgParser.GetValueEnum<string, T>(Value, index);
	}

	public IItemSubData GetSubData()
	{
		return _itemSub;
	}

	public T GetSubData<T>() where T : class, IItemSubData
	{
		return GetSubData() as T;
	}
}
