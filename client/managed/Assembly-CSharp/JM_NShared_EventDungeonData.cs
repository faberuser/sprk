using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_EventDungeonData
{
	public static EventDungeonData Parse(string s)
	{
		return Parse(s, new EventDungeonData());
	}

	public static EventDungeonData Parse(string s, EventDungeonData o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static EventDungeonData Parse(IDictionary d)
	{
		return Parse(d, new EventDungeonData());
	}

	public static EventDungeonData Parse(IDictionary d, EventDungeonData o)
	{
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.DungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "DungeonIndex");
		o.ShowIndex = BaseJsonMarshaler.ParseValueInt(d, "ShowIndex");
		o.DecorationSelectorName = BaseJsonMarshaler.ParseValueString(d, "DecorationSelectorName");
		o.PrefabName = BaseJsonMarshaler.ParseValueString(d, "PrefabName");
		o.PropTypes = BaseJsonMarshaler.ParseArray<EventDungeonPropType>(d, "PropTypes");
		o.CloseType = BaseJsonMarshaler.ParseEnum<EEventDungeonCloseType>(d, "CloseType");
		o.OpenType1 = BaseJsonMarshaler.ParseValueString(d, "OpenType1");
		o.OpenTypeValues1 = BaseJsonMarshaler.ParseArrayCast(d, "OpenTypeValues1", BaseJsonMarshaler.CastToString);
		o.OpenType2 = BaseJsonMarshaler.ParseValueString(d, "OpenType2");
		o.OpenTypeValues2 = BaseJsonMarshaler.ParseArrayCast(d, "OpenTypeValues2", BaseJsonMarshaler.CastToString);
		o.EventDungeonPointRate_Easy = BaseJsonMarshaler.ParseValueInt(d, "EventDungeonPointRate_Easy");
		o.EventDungeonPointAmount_Easy = BaseJsonMarshaler.ParseValueInt(d, "EventDungeonPointAmount_Easy");
		o.EventDungeonPointRate_Normal = BaseJsonMarshaler.ParseValueInt(d, "EventDungeonPointRate_Normal");
		o.EventDungeonPointAmount_Normal = BaseJsonMarshaler.ParseValueInt(d, "EventDungeonPointAmount_Normal");
		o.EventDungeonPointRate_Hard = BaseJsonMarshaler.ParseValueInt(d, "EventDungeonPointRate_Hard");
		o.EventDungeonPointAmount_Hard = BaseJsonMarshaler.ParseValueInt(d, "EventDungeonPointAmount_Hard");
		o.EventDungeonPointRate_Hell = BaseJsonMarshaler.ParseValueInt(d, "EventDungeonPointRate_Hell");
		o.EventDungeonPointAmount_Hell = BaseJsonMarshaler.ParseValueInt(d, "EventDungeonPointAmount_Hell");
		o.EventDungeonPoint2Rate_Easy = BaseJsonMarshaler.ParseValueInt(d, "EventDungeonPoint2Rate_Easy");
		o.EventDungeonPoint2Amount_Easy = BaseJsonMarshaler.ParseValueInt(d, "EventDungeonPoint2Amount_Easy");
		o.EventDungeonPoint2Rate_Normal = BaseJsonMarshaler.ParseValueInt(d, "EventDungeonPoint2Rate_Normal");
		o.EventDungeonPoint2Amount_Normal = BaseJsonMarshaler.ParseValueInt(d, "EventDungeonPoint2Amount_Normal");
		o.EventDungeonPoint2Rate_Hard = BaseJsonMarshaler.ParseValueInt(d, "EventDungeonPoint2Rate_Hard");
		o.EventDungeonPoint2Amount_Hard = BaseJsonMarshaler.ParseValueInt(d, "EventDungeonPoint2Amount_Hard");
		o.EventDungeonPoint2Rate_Hell = BaseJsonMarshaler.ParseValueInt(d, "EventDungeonPoint2Rate_Hell");
		o.EventDungeonPoint2Amount_Hell = BaseJsonMarshaler.ParseValueInt(d, "EventDungeonPoint2Amount_Hell");
		o.PortraitPath = BaseJsonMarshaler.ParseValueString(d, "PortraitPath");
		o.DungeonName = BaseJsonMarshaler.ParseValueString(d, "DungeonName");
		o.ResetAmount = BaseJsonMarshaler.ParseArrayCast(d, "ResetAmount", BaseJsonMarshaler.CastToInt);
		o.MaxResetCount = BaseJsonMarshaler.ParseValueInt(d, "MaxResetCount");
		o.ResetCurrencyType = BaseJsonMarshaler.ParseValueString(d, "ResetCurrencyType");
		o.HelpTexts = BaseJsonMarshaler.ParseArrayCast(d, "HelpTexts", BaseJsonMarshaler.CastToString);
		o.OpenedDungeonDesc = BaseJsonMarshaler.ParseValueString(d, "OpenedDungeonDesc");
		o.ClosedDungeonDesc = BaseJsonMarshaler.ParseValueString(d, "ClosedDungeonDesc");
		o.BanIndex = BaseJsonMarshaler.ParseValueInt(d, "BanIndex");
		o.RewardIndex = BaseJsonMarshaler.ParseValueInt(d, "RewardIndex");
		o.DungeonCardPrefix = BaseJsonMarshaler.ParseValueString(d, "DungeonCardPrefix");
		o.DungeonCardName = BaseJsonMarshaler.ParseValueString(d, "DungeonCardName");
		return o;
	}

	public static string EncodeJsonString(EventDungeonData o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(EventDungeonData o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ChapterIndex", o.ChapterIndex.ToString());
		dictionary.Add("DungeonIndex", o.DungeonIndex.ToString());
		dictionary.Add("ShowIndex", o.ShowIndex.ToString());
		if (o.DecorationSelectorName != null)
		{
			dictionary.Add("DecorationSelectorName", o.DecorationSelectorName);
		}
		if (o.PrefabName != null)
		{
			dictionary.Add("PrefabName", o.PrefabName);
		}
		dictionary.Add("PropTypes", BaseJsonMarshaler.EncodeArray(o.PropTypes));
		dictionary.Add("CloseType", o.CloseType.ToString());
		if (o.OpenType1 != null)
		{
			dictionary.Add("OpenType1", o.OpenType1);
		}
		dictionary.Add("OpenTypeValues1", BaseJsonMarshaler.EncodeArray(o.OpenTypeValues1));
		if (o.OpenType2 != null)
		{
			dictionary.Add("OpenType2", o.OpenType2);
		}
		dictionary.Add("OpenTypeValues2", BaseJsonMarshaler.EncodeArray(o.OpenTypeValues2));
		dictionary.Add("EventDungeonPointRate_Easy", o.EventDungeonPointRate_Easy.ToString());
		dictionary.Add("EventDungeonPointAmount_Easy", o.EventDungeonPointAmount_Easy.ToString());
		dictionary.Add("EventDungeonPointRate_Normal", o.EventDungeonPointRate_Normal.ToString());
		dictionary.Add("EventDungeonPointAmount_Normal", o.EventDungeonPointAmount_Normal.ToString());
		dictionary.Add("EventDungeonPointRate_Hard", o.EventDungeonPointRate_Hard.ToString());
		dictionary.Add("EventDungeonPointAmount_Hard", o.EventDungeonPointAmount_Hard.ToString());
		dictionary.Add("EventDungeonPointRate_Hell", o.EventDungeonPointRate_Hell.ToString());
		dictionary.Add("EventDungeonPointAmount_Hell", o.EventDungeonPointAmount_Hell.ToString());
		dictionary.Add("EventDungeonPoint2Rate_Easy", o.EventDungeonPoint2Rate_Easy.ToString());
		dictionary.Add("EventDungeonPoint2Amount_Easy", o.EventDungeonPoint2Amount_Easy.ToString());
		dictionary.Add("EventDungeonPoint2Rate_Normal", o.EventDungeonPoint2Rate_Normal.ToString());
		dictionary.Add("EventDungeonPoint2Amount_Normal", o.EventDungeonPoint2Amount_Normal.ToString());
		dictionary.Add("EventDungeonPoint2Rate_Hard", o.EventDungeonPoint2Rate_Hard.ToString());
		dictionary.Add("EventDungeonPoint2Amount_Hard", o.EventDungeonPoint2Amount_Hard.ToString());
		dictionary.Add("EventDungeonPoint2Rate_Hell", o.EventDungeonPoint2Rate_Hell.ToString());
		dictionary.Add("EventDungeonPoint2Amount_Hell", o.EventDungeonPoint2Amount_Hell.ToString());
		if (o.PortraitPath != null)
		{
			dictionary.Add("PortraitPath", o.PortraitPath);
		}
		if (o.DungeonName != null)
		{
			dictionary.Add("DungeonName", o.DungeonName);
		}
		dictionary.Add("ResetAmount", BaseJsonMarshaler.EncodeArray(o.ResetAmount));
		dictionary.Add("MaxResetCount", o.MaxResetCount.ToString());
		if (o.ResetCurrencyType != null)
		{
			dictionary.Add("ResetCurrencyType", o.ResetCurrencyType);
		}
		dictionary.Add("HelpTexts", BaseJsonMarshaler.EncodeArray(o.HelpTexts));
		if (o.OpenedDungeonDesc != null)
		{
			dictionary.Add("OpenedDungeonDesc", o.OpenedDungeonDesc);
		}
		if (o.ClosedDungeonDesc != null)
		{
			dictionary.Add("ClosedDungeonDesc", o.ClosedDungeonDesc);
		}
		dictionary.Add("BanIndex", o.BanIndex.ToString());
		dictionary.Add("RewardIndex", o.RewardIndex.ToString());
		if (o.DungeonCardPrefix != null)
		{
			dictionary.Add("DungeonCardPrefix", o.DungeonCardPrefix);
		}
		if (o.DungeonCardName != null)
		{
			dictionary.Add("DungeonCardName", o.DungeonCardName);
		}
		return dictionary;
	}
}
