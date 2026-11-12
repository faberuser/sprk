using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_PartyDungeonRoomInfo
{
	public static PartyDungeonRoomInfo Parse(string s)
	{
		return Parse(s, new PartyDungeonRoomInfo());
	}

	public static PartyDungeonRoomInfo Parse(string s, PartyDungeonRoomInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PartyDungeonRoomInfo Parse(IDictionary d)
	{
		return Parse(d, new PartyDungeonRoomInfo());
	}

	public static PartyDungeonRoomInfo Parse(IDictionary d, PartyDungeonRoomInfo o)
	{
		o.RoomNo = BaseJsonMarshaler.ParseValueLong(d, "RoomNo");
		o.DungeonType = BaseJsonMarshaler.ParseEnum<PartyDungeonType>(d, "DungeonType");
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.DungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "DungeonIndex");
		o.DungeonDifficulty = BaseJsonMarshaler.ParseValueInt(d, "DungeonDifficulty");
		o.Opened = BaseJsonMarshaler.ParseValueByte(d, "Opened");
		o.MasterAccountId = BaseJsonMarshaler.ParseValueLong(d, "MasterAccountId");
		o.MasterNick = BaseJsonMarshaler.ParseValueString(d, "MasterNick");
		o.CurPartyMember = BaseJsonMarshaler.ParseValueByte(d, "CurPartyMember");
		o.IsAutoRoot = BaseJsonMarshaler.ParseValueBool(d, "IsAutoRoot");
		return o;
	}

	public static string EncodeJsonString(PartyDungeonRoomInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PartyDungeonRoomInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("RoomNo", o.RoomNo.ToString());
		dictionary.Add("DungeonType", o.DungeonType.ToString());
		dictionary.Add("ChapterIndex", o.ChapterIndex.ToString());
		dictionary.Add("DungeonIndex", o.DungeonIndex.ToString());
		dictionary.Add("DungeonDifficulty", o.DungeonDifficulty.ToString());
		dictionary.Add("Opened", o.Opened.ToString());
		dictionary.Add("MasterAccountId", o.MasterAccountId.ToString());
		if (o.MasterNick != null)
		{
			dictionary.Add("MasterNick", o.MasterNick);
		}
		dictionary.Add("CurPartyMember", o.CurPartyMember.ToString());
		dictionary.Add("IsAutoRoot", o.IsAutoRoot.ToString());
		return dictionary;
	}
}
