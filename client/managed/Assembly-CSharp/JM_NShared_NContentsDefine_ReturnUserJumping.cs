using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NContentsDefine;

public static class JM_NShared_NContentsDefine_ReturnUserJumping
{
	public static ReturnUserJumping Parse(string s)
	{
		return Parse(s, new ReturnUserJumping());
	}

	public static ReturnUserJumping Parse(string s, ReturnUserJumping o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static ReturnUserJumping Parse(IDictionary d)
	{
		return Parse(d, new ReturnUserJumping());
	}

	public static ReturnUserJumping Parse(IDictionary d, ReturnUserJumping o)
	{
		o.Jumping_PayShop_Product_Indices = BaseJsonMarshaler.ParseArrayCast(d, "Jumping_PayShop_Product_Indices", BaseJsonMarshaler.CastToInt);
		o.ForceUnlockMission_in_MissionSubCategoryComponent = BaseJsonMarshaler.ParseList<ReturnUserJumping.MissionCategoryKey, List<ReturnUserJumping.MissionCategoryKey>>(d, "ForceUnlockMission_in_MissionSubCategoryComponent", JM_NShared_NContentsDefine_ReturnUserJumping_MissionCategoryKey.Parse);
		o.Localize_Need_Purchase_PayShop_Product = BaseJsonMarshaler.ParseValueString(d, "Localize_Need_Purchase_PayShop_Product");
		o.Localize_Prerequisite_MissionCategory_Not_Open = BaseJsonMarshaler.ParseValueString(d, "Localize_Prerequisite_MissionCategory_Not_Open");
		o.Localize_Prerequisite_MissionCategory_Not_Open_Popup = BaseJsonMarshaler.ParseValueString(d, "Localize_Prerequisite_MissionCategory_Not_Open_Popup");
		o.ExtraWidgets_in_MissionNodeWindowV3 = BaseJsonMarshaler.ParseList<ReturnUserJumping.ExtraWidget_in_MissionNodeWindowV3, List<ReturnUserJumping.ExtraWidget_in_MissionNodeWindowV3>>(d, "ExtraWidgets_in_MissionNodeWindowV3", JM_NShared_NContentsDefine_ReturnUserJumping_ExtraWidget_in_MissionNodeWindowV3.Parse);
		return o;
	}

	public static string EncodeJsonString(ReturnUserJumping o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(ReturnUserJumping o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Jumping_PayShop_Product_Indices", BaseJsonMarshaler.EncodeArray(o.Jumping_PayShop_Product_Indices));
		dictionary.Add("ForceUnlockMission_in_MissionSubCategoryComponent", BaseJsonMarshaler.EncodeArray(o.ForceUnlockMission_in_MissionSubCategoryComponent, JM_NShared_NContentsDefine_ReturnUserJumping_MissionCategoryKey.EncodeJsonObject));
		if (o.Localize_Need_Purchase_PayShop_Product != null)
		{
			dictionary.Add("Localize_Need_Purchase_PayShop_Product", o.Localize_Need_Purchase_PayShop_Product);
		}
		if (o.Localize_Prerequisite_MissionCategory_Not_Open != null)
		{
			dictionary.Add("Localize_Prerequisite_MissionCategory_Not_Open", o.Localize_Prerequisite_MissionCategory_Not_Open);
		}
		if (o.Localize_Prerequisite_MissionCategory_Not_Open_Popup != null)
		{
			dictionary.Add("Localize_Prerequisite_MissionCategory_Not_Open_Popup", o.Localize_Prerequisite_MissionCategory_Not_Open_Popup);
		}
		dictionary.Add("ExtraWidgets_in_MissionNodeWindowV3", BaseJsonMarshaler.EncodeArray(o.ExtraWidgets_in_MissionNodeWindowV3, JM_NShared_NContentsDefine_ReturnUserJumping_ExtraWidget_in_MissionNodeWindowV3.EncodeJsonObject));
		return dictionary;
	}
}
