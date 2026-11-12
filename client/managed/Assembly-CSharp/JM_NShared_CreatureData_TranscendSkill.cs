using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_CreatureData_TranscendSkill
{
	public static CreatureData.TranscendSkill Parse(string s)
	{
		return Parse(s, new CreatureData.TranscendSkill());
	}

	public static CreatureData.TranscendSkill Parse(string s, CreatureData.TranscendSkill o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static CreatureData.TranscendSkill Parse(IDictionary d)
	{
		return Parse(d, new CreatureData.TranscendSkill());
	}

	public static CreatureData.TranscendSkill Parse(IDictionary d, CreatureData.TranscendSkill o)
	{
		o.Transcended = BaseJsonMarshaler.ParseValueInt(d, "Transcended");
		o.Index = BaseJsonMarshaler.ParseValueInt(d, "Index");
		o.TranscendSkillIndex = BaseJsonMarshaler.ParseValueInt(d, "TranscendSkillIndex");
		o.ReqSKillPoint = BaseJsonMarshaler.ParseValueInt(d, "ReqSKillPoint");
		o.Level = BaseJsonMarshaler.ParseValueInt(d, "Level");
		o.MaxLevel = BaseJsonMarshaler.ParseValueInt(d, "MaxLevel");
		return o;
	}

	public static string EncodeJsonString(CreatureData.TranscendSkill o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(CreatureData.TranscendSkill o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Transcended", o.Transcended.ToString());
		dictionary.Add("Index", o.Index.ToString());
		dictionary.Add("TranscendSkillIndex", o.TranscendSkillIndex.ToString());
		dictionary.Add("ReqSKillPoint", o.ReqSKillPoint.ToString());
		dictionary.Add("Level", o.Level.ToString());
		dictionary.Add("MaxLevel", o.MaxLevel.ToString());
		return dictionary;
	}
}
