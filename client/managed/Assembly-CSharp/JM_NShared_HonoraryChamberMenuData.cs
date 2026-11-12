using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_HonoraryChamberMenuData
{
	public static HonoraryChamberMenuData Parse(string s)
	{
		return Parse(s, new HonoraryChamberMenuData());
	}

	public static HonoraryChamberMenuData Parse(string s, HonoraryChamberMenuData o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static HonoraryChamberMenuData Parse(IDictionary d)
	{
		return Parse(d, new HonoraryChamberMenuData());
	}

	public static HonoraryChamberMenuData Parse(IDictionary d, HonoraryChamberMenuData o)
	{
		o.Index = BaseJsonMarshaler.ParseValueInt(d, "Index");
		o.ParentIndex = BaseJsonMarshaler.ParseValueInt(d, "ParentIndex");
		o.Name = BaseJsonMarshaler.ParseValueString(d, "Name");
		o.ContentsType = BaseJsonMarshaler.ParseEnum<RecordsOfHonorContentType>(d, "ContentsType");
		return o;
	}

	public static string EncodeJsonString(HonoraryChamberMenuData o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(HonoraryChamberMenuData o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Index", o.Index.ToString());
		dictionary.Add("ParentIndex", o.ParentIndex.ToString());
		if (o.Name != null)
		{
			dictionary.Add("Name", o.Name);
		}
		dictionary.Add("ContentsType", o.ContentsType.ToString());
		return dictionary;
	}
}
