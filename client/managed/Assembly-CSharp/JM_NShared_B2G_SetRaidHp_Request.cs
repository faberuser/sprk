using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.B2G_SetRaidHp;

public static class JM_NShared_B2G_SetRaidHp_Request
{
	public static NShared.B2G_SetRaidHp.Request Parse(string s)
	{
		return Parse(s, new NShared.B2G_SetRaidHp.Request());
	}

	public static NShared.B2G_SetRaidHp.Request Parse(string s, NShared.B2G_SetRaidHp.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.B2G_SetRaidHp.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.B2G_SetRaidHp.Request());
	}

	public static NShared.B2G_SetRaidHp.Request Parse(IDictionary d, NShared.B2G_SetRaidHp.Request o)
	{
		o.MasterId = BaseJsonMarshaler.ParseValueLong(d, "MasterId");
		o.RaidIndex = BaseJsonMarshaler.ParseValueInt(d, "RaidIndex");
		o.RaidLevel = BaseJsonMarshaler.ParseValueInt(d, "RaidLevel");
		o.MaxHp = BaseJsonMarshaler.ParseValueLong(d, "MaxHp");
		o.CurrentHp = BaseJsonMarshaler.ParseValueLong(d, "CurrentHp");
		o.BattleEnd = BaseJsonMarshaler.ParseValueBool(d, "BattleEnd");
		o.Win = BaseJsonMarshaler.ParseValueBool(d, "Win");
		o.TotalDamageHp = BaseJsonMarshaler.ParseValueLong(d, "TotalDamageHp");
		return o;
	}

	public static string EncodeJsonString(NShared.B2G_SetRaidHp.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.B2G_SetRaidHp.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("MasterId", o.MasterId.ToString());
		dictionary.Add("RaidIndex", o.RaidIndex.ToString());
		dictionary.Add("RaidLevel", o.RaidLevel.ToString());
		dictionary.Add("MaxHp", o.MaxHp.ToString());
		dictionary.Add("CurrentHp", o.CurrentHp.ToString());
		dictionary.Add("BattleEnd", o.BattleEnd.ToString());
		dictionary.Add("Win", o.Win.ToString());
		dictionary.Add("TotalDamageHp", o.TotalDamageHp.ToString());
		return dictionary;
	}
}
