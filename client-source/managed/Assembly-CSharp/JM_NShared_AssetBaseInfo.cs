using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_AssetBaseInfo
{
	public static AssetBaseInfo Parse(string s)
	{
		return Parse(s, new AssetBaseInfo());
	}

	public static AssetBaseInfo Parse(string s, AssetBaseInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static AssetBaseInfo Parse(IDictionary d)
	{
		return Parse(d, new AssetBaseInfo());
	}

	public static AssetBaseInfo Parse(IDictionary d, AssetBaseInfo o)
	{
		o.Cat = BaseJsonMarshaler.ParseValueInt(d, "Cat");
		o.Pri = BaseJsonMarshaler.ParseValueInt(d, "Pri");
		o.Length = BaseJsonMarshaler.ParseValueInt(d, "Length");
		o.Md5 = BaseJsonMarshaler.ParseValueString(d, "Md5");
		o.Crc = BaseJsonMarshaler.ParseValueUInt(d, "Crc");
		o.Time = BaseJsonMarshaler.ParseValueLong(d, "Time");
		return o;
	}

	public static string EncodeJsonString(AssetBaseInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(AssetBaseInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Cat", o.Cat.ToString());
		dictionary.Add("Pri", o.Pri.ToString());
		dictionary.Add("Length", o.Length.ToString());
		if (o.Md5 != null)
		{
			dictionary.Add("Md5", o.Md5);
		}
		dictionary.Add("Crc", o.Crc.ToString());
		dictionary.Add("Time", o.Time.ToString());
		return dictionary;
	}
}
