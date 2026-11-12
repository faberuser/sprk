using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_DelegateGuildMaster
{
	public static DelegateGuildMaster Parse(string s)
	{
		return Parse(s, new DelegateGuildMaster());
	}

	public static DelegateGuildMaster Parse(string s, DelegateGuildMaster o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static DelegateGuildMaster Parse(IDictionary d)
	{
		return Parse(d, new DelegateGuildMaster());
	}

	public static DelegateGuildMaster Parse(IDictionary d, DelegateGuildMaster o)
	{
		o.PrevMasterId = BaseJsonMarshaler.ParseValueLong(d, "PrevMasterId");
		o.CurMasterId = BaseJsonMarshaler.ParseValueLong(d, "CurMasterId");
		o.PrevMasterNick = BaseJsonMarshaler.ParseValueString(d, "PrevMasterNick");
		o.CurMasterNick = BaseJsonMarshaler.ParseValueString(d, "CurMasterNick");
		o.Forced = BaseJsonMarshaler.ParseValueBool(d, "Forced");
		return o;
	}

	public static string EncodeJsonString(DelegateGuildMaster o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(DelegateGuildMaster o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("PrevMasterId", o.PrevMasterId.ToString());
		dictionary.Add("CurMasterId", o.CurMasterId.ToString());
		if (o.PrevMasterNick != null)
		{
			dictionary.Add("PrevMasterNick", o.PrevMasterNick);
		}
		if (o.CurMasterNick != null)
		{
			dictionary.Add("CurMasterNick", o.CurMasterNick);
		}
		dictionary.Add("Forced", o.Forced.ToString());
		return dictionary;
	}
}
