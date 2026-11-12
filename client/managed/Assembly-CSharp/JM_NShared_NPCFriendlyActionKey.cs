using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_NPCFriendlyActionKey
{
	public static NPCFriendlyActionKey Parse(string s)
	{
		return Parse(s, new NPCFriendlyActionKey());
	}

	public static NPCFriendlyActionKey Parse(string s, NPCFriendlyActionKey o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NPCFriendlyActionKey Parse(IDictionary d)
	{
		return Parse(d, new NPCFriendlyActionKey());
	}

	public static NPCFriendlyActionKey Parse(IDictionary d, NPCFriendlyActionKey o)
	{
		return o;
	}

	public static string EncodeJsonString(NPCFriendlyActionKey o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NPCFriendlyActionKey o)
	{
		return new Dictionary<string, object>();
	}
}
