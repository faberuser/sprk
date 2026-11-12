using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NEvent;

public static class JM_NShared_NEvent_EnterHeroTranscendSkillListPage
{
	public static EnterHeroTranscendSkillListPage Parse(string s)
	{
		return Parse(s, new EnterHeroTranscendSkillListPage());
	}

	public static EnterHeroTranscendSkillListPage Parse(string s, EnterHeroTranscendSkillListPage o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static EnterHeroTranscendSkillListPage Parse(IDictionary d)
	{
		return Parse(d, new EnterHeroTranscendSkillListPage());
	}

	public static EnterHeroTranscendSkillListPage Parse(IDictionary d, EnterHeroTranscendSkillListPage o)
	{
		o.IsMine = BaseJsonMarshaler.ParseValueBool(d, "IsMine");
		o.HasPoint = BaseJsonMarshaler.ParseValueBool(d, "HasPoint");
		return o;
	}

	public static string EncodeJsonString(EnterHeroTranscendSkillListPage o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(EnterHeroTranscendSkillListPage o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("IsMine", o.IsMine.ToString());
		dictionary.Add("HasPoint", o.HasPoint.ToString());
		return dictionary;
	}
}
