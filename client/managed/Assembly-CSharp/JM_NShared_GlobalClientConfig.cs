using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_GlobalClientConfig
{
	public static GlobalClientConfig Parse(string s)
	{
		return Parse(s, new GlobalClientConfig());
	}

	public static GlobalClientConfig Parse(string s, GlobalClientConfig o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GlobalClientConfig Parse(IDictionary d)
	{
		return Parse(d, new GlobalClientConfig());
	}

	public static GlobalClientConfig Parse(IDictionary d, GlobalClientConfig o)
	{
		o.AdjustAppToken = BaseJsonMarshaler.ParseValueString(d, "AdjustAppToken");
		o.UseAdjustAppSecret = BaseJsonMarshaler.ParseValueBool(d, "UseAdjustAppSecret");
		o.AndroidAdjustAppSecretId = BaseJsonMarshaler.ParseValueInt(d, "AndroidAdjustAppSecretId");
		o.AndroidAdjustAppSecretInfo = BaseJsonMarshaler.ParseArrayCast(d, "AndroidAdjustAppSecretInfo", BaseJsonMarshaler.CastToLong);
		o.IosAdjustAppSecretId = BaseJsonMarshaler.ParseValueInt(d, "IosAdjustAppSecretId");
		o.IosAdjustAppSecretInfo = BaseJsonMarshaler.ParseArrayCast(d, "IosAdjustAppSecretInfo", BaseJsonMarshaler.CastToLong);
		o.WindowsAdjustAppSecretId = BaseJsonMarshaler.ParseValueInt(d, "WindowsAdjustAppSecretId");
		o.WindowsAdjustAppSecretInfo = BaseJsonMarshaler.ParseArrayCast(d, "WindowsAdjustAppSecretInfo", BaseJsonMarshaler.CastToLong);
		return o;
	}

	public static string EncodeJsonString(GlobalClientConfig o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GlobalClientConfig o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.AdjustAppToken != null)
		{
			dictionary.Add("AdjustAppToken", o.AdjustAppToken);
		}
		dictionary.Add("UseAdjustAppSecret", o.UseAdjustAppSecret.ToString());
		dictionary.Add("AndroidAdjustAppSecretId", o.AndroidAdjustAppSecretId.ToString());
		dictionary.Add("AndroidAdjustAppSecretInfo", BaseJsonMarshaler.EncodeArray(o.AndroidAdjustAppSecretInfo));
		dictionary.Add("IosAdjustAppSecretId", o.IosAdjustAppSecretId.ToString());
		dictionary.Add("IosAdjustAppSecretInfo", BaseJsonMarshaler.EncodeArray(o.IosAdjustAppSecretInfo));
		dictionary.Add("WindowsAdjustAppSecretId", o.WindowsAdjustAppSecretId.ToString());
		dictionary.Add("WindowsAdjustAppSecretInfo", BaseJsonMarshaler.EncodeArray(o.WindowsAdjustAppSecretInfo));
		return dictionary;
	}
}
