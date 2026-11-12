using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_WaveCenterPositionInfo
{
	public static WaveCenterPositionInfo Parse(string s)
	{
		return Parse(s, new WaveCenterPositionInfo());
	}

	public static WaveCenterPositionInfo Parse(string s, WaveCenterPositionInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static WaveCenterPositionInfo Parse(IDictionary d)
	{
		return Parse(d, new WaveCenterPositionInfo());
	}

	public static WaveCenterPositionInfo Parse(IDictionary d, WaveCenterPositionInfo o)
	{
		o.Mx = BaseJsonMarshaler.ParseValueLong(d, "Mx");
		o.My = BaseJsonMarshaler.ParseValueLong(d, "My");
		o.Mz = BaseJsonMarshaler.ParseValueLong(d, "Mz");
		o.GroupIndex = BaseJsonMarshaler.ParseValueInt(d, "GroupIndex");
		return o;
	}

	public static string EncodeJsonString(WaveCenterPositionInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(WaveCenterPositionInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Mx", o.Mx.ToString());
		dictionary.Add("My", o.My.ToString());
		dictionary.Add("Mz", o.Mz.ToString());
		dictionary.Add("GroupIndex", o.GroupIndex.ToString());
		return dictionary;
	}
}
