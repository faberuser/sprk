using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared.NEvent;

public static class JM_NShared_NEvent_EnterHeroEquipListPage
{
	public static EnterHeroEquipListPage Parse(string s)
	{
		return Parse(s, new EnterHeroEquipListPage());
	}

	public static EnterHeroEquipListPage Parse(string s, EnterHeroEquipListPage o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static EnterHeroEquipListPage Parse(IDictionary d)
	{
		return Parse(d, new EnterHeroEquipListPage());
	}

	public static EnterHeroEquipListPage Parse(IDictionary d, EnterHeroEquipListPage o)
	{
		return o;
	}

	public static string EncodeJsonString(EnterHeroEquipListPage o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(EnterHeroEquipListPage o)
	{
		return new Dictionary<string, object>();
	}
}
