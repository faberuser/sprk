using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.UseMultipleHeroSelectItem;

public static class JM_NShared_UseMultipleHeroSelectItem_Request
{
	public static NShared.UseMultipleHeroSelectItem.Request Parse(string s)
	{
		return Parse(s, new NShared.UseMultipleHeroSelectItem.Request());
	}

	public static NShared.UseMultipleHeroSelectItem.Request Parse(string s, NShared.UseMultipleHeroSelectItem.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.UseMultipleHeroSelectItem.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.UseMultipleHeroSelectItem.Request());
	}

	public static NShared.UseMultipleHeroSelectItem.Request Parse(IDictionary d, NShared.UseMultipleHeroSelectItem.Request o)
	{
		o.ItemIndex = BaseJsonMarshaler.ParseValueInt(d, "ItemIndex");
		o.LeftHeroIndices = BaseJsonMarshaler.ParseArrayCast(d, "LeftHeroIndices", BaseJsonMarshaler.CastToInt);
		o.RightHeroIndices = BaseJsonMarshaler.ParseArrayCast(d, "RightHeroIndices", BaseJsonMarshaler.CastToInt);
		o.LeftAllOwnedReward = BaseJsonMarshaler.ParseValueBool(d, "LeftAllOwnedReward");
		o.RightAllOwnedReward = BaseJsonMarshaler.ParseValueBool(d, "RightAllOwnedReward");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.UseMultipleHeroSelectItem.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.UseMultipleHeroSelectItem.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ItemIndex", o.ItemIndex.ToString());
		dictionary.Add("LeftHeroIndices", BaseJsonMarshaler.EncodeArray(o.LeftHeroIndices));
		dictionary.Add("RightHeroIndices", BaseJsonMarshaler.EncodeArray(o.RightHeroIndices));
		dictionary.Add("LeftAllOwnedReward", o.LeftAllOwnedReward.ToString());
		dictionary.Add("RightAllOwnedReward", o.RightAllOwnedReward.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
