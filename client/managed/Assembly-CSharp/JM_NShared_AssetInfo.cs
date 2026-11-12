using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_AssetInfo
{
	public static AssetInfo Parse(string s)
	{
		return Parse(s, new AssetInfo());
	}

	public static AssetInfo Parse(string s, AssetInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static AssetInfo Parse(IDictionary d)
	{
		return Parse(d, new AssetInfo());
	}

	public static AssetInfo Parse(IDictionary d, AssetInfo o)
	{
		o.EntryName = BaseJsonMarshaler.ParseValueString(d, "EntryName");
		o.Cat = BaseJsonMarshaler.ParseValueInt(d, "Cat");
		o.Pri = BaseJsonMarshaler.ParseValueInt(d, "Pri");
		o.Length = BaseJsonMarshaler.ParseValueInt(d, "Length");
		o.Md5 = BaseJsonMarshaler.ParseValueString(d, "Md5");
		o.Crc = BaseJsonMarshaler.ParseValueUInt(d, "Crc");
		o.Time = BaseJsonMarshaler.ParseValueLong(d, "Time");
		return o;
	}

	public static string EncodeJsonString(AssetInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(AssetInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.EntryName != null)
		{
			dictionary.Add("EntryName", o.EntryName);
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
