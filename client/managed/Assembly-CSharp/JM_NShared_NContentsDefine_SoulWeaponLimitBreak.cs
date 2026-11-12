using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NContentsDefine;

public static class JM_NShared_NContentsDefine_SoulWeaponLimitBreak
{
	public static SoulWeaponLimitBreak Parse(string s)
	{
		return Parse(s, new SoulWeaponLimitBreak());
	}

	public static SoulWeaponLimitBreak Parse(string s, SoulWeaponLimitBreak o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static SoulWeaponLimitBreak Parse(IDictionary d)
	{
		return Parse(d, new SoulWeaponLimitBreak());
	}

	public static SoulWeaponLimitBreak Parse(IDictionary d, SoulWeaponLimitBreak o)
	{
		o.Datas = BaseJsonMarshaler.ParseList<SoulWeaponLimitBreak.LimitBreakData, List<SoulWeaponLimitBreak.LimitBreakData>>(d, "Datas", JM_NShared_NContentsDefine_SoulWeaponLimitBreak_LimitBreakData.Parse);
		return o;
	}

	public static string EncodeJsonString(SoulWeaponLimitBreak o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(SoulWeaponLimitBreak o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Datas", BaseJsonMarshaler.EncodeArray(o.Datas, JM_NShared_NContentsDefine_SoulWeaponLimitBreak_LimitBreakData.EncodeJsonObject));
		return dictionary;
	}
}
