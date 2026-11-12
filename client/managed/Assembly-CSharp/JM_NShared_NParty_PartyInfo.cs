using System;
using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NParty;

public static class JM_NShared_NParty_PartyInfo
{
	public static PartyInfo Parse(string s)
	{
		return Parse(s, new PartyInfo());
	}

	public static PartyInfo Parse(string s, PartyInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PartyInfo Parse(IDictionary d)
	{
		return Parse(d, new PartyInfo());
	}

	public static PartyInfo Parse(IDictionary d, PartyInfo o)
	{
		o.RaidIndex = BaseJsonMarshaler.ParseValueInt(d, "RaidIndex");
		o.RaidLevel = BaseJsonMarshaler.ParseValueInt(d, "RaidLevel");
		o.IsRepeatBattle = BaseJsonMarshaler.ParseValueBool(d, "IsRepeatBattle");
		o.PartyMembers = BaseJsonMarshaler.ParseList<MemberInfo, List<MemberInfo>>(d, "PartyMembers", JM_NShared_NParty_MemberInfo.Parse);
		o.RewardItems = BaseJsonMarshaler.ParseArray(d, "RewardItems", JM_NShared_RewardItemInfo.Parse);
		o.MasterServerPrivateIp = BaseJsonMarshaler.ParseValueString(d, "MasterServerPrivateIp");
		o.MasterServerPort = BaseJsonMarshaler.ParseValueInt(d, "MasterServerPort");
		o.CreatedTime = BaseJsonMarshaler.ParseValue<DateTime>(d, "CreatedTime");
		o.Bannedids = BaseJsonMarshaler.ParseArrayCast(d, "Bannedids", BaseJsonMarshaler.CastToLong);
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.DungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "DungeonIndex");
		o.Difficulty = BaseJsonMarshaler.ParseEnum<PartyDungeonDifficultyType>(d, "Difficulty");
		o.GuildSuppressDungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "GuildSuppressDungeonIndex");
		o.PartyRoom = BaseJsonMarshaler.ParseObject(d, "PartyRoom", JM_NShared_RaidRoomInfo.Parse);
		return o;
	}

	public static string EncodeJsonString(PartyInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PartyInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("RaidIndex", o.RaidIndex.ToString());
		dictionary.Add("RaidLevel", o.RaidLevel.ToString());
		dictionary.Add("IsRepeatBattle", o.IsRepeatBattle.ToString());
		dictionary.Add("PartyMembers", BaseJsonMarshaler.EncodeArray(o.PartyMembers, JM_NShared_NParty_MemberInfo.EncodeJsonObject));
		dictionary.Add("RewardItems", BaseJsonMarshaler.EncodeArray(o.RewardItems, JM_NShared_RewardItemInfo.EncodeJsonObject));
		if (o.MasterServerPrivateIp != null)
		{
			dictionary.Add("MasterServerPrivateIp", o.MasterServerPrivateIp);
		}
		dictionary.Add("MasterServerPort", o.MasterServerPort.ToString());
		dictionary.Add("CreatedTime", o.CreatedTime.ToString());
		dictionary.Add("Bannedids", BaseJsonMarshaler.EncodeArray(o.Bannedids));
		dictionary.Add("ChapterIndex", o.ChapterIndex.ToString());
		dictionary.Add("DungeonIndex", o.DungeonIndex.ToString());
		dictionary.Add("Difficulty", o.Difficulty.ToString());
		dictionary.Add("GuildSuppressDungeonIndex", o.GuildSuppressDungeonIndex.ToString());
		dictionary.Add("PartyRoom", JM_NShared_RaidRoomInfo.EncodeJsonObject(o.PartyRoom));
		return dictionary;
	}
}
