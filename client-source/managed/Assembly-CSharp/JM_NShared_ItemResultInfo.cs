using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_ItemResultInfo
{
	public static ItemResultInfo Parse(string s)
	{
		return Parse(s, new ItemResultInfo());
	}

	public static ItemResultInfo Parse(string s, ItemResultInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static ItemResultInfo Parse(IDictionary d)
	{
		return Parse(d, new ItemResultInfo());
	}

	public static ItemResultInfo Parse(IDictionary d, ItemResultInfo o)
	{
		o.ItemIndex = BaseJsonMarshaler.ParseValueInt(d, "ItemIndex");
		o.AddCount = BaseJsonMarshaler.ParseValueInt(d, "AddCount");
		o.NewCount = BaseJsonMarshaler.ParseValueInt(d, "NewCount");
		o.AddBoosterCount = BaseJsonMarshaler.ParseValueInt(d, "AddBoosterCount");
		o.AddNPCBoosterCount = BaseJsonMarshaler.ParseValueInt(d, "AddNPCBoosterCount");
		o.AddBonusAssignedItemPercent = BaseJsonMarshaler.ParseValueInt(d, "AddBonusAssignedItemPercent");
		o.Locked = BaseJsonMarshaler.ParseValueByte(d, "Locked");
		o.IsFirstClearReward = BaseJsonMarshaler.ParseValueBool(d, "IsFirstClearReward");
		return o;
	}

	public static string EncodeJsonString(ItemResultInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(ItemResultInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ItemIndex", o.ItemIndex.ToString());
		dictionary.Add("AddCount", o.AddCount.ToString());
		dictionary.Add("NewCount", o.NewCount.ToString());
		dictionary.Add("AddBoosterCount", o.AddBoosterCount.ToString());
		dictionary.Add("AddNPCBoosterCount", o.AddNPCBoosterCount.ToString());
		dictionary.Add("AddBonusAssignedItemPercent", o.AddBonusAssignedItemPercent.ToString());
		dictionary.Add("Locked", o.Locked.ToString());
		dictionary.Add("IsFirstClearReward", o.IsFirstClearReward.ToString());
		return dictionary;
	}
}
