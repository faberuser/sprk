using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NRoomMessage.NProtocol;

public static class JM_NShared_NRoomMessage_NProtocol_RoomChat
{
	public static RoomChat Parse(string s)
	{
		return Parse(s, new RoomChat());
	}

	public static RoomChat Parse(string s, RoomChat o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static RoomChat Parse(IDictionary d)
	{
		return Parse(d, new RoomChat());
	}

	public static RoomChat Parse(IDictionary d, RoomChat o)
	{
		o.IsSenderMaster = BaseJsonMarshaler.ParseValueBool(d, "IsSenderMaster");
		o.SenderAvatarIndex = BaseJsonMarshaler.ParseValueInt(d, "SenderAvatarIndex");
		o.SenderTeamLevel = BaseJsonMarshaler.ParseValueInt(d, "SenderTeamLevel");
		o.SenderName = BaseJsonMarshaler.ParseValueString(d, "SenderName");
		o.Chat = BaseJsonMarshaler.ParseValueString(d, "Chat");
		o.SendTime = BaseJsonMarshaler.ParseValueString(d, "SendTime");
		return o;
	}

	public static string EncodeJsonString(RoomChat o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(RoomChat o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("IsSenderMaster", o.IsSenderMaster.ToString());
		dictionary.Add("SenderAvatarIndex", o.SenderAvatarIndex.ToString());
		dictionary.Add("SenderTeamLevel", o.SenderTeamLevel.ToString());
		if (o.SenderName != null)
		{
			dictionary.Add("SenderName", o.SenderName);
		}
		if (o.Chat != null)
		{
			dictionary.Add("Chat", o.Chat);
		}
		if (o.SendTime != null)
		{
			dictionary.Add("SendTime", o.SendTime);
		}
		return dictionary;
	}
}
