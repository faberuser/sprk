using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_WorldChat
{
	public static WorldChat Parse(string s)
	{
		return Parse(s, new WorldChat());
	}

	public static WorldChat Parse(string s, WorldChat o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static WorldChat Parse(IDictionary d)
	{
		return Parse(d, new WorldChat());
	}

	public static WorldChat Parse(IDictionary d, WorldChat o)
	{
		o.SenderAvatarIndex = BaseJsonMarshaler.ParseValueInt(d, "SenderAvatarIndex");
		o.SenderTeamLevel = BaseJsonMarshaler.ParseValueInt(d, "SenderTeamLevel");
		o.SenderName = BaseJsonMarshaler.ParseValueString(d, "SenderName");
		o.ReceiverId = BaseJsonMarshaler.ParseValueLong(d, "ReceiverId");
		o.ReceiverName = BaseJsonMarshaler.ParseValueString(d, "ReceiverName");
		o.Chat = BaseJsonMarshaler.ParseValueString(d, "Chat");
		o.SendTime = BaseJsonMarshaler.ParseValueString(d, "SendTime");
		o.AdminLevel = BaseJsonMarshaler.ParseValueInt(d, "AdminLevel");
		o.LinkedItem = BaseJsonMarshaler.ParseArray(d, "LinkedItem", JM_NShared_NMessage_NProtocol_LinkedItemInfo.Parse);
		o.EmoticonIndex = BaseJsonMarshaler.ParseValueInt(d, "EmoticonIndex");
		o.IsShowBattleEmoti = BaseJsonMarshaler.ParseValueBool(d, "IsShowBattleEmoti");
		o.ChatIconList = BaseJsonMarshaler.ParseArrayCast(d, "ChatIconList", BaseJsonMarshaler.CastToInt);
		return o;
	}

	public static string EncodeJsonString(WorldChat o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(WorldChat o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("SenderAvatarIndex", o.SenderAvatarIndex.ToString());
		dictionary.Add("SenderTeamLevel", o.SenderTeamLevel.ToString());
		if (o.SenderName != null)
		{
			dictionary.Add("SenderName", o.SenderName);
		}
		dictionary.Add("ReceiverId", o.ReceiverId.ToString());
		if (o.ReceiverName != null)
		{
			dictionary.Add("ReceiverName", o.ReceiverName);
		}
		if (o.Chat != null)
		{
			dictionary.Add("Chat", o.Chat);
		}
		if (o.SendTime != null)
		{
			dictionary.Add("SendTime", o.SendTime);
		}
		dictionary.Add("AdminLevel", o.AdminLevel.ToString());
		dictionary.Add("LinkedItem", BaseJsonMarshaler.EncodeArray(o.LinkedItem, JM_NShared_NMessage_NProtocol_LinkedItemInfo.EncodeJsonObject));
		dictionary.Add("EmoticonIndex", o.EmoticonIndex.ToString());
		dictionary.Add("IsShowBattleEmoti", o.IsShowBattleEmoti.ToString());
		dictionary.Add("ChatIconList", BaseJsonMarshaler.EncodeArray(o.ChatIconList));
		return dictionary;
	}
}
