using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_NextTeamEnterBattleLog
{
	public static NextTeamEnterBattleLog Parse(string s)
	{
		return Parse(s, new NextTeamEnterBattleLog());
	}

	public static NextTeamEnterBattleLog Parse(string s, NextTeamEnterBattleLog o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NextTeamEnterBattleLog Parse(IDictionary d)
	{
		return Parse(d, new NextTeamEnterBattleLog());
	}

	public static NextTeamEnterBattleLog Parse(IDictionary d, NextTeamEnterBattleLog o)
	{
		o.PlayerId = BaseJsonMarshaler.ParseValueLong(d, "PlayerId");
		o.CreateInfos = BaseJsonMarshaler.ParseArray(d, "CreateInfos", JM_NShared_BattleCreatureCreateInfo.Parse);
		o.WaveCenterPositionInfos = BaseJsonMarshaler.ParseArray(d, "WaveCenterPositionInfos", JM_NShared_WaveCenterPositionInfo.Parse);
		o.PrevTeamUidList = BaseJsonMarshaler.ParseArrayCast(d, "PrevTeamUidList", BaseJsonMarshaler.CastToInt);
		return o;
	}

	public static string EncodeJsonString(NextTeamEnterBattleLog o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NextTeamEnterBattleLog o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("PlayerId", o.PlayerId.ToString());
		dictionary.Add("CreateInfos", BaseJsonMarshaler.EncodeArray(o.CreateInfos, JM_NShared_BattleCreatureCreateInfo.EncodeJsonObject));
		dictionary.Add("WaveCenterPositionInfos", BaseJsonMarshaler.EncodeArray(o.WaveCenterPositionInfos, JM_NShared_WaveCenterPositionInfo.EncodeJsonObject));
		dictionary.Add("PrevTeamUidList", BaseJsonMarshaler.EncodeArray(o.PrevTeamUidList));
		return dictionary;
	}
}
