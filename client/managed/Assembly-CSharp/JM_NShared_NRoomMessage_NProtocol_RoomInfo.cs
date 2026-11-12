using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NRoomMessage.NProtocol;

public static class JM_NShared_NRoomMessage_NProtocol_RoomInfo
{
	public static RoomInfo Parse(string s)
	{
		return Parse(s, new RoomInfo());
	}

	public static RoomInfo Parse(string s, RoomInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static RoomInfo Parse(IDictionary d)
	{
		return Parse(d, new RoomInfo());
	}

	public static RoomInfo Parse(IDictionary d, RoomInfo o)
	{
		o.Opened = BaseJsonMarshaler.ParseValueByte(d, "Opened");
		o.RoomName = BaseJsonMarshaler.ParseValueString(d, "RoomName");
		return o;
	}

	public static string EncodeJsonString(RoomInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(RoomInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Opened", o.Opened.ToString());
		if (o.RoomName != null)
		{
			dictionary.Add("RoomName", o.RoomName);
		}
		return dictionary;
	}
}
