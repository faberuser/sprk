using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_GachaSelectItemData
{
	public static GachaSelectItemData Parse(string s)
	{
		return Parse(s, new GachaSelectItemData());
	}

	public static GachaSelectItemData Parse(string s, GachaSelectItemData o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GachaSelectItemData Parse(IDictionary d)
	{
		return Parse(d, new GachaSelectItemData());
	}

	public static GachaSelectItemData Parse(IDictionary d, GachaSelectItemData o)
	{
		o.ItemIndex = BaseJsonMarshaler.ParseValueInt(d, "ItemIndex");
		o.GachaType = BaseJsonMarshaler.ParseEnum<EquipGachaType>(d, "GachaType");
		o.GachaIndices = BaseJsonMarshaler.ParseArrayCast(d, "GachaIndices", BaseJsonMarshaler.CastToInt);
		o.GachaCategoryType = BaseJsonMarshaler.ParseEnum<NewPayShopCategoryType>(d, "GachaCategoryType");
		return o;
	}

	public static string EncodeJsonString(GachaSelectItemData o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GachaSelectItemData o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ItemIndex", o.ItemIndex.ToString());
		dictionary.Add("GachaType", o.GachaType.ToString());
		dictionary.Add("GachaIndices", BaseJsonMarshaler.EncodeArray(o.GachaIndices));
		dictionary.Add("GachaCategoryType", o.GachaCategoryType.ToString());
		return dictionary;
	}
}
