using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_PartyMemberEmoti
{
	public static PartyMemberEmoti Parse(string s)
	{
		return Parse(s, new PartyMemberEmoti());
	}

	public static PartyMemberEmoti Parse(string s, PartyMemberEmoti o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PartyMemberEmoti Parse(IDictionary d)
	{
		return Parse(d, new PartyMemberEmoti());
	}

	public static PartyMemberEmoti Parse(IDictionary d, PartyMemberEmoti o)
	{
		o.ChatType = BaseJsonMarshaler.ParseEnum<ChatType>(d, "ChatType");
		o.Emoti = BaseJsonMarshaler.ParseEnum<EmotiType>(d, "Emoti");
		o.SenderName = BaseJsonMarshaler.ParseValueString(d, "SenderName");
		o.SenderAvatarIndex = BaseJsonMarshaler.ParseValueInt(d, "SenderAvatarIndex");
		o.SenderTeamLevel = BaseJsonMarshaler.ParseValueInt(d, "SenderTeamLevel");
		o.ReceiverId = BaseJsonMarshaler.ParseValueLong(d, "ReceiverId");
		o.ReceiverName = BaseJsonMarshaler.ParseValueString(d, "ReceiverName");
		return o;
	}

	public static string EncodeJsonString(PartyMemberEmoti o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PartyMemberEmoti o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ChatType", o.ChatType.ToString());
		dictionary.Add("Emoti", o.Emoti.ToString());
		if (o.SenderName != null)
		{
			dictionary.Add("SenderName", o.SenderName);
		}
		dictionary.Add("SenderAvatarIndex", o.SenderAvatarIndex.ToString());
		dictionary.Add("SenderTeamLevel", o.SenderTeamLevel.ToString());
		dictionary.Add("ReceiverId", o.ReceiverId.ToString());
		if (o.ReceiverName != null)
		{
			dictionary.Add("ReceiverName", o.ReceiverName);
		}
		return dictionary;
	}
}
