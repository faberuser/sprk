namespace NShared;

[JsonSerializable(Javascript = true)]
public class MailInfo : IAccountAttrKey<long>
{
	long IAccountAttrKey<long>.Key => MailIndex;

	public long MailIndex { get; set; }

	public int OpenIndex { get; set; }

	public long SenderId { get; set; }

	public string SenderName { get; set; }

	public long ReceiverId { get; set; }

	public string Title { get; set; }

	public string Content { get; set; }

	public int SendGold { get; set; }

	public int SendGem { get; set; }

	public int PvpCoin { get; set; }

	public int Mileage { get; set; }

	public int LuaPoint { get; set; }

	public int GuildPoint { get; set; }

	public int WorldBossPoint { get; set; }

	public int EventDungeonPoint { get; set; }

	public int EventDungeonPoint2 { get; set; }

	public int GloryPoint { get; set; }

	public int EventGiftPoint { get; set; }

	public int SendChicken { get; set; }

	public int SendSword { get; set; }

	public int SendSword2 { get; set; }

	public int SendUndergroundPrisonKey { get; set; }

	public int SendUndergroundLabyrinthKey { get; set; }

	public int SendHideoutKey { get; set; }

	public int SendChallengeTowerKey { get; set; }

	public int SendMazeKey { get; set; }

	public string ReadTime { get; set; }

	public string ExpireTime { get; set; }

	public string OpenTime { get; set; }

	public int OpenRemainTime { get; set; }

	public MailItemInfo[] ItemInfos { get; set; }

	public EquipItemInfo[] EquipItemInfos { get; set; }

	public MailCurrencyInfo[] CurrencyInfos { get; set; }

	public MailStaminaInfo[] StaminaInfos { get; set; }
}
