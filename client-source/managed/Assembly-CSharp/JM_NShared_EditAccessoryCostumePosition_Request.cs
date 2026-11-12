using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.EditAccessoryCostumePosition;

public static class JM_NShared_EditAccessoryCostumePosition_Request
{
	public static NShared.EditAccessoryCostumePosition.Request Parse(string s)
	{
		return Parse(s, new NShared.EditAccessoryCostumePosition.Request());
	}

	public static NShared.EditAccessoryCostumePosition.Request Parse(string s, NShared.EditAccessoryCostumePosition.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.EditAccessoryCostumePosition.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.EditAccessoryCostumePosition.Request());
	}

	public static NShared.EditAccessoryCostumePosition.Request Parse(IDictionary d, NShared.EditAccessoryCostumePosition.Request o)
	{
		o.HeroIndex = BaseJsonMarshaler.ParseValueInt(d, "HeroIndex");
		o.AccessoryCostumePositionInfo = BaseJsonMarshaler.ParseValueString(d, "AccessoryCostumePositionInfo");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.EditAccessoryCostumePosition.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.EditAccessoryCostumePosition.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("HeroIndex", o.HeroIndex.ToString());
		if (o.AccessoryCostumePositionInfo != null)
		{
			dictionary.Add("AccessoryCostumePositionInfo", o.AccessoryCostumePositionInfo);
		}
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
