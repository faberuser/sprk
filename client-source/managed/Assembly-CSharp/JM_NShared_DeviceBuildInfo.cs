using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_DeviceBuildInfo
{
	public static DeviceBuildInfo Parse(string s)
	{
		return Parse(s, new DeviceBuildInfo());
	}

	public static DeviceBuildInfo Parse(string s, DeviceBuildInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static DeviceBuildInfo Parse(IDictionary d)
	{
		return Parse(d, new DeviceBuildInfo());
	}

	public static DeviceBuildInfo Parse(IDictionary d, DeviceBuildInfo o)
	{
		o.Fingerprint = BaseJsonMarshaler.ParseValueString(d, "Fingerprint");
		o.Model = BaseJsonMarshaler.ParseValueString(d, "Model");
		o.Brand = BaseJsonMarshaler.ParseValueString(d, "Brand");
		o.Device = BaseJsonMarshaler.ParseValueString(d, "Device");
		o.Product = BaseJsonMarshaler.ParseValueString(d, "Product");
		o.Type = BaseJsonMarshaler.ParseValueString(d, "Type");
		o.Serial = BaseJsonMarshaler.ParseValueString(d, "Serial");
		o.Board = BaseJsonMarshaler.ParseValueString(d, "Board");
		return o;
	}

	public static string EncodeJsonString(DeviceBuildInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(DeviceBuildInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.Fingerprint != null)
		{
			dictionary.Add("Fingerprint", o.Fingerprint);
		}
		if (o.Model != null)
		{
			dictionary.Add("Model", o.Model);
		}
		if (o.Brand != null)
		{
			dictionary.Add("Brand", o.Brand);
		}
		if (o.Device != null)
		{
			dictionary.Add("Device", o.Device);
		}
		if (o.Product != null)
		{
			dictionary.Add("Product", o.Product);
		}
		if (o.Type != null)
		{
			dictionary.Add("Type", o.Type);
		}
		if (o.Serial != null)
		{
			dictionary.Add("Serial", o.Serial);
		}
		if (o.Board != null)
		{
			dictionary.Add("Board", o.Board);
		}
		return dictionary;
	}
}
