using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_InvitePartyReq
{
	public static InvitePartyReq Parse(string s)
	{
		return Parse(s, new InvitePartyReq());
	}

	public static InvitePartyReq Parse(string s, InvitePartyReq o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static InvitePartyReq Parse(IDictionary d)
	{
		return Parse(d, new InvitePartyReq());
	}

	public static InvitePartyReq Parse(IDictionary d, InvitePartyReq o)
	{
		o.InvitorName = BaseJsonMarshaler.ParseValueString(d, "InvitorName");
		o.RaidIndex = BaseJsonMarshaler.ParseValueInt(d, "RaidIndex");
		o.RaidLevel = BaseJsonMarshaler.ParseValueInt(d, "RaidLevel");
		o.BannedIds = BaseJsonMarshaler.ParseArrayCast(d, "BannedIds", BaseJsonMarshaler.CastToLong);
		o.RoomNo = BaseJsonMarshaler.ParseValueLong(d, "RoomNo");
		o.IsSendOne = BaseJsonMarshaler.ParseValueBool(d, "IsSendOne");
		o.dungeonType = BaseJsonMarshaler.ParseEnum<PartyDungeonType>(d, "dungeonType");
		o.chapterIndex = BaseJsonMarshaler.ParseValueInt(d, "chapterIndex");
		o.dungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "dungeonIndex");
		o.difficulty = BaseJsonMarshaler.ParseEnum<PartyDungeonDifficultyType>(d, "difficulty");
		o.AffixIndices = BaseJsonMarshaler.ParseArrayCast(d, "AffixIndices", BaseJsonMarshaler.CastToInt);
		return o;
	}

	public static string EncodeJsonString(InvitePartyReq o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(InvitePartyReq o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.InvitorName != null)
		{
			dictionary.Add("InvitorName", o.InvitorName);
		}
		dictionary.Add("RaidIndex", o.RaidIndex.ToString());
		dictionary.Add("RaidLevel", o.RaidLevel.ToString());
		dictionary.Add("BannedIds", BaseJsonMarshaler.EncodeArray(o.BannedIds));
		dictionary.Add("RoomNo", o.RoomNo.ToString());
		dictionary.Add("IsSendOne", o.IsSendOne.ToString());
		dictionary.Add("dungeonType", o.dungeonType.ToString());
		dictionary.Add("chapterIndex", o.chapterIndex.ToString());
		dictionary.Add("dungeonIndex", o.dungeonIndex.ToString());
		dictionary.Add("difficulty", o.difficulty.ToString());
		dictionary.Add("AffixIndices", BaseJsonMarshaler.EncodeArray(o.AffixIndices));
		return dictionary;
	}
}
