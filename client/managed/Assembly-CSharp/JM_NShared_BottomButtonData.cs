using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_BottomButtonData
{
	public static BottomButtonData Parse(string s)
	{
		return Parse(s, new BottomButtonData());
	}

	public static BottomButtonData Parse(string s, BottomButtonData o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static BottomButtonData Parse(IDictionary d)
	{
		return Parse(d, new BottomButtonData());
	}

	public static BottomButtonData Parse(IDictionary d, BottomButtonData o)
	{
		o.Type = BaseJsonMarshaler.ParseEnum<ERightMenuButton>(d, "Type");
		o.SubType = BaseJsonMarshaler.ParseValueString(d, "SubType");
		o.TutorialIndex = BaseJsonMarshaler.ParseValueInt(d, "TutorialIndex");
		o.LabelKey = BaseJsonMarshaler.ParseValueString(d, "LabelKey");
		o.BgScale = BaseJsonMarshaler.ParseValueFloat(d, "BgScale");
		o.ContentsOpenCode = BaseJsonMarshaler.ParseValueString(d, "ContentsOpenCode");
		o.ForceOpenDungeon = BaseJsonMarshaler.ParseArrayCast(d, "ForceOpenDungeon", BaseJsonMarshaler.CastToString);
		return o;
	}

	public static string EncodeJsonString(BottomButtonData o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(BottomButtonData o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Type", o.Type.ToString());
		if (o.SubType != null)
		{
			dictionary.Add("SubType", o.SubType);
		}
		dictionary.Add("TutorialIndex", o.TutorialIndex.ToString());
		if (o.LabelKey != null)
		{
			dictionary.Add("LabelKey", o.LabelKey);
		}
		dictionary.Add("BgScale", o.BgScale.ToString());
		if (o.ContentsOpenCode != null)
		{
			dictionary.Add("ContentsOpenCode", o.ContentsOpenCode);
		}
		dictionary.Add("ForceOpenDungeon", BaseJsonMarshaler.EncodeArray(o.ForceOpenDungeon));
		return dictionary;
	}
}
