using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_MailInfo
{
	public static MailInfo Parse(string s)
	{
		return Parse(s, new MailInfo());
	}

	public static MailInfo Parse(string s, MailInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static MailInfo Parse(IDictionary d)
	{
		return Parse(d, new MailInfo());
	}

	public static MailInfo Parse(IDictionary d, MailInfo o)
	{
		o.MailIndex = BaseJsonMarshaler.ParseValueLong(d, "MailIndex");
		o.OpenIndex = BaseJsonMarshaler.ParseValueInt(d, "OpenIndex");
		o.SenderId = BaseJsonMarshaler.ParseValueLong(d, "SenderId");
		o.SenderName = BaseJsonMarshaler.ParseValueString(d, "SenderName");
		o.ReceiverId = BaseJsonMarshaler.ParseValueLong(d, "ReceiverId");
		o.Title = BaseJsonMarshaler.ParseValueString(d, "Title");
		o.Content = BaseJsonMarshaler.ParseValueString(d, "Content");
		o.SendGold = BaseJsonMarshaler.ParseValueInt(d, "SendGold");
		o.SendGem = BaseJsonMarshaler.ParseValueInt(d, "SendGem");
		o.PvpCoin = BaseJsonMarshaler.ParseValueInt(d, "PvpCoin");
		o.Mileage = BaseJsonMarshaler.ParseValueInt(d, "Mileage");
		o.LuaPoint = BaseJsonMarshaler.ParseValueInt(d, "LuaPoint");
		o.GuildPoint = BaseJsonMarshaler.ParseValueInt(d, "GuildPoint");
		o.WorldBossPoint = BaseJsonMarshaler.ParseValueInt(d, "WorldBossPoint");
		o.EventDungeonPoint = BaseJsonMarshaler.ParseValueInt(d, "EventDungeonPoint");
		o.EventDungeonPoint2 = BaseJsonMarshaler.ParseValueInt(d, "EventDungeonPoint2");
		o.GloryPoint = BaseJsonMarshaler.ParseValueInt(d, "GloryPoint");
		o.EventGiftPoint = BaseJsonMarshaler.ParseValueInt(d, "EventGiftPoint");
		o.SendChicken = BaseJsonMarshaler.ParseValueInt(d, "SendChicken");
		o.SendSword = BaseJsonMarshaler.ParseValueInt(d, "SendSword");
		o.SendSword2 = BaseJsonMarshaler.ParseValueInt(d, "SendSword2");
		o.SendUndergroundPrisonKey = BaseJsonMarshaler.ParseValueInt(d, "SendUndergroundPrisonKey");
		o.SendUndergroundLabyrinthKey = BaseJsonMarshaler.ParseValueInt(d, "SendUndergroundLabyrinthKey");
		o.SendHideoutKey = BaseJsonMarshaler.ParseValueInt(d, "SendHideoutKey");
		o.SendChallengeTowerKey = BaseJsonMarshaler.ParseValueInt(d, "SendChallengeTowerKey");
		o.SendMazeKey = BaseJsonMarshaler.ParseValueInt(d, "SendMazeKey");
		o.ReadTime = BaseJsonMarshaler.ParseValueString(d, "ReadTime");
		o.ExpireTime = BaseJsonMarshaler.ParseValueString(d, "ExpireTime");
		o.OpenTime = BaseJsonMarshaler.ParseValueString(d, "OpenTime");
		o.OpenRemainTime = BaseJsonMarshaler.ParseValueInt(d, "OpenRemainTime");
		o.ItemInfos = BaseJsonMarshaler.ParseArray(d, "ItemInfos", JM_NShared_MailItemInfo.Parse);
		o.EquipItemInfos = BaseJsonMarshaler.ParseArray(d, "EquipItemInfos", JM_NShared_EquipItemInfo.Parse);
		o.CurrencyInfos = BaseJsonMarshaler.ParseArray(d, "CurrencyInfos", JM_NShared_MailCurrencyInfo.Parse);
		o.StaminaInfos = BaseJsonMarshaler.ParseArray(d, "StaminaInfos", JM_NShared_MailStaminaInfo.Parse);
		return o;
	}

	public static string EncodeJsonString(MailInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(MailInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("MailIndex", o.MailIndex.ToString());
		dictionary.Add("OpenIndex", o.OpenIndex.ToString());
		dictionary.Add("SenderId", o.SenderId.ToString());
		if (o.SenderName != null)
		{
			dictionary.Add("SenderName", o.SenderName);
		}
		dictionary.Add("ReceiverId", o.ReceiverId.ToString());
		if (o.Title != null)
		{
			dictionary.Add("Title", o.Title);
		}
		if (o.Content != null)
		{
			dictionary.Add("Content", o.Content);
		}
		dictionary.Add("SendGold", o.SendGold.ToString());
		dictionary.Add("SendGem", o.SendGem.ToString());
		dictionary.Add("PvpCoin", o.PvpCoin.ToString());
		dictionary.Add("Mileage", o.Mileage.ToString());
		dictionary.Add("LuaPoint", o.LuaPoint.ToString());
		dictionary.Add("GuildPoint", o.GuildPoint.ToString());
		dictionary.Add("WorldBossPoint", o.WorldBossPoint.ToString());
		dictionary.Add("EventDungeonPoint", o.EventDungeonPoint.ToString());
		dictionary.Add("EventDungeonPoint2", o.EventDungeonPoint2.ToString());
		dictionary.Add("GloryPoint", o.GloryPoint.ToString());
		dictionary.Add("EventGiftPoint", o.EventGiftPoint.ToString());
		dictionary.Add("SendChicken", o.SendChicken.ToString());
		dictionary.Add("SendSword", o.SendSword.ToString());
		dictionary.Add("SendSword2", o.SendSword2.ToString());
		dictionary.Add("SendUndergroundPrisonKey", o.SendUndergroundPrisonKey.ToString());
		dictionary.Add("SendUndergroundLabyrinthKey", o.SendUndergroundLabyrinthKey.ToString());
		dictionary.Add("SendHideoutKey", o.SendHideoutKey.ToString());
		dictionary.Add("SendChallengeTowerKey", o.SendChallengeTowerKey.ToString());
		dictionary.Add("SendMazeKey", o.SendMazeKey.ToString());
		if (o.ReadTime != null)
		{
			dictionary.Add("ReadTime", o.ReadTime);
		}
		if (o.ExpireTime != null)
		{
			dictionary.Add("ExpireTime", o.ExpireTime);
		}
		if (o.OpenTime != null)
		{
			dictionary.Add("OpenTime", o.OpenTime);
		}
		dictionary.Add("OpenRemainTime", o.OpenRemainTime.ToString());
		dictionary.Add("ItemInfos", BaseJsonMarshaler.EncodeArray(o.ItemInfos, JM_NShared_MailItemInfo.EncodeJsonObject));
		dictionary.Add("EquipItemInfos", BaseJsonMarshaler.EncodeArray(o.EquipItemInfos, JM_NShared_EquipItemInfo.EncodeJsonObject));
		dictionary.Add("CurrencyInfos", BaseJsonMarshaler.EncodeArray(o.CurrencyInfos, JM_NShared_MailCurrencyInfo.EncodeJsonObject));
		dictionary.Add("StaminaInfos", BaseJsonMarshaler.EncodeArray(o.StaminaInfos, JM_NShared_MailStaminaInfo.EncodeJsonObject));
		return dictionary;
	}
}
