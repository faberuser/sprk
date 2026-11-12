using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NContentsDefine;

public static class JM_NShared_NContentsDefine_ReturnUserJumping_ExtraWidget_in_MissionNodeWindowV3
{
	public static ReturnUserJumping.ExtraWidget_in_MissionNodeWindowV3 Parse(string s)
	{
		return Parse(s, new ReturnUserJumping.ExtraWidget_in_MissionNodeWindowV3());
	}

	public static ReturnUserJumping.ExtraWidget_in_MissionNodeWindowV3 Parse(string s, ReturnUserJumping.ExtraWidget_in_MissionNodeWindowV3 o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static ReturnUserJumping.ExtraWidget_in_MissionNodeWindowV3 Parse(IDictionary d)
	{
		return Parse(d, new ReturnUserJumping.ExtraWidget_in_MissionNodeWindowV3());
	}

	public static ReturnUserJumping.ExtraWidget_in_MissionNodeWindowV3 Parse(IDictionary d, ReturnUserJumping.ExtraWidget_in_MissionNodeWindowV3 o)
	{
		o.MissionCategory = BaseJsonMarshaler.ParseObject(d, "MissionCategory", JM_NShared_NContentsDefine_ReturnUserJumping_MissionCategoryKey.Parse);
		o.Show_Button_ReceiveAll = BaseJsonMarshaler.ParseValueBool(d, "Show_Button_ReceiveAll");
		o.Disable_NotiPopup = BaseJsonMarshaler.ParseValueBool(d, "Disable_NotiPopup");
		o.Linked_PlayerAnyMisc = BaseJsonMarshaler.ParseValueString(d, "Linked_PlayerAnyMisc");
		o.PayShop_Product_Index = BaseJsonMarshaler.ParseValueInt(d, "PayShop_Product_Index");
		o.Prerequisite_MissionCategory = BaseJsonMarshaler.ParseObject(d, "Prerequisite_MissionCategory", JM_NShared_NContentsDefine_ReturnUserJumping_MissionCategoryKey.Parse);
		return o;
	}

	public static string EncodeJsonString(ReturnUserJumping.ExtraWidget_in_MissionNodeWindowV3 o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(ReturnUserJumping.ExtraWidget_in_MissionNodeWindowV3 o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("MissionCategory", JM_NShared_NContentsDefine_ReturnUserJumping_MissionCategoryKey.EncodeJsonObject(o.MissionCategory));
		dictionary.Add("Show_Button_ReceiveAll", o.Show_Button_ReceiveAll.ToString());
		dictionary.Add("Disable_NotiPopup", o.Disable_NotiPopup.ToString());
		if (o.Linked_PlayerAnyMisc != null)
		{
			dictionary.Add("Linked_PlayerAnyMisc", o.Linked_PlayerAnyMisc);
		}
		dictionary.Add("PayShop_Product_Index", o.PayShop_Product_Index.ToString());
		dictionary.Add("Prerequisite_MissionCategory", JM_NShared_NContentsDefine_ReturnUserJumping_MissionCategoryKey.EncodeJsonObject(o.Prerequisite_MissionCategory));
		return dictionary;
	}
}
