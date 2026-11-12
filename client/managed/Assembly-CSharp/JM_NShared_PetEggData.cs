using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_PetEggData
{
	public static PetEggData Parse(string s)
	{
		return Parse(s, new PetEggData());
	}

	public static PetEggData Parse(string s, PetEggData o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PetEggData Parse(IDictionary d)
	{
		return Parse(d, new PetEggData());
	}

	public static PetEggData Parse(IDictionary d, PetEggData o)
	{
		o.Index = BaseJsonMarshaler.ParseValueInt(d, "Index");
		o.ReqTime = BaseJsonMarshaler.ParseValueInt(d, "ReqTime");
		o.ItemGroupCode = BaseJsonMarshaler.ParseValueString(d, "ItemGroupCode");
		return o;
	}

	public static string EncodeJsonString(PetEggData o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PetEggData o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Index", o.Index.ToString());
		dictionary.Add("ReqTime", o.ReqTime.ToString());
		if (o.ItemGroupCode != null)
		{
			dictionary.Add("ItemGroupCode", o.ItemGroupCode);
		}
		return dictionary;
	}
}
