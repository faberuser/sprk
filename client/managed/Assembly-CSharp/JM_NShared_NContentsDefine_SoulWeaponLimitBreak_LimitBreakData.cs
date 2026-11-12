using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NContentsDefine;

public static class JM_NShared_NContentsDefine_SoulWeaponLimitBreak_LimitBreakData
{
	public static SoulWeaponLimitBreak.LimitBreakData Parse(string s)
	{
		return Parse(s, new SoulWeaponLimitBreak.LimitBreakData());
	}

	public static SoulWeaponLimitBreak.LimitBreakData Parse(string s, SoulWeaponLimitBreak.LimitBreakData o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static SoulWeaponLimitBreak.LimitBreakData Parse(IDictionary d)
	{
		return Parse(d, new SoulWeaponLimitBreak.LimitBreakData());
	}

	public static SoulWeaponLimitBreak.LimitBreakData Parse(IDictionary d, SoulWeaponLimitBreak.LimitBreakData o)
	{
		o.DetailIndex = BaseJsonMarshaler.ParseValueInt(d, "DetailIndex");
		o.Star = BaseJsonMarshaler.ParseValueInt(d, "Star");
		o.SuccessRatio = BaseJsonMarshaler.ParseValueInt(d, "SuccessRatio");
		o.FailBonusRatio = BaseJsonMarshaler.ParseValueInt(d, "FailBonusRatio");
		o.GoldCount = BaseJsonMarshaler.ParseValueInt(d, "GoldCount");
		o.WeaponUniqueCount = BaseJsonMarshaler.ParseValueInt(d, "WeaponUniqueCount");
		o.SoulStoneCount = BaseJsonMarshaler.ParseValueInt(d, "SoulStoneCount");
		o.TransStoneIndex = BaseJsonMarshaler.ParseValueInt(d, "TransStoneIndex");
		o.TransStoneCount = BaseJsonMarshaler.ParseValueInt(d, "TransStoneCount");
		return o;
	}

	public static string EncodeJsonString(SoulWeaponLimitBreak.LimitBreakData o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(SoulWeaponLimitBreak.LimitBreakData o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("DetailIndex", o.DetailIndex.ToString());
		dictionary.Add("Star", o.Star.ToString());
		dictionary.Add("SuccessRatio", o.SuccessRatio.ToString());
		dictionary.Add("FailBonusRatio", o.FailBonusRatio.ToString());
		dictionary.Add("GoldCount", o.GoldCount.ToString());
		dictionary.Add("WeaponUniqueCount", o.WeaponUniqueCount.ToString());
		dictionary.Add("SoulStoneCount", o.SoulStoneCount.ToString());
		dictionary.Add("TransStoneIndex", o.TransStoneIndex.ToString());
		dictionary.Add("TransStoneCount", o.TransStoneCount.ToString());
		return dictionary;
	}
}
