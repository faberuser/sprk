using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_PetStatDataBase
{
	public static PetStatDataBase Parse(string s)
	{
		return Parse(s, new PetStatDataBase());
	}

	public static PetStatDataBase Parse(string s, PetStatDataBase o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PetStatDataBase Parse(IDictionary d)
	{
		return Parse(d, new PetStatDataBase());
	}

	public static PetStatDataBase Parse(IDictionary d, PetStatDataBase o)
	{
		o.OptionType = BaseJsonMarshaler.ParseEnum<OperationType>(d, "OptionType");
		o.OptionValue = BaseJsonMarshaler.ParseValueInt(d, "OptionValue");
		o.Index = BaseJsonMarshaler.ParseValueInt(d, "Index");
		return o;
	}

	public static string EncodeJsonString(PetStatDataBase o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PetStatDataBase o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("OptionType", o.OptionType.ToString());
		dictionary.Add("OptionValue", o.OptionValue.ToString());
		dictionary.Add("Index", o.Index.ToString());
		return dictionary;
	}
}
