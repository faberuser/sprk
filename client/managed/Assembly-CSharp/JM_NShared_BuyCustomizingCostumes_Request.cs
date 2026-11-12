using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.BuyCustomizingCostumes;

public static class JM_NShared_BuyCustomizingCostumes_Request
{
	public static NShared.BuyCustomizingCostumes.Request Parse(string s)
	{
		return Parse(s, new NShared.BuyCustomizingCostumes.Request());
	}

	public static NShared.BuyCustomizingCostumes.Request Parse(string s, NShared.BuyCustomizingCostumes.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.BuyCustomizingCostumes.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.BuyCustomizingCostumes.Request());
	}

	public static NShared.BuyCustomizingCostumes.Request Parse(IDictionary d, NShared.BuyCustomizingCostumes.Request o)
	{
		o.HeroIndex = BaseJsonMarshaler.ParseValueInt(d, "HeroIndex");
		o.AccessoryCostumePositionInfo = BaseJsonMarshaler.ParseValueString(d, "AccessoryCostumePositionInfo");
		o.UnsetAccessoryCostumeIndices = BaseJsonMarshaler.ParseArrayCast(d, "UnsetAccessoryCostumeIndices", BaseJsonMarshaler.CastToInt);
		o.CostumeIndex = BaseJsonMarshaler.ParseValueInt(d, "CostumeIndex");
		o.WeaponCostumeIndex = BaseJsonMarshaler.ParseValueInt(d, "WeaponCostumeIndex");
		o.HideUniqueWeapon = BaseJsonMarshaler.ParseValueByte(d, "HideUniqueWeapon");
		o.HairCostumeIndex = BaseJsonMarshaler.ParseValueInt(d, "HairCostumeIndex");
		o.BuyGem = BaseJsonMarshaler.ParseValueInt(d, "BuyGem");
		o.BuyGold = BaseJsonMarshaler.ParseValueInt(d, "BuyGold");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.BuyCustomizingCostumes.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.BuyCustomizingCostumes.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("HeroIndex", o.HeroIndex.ToString());
		if (o.AccessoryCostumePositionInfo != null)
		{
			dictionary.Add("AccessoryCostumePositionInfo", o.AccessoryCostumePositionInfo);
		}
		dictionary.Add("UnsetAccessoryCostumeIndices", BaseJsonMarshaler.EncodeArray(o.UnsetAccessoryCostumeIndices));
		dictionary.Add("CostumeIndex", o.CostumeIndex.ToString());
		dictionary.Add("WeaponCostumeIndex", o.WeaponCostumeIndex.ToString());
		dictionary.Add("HideUniqueWeapon", o.HideUniqueWeapon.ToString());
		dictionary.Add("HairCostumeIndex", o.HairCostumeIndex.ToString());
		dictionary.Add("BuyGem", o.BuyGem.ToString());
		dictionary.Add("BuyGold", o.BuyGold.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
