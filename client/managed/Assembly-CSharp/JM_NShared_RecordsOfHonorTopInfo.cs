using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_RecordsOfHonorTopInfo
{
	public static RecordsOfHonorTopInfo Parse(string s)
	{
		return Parse(s, new RecordsOfHonorTopInfo());
	}

	public static RecordsOfHonorTopInfo Parse(string s, RecordsOfHonorTopInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static RecordsOfHonorTopInfo Parse(IDictionary d)
	{
		return Parse(d, new RecordsOfHonorTopInfo());
	}

	public static RecordsOfHonorTopInfo Parse(IDictionary d, RecordsOfHonorTopInfo o)
	{
		o.ContentType = BaseJsonMarshaler.ParseEnum<RecordsOfHonorContentType>(d, "ContentType");
		o.CountryCode = BaseJsonMarshaler.ParseValueString(d, "CountryCode");
		o.Name = BaseJsonMarshaler.ParseValueString(d, "Name");
		o.AvatarHeroIndex = BaseJsonMarshaler.ParseValueInt(d, "AvatarHeroIndex");
		return o;
	}

	public static string EncodeJsonString(RecordsOfHonorTopInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(RecordsOfHonorTopInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ContentType", o.ContentType.ToString());
		if (o.CountryCode != null)
		{
			dictionary.Add("CountryCode", o.CountryCode);
		}
		if (o.Name != null)
		{
			dictionary.Add("Name", o.Name);
		}
		dictionary.Add("AvatarHeroIndex", o.AvatarHeroIndex.ToString());
		return dictionary;
	}
}
