using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_RightAwayProductEvent
{
	public static RightAwayProductEvent Parse(string s)
	{
		return Parse(s, new RightAwayProductEvent());
	}

	public static RightAwayProductEvent Parse(string s, RightAwayProductEvent o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static RightAwayProductEvent Parse(IDictionary d)
	{
		return Parse(d, new RightAwayProductEvent());
	}

	public static RightAwayProductEvent Parse(IDictionary d, RightAwayProductEvent o)
	{
		o.ProductInfos = BaseJsonMarshaler.ParseArray(d, "ProductInfos", JM_NShared_RightAwayProductInfo.Parse);
		o.RemovedIndices = BaseJsonMarshaler.ParseArrayCast(d, "RemovedIndices", BaseJsonMarshaler.CastToInt);
		return o;
	}

	public static string EncodeJsonString(RightAwayProductEvent o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(RightAwayProductEvent o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ProductInfos", BaseJsonMarshaler.EncodeArray(o.ProductInfos, JM_NShared_RightAwayProductInfo.EncodeJsonObject));
		dictionary.Add("RemovedIndices", BaseJsonMarshaler.EncodeArray(o.RemovedIndices));
		return dictionary;
	}
}
