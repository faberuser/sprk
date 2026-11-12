using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_PayShopImageUrlInfo
{
	public static PayShopImageUrlInfo Parse(string s)
	{
		return Parse(s, new PayShopImageUrlInfo());
	}

	public static PayShopImageUrlInfo Parse(string s, PayShopImageUrlInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PayShopImageUrlInfo Parse(IDictionary d)
	{
		return Parse(d, new PayShopImageUrlInfo());
	}

	public static PayShopImageUrlInfo Parse(IDictionary d, PayShopImageUrlInfo o)
	{
		o.Index = BaseJsonMarshaler.ParseValueInt(d, "Index");
		o.ImageUrl = BaseJsonMarshaler.ParseValueString(d, "ImageUrl");
		o.Image2Url = BaseJsonMarshaler.ParseValueString(d, "Image2Url");
		return o;
	}

	public static string EncodeJsonString(PayShopImageUrlInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PayShopImageUrlInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Index", o.Index.ToString());
		if (o.ImageUrl != null)
		{
			dictionary.Add("ImageUrl", o.ImageUrl);
		}
		if (o.Image2Url != null)
		{
			dictionary.Add("Image2Url", o.Image2Url);
		}
		return dictionary;
	}
}
