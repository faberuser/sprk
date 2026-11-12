using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_PetMiscInfo
{
	public static PetMiscInfo Parse(string s)
	{
		return Parse(s, new PetMiscInfo());
	}

	public static PetMiscInfo Parse(string s, PetMiscInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PetMiscInfo Parse(IDictionary d)
	{
		return Parse(d, new PetMiscInfo());
	}

	public static PetMiscInfo Parse(IDictionary d, PetMiscInfo o)
	{
		o.MiscKey = BaseJsonMarshaler.ParseEnum<PetMiscType>(d, "MiscKey");
		o.MiscValue = BaseJsonMarshaler.ParseValueString(d, "MiscValue");
		o.UpdatedTime = BaseJsonMarshaler.ParseValueString(d, "UpdatedTime");
		return o;
	}

	public static string EncodeJsonString(PetMiscInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PetMiscInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("MiscKey", o.MiscKey.ToString());
		if (o.MiscValue != null)
		{
			dictionary.Add("MiscValue", o.MiscValue);
		}
		if (o.UpdatedTime != null)
		{
			dictionary.Add("UpdatedTime", o.UpdatedTime);
		}
		return dictionary;
	}
}
