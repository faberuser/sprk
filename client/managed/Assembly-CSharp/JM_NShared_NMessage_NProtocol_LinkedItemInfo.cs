using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_LinkedItemInfo
{
	public static LinkedItemInfo Parse(string s)
	{
		return Parse(s, new LinkedItemInfo());
	}

	public static LinkedItemInfo Parse(string s, LinkedItemInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static LinkedItemInfo Parse(IDictionary d)
	{
		return Parse(d, new LinkedItemInfo());
	}

	public static LinkedItemInfo Parse(IDictionary d, LinkedItemInfo o)
	{
		o.MaxEquipLevel = BaseJsonMarshaler.ParseValueInt(d, "MaxEquipLevel");
		o.EquipItemInfo = BaseJsonMarshaler.ParseObject(d, "EquipItemInfo", JM_NShared_EquipItemInfo.Parse);
		return o;
	}

	public static string EncodeJsonString(LinkedItemInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(LinkedItemInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("MaxEquipLevel", o.MaxEquipLevel.ToString());
		dictionary.Add("EquipItemInfo", JM_NShared_EquipItemInfo.EncodeJsonObject(o.EquipItemInfo));
		return dictionary;
	}
}
