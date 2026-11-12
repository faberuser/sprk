using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.RefreshNode;

public static class JM_NShared_RefreshNode_Request
{
	public static NShared.RefreshNode.Request Parse(string s)
	{
		return Parse(s, new NShared.RefreshNode.Request());
	}

	public static NShared.RefreshNode.Request Parse(string s, NShared.RefreshNode.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.RefreshNode.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.RefreshNode.Request());
	}

	public static NShared.RefreshNode.Request Parse(IDictionary d, NShared.RefreshNode.Request o)
	{
		o.NodeIndex = BaseJsonMarshaler.ParseValueInt(d, "NodeIndex");
		o.UseGem = BaseJsonMarshaler.ParseValueBool(d, "UseGem");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.RefreshNode.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.RefreshNode.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("NodeIndex", o.NodeIndex.ToString());
		dictionary.Add("UseGem", o.UseGem.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
