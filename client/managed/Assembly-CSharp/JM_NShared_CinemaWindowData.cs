using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_CinemaWindowData
{
	public static CinemaWindowData Parse(string s)
	{
		return Parse(s, new CinemaWindowData());
	}

	public static CinemaWindowData Parse(string s, CinemaWindowData o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static CinemaWindowData Parse(IDictionary d)
	{
		return Parse(d, new CinemaWindowData());
	}

	public static CinemaWindowData Parse(IDictionary d, CinemaWindowData o)
	{
		o.Name = BaseJsonMarshaler.ParseValueString(d, "Name");
		o.IsOpenPlayOnce = BaseJsonMarshaler.ParseValueBool(d, "IsOpenPlayOnce");
		o.OpenNodeTreePack = BaseJsonMarshaler.ParseValueString(d, "OpenNodeTreePack");
		o.OpenNodeTreeName = BaseJsonMarshaler.ParseValueString(d, "OpenNodeTreeName");
		o.IsClosePlayOnce = BaseJsonMarshaler.ParseValueBool(d, "IsClosePlayOnce");
		o.CloseNodeTreePack = BaseJsonMarshaler.ParseValueString(d, "CloseNodeTreePack");
		o.CloseNodeTreeName = BaseJsonMarshaler.ParseValueString(d, "CloseNodeTreeName");
		o.IsOwnerWindow = BaseJsonMarshaler.ParseValueBool(d, "IsOwnerWindow");
		return o;
	}

	public static string EncodeJsonString(CinemaWindowData o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(CinemaWindowData o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.Name != null)
		{
			dictionary.Add("Name", o.Name);
		}
		dictionary.Add("IsOpenPlayOnce", o.IsOpenPlayOnce.ToString());
		if (o.OpenNodeTreePack != null)
		{
			dictionary.Add("OpenNodeTreePack", o.OpenNodeTreePack);
		}
		if (o.OpenNodeTreeName != null)
		{
			dictionary.Add("OpenNodeTreeName", o.OpenNodeTreeName);
		}
		dictionary.Add("IsClosePlayOnce", o.IsClosePlayOnce.ToString());
		if (o.CloseNodeTreePack != null)
		{
			dictionary.Add("CloseNodeTreePack", o.CloseNodeTreePack);
		}
		if (o.CloseNodeTreeName != null)
		{
			dictionary.Add("CloseNodeTreeName", o.CloseNodeTreeName);
		}
		dictionary.Add("IsOwnerWindow", o.IsOwnerWindow.ToString());
		return dictionary;
	}
}
