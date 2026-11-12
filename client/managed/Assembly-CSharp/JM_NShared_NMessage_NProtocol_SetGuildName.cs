using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_SetGuildName
{
	public static SetGuildName Parse(string s)
	{
		return Parse(s, new SetGuildName());
	}

	public static SetGuildName Parse(string s, SetGuildName o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static SetGuildName Parse(IDictionary d)
	{
		return Parse(d, new SetGuildName());
	}

	public static SetGuildName Parse(IDictionary d, SetGuildName o)
	{
		o.Name = BaseJsonMarshaler.ParseValueString(d, "Name");
		return o;
	}

	public static string EncodeJsonString(SetGuildName o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(SetGuildName o)
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
		return dictionary;
	}
}
