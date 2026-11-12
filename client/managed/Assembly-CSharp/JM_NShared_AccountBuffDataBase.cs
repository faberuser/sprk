using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_AccountBuffDataBase
{
	public static AccountBuffDataBase Parse(string s)
	{
		return Parse(s, new AccountBuffDataBase());
	}

	public static AccountBuffDataBase Parse(string s, AccountBuffDataBase o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static AccountBuffDataBase Parse(IDictionary d)
	{
		return Parse(d, new AccountBuffDataBase());
	}

	public static AccountBuffDataBase Parse(IDictionary d, AccountBuffDataBase o)
	{
		o.AccountBuffIndex = BaseJsonMarshaler.ParseValueInt(d, "AccountBuffIndex");
		o.SkillIndex = BaseJsonMarshaler.ParseValueInt(d, "SkillIndex");
		o.SkillLevel = BaseJsonMarshaler.ParseValueInt(d, "SkillLevel");
		return o;
	}

	public static string EncodeJsonString(AccountBuffDataBase o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(AccountBuffDataBase o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("AccountBuffIndex", o.AccountBuffIndex.ToString());
		dictionary.Add("SkillIndex", o.SkillIndex.ToString());
		dictionary.Add("SkillLevel", o.SkillLevel.ToString());
		return dictionary;
	}
}
