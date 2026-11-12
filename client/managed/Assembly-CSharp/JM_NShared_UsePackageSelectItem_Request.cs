using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.UsePackageSelectItem;

public static class JM_NShared_UsePackageSelectItem_Request
{
	public static NShared.UsePackageSelectItem.Request Parse(string s)
	{
		return Parse(s, new NShared.UsePackageSelectItem.Request());
	}

	public static NShared.UsePackageSelectItem.Request Parse(string s, NShared.UsePackageSelectItem.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.UsePackageSelectItem.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.UsePackageSelectItem.Request());
	}

	public static NShared.UsePackageSelectItem.Request Parse(IDictionary d, NShared.UsePackageSelectItem.Request o)
	{
		o.ItemIndex = BaseJsonMarshaler.ParseValueInt(d, "ItemIndex");
		o.ItemCount = BaseJsonMarshaler.ParseValueInt(d, "ItemCount");
		o.SelectItemIndex = BaseJsonMarshaler.ParseValueInt(d, "SelectItemIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.UsePackageSelectItem.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.UsePackageSelectItem.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ItemIndex", o.ItemIndex.ToString());
		dictionary.Add("ItemCount", o.ItemCount.ToString());
		dictionary.Add("SelectItemIndex", o.SelectItemIndex.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
