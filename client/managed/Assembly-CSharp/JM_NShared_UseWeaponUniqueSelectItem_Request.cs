using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.UseWeaponUniqueSelectItem;

public static class JM_NShared_UseWeaponUniqueSelectItem_Request
{
	public static NShared.UseWeaponUniqueSelectItem.Request Parse(string s)
	{
		return Parse(s, new NShared.UseWeaponUniqueSelectItem.Request());
	}

	public static NShared.UseWeaponUniqueSelectItem.Request Parse(string s, NShared.UseWeaponUniqueSelectItem.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.UseWeaponUniqueSelectItem.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.UseWeaponUniqueSelectItem.Request());
	}

	public static NShared.UseWeaponUniqueSelectItem.Request Parse(IDictionary d, NShared.UseWeaponUniqueSelectItem.Request o)
	{
		o.ItemIndex = BaseJsonMarshaler.ParseValueInt(d, "ItemIndex");
		o.WeaponUniqueItemIndex = BaseJsonMarshaler.ParseValueInt(d, "WeaponUniqueItemIndex");
		o.ItemCount = BaseJsonMarshaler.ParseValueInt(d, "ItemCount");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.UseWeaponUniqueSelectItem.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.UseWeaponUniqueSelectItem.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ItemIndex", o.ItemIndex.ToString());
		dictionary.Add("WeaponUniqueItemIndex", o.WeaponUniqueItemIndex.ToString());
		dictionary.Add("ItemCount", o.ItemCount.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
