using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_PartyRoomReady
{
	public static PartyRoomReady Parse(string s)
	{
		return Parse(s, new PartyRoomReady());
	}

	public static PartyRoomReady Parse(string s, PartyRoomReady o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PartyRoomReady Parse(IDictionary d)
	{
		return Parse(d, new PartyRoomReady());
	}

	public static PartyRoomReady Parse(IDictionary d, PartyRoomReady o)
	{
		o.IsReady = BaseJsonMarshaler.ParseValueBool(d, "IsReady");
		return o;
	}

	public static string EncodeJsonString(PartyRoomReady o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PartyRoomReady o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("IsReady", o.IsReady.ToString());
		return dictionary;
	}
}
