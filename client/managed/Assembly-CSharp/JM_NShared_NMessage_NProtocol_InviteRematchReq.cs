using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_InviteRematchReq
{
	public static InviteRematchReq Parse(string s)
	{
		return Parse(s, new InviteRematchReq());
	}

	public static InviteRematchReq Parse(string s, InviteRematchReq o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static InviteRematchReq Parse(IDictionary d)
	{
		return Parse(d, new InviteRematchReq());
	}

	public static InviteRematchReq Parse(IDictionary d, InviteRematchReq o)
	{
		o.BattleType = BaseJsonMarshaler.ParseEnum<BattleType>(d, "BattleType");
		o.Nick = BaseJsonMarshaler.ParseValueString(d, "Nick");
		o.TeamLevel = BaseJsonMarshaler.ParseValueInt(d, "TeamLevel");
		o.AvatarIndex = BaseJsonMarshaler.ParseValueInt(d, "AvatarIndex");
		o.ChatperIndex = BaseJsonMarshaler.ParseValueInt(d, "ChatperIndex");
		o.DungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "DungeonIndex");
		o.LastMatchInfo = BaseJsonMarshaler.ParseObject(d, "LastMatchInfo", JM_NShared_MatchInfo.Parse);
		return o;
	}

	public static string EncodeJsonString(InviteRematchReq o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(InviteRematchReq o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("BattleType", o.BattleType.ToString());
		if (o.Nick != null)
		{
			dictionary.Add("Nick", o.Nick);
		}
		dictionary.Add("TeamLevel", o.TeamLevel.ToString());
		dictionary.Add("AvatarIndex", o.AvatarIndex.ToString());
		dictionary.Add("ChatperIndex", o.ChatperIndex.ToString());
		dictionary.Add("DungeonIndex", o.DungeonIndex.ToString());
		dictionary.Add("LastMatchInfo", JM_NShared_MatchInfo.EncodeJsonObject(o.LastMatchInfo));
		return dictionary;
	}
}
