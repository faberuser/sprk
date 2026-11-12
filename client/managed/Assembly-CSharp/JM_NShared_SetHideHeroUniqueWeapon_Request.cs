using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.SetHideHeroUniqueWeapon;

public static class JM_NShared_SetHideHeroUniqueWeapon_Request
{
	public static NShared.SetHideHeroUniqueWeapon.Request Parse(string s)
	{
		return Parse(s, new NShared.SetHideHeroUniqueWeapon.Request());
	}

	public static NShared.SetHideHeroUniqueWeapon.Request Parse(string s, NShared.SetHideHeroUniqueWeapon.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.SetHideHeroUniqueWeapon.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.SetHideHeroUniqueWeapon.Request());
	}

	public static NShared.SetHideHeroUniqueWeapon.Request Parse(IDictionary d, NShared.SetHideHeroUniqueWeapon.Request o)
	{
		o.HeroIndex = BaseJsonMarshaler.ParseValueInt(d, "HeroIndex");
		o.HideUniqueWeapon = BaseJsonMarshaler.ParseValueByte(d, "HideUniqueWeapon");
		o.WeaponCpstumeIndex = BaseJsonMarshaler.ParseValueInt(d, "WeaponCpstumeIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.SetHideHeroUniqueWeapon.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.SetHideHeroUniqueWeapon.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("HeroIndex", o.HeroIndex.ToString());
		dictionary.Add("HideUniqueWeapon", o.HideUniqueWeapon.ToString());
		dictionary.Add("WeaponCpstumeIndex", o.WeaponCpstumeIndex.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
