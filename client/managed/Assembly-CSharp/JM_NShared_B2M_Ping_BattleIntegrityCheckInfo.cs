using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.B2M_Ping;

public static class JM_NShared_B2M_Ping_BattleIntegrityCheckInfo
{
	public static BattleIntegrityCheckInfo Parse(string s)
	{
		return Parse(s, new BattleIntegrityCheckInfo());
	}

	public static BattleIntegrityCheckInfo Parse(string s, BattleIntegrityCheckInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static BattleIntegrityCheckInfo Parse(IDictionary d)
	{
		return Parse(d, new BattleIntegrityCheckInfo());
	}

	public static BattleIntegrityCheckInfo Parse(IDictionary d, BattleIntegrityCheckInfo o)
	{
		o.RequestServer = BaseJsonMarshaler.ParseValueString(d, "RequestServer");
		o.IntegrityType = BaseJsonMarshaler.ParseEnum<IntegrityType>(d, "IntegrityType");
		o.RequestId = BaseJsonMarshaler.ParseValueString(d, "RequestId");
		o.AccountId = BaseJsonMarshaler.ParseValueLong(d, "AccountId");
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.DungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "DungeonIndex");
		o.Difficulty = BaseJsonMarshaler.ParseValueInt(d, "Difficulty");
		o.BattleType = BaseJsonMarshaler.ParseEnum<BattleType>(d, "BattleType");
		o.BattleLogString = BaseJsonMarshaler.ParseValueString(d, "BattleLogString");
		o.ExtraData = BaseJsonMarshaler.ParseObject(d, "ExtraData", JM_NShared_BattleInstanceExtraData.Parse);
		o.LeftHeroInfos = BaseJsonMarshaler.ParseArray(d, "LeftHeroInfos", JM_NShared_CachedHeroInfo.Parse);
		o.LeftInitialHeroInfos = BaseJsonMarshaler.ParseArray(d, "LeftInitialHeroInfos", JM_NShared_ResultCreatureInfo.Parse);
		o.RightHeroInfos = BaseJsonMarshaler.ParseArray(d, "RightHeroInfos", JM_NShared_CachedHeroInfo.Parse);
		o.RightInitialHeroInfos = BaseJsonMarshaler.ParseArray(d, "RightInitialHeroInfos", JM_NShared_ResultCreatureInfo.Parse);
		o.CreatureInfoString = BaseJsonMarshaler.ParseValueString(d, "CreatureInfoString");
		o.CheckPriority = BaseJsonMarshaler.ParseEnum<CheckPriority>(d, "CheckPriority");
		o.CheckMode = BaseJsonMarshaler.ParseEnum<BattleIntegrityCheckMode>(d, "CheckMode");
		return o;
	}

	public static string EncodeJsonString(BattleIntegrityCheckInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(BattleIntegrityCheckInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.RequestServer != null)
		{
			dictionary.Add("RequestServer", o.RequestServer);
		}
		dictionary.Add("IntegrityType", o.IntegrityType.ToString());
		if (o.RequestId != null)
		{
			dictionary.Add("RequestId", o.RequestId);
		}
		dictionary.Add("AccountId", o.AccountId.ToString());
		dictionary.Add("ChapterIndex", o.ChapterIndex.ToString());
		dictionary.Add("DungeonIndex", o.DungeonIndex.ToString());
		dictionary.Add("Difficulty", o.Difficulty.ToString());
		dictionary.Add("BattleType", o.BattleType.ToString());
		if (o.BattleLogString != null)
		{
			dictionary.Add("BattleLogString", o.BattleLogString);
		}
		dictionary.Add("ExtraData", JM_NShared_BattleInstanceExtraData.EncodeJsonObject(o.ExtraData));
		dictionary.Add("LeftHeroInfos", BaseJsonMarshaler.EncodeArray(o.LeftHeroInfos, JM_NShared_CachedHeroInfo.EncodeJsonObject));
		dictionary.Add("LeftInitialHeroInfos", BaseJsonMarshaler.EncodeArray(o.LeftInitialHeroInfos, JM_NShared_ResultCreatureInfo.EncodeJsonObject));
		dictionary.Add("RightHeroInfos", BaseJsonMarshaler.EncodeArray(o.RightHeroInfos, JM_NShared_CachedHeroInfo.EncodeJsonObject));
		dictionary.Add("RightInitialHeroInfos", BaseJsonMarshaler.EncodeArray(o.RightInitialHeroInfos, JM_NShared_ResultCreatureInfo.EncodeJsonObject));
		if (o.CreatureInfoString != null)
		{
			dictionary.Add("CreatureInfoString", o.CreatureInfoString);
		}
		dictionary.Add("CheckPriority", o.CheckPriority.ToString());
		dictionary.Add("CheckMode", o.CheckMode.ToString());
		return dictionary;
	}
}
