using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_DropBonusEventRemoved
{
	public static DropBonusEventRemoved Parse(string s)
	{
		return Parse(s, new DropBonusEventRemoved());
	}

	public static DropBonusEventRemoved Parse(string s, DropBonusEventRemoved o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static DropBonusEventRemoved Parse(IDictionary d)
	{
		return Parse(d, new DropBonusEventRemoved());
	}

	public static DropBonusEventRemoved Parse(IDictionary d, DropBonusEventRemoved o)
	{
		o.Id = BaseJsonMarshaler.ParseValueInt(d, "Id");
		return o;
	}

	public static string EncodeJsonString(DropBonusEventRemoved o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(DropBonusEventRemoved o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Id", o.Id.ToString());
		return dictionary;
	}
}
