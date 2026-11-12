using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_CachedHeroCostumeInfo
{
	public static CachedHeroCostumeInfo Parse(string s)
	{
		return Parse(s, new CachedHeroCostumeInfo());
	}

	public static CachedHeroCostumeInfo Parse(string s, CachedHeroCostumeInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static CachedHeroCostumeInfo Parse(IDictionary d)
	{
		return Parse(d, new CachedHeroCostumeInfo());
	}

	public static CachedHeroCostumeInfo Parse(IDictionary d, CachedHeroCostumeInfo o)
	{
		o.HeroIndex = BaseJsonMarshaler.ParseValueInt(d, "HeroIndex");
		o.CostumeIndex = BaseJsonMarshaler.ParseValueInt(d, "CostumeIndex");
		o.HideUniqueWeapon = BaseJsonMarshaler.ParseValueByte(d, "HideUniqueWeapon");
		o.WeaponCostumeIndex = BaseJsonMarshaler.ParseValueInt(d, "WeaponCostumeIndex");
		o.HairCostumeIndex = BaseJsonMarshaler.ParseValueInt(d, "HairCostumeIndex");
		o.AccessoryCostumeIndex1 = BaseJsonMarshaler.ParseValueInt(d, "AccessoryCostumeIndex1");
		o.AccessoryCostumeIndex2 = BaseJsonMarshaler.ParseValueInt(d, "AccessoryCostumeIndex2");
		o.AccessoryCostumeIndex3 = BaseJsonMarshaler.ParseValueInt(d, "AccessoryCostumeIndex3");
		o.AccessoryCostumeIndex4 = BaseJsonMarshaler.ParseValueInt(d, "AccessoryCostumeIndex4");
		o.AccessoryCostumeIndex5 = BaseJsonMarshaler.ParseValueInt(d, "AccessoryCostumeIndex5");
		o.AccessoryCostumeIndex6 = BaseJsonMarshaler.ParseValueInt(d, "AccessoryCostumeIndex6");
		o.AccessoryPositionInfo1 = BaseJsonMarshaler.ParseObject(d, "AccessoryPositionInfo1", JM_NShared_PositionInfo.Parse);
		o.AccessoryPositionInfo2 = BaseJsonMarshaler.ParseObject(d, "AccessoryPositionInfo2", JM_NShared_PositionInfo.Parse);
		o.AccessoryPositionInfo3 = BaseJsonMarshaler.ParseObject(d, "AccessoryPositionInfo3", JM_NShared_PositionInfo.Parse);
		o.AccessoryPositionInfo4 = BaseJsonMarshaler.ParseObject(d, "AccessoryPositionInfo4", JM_NShared_PositionInfo.Parse);
		o.AccessoryPositionInfo5 = BaseJsonMarshaler.ParseObject(d, "AccessoryPositionInfo5", JM_NShared_PositionInfo.Parse);
		o.AccessoryPositionInfo6 = BaseJsonMarshaler.ParseObject(d, "AccessoryPositionInfo6", JM_NShared_PositionInfo.Parse);
		SprkAccessories.Read(o, d);
		return o;
	}

	public static string EncodeJsonString(CachedHeroCostumeInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(CachedHeroCostumeInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			SprkAccessories.Write(dictionary, o);
			return dictionary;
		}
		dictionary.Add("HeroIndex", o.HeroIndex.ToString());
		dictionary.Add("CostumeIndex", o.CostumeIndex.ToString());
		dictionary.Add("HideUniqueWeapon", o.HideUniqueWeapon.ToString());
		dictionary.Add("WeaponCostumeIndex", o.WeaponCostumeIndex.ToString());
		dictionary.Add("HairCostumeIndex", o.HairCostumeIndex.ToString());
		dictionary.Add("AccessoryCostumeIndex1", o.AccessoryCostumeIndex1.ToString());
		dictionary.Add("AccessoryCostumeIndex2", o.AccessoryCostumeIndex2.ToString());
		dictionary.Add("AccessoryCostumeIndex3", o.AccessoryCostumeIndex3.ToString());
		dictionary.Add("AccessoryCostumeIndex4", o.AccessoryCostumeIndex4.ToString());
		dictionary.Add("AccessoryCostumeIndex5", o.AccessoryCostumeIndex5.ToString());
		dictionary.Add("AccessoryCostumeIndex6", o.AccessoryCostumeIndex6.ToString());
		dictionary.Add("AccessoryPositionInfo1", JM_NShared_PositionInfo.EncodeJsonObject(o.AccessoryPositionInfo1));
		dictionary.Add("AccessoryPositionInfo2", JM_NShared_PositionInfo.EncodeJsonObject(o.AccessoryPositionInfo2));
		dictionary.Add("AccessoryPositionInfo3", JM_NShared_PositionInfo.EncodeJsonObject(o.AccessoryPositionInfo3));
		dictionary.Add("AccessoryPositionInfo4", JM_NShared_PositionInfo.EncodeJsonObject(o.AccessoryPositionInfo4));
		dictionary.Add("AccessoryPositionInfo5", JM_NShared_PositionInfo.EncodeJsonObject(o.AccessoryPositionInfo5));
		dictionary.Add("AccessoryPositionInfo6", JM_NShared_PositionInfo.EncodeJsonObject(o.AccessoryPositionInfo6));
		SprkAccessories.Write(dictionary, o);
		return dictionary;
	}
}
