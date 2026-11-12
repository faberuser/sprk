using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.UsePetSelectItem;

public static class JM_NShared_UsePetSelectItem_Request
{
	public static NShared.UsePetSelectItem.Request Parse(string s)
	{
		return Parse(s, new NShared.UsePetSelectItem.Request());
	}

	public static NShared.UsePetSelectItem.Request Parse(string s, NShared.UsePetSelectItem.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.UsePetSelectItem.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.UsePetSelectItem.Request());
	}

	public static NShared.UsePetSelectItem.Request Parse(IDictionary d, NShared.UsePetSelectItem.Request o)
	{
		o.ItemIndex = BaseJsonMarshaler.ParseValueInt(d, "ItemIndex");
		o.PetIndex = BaseJsonMarshaler.ParseValueInt(d, "PetIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.UsePetSelectItem.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.UsePetSelectItem.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ItemIndex", o.ItemIndex.ToString());
		dictionary.Add("PetIndex", o.PetIndex.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
