using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_MissionCategoryData
{
	public static MissionCategoryData Parse(string s)
	{
		return Parse(s, new MissionCategoryData());
	}

	public static MissionCategoryData Parse(string s, MissionCategoryData o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static MissionCategoryData Parse(IDictionary d)
	{
		return Parse(d, new MissionCategoryData());
	}

	public static MissionCategoryData Parse(IDictionary d, MissionCategoryData o)
	{
		o.MainCategoryIndex = BaseJsonMarshaler.ParseValueInt(d, "MainCategoryIndex");
		o.SubCategoryIndex = BaseJsonMarshaler.ParseValueInt(d, "SubCategoryIndex");
		o.MissionType = BaseJsonMarshaler.ParseEnum<EMissionType>(d, "MissionType");
		o.MainCategoryLocalKey = BaseJsonMarshaler.ParseValueString(d, "MainCategoryLocalKey");
		o.SubCategoryLocalKey = BaseJsonMarshaler.ParseValueString(d, "SubCategoryLocalKey");
		o.RewardQuestType = BaseJsonMarshaler.ParseValueString(d, "RewardQuestType");
		o.RewardQuestIndex = BaseJsonMarshaler.ParseValueInt(d, "RewardQuestIndex");
		o.UIPropName = BaseJsonMarshaler.ParseValueString(d, "UIPropName");
		o.SubCategoryButtonPath = BaseJsonMarshaler.ParseValueString(d, "SubCategoryButtonPath");
		o.UIBackgroundPath = BaseJsonMarshaler.ParseValueString(d, "UIBackgroundPath");
		o.OpenConditionType = BaseJsonMarshaler.ParseValueString(d, "OpenConditionType");
		o.OpenConditionValue = BaseJsonMarshaler.ParseArrayCast(d, "OpenConditionValue", BaseJsonMarshaler.CastToString);
		o.OpenConditionLocalKey = BaseJsonMarshaler.ParseValueString(d, "OpenConditionLocalKey");
		o.MissionComponentValue = BaseJsonMarshaler.ParseValueInt(d, "MissionComponentValue");
		o.ButtonPrefabName = BaseJsonMarshaler.ParseValueString(d, "ButtonPrefabName");
		o.SubCategoryDescLocalKey = BaseJsonMarshaler.ParseValueString(d, "SubCategoryDescLocalKey");
		o.IconLocalKey = BaseJsonMarshaler.ParseValueString(d, "IconLocalKey");
		o.IsReservePopup = BaseJsonMarshaler.ParseValueBool(d, "IsReservePopup");
		o.CtrAdid = BaseJsonMarshaler.ParseValueString(d, "CtrAdid");
		o.PopupTitleLocalKey = BaseJsonMarshaler.ParseValueString(d, "PopupTitleLocalKey");
		o.PopupDescLocalKey = BaseJsonMarshaler.ParseValueString(d, "PopupDescLocalKey");
		o.PopupImageAssetPath = BaseJsonMarshaler.ParseValueString(d, "PopupImageAssetPath");
		o.TopViewLayoutType = BaseJsonMarshaler.ParseEnum<EMissionTopViewLayout>(d, "TopViewLayoutType");
		o.TopViewButtonGroupType = BaseJsonMarshaler.ParseEnum<EMissionTopViewButtonGroup>(d, "TopViewButtonGroupType");
		o.TopViewRewardType = BaseJsonMarshaler.ParseEnum<EMissionTopViewRewardType>(d, "TopViewRewardType");
		return o;
	}

	public static string EncodeJsonString(MissionCategoryData o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(MissionCategoryData o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("MainCategoryIndex", o.MainCategoryIndex.ToString());
		dictionary.Add("SubCategoryIndex", o.SubCategoryIndex.ToString());
		dictionary.Add("MissionType", o.MissionType.ToString());
		if (o.MainCategoryLocalKey != null)
		{
			dictionary.Add("MainCategoryLocalKey", o.MainCategoryLocalKey);
		}
		if (o.SubCategoryLocalKey != null)
		{
			dictionary.Add("SubCategoryLocalKey", o.SubCategoryLocalKey);
		}
		if (o.RewardQuestType != null)
		{
			dictionary.Add("RewardQuestType", o.RewardQuestType);
		}
		dictionary.Add("RewardQuestIndex", o.RewardQuestIndex.ToString());
		if (o.UIPropName != null)
		{
			dictionary.Add("UIPropName", o.UIPropName);
		}
		if (o.SubCategoryButtonPath != null)
		{
			dictionary.Add("SubCategoryButtonPath", o.SubCategoryButtonPath);
		}
		if (o.UIBackgroundPath != null)
		{
			dictionary.Add("UIBackgroundPath", o.UIBackgroundPath);
		}
		if (o.OpenConditionType != null)
		{
			dictionary.Add("OpenConditionType", o.OpenConditionType);
		}
		dictionary.Add("OpenConditionValue", BaseJsonMarshaler.EncodeArray(o.OpenConditionValue));
		if (o.OpenConditionLocalKey != null)
		{
			dictionary.Add("OpenConditionLocalKey", o.OpenConditionLocalKey);
		}
		dictionary.Add("MissionComponentValue", o.MissionComponentValue.ToString());
		if (o.ButtonPrefabName != null)
		{
			dictionary.Add("ButtonPrefabName", o.ButtonPrefabName);
		}
		if (o.SubCategoryDescLocalKey != null)
		{
			dictionary.Add("SubCategoryDescLocalKey", o.SubCategoryDescLocalKey);
		}
		if (o.IconLocalKey != null)
		{
			dictionary.Add("IconLocalKey", o.IconLocalKey);
		}
		dictionary.Add("IsReservePopup", o.IsReservePopup.ToString());
		if (o.CtrAdid != null)
		{
			dictionary.Add("CtrAdid", o.CtrAdid);
		}
		if (o.PopupTitleLocalKey != null)
		{
			dictionary.Add("PopupTitleLocalKey", o.PopupTitleLocalKey);
		}
		if (o.PopupDescLocalKey != null)
		{
			dictionary.Add("PopupDescLocalKey", o.PopupDescLocalKey);
		}
		if (o.PopupImageAssetPath != null)
		{
			dictionary.Add("PopupImageAssetPath", o.PopupImageAssetPath);
		}
		dictionary.Add("TopViewLayoutType", o.TopViewLayoutType.ToString());
		dictionary.Add("TopViewButtonGroupType", o.TopViewButtonGroupType.ToString());
		dictionary.Add("TopViewRewardType", o.TopViewRewardType.ToString());
		return dictionary;
	}
}
