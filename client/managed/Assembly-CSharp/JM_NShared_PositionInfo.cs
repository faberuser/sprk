using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_PositionInfo
{
	public static PositionInfo Parse(string s)
	{
		return Parse(s, new PositionInfo());
	}

	public static PositionInfo Parse(string s, PositionInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PositionInfo Parse(IDictionary d)
	{
		return Parse(d, new PositionInfo());
	}

	public static PositionInfo Parse(IDictionary d, PositionInfo o)
	{
		o.PositionX = BaseJsonMarshaler.ParseValueFloat(d, "PositionX");
		o.PositionY = BaseJsonMarshaler.ParseValueFloat(d, "PositionY");
		o.PositionZ = BaseJsonMarshaler.ParseValueFloat(d, "PositionZ");
		o.RotationX = BaseJsonMarshaler.ParseValueFloat(d, "RotationX");
		o.RotationY = BaseJsonMarshaler.ParseValueFloat(d, "RotationY");
		o.RotationZ = BaseJsonMarshaler.ParseValueFloat(d, "RotationZ");
		o.Scale = BaseJsonMarshaler.ParseValueFloat(d, "Scale");
		return o;
	}

	public static string EncodeJsonString(PositionInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PositionInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("PositionX", o.PositionX.ToString());
		dictionary.Add("PositionY", o.PositionY.ToString());
		dictionary.Add("PositionZ", o.PositionZ.ToString());
		dictionary.Add("RotationX", o.RotationX.ToString());
		dictionary.Add("RotationY", o.RotationY.ToString());
		dictionary.Add("RotationZ", o.RotationZ.ToString());
		dictionary.Add("Scale", o.Scale.ToString());
		return dictionary;
	}
}
