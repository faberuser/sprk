using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared.NEvent;

public static class JM_NShared_NEvent_EnterHeroSkillListPage
{
	public static EnterHeroSkillListPage Parse(string s)
	{
		return Parse(s, new EnterHeroSkillListPage());
	}

	public static EnterHeroSkillListPage Parse(string s, EnterHeroSkillListPage o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static EnterHeroSkillListPage Parse(IDictionary d)
	{
		return Parse(d, new EnterHeroSkillListPage());
	}

	public static EnterHeroSkillListPage Parse(IDictionary d, EnterHeroSkillListPage o)
	{
		return o;
	}

	public static string EncodeJsonString(EnterHeroSkillListPage o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(EnterHeroSkillListPage o)
	{
		return new Dictionary<string, object>();
	}
}
