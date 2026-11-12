using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_SelectShopInfo
{
	public static SelectShopInfo Parse(string s)
	{
		return Parse(s, new SelectShopInfo());
	}

	public static SelectShopInfo Parse(string s, SelectShopInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static SelectShopInfo Parse(IDictionary d)
	{
		return Parse(d, new SelectShopInfo());
	}

	public static SelectShopInfo Parse(IDictionary d, SelectShopInfo o)
	{
		o.Index = BaseJsonMarshaler.ParseValueInt(d, "Index");
		o.BeginTime = BaseJsonMarshaler.ParseValueString(d, "BeginTime");
		o.EndTime = BaseJsonMarshaler.ParseValueString(d, "EndTime");
		o.RestockEndTime = BaseJsonMarshaler.ParseValueString(d, "RestockEndTime");
		o.CategoryGroup = BaseJsonMarshaler.ParseValueString(d, "CategoryGroup");
		o.ResetCount = BaseJsonMarshaler.ParseValueInt(d, "ResetCount");
		o.ShowIndex = BaseJsonMarshaler.ParseValueInt(d, "ShowIndex");
		o.ItemInfos = BaseJsonMarshaler.ParseArray(d, "ItemInfos", JM_NShared_PayShopProductItemInfo.Parse);
		o.EquipItemInfos = BaseJsonMarshaler.ParseArray(d, "EquipItemInfos", JM_NShared_PayShopProductEquipItemInfo.Parse);
		o.PayShopProductInfos = BaseJsonMarshaler.ParseArray(d, "PayShopProductInfos", JM_NShared_NewPayShopProductInfo.Parse);
		return o;
	}

	public static string EncodeJsonString(SelectShopInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(SelectShopInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Index", o.Index.ToString());
		if (o.BeginTime != null)
		{
			dictionary.Add("BeginTime", o.BeginTime);
		}
		if (o.EndTime != null)
		{
			dictionary.Add("EndTime", o.EndTime);
		}
		if (o.RestockEndTime != null)
		{
			dictionary.Add("RestockEndTime", o.RestockEndTime);
		}
		if (o.CategoryGroup != null)
		{
			dictionary.Add("CategoryGroup", o.CategoryGroup);
		}
		dictionary.Add("ResetCount", o.ResetCount.ToString());
		dictionary.Add("ShowIndex", o.ShowIndex.ToString());
		dictionary.Add("ItemInfos", BaseJsonMarshaler.EncodeArray(o.ItemInfos, JM_NShared_PayShopProductItemInfo.EncodeJsonObject));
		dictionary.Add("EquipItemInfos", BaseJsonMarshaler.EncodeArray(o.EquipItemInfos, JM_NShared_PayShopProductEquipItemInfo.EncodeJsonObject));
		dictionary.Add("PayShopProductInfos", BaseJsonMarshaler.EncodeArray(o.PayShopProductInfos, JM_NShared_NewPayShopProductInfo.EncodeJsonObject));
		return dictionary;
	}
}
