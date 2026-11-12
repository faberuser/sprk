using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.UseCostumeSelectItem;

public static class JM_NShared_UseCostumeSelectItem_Request
{
	public static NShared.UseCostumeSelectItem.Request Parse(string s)
	{
		return Parse(s, new NShared.UseCostumeSelectItem.Request());
	}

	public static NShared.UseCostumeSelectItem.Request Parse(string s, NShared.UseCostumeSelectItem.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.UseCostumeSelectItem.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.UseCostumeSelectItem.Request());
	}

	public static NShared.UseCostumeSelectItem.Request Parse(IDictionary d, NShared.UseCostumeSelectItem.Request o)
	{
		o.ItemIndex = BaseJsonMarshaler.ParseValueInt(d, "ItemIndex");
		o.CostumeIndex = BaseJsonMarshaler.ParseValueInt(d, "CostumeIndex");
		o.GetOtherReward = BaseJsonMarshaler.ParseValueBool(d, "GetOtherReward");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.UseCostumeSelectItem.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.UseCostumeSelectItem.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ItemIndex", o.ItemIndex.ToString());
		dictionary.Add("CostumeIndex", o.CostumeIndex.ToString());
		dictionary.Add("GetOtherReward", o.GetOtherReward.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
