using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_PartyRoomRepeat
{
	public static PartyRoomRepeat Parse(string s)
	{
		return Parse(s, new PartyRoomRepeat());
	}

	public static PartyRoomRepeat Parse(string s, PartyRoomRepeat o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PartyRoomRepeat Parse(IDictionary d)
	{
		return Parse(d, new PartyRoomRepeat());
	}

	public static PartyRoomRepeat Parse(IDictionary d, PartyRoomRepeat o)
	{
		o.IsRepeat = BaseJsonMarshaler.ParseValueBool(d, "IsRepeat");
		o.Result = BaseJsonMarshaler.ParseEnum<PartyRoomRepeat.ResultType>(d, "Result");
		return o;
	}

	public static string EncodeJsonString(PartyRoomRepeat o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PartyRoomRepeat o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("IsRepeat", o.IsRepeat.ToString());
		dictionary.Add("Result", o.Result.ToString());
		return dictionary;
	}
}
