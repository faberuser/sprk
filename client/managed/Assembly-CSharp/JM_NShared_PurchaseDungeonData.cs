using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_PurchaseDungeonData
{
	public static PurchaseDungeonData Parse(string s)
	{
		return Parse(s, new PurchaseDungeonData());
	}

	public static PurchaseDungeonData Parse(string s, PurchaseDungeonData o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PurchaseDungeonData Parse(IDictionary d)
	{
		return Parse(d, new PurchaseDungeonData());
	}

	public static PurchaseDungeonData Parse(IDictionary d, PurchaseDungeonData o)
	{
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.DungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "DungeonIndex");
		o.Buyable = BaseJsonMarshaler.ParseValueBool(d, "Buyable");
		o.CurrencyType = BaseJsonMarshaler.ParseEnum<CurrencyType>(d, "CurrencyType");
		o.Price = BaseJsonMarshaler.ParseValueInt(d, "Price");
		o.DungeonPortraitPath = BaseJsonMarshaler.ParseValueString(d, "DungeonPortraitPath");
		o.DungeonName = BaseJsonMarshaler.ParseValueString(d, "DungeonName");
		o.RewardLocalKey = BaseJsonMarshaler.ParseValueString(d, "RewardLocalKey");
		o.BoosterItemCode = BaseJsonMarshaler.ParseValueString(d, "BoosterItemCode");
		o.EnterCondition = BaseJsonMarshaler.ParseArrayCast(d, "EnterCondition", BaseJsonMarshaler.CastToString);
		o.DungeonRewardPath = BaseJsonMarshaler.ParseValueString(d, "DungeonRewardPath");
		return o;
	}

	public static string EncodeJsonString(PurchaseDungeonData o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PurchaseDungeonData o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ChapterIndex", o.ChapterIndex.ToString());
		dictionary.Add("DungeonIndex", o.DungeonIndex.ToString());
		dictionary.Add("Buyable", o.Buyable.ToString());
		dictionary.Add("CurrencyType", o.CurrencyType.ToString());
		dictionary.Add("Price", o.Price.ToString());
		if (o.DungeonPortraitPath != null)
		{
			dictionary.Add("DungeonPortraitPath", o.DungeonPortraitPath);
		}
		if (o.DungeonName != null)
		{
			dictionary.Add("DungeonName", o.DungeonName);
		}
		if (o.RewardLocalKey != null)
		{
			dictionary.Add("RewardLocalKey", o.RewardLocalKey);
		}
		if (o.BoosterItemCode != null)
		{
			dictionary.Add("BoosterItemCode", o.BoosterItemCode);
		}
		dictionary.Add("EnterCondition", BaseJsonMarshaler.EncodeArray(o.EnterCondition));
		if (o.DungeonRewardPath != null)
		{
			dictionary.Add("DungeonRewardPath", o.DungeonRewardPath);
		}
		return dictionary;
	}
}
