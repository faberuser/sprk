using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetGachaItemGroupCode;

public static class JM_NShared_GetGachaItemGroupCode_Request
{
	public static NShared.GetGachaItemGroupCode.Request Parse(string s)
	{
		return Parse(s, new NShared.GetGachaItemGroupCode.Request());
	}

	public static NShared.GetGachaItemGroupCode.Request Parse(string s, NShared.GetGachaItemGroupCode.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetGachaItemGroupCode.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetGachaItemGroupCode.Request());
	}

	public static NShared.GetGachaItemGroupCode.Request Parse(IDictionary d, NShared.GetGachaItemGroupCode.Request o)
	{
		o.GachaIndex = BaseJsonMarshaler.ParseValueInt(d, "GachaIndex");
		o.HighGachaCategory = BaseJsonMarshaler.ParseEnum<HighGachaCategoryType>(d, "HighGachaCategory");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetGachaItemGroupCode.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetGachaItemGroupCode.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("GachaIndex", o.GachaIndex.ToString());
		dictionary.Add("HighGachaCategory", o.HighGachaCategory.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
