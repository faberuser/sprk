using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.UseAccessorySelectItem;

public static class JM_NShared_UseAccessorySelectItem_Request
{
	public static NShared.UseAccessorySelectItem.Request Parse(string s)
	{
		return Parse(s, new NShared.UseAccessorySelectItem.Request());
	}

	public static NShared.UseAccessorySelectItem.Request Parse(string s, NShared.UseAccessorySelectItem.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.UseAccessorySelectItem.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.UseAccessorySelectItem.Request());
	}

	public static NShared.UseAccessorySelectItem.Request Parse(IDictionary d, NShared.UseAccessorySelectItem.Request o)
	{
		o.ItemIndex = BaseJsonMarshaler.ParseValueInt(d, "ItemIndex");
		o.AccessoryCostumeIndex = BaseJsonMarshaler.ParseValueInt(d, "AccessoryCostumeIndex");
		o.PositionInfo = BaseJsonMarshaler.ParseValueString(d, "PositionInfo");
		o.HeroIndex = BaseJsonMarshaler.ParseValueInt(d, "HeroIndex");
		o.GetOtherReward = BaseJsonMarshaler.ParseValueBool(d, "GetOtherReward");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.UseAccessorySelectItem.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.UseAccessorySelectItem.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ItemIndex", o.ItemIndex.ToString());
		dictionary.Add("AccessoryCostumeIndex", o.AccessoryCostumeIndex.ToString());
		if (o.PositionInfo != null)
		{
			dictionary.Add("PositionInfo", o.PositionInfo);
		}
		dictionary.Add("HeroIndex", o.HeroIndex.ToString());
		dictionary.Add("GetOtherReward", o.GetOtherReward.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
