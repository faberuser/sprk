using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_HeroAnimationViewData
{
	public static HeroAnimationViewData Parse(string s)
	{
		return Parse(s, new HeroAnimationViewData());
	}

	public static HeroAnimationViewData Parse(string s, HeroAnimationViewData o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static HeroAnimationViewData Parse(IDictionary d)
	{
		return Parse(d, new HeroAnimationViewData());
	}

	public static HeroAnimationViewData Parse(IDictionary d, HeroAnimationViewData o)
	{
		o.Index = BaseJsonMarshaler.ParseValueInt(d, "Index");
		o.HeroIndex = BaseJsonMarshaler.ParseValueInt(d, "HeroIndex");
		o.Animation = BaseJsonMarshaler.ParseValueString(d, "Animation");
		o.AnimationText = BaseJsonMarshaler.ParseValueString(d, "AnimationText");
		o.IconType = BaseJsonMarshaler.ParseValueString(d, "IconType");
		o.VoiceCode = BaseJsonMarshaler.ParseValueString(d, "VoiceCode");
		o.SkipLegendCostume = BaseJsonMarshaler.ParseValueBool(d, "SkipLegendCostume");
		return o;
	}

	public static string EncodeJsonString(HeroAnimationViewData o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(HeroAnimationViewData o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Index", o.Index.ToString());
		dictionary.Add("HeroIndex", o.HeroIndex.ToString());
		if (o.Animation != null)
		{
			dictionary.Add("Animation", o.Animation);
		}
		if (o.AnimationText != null)
		{
			dictionary.Add("AnimationText", o.AnimationText);
		}
		if (o.IconType != null)
		{
			dictionary.Add("IconType", o.IconType);
		}
		if (o.VoiceCode != null)
		{
			dictionary.Add("VoiceCode", o.VoiceCode);
		}
		dictionary.Add("SkipLegendCostume", o.SkipLegendCostume.ToString());
		return dictionary;
	}
}
