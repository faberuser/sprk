using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_PartyMemberDungeonInfoRes
{
	public static PartyMemberDungeonInfoRes Parse(string s)
	{
		return Parse(s, new PartyMemberDungeonInfoRes());
	}

	public static PartyMemberDungeonInfoRes Parse(string s, PartyMemberDungeonInfoRes o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PartyMemberDungeonInfoRes Parse(IDictionary d)
	{
		return Parse(d, new PartyMemberDungeonInfoRes());
	}

	public static PartyMemberDungeonInfoRes Parse(IDictionary d, PartyMemberDungeonInfoRes o)
	{
		o.CanJoin = BaseJsonMarshaler.ParseValueBool(d, "CanJoin");
		o.RoomNo = BaseJsonMarshaler.ParseValueLong(d, "RoomNo");
		o.RaidIndex = BaseJsonMarshaler.ParseValueInt(d, "RaidIndex");
		o.RaidLevel = BaseJsonMarshaler.ParseValueInt(d, "RaidLevel");
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.DungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "DungeonIndex");
		return o;
	}

	public static string EncodeJsonString(PartyMemberDungeonInfoRes o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PartyMemberDungeonInfoRes o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("CanJoin", o.CanJoin.ToString());
		dictionary.Add("RoomNo", o.RoomNo.ToString());
		dictionary.Add("RaidIndex", o.RaidIndex.ToString());
		dictionary.Add("RaidLevel", o.RaidLevel.ToString());
		dictionary.Add("ChapterIndex", o.ChapterIndex.ToString());
		dictionary.Add("DungeonIndex", o.DungeonIndex.ToString());
		return dictionary;
	}
}
