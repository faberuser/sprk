using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_EquipGachaInfo
{
	public static EquipGachaInfo Parse(string s)
	{
		return Parse(s, new EquipGachaInfo());
	}

	public static EquipGachaInfo Parse(string s, EquipGachaInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static EquipGachaInfo Parse(IDictionary d)
	{
		return Parse(d, new EquipGachaInfo());
	}

	public static EquipGachaInfo Parse(IDictionary d, EquipGachaInfo o)
	{
		o.GachaIndex = BaseJsonMarshaler.ParseValueInt(d, "GachaIndex");
		o.BeginTime = BaseJsonMarshaler.ParseValueString(d, "BeginTime");
		o.EndTime = BaseJsonMarshaler.ParseValueString(d, "EndTime");
		o.OnSale = BaseJsonMarshaler.ParseValueBool(d, "OnSale");
		o.Discount = BaseJsonMarshaler.ParseValueInt(d, "Discount");
		o.Durational = BaseJsonMarshaler.ParseValueInt(d, "Durational");
		o.SaleResetTime = BaseJsonMarshaler.ParseValueString(d, "SaleResetTime");
		o.ProgressResetTime = BaseJsonMarshaler.ParseValueString(d, "ProgressResetTime");
		o.IsUTCBeginTime = BaseJsonMarshaler.ParseValueInt(d, "IsUTCBeginTime");
		o.IsUTCEndTime = BaseJsonMarshaler.ParseValueInt(d, "IsUTCEndTime");
		o.IsUTCProgressResetTime = BaseJsonMarshaler.ParseValueInt(d, "IsUTCProgressResetTime");
		o.LastDiscountedTime = BaseJsonMarshaler.ParseValueString(d, "LastDiscountedTime");
		o.RemainNumberOfDiscount = BaseJsonMarshaler.ParseValueInt(d, "RemainNumberOfDiscount");
		return o;
	}

	public static string EncodeJsonString(EquipGachaInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(EquipGachaInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("GachaIndex", o.GachaIndex.ToString());
		if (o.BeginTime != null)
		{
			dictionary.Add("BeginTime", o.BeginTime);
		}
		if (o.EndTime != null)
		{
			dictionary.Add("EndTime", o.EndTime);
		}
		dictionary.Add("OnSale", o.OnSale.ToString());
		dictionary.Add("Discount", o.Discount.ToString());
		dictionary.Add("Durational", o.Durational.ToString());
		if (o.SaleResetTime != null)
		{
			dictionary.Add("SaleResetTime", o.SaleResetTime);
		}
		if (o.ProgressResetTime != null)
		{
			dictionary.Add("ProgressResetTime", o.ProgressResetTime);
		}
		dictionary.Add("IsUTCBeginTime", o.IsUTCBeginTime.ToString());
		dictionary.Add("IsUTCEndTime", o.IsUTCEndTime.ToString());
		dictionary.Add("IsUTCProgressResetTime", o.IsUTCProgressResetTime.ToString());
		if (o.LastDiscountedTime != null)
		{
			dictionary.Add("LastDiscountedTime", o.LastDiscountedTime);
		}
		dictionary.Add("RemainNumberOfDiscount", o.RemainNumberOfDiscount.ToString());
		return dictionary;
	}
}
