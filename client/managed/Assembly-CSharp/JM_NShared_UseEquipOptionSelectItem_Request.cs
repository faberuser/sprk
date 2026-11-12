using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.UseEquipOptionSelectItem;

public static class JM_NShared_UseEquipOptionSelectItem_Request
{
	public static NShared.UseEquipOptionSelectItem.Request Parse(string s)
	{
		return Parse(s, new NShared.UseEquipOptionSelectItem.Request());
	}

	public static NShared.UseEquipOptionSelectItem.Request Parse(string s, NShared.UseEquipOptionSelectItem.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.UseEquipOptionSelectItem.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.UseEquipOptionSelectItem.Request());
	}

	public static NShared.UseEquipOptionSelectItem.Request Parse(IDictionary d, NShared.UseEquipOptionSelectItem.Request o)
	{
		o.ItemIndex = BaseJsonMarshaler.ParseValueInt(d, "ItemIndex");
		o.EquipOptionItemIndex = BaseJsonMarshaler.ParseValueInt(d, "EquipOptionItemIndex");
		o.EquipOptionIndices = BaseJsonMarshaler.ParseArrayCast(d, "EquipOptionIndices", BaseJsonMarshaler.CastToInt);
		o.EquipExtraOptionIndices = BaseJsonMarshaler.ParseArrayCast(d, "EquipExtraOptionIndices", BaseJsonMarshaler.CastToInt);
		o.ItemCount = BaseJsonMarshaler.ParseValueInt(d, "ItemCount");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.UseEquipOptionSelectItem.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.UseEquipOptionSelectItem.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ItemIndex", o.ItemIndex.ToString());
		dictionary.Add("EquipOptionItemIndex", o.EquipOptionItemIndex.ToString());
		dictionary.Add("EquipOptionIndices", BaseJsonMarshaler.EncodeArray(o.EquipOptionIndices));
		dictionary.Add("EquipExtraOptionIndices", BaseJsonMarshaler.EncodeArray(o.EquipExtraOptionIndices));
		dictionary.Add("ItemCount", o.ItemCount.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
