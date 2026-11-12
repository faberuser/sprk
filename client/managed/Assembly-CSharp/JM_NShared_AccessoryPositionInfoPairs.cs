using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_AccessoryPositionInfoPairs
{
	public static AccessoryPositionInfoPairs Parse(string s)
	{
		return Parse(s, new AccessoryPositionInfoPairs());
	}

	public static AccessoryPositionInfoPairs Parse(string s, AccessoryPositionInfoPairs o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static AccessoryPositionInfoPairs Parse(IDictionary d)
	{
		return Parse(d, new AccessoryPositionInfoPairs());
	}

	public static AccessoryPositionInfoPairs Parse(IDictionary d, AccessoryPositionInfoPairs o)
	{
		o.Container = BaseJsonMarshaler.ParseDictionary<int, PositionInfo, Dictionary<int, PositionInfo>>(d, "Container", JM_NShared_PositionInfo.Parse);
		return o;
	}

	public static string EncodeJsonString(AccessoryPositionInfoPairs o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(AccessoryPositionInfoPairs o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Container", BaseJsonMarshaler.EncodeDictionary(o.Container, JM_NShared_PositionInfo.EncodeJsonObject));
		return dictionary;
	}
}
