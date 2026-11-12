using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_NewPayShopProductInfo
{
	public static NewPayShopProductInfo Parse(string s)
	{
		return Parse(s, new NewPayShopProductInfo());
	}

	public static NewPayShopProductInfo Parse(string s, NewPayShopProductInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NewPayShopProductInfo Parse(IDictionary d)
	{
		return Parse(d, new NewPayShopProductInfo());
	}

	public static NewPayShopProductInfo Parse(IDictionary d, NewPayShopProductInfo o)
	{
		o.Index = BaseJsonMarshaler.ParseValueInt(d, "Index");
		o.Category = BaseJsonMarshaler.ParseValueString(d, "Category");
		o.ProductType = BaseJsonMarshaler.ParseEnum<ProductType>(d, "ProductType");
		o.ProductName = BaseJsonMarshaler.ParseValueString(d, "ProductName");
		o.ProductDesc = BaseJsonMarshaler.ParseValueString(d, "ProductDesc");
		o.ProductDetailDesc = BaseJsonMarshaler.ParseValueString(d, "ProductDetailDesc");
		o.AppleId = BaseJsonMarshaler.ParseValueString(d, "AppleId");
		o.GoogleId = BaseJsonMarshaler.ParseValueString(d, "GoogleId");
		o.OneStoreId = BaseJsonMarshaler.ParseValueString(d, "OneStoreId");
		o.UDPStoreId = BaseJsonMarshaler.ParseValueString(d, "UDPStoreId");
		o.PriceType = BaseJsonMarshaler.ParseEnum<CurrencyType>(d, "PriceType");
		o.Price = BaseJsonMarshaler.ParseValueLong(d, "Price");
		o.KRW = BaseJsonMarshaler.ParseValueString(d, "KRW");
		o.USD = BaseJsonMarshaler.ParseValueString(d, "USD");
		o.THB = BaseJsonMarshaler.ParseValueString(d, "THB");
		o.VND = BaseJsonMarshaler.ParseValueString(d, "VND");
		o.HeroItemIndex = BaseJsonMarshaler.ParseValueInt(d, "HeroItemIndex");
		o.UseItemCode = BaseJsonMarshaler.ParseArrayCast(d, "UseItemCode", BaseJsonMarshaler.CastToString);
		o.Count = BaseJsonMarshaler.ParseArrayCast(d, "Count", BaseJsonMarshaler.CastToInt);
		o.Gem = BaseJsonMarshaler.ParseValueInt(d, "Gem");
		o.FirstGem = BaseJsonMarshaler.ParseValueInt(d, "FirstGem");
		o.BonusGem = BaseJsonMarshaler.ParseValueInt(d, "BonusGem");
		o.IsPayBack = BaseJsonMarshaler.ParseValueBool(d, "IsPayBack");
		o.Gold = BaseJsonMarshaler.ParseValueInt(d, "Gold");
		o.Mileage = BaseJsonMarshaler.ParseValueInt(d, "Mileage");
		o.LuaPoint = BaseJsonMarshaler.ParseValueInt(d, "LuaPoint");
		o.ShopEventPoint = BaseJsonMarshaler.ParseValueInt(d, "ShopEventPoint");
		o.LimitedShopEventPoint = BaseJsonMarshaler.ParseValueInt(d, "LimitedShopEventPoint");
		o.ShowIndex = BaseJsonMarshaler.ParseValueInt(d, "ShowIndex");
		o.CardTitle = BaseJsonMarshaler.ParseValueString(d, "CardTitle");
		o.IsNew = BaseJsonMarshaler.ParseValueByte(d, "IsNew");
		o.WithdrawTextKey = BaseJsonMarshaler.ParseValueString(d, "WithdrawTextKey");
		o.IconPath = BaseJsonMarshaler.ParseValueString(d, "IconPath");
		o.LongIconPath = BaseJsonMarshaler.ParseValueString(d, "LongIconPath");
		o.CardType = BaseJsonMarshaler.ParseValueInt(d, "CardType");
		o.Flag = BaseJsonMarshaler.ParseEnum<PayShopFlagType>(d, "Flag");
		o.FirstPurchase = BaseJsonMarshaler.ParseValueByte(d, "FirstPurchase");
		o.ShowProbability = BaseJsonMarshaler.ParseValueBool(d, "ShowProbability");
		o.ProbabilityIndices = BaseJsonMarshaler.ParseArrayCast(d, "ProbabilityIndices", BaseJsonMarshaler.CastToInt);
		o.Durational = BaseJsonMarshaler.ParseValueInt(d, "Durational");
		o.BeginTime = BaseJsonMarshaler.ParseValueString(d, "BeginTime");
		o.EndTime = BaseJsonMarshaler.ParseValueString(d, "EndTime");
		o.DailyGem = BaseJsonMarshaler.ParseValueInt(d, "DailyGem");
		o.DurationDay = BaseJsonMarshaler.ParseValueInt(d, "DurationDay");
		o.ResetMidnight = BaseJsonMarshaler.ParseValueBool(d, "ResetMidnight");
		o.PurchasableCount = BaseJsonMarshaler.ParseValueInt(d, "PurchasableCount");
		o.ResetType = BaseJsonMarshaler.ParseEnum<ResetType>(d, "ResetType");
		o.DailyReset = BaseJsonMarshaler.ParseValueBool(d, "DailyReset");
		o.WeeklyReset = BaseJsonMarshaler.ParseArrayCast(d, "WeeklyReset", BaseJsonMarshaler.CastToInt);
		o.MonthlyResetDate = BaseJsonMarshaler.ParseArrayCast(d, "MonthlyResetDate", BaseJsonMarshaler.CastToInt);
		o.PrePurchased = BaseJsonMarshaler.ParseValueInt(d, "PrePurchased");
		o.StepProductGroup = BaseJsonMarshaler.ParseArrayCast(d, "StepProductGroup", BaseJsonMarshaler.CastToInt);
		o.ExceptServer = BaseJsonMarshaler.ParseArrayCast(d, "ExceptServer", BaseJsonMarshaler.CastToString);
		o.BuyGroup = BaseJsonMarshaler.ParseValueInt(d, "BuyGroup");
		o.SelectAttendanceIndices = BaseJsonMarshaler.ParseArrayCast(d, "SelectAttendanceIndices", BaseJsonMarshaler.CastToInt);
		o.BuyableDay = BaseJsonMarshaler.ParseValueInt(d, "BuyableDay");
		o.IsPurchaseMarketing = BaseJsonMarshaler.ParseValueBool(d, "IsPurchaseMarketing");
		o.BonusProductInfos = BaseJsonMarshaler.ParseArrayCast(d, "BonusProductInfos", BaseJsonMarshaler.CastToString);
		o.ItemInfos = BaseJsonMarshaler.ParseArray(d, "ItemInfos", JM_NShared_PayShopProductItemInfo.Parse);
		o.ShowGMTool = BaseJsonMarshaler.ParseValueBool(d, "ShowGMTool");
		o.OpenDate = BaseJsonMarshaler.ParseValueInt(d, "OpenDate");
		o.SmartRating = BaseJsonMarshaler.ParseValueInt(d, "SmartRating");
		return o;
	}

	public static string EncodeJsonString(NewPayShopProductInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NewPayShopProductInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Index", o.Index.ToString());
		if (o.Category != null)
		{
			dictionary.Add("Category", o.Category);
		}
		dictionary.Add("ProductType", o.ProductType.ToString());
		if (o.ProductName != null)
		{
			dictionary.Add("ProductName", o.ProductName);
		}
		if (o.ProductDesc != null)
		{
			dictionary.Add("ProductDesc", o.ProductDesc);
		}
		if (o.ProductDetailDesc != null)
		{
			dictionary.Add("ProductDetailDesc", o.ProductDetailDesc);
		}
		if (o.AppleId != null)
		{
			dictionary.Add("AppleId", o.AppleId);
		}
		if (o.GoogleId != null)
		{
			dictionary.Add("GoogleId", o.GoogleId);
		}
		if (o.OneStoreId != null)
		{
			dictionary.Add("OneStoreId", o.OneStoreId);
		}
		if (o.UDPStoreId != null)
		{
			dictionary.Add("UDPStoreId", o.UDPStoreId);
		}
		dictionary.Add("PriceType", o.PriceType.ToString());
		dictionary.Add("Price", o.Price.ToString());
		if (o.KRW != null)
		{
			dictionary.Add("KRW", o.KRW);
		}
		if (o.USD != null)
		{
			dictionary.Add("USD", o.USD);
		}
		if (o.THB != null)
		{
			dictionary.Add("THB", o.THB);
		}
		if (o.VND != null)
		{
			dictionary.Add("VND", o.VND);
		}
		dictionary.Add("HeroItemIndex", o.HeroItemIndex.ToString());
		dictionary.Add("UseItemCode", BaseJsonMarshaler.EncodeArray(o.UseItemCode));
		dictionary.Add("Count", BaseJsonMarshaler.EncodeArray(o.Count));
		dictionary.Add("Gem", o.Gem.ToString());
		dictionary.Add("FirstGem", o.FirstGem.ToString());
		dictionary.Add("BonusGem", o.BonusGem.ToString());
		dictionary.Add("IsPayBack", o.IsPayBack.ToString());
		dictionary.Add("Gold", o.Gold.ToString());
		dictionary.Add("Mileage", o.Mileage.ToString());
		dictionary.Add("LuaPoint", o.LuaPoint.ToString());
		dictionary.Add("ShopEventPoint", o.ShopEventPoint.ToString());
		dictionary.Add("LimitedShopEventPoint", o.LimitedShopEventPoint.ToString());
		dictionary.Add("ShowIndex", o.ShowIndex.ToString());
		if (o.CardTitle != null)
		{
			dictionary.Add("CardTitle", o.CardTitle);
		}
		dictionary.Add("IsNew", o.IsNew.ToString());
		if (o.WithdrawTextKey != null)
		{
			dictionary.Add("WithdrawTextKey", o.WithdrawTextKey);
		}
		if (o.IconPath != null)
		{
			dictionary.Add("IconPath", o.IconPath);
		}
		if (o.LongIconPath != null)
		{
			dictionary.Add("LongIconPath", o.LongIconPath);
		}
		dictionary.Add("CardType", o.CardType.ToString());
		dictionary.Add("Flag", o.Flag.ToString());
		dictionary.Add("FirstPurchase", o.FirstPurchase.ToString());
		dictionary.Add("ShowProbability", o.ShowProbability.ToString());
		dictionary.Add("ProbabilityIndices", BaseJsonMarshaler.EncodeArray(o.ProbabilityIndices));
		dictionary.Add("Durational", o.Durational.ToString());
		if (o.BeginTime != null)
		{
			dictionary.Add("BeginTime", o.BeginTime);
		}
		if (o.EndTime != null)
		{
			dictionary.Add("EndTime", o.EndTime);
		}
		dictionary.Add("DailyGem", o.DailyGem.ToString());
		dictionary.Add("DurationDay", o.DurationDay.ToString());
		dictionary.Add("ResetMidnight", o.ResetMidnight.ToString());
		dictionary.Add("PurchasableCount", o.PurchasableCount.ToString());
		dictionary.Add("ResetType", o.ResetType.ToString());
		dictionary.Add("DailyReset", o.DailyReset.ToString());
		dictionary.Add("WeeklyReset", BaseJsonMarshaler.EncodeArray(o.WeeklyReset));
		dictionary.Add("MonthlyResetDate", BaseJsonMarshaler.EncodeArray(o.MonthlyResetDate));
		dictionary.Add("PrePurchased", o.PrePurchased.ToString());
		dictionary.Add("StepProductGroup", BaseJsonMarshaler.EncodeArray(o.StepProductGroup));
		dictionary.Add("ExceptServer", BaseJsonMarshaler.EncodeArray(o.ExceptServer));
		dictionary.Add("BuyGroup", o.BuyGroup.ToString());
		dictionary.Add("SelectAttendanceIndices", BaseJsonMarshaler.EncodeArray(o.SelectAttendanceIndices));
		dictionary.Add("BuyableDay", o.BuyableDay.ToString());
		dictionary.Add("IsPurchaseMarketing", o.IsPurchaseMarketing.ToString());
		dictionary.Add("BonusProductInfos", BaseJsonMarshaler.EncodeArray(o.BonusProductInfos));
		dictionary.Add("ItemInfos", BaseJsonMarshaler.EncodeArray(o.ItemInfos, JM_NShared_PayShopProductItemInfo.EncodeJsonObject));
		dictionary.Add("ShowGMTool", o.ShowGMTool.ToString());
		dictionary.Add("OpenDate", o.OpenDate.ToString());
		dictionary.Add("SmartRating", o.SmartRating.ToString());
		return dictionary;
	}
}
