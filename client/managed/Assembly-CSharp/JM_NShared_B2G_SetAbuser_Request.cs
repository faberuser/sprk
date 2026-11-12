using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.B2G_SetAbuser;

public static class JM_NShared_B2G_SetAbuser_Request
{
	public static NShared.B2G_SetAbuser.Request Parse(string s)
	{
		return Parse(s, new NShared.B2G_SetAbuser.Request());
	}

	public static NShared.B2G_SetAbuser.Request Parse(string s, NShared.B2G_SetAbuser.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.B2G_SetAbuser.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.B2G_SetAbuser.Request());
	}

	public static NShared.B2G_SetAbuser.Request Parse(IDictionary d, NShared.B2G_SetAbuser.Request o)
	{
		o.AccountId = BaseJsonMarshaler.ParseValueLong(d, "AccountId");
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.DungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "DungeonIndex");
		o.Difficulty = BaseJsonMarshaler.ParseValueInt(d, "Difficulty");
		o.BattleVersion = BaseJsonMarshaler.ParseValueString(d, "BattleVersion");
		o.BattleLogString = BaseJsonMarshaler.ParseValueString(d, "BattleLogString");
		o.CreatureInfoString = BaseJsonMarshaler.ParseValueString(d, "CreatureInfoString");
		o.VerifiedCreatureInfoString = BaseJsonMarshaler.ParseValueString(d, "VerifiedCreatureInfoString");
		o.CheckResult = BaseJsonMarshaler.ParseValueString(d, "CheckResult");
		o.AbusingCategory = BaseJsonMarshaler.ParseValueString(d, "AbusingCategory");
		o.LeftValue = BaseJsonMarshaler.ParseValueInt(d, "LeftValue");
		o.RightValue = BaseJsonMarshaler.ParseValueInt(d, "RightValue");
		return o;
	}

	public static string EncodeJsonString(NShared.B2G_SetAbuser.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.B2G_SetAbuser.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("AccountId", o.AccountId.ToString());
		dictionary.Add("ChapterIndex", o.ChapterIndex.ToString());
		dictionary.Add("DungeonIndex", o.DungeonIndex.ToString());
		dictionary.Add("Difficulty", o.Difficulty.ToString());
		if (o.BattleVersion != null)
		{
			dictionary.Add("BattleVersion", o.BattleVersion);
		}
		if (o.BattleLogString != null)
		{
			dictionary.Add("BattleLogString", o.BattleLogString);
		}
		if (o.CreatureInfoString != null)
		{
			dictionary.Add("CreatureInfoString", o.CreatureInfoString);
		}
		if (o.VerifiedCreatureInfoString != null)
		{
			dictionary.Add("VerifiedCreatureInfoString", o.VerifiedCreatureInfoString);
		}
		if (o.CheckResult != null)
		{
			dictionary.Add("CheckResult", o.CheckResult);
		}
		if (o.AbusingCategory != null)
		{
			dictionary.Add("AbusingCategory", o.AbusingCategory);
		}
		dictionary.Add("LeftValue", o.LeftValue.ToString());
		dictionary.Add("RightValue", o.RightValue.ToString());
		return dictionary;
	}
}
