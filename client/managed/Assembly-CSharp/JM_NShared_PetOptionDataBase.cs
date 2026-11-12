using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_PetOptionDataBase
{
	public static PetOptionDataBase Parse(string s)
	{
		return Parse(s, new PetOptionDataBase());
	}

	public static PetOptionDataBase Parse(string s, PetOptionDataBase o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PetOptionDataBase Parse(IDictionary d)
	{
		return Parse(d, new PetOptionDataBase());
	}

	public static PetOptionDataBase Parse(IDictionary d, PetOptionDataBase o)
	{
		o.Index = BaseJsonMarshaler.ParseValueInt(d, "Index");
		return o;
	}

	public static string EncodeJsonString(PetOptionDataBase o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PetOptionDataBase o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Index", o.Index.ToString());
		return dictionary;
	}
}
