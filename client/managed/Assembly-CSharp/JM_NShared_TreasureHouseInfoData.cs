using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_TreasureHouseInfoData
{
	public static TreasureHouseInfoData Parse(string s)
	{
		return Parse(s, new TreasureHouseInfoData());
	}

	public static TreasureHouseInfoData Parse(string s, TreasureHouseInfoData o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static TreasureHouseInfoData Parse(IDictionary d)
	{
		return Parse(d, new TreasureHouseInfoData());
	}

	public static TreasureHouseInfoData Parse(IDictionary d, TreasureHouseInfoData o)
	{
		o.Index = BaseJsonMarshaler.ParseValueInt(d, "Index");
		o.EnterCount = BaseJsonMarshaler.ParseValueInt(d, "EnterCount");
		o.ContentsDesc = BaseJsonMarshaler.ParseValueString(d, "ContentsDesc");
		o.ChangeCurrencyType = BaseJsonMarshaler.ParseValueString(d, "ChangeCurrencyType");
		o.ChangeCurrencyCost = BaseJsonMarshaler.ParseValueInt(d, "ChangeCurrencyCost");
		return o;
	}

	public static string EncodeJsonString(TreasureHouseInfoData o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(TreasureHouseInfoData o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Index", o.Index.ToString());
		dictionary.Add("EnterCount", o.EnterCount.ToString());
		if (o.ContentsDesc != null)
		{
			dictionary.Add("ContentsDesc", o.ContentsDesc);
		}
		if (o.ChangeCurrencyType != null)
		{
			dictionary.Add("ChangeCurrencyType", o.ChangeCurrencyType);
		}
		dictionary.Add("ChangeCurrencyCost", o.ChangeCurrencyCost.ToString());
		return dictionary;
	}
}
