using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_BattleLoginReq
{
	public static BattleLoginReq Parse(string s)
	{
		return Parse(s, new BattleLoginReq());
	}

	public static BattleLoginReq Parse(string s, BattleLoginReq o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static BattleLoginReq Parse(IDictionary d)
	{
		return Parse(d, new BattleLoginReq());
	}

	public static BattleLoginReq Parse(IDictionary d, BattleLoginReq o)
	{
		o.MatchUid = BaseJsonMarshaler.ParseValueLong(d, "MatchUid");
		o.AccountId = BaseJsonMarshaler.ParseValueLong(d, "AccountId");
		o.MasterId = BaseJsonMarshaler.ParseValueLong(d, "MasterId");
		o.GameServerHost = BaseJsonMarshaler.ParseValueString(d, "GameServerHost");
		o.MatchServerHost = BaseJsonMarshaler.ParseValueString(d, "MatchServerHost");
		o.HeroIndices = BaseJsonMarshaler.ParseArrayCast(d, "HeroIndices", BaseJsonMarshaler.CastToInt);
		o.AiHeroIndices = BaseJsonMarshaler.ParseArrayCast(d, "AiHeroIndices", BaseJsonMarshaler.CastToInt);
		o.GroupHeroIndices = BaseJsonMarshaler.ParseArrayCast(d, "GroupHeroIndices", BaseJsonMarshaler.CastToInt);
		o.UserInfos = BaseJsonMarshaler.ParseArray(d, "UserInfos", JM_NShared_CachedUserInfo.Parse);
		o.IsEclipse = BaseJsonMarshaler.ParseValueBool(d, "IsEclipse");
		o.IsEclipseContinuousBattle = BaseJsonMarshaler.ParseValueBool(d, "IsEclipseContinuousBattle");
		o.GameSpeedRatio = BaseJsonMarshaler.ParseValueInt(d, "GameSpeedRatio");
		o.ExtraData = BaseJsonMarshaler.ParseObject(d, "ExtraData", JM_NShared_BattleInstanceExtraData.Parse);
		o.EnableTraceLogToFile = BaseJsonMarshaler.ParseValueBool(d, "EnableTraceLogToFile");
		o.RequestId = BaseJsonMarshaler.ParseValueInt(d, "RequestId");
		return o;
	}

	public static string EncodeJsonString(BattleLoginReq o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(BattleLoginReq o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return SprkConquest.Login(dictionary);
		}
		dictionary.Add("MatchUid", o.MatchUid.ToString());
		dictionary.Add("AccountId", o.AccountId.ToString());
		dictionary.Add("MasterId", o.MasterId.ToString());
		if (o.GameServerHost != null)
		{
			dictionary.Add("GameServerHost", o.GameServerHost);
		}
		if (o.MatchServerHost != null)
		{
			dictionary.Add("MatchServerHost", o.MatchServerHost);
		}
		dictionary.Add("HeroIndices", BaseJsonMarshaler.EncodeArray(o.HeroIndices));
		dictionary.Add("AiHeroIndices", BaseJsonMarshaler.EncodeArray(o.AiHeroIndices));
		dictionary.Add("GroupHeroIndices", BaseJsonMarshaler.EncodeArray(o.GroupHeroIndices));
		dictionary.Add("UserInfos", BaseJsonMarshaler.EncodeArray(o.UserInfos, JM_NShared_CachedUserInfo.EncodeJsonObject));
		dictionary.Add("IsEclipse", o.IsEclipse.ToString());
		dictionary.Add("IsEclipseContinuousBattle", o.IsEclipseContinuousBattle.ToString());
		dictionary.Add("GameSpeedRatio", o.GameSpeedRatio.ToString());
		dictionary.Add("ExtraData", JM_NShared_BattleInstanceExtraData.EncodeJsonObject(o.ExtraData));
		dictionary.Add("EnableTraceLogToFile", o.EnableTraceLogToFile.ToString());
		dictionary.Add("RequestId", o.RequestId.ToString());
		return SprkConquest.Login(dictionary);
	}
}
