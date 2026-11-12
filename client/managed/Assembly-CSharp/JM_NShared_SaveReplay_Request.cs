using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.SaveReplay;

public static class JM_NShared_SaveReplay_Request
{
	public static NShared.SaveReplay.Request Parse(string s)
	{
		return Parse(s, new NShared.SaveReplay.Request());
	}

	public static NShared.SaveReplay.Request Parse(string s, NShared.SaveReplay.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.SaveReplay.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.SaveReplay.Request());
	}

	public static NShared.SaveReplay.Request Parse(IDictionary d, NShared.SaveReplay.Request o)
	{
		o.Type = BaseJsonMarshaler.ParseEnum<ReplayType>(d, "Type");
		o.Info = BaseJsonMarshaler.ParseValueString(d, "Info");
		o.BattleLogs = BaseJsonMarshaler.ParseValueString(d, "BattleLogs");
		o.AccountIds = BaseJsonMarshaler.ParseArrayCast(d, "AccountIds", BaseJsonMarshaler.CastToLong);
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.SaveReplay.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.SaveReplay.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Type", o.Type.ToString());
		if (o.Info != null)
		{
			dictionary.Add("Info", o.Info);
		}
		if (o.BattleLogs != null)
		{
			dictionary.Add("BattleLogs", o.BattleLogs);
		}
		dictionary.Add("AccountIds", BaseJsonMarshaler.EncodeArray(o.AccountIds));
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
