using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_PlayerItemUseInfo
{
	public static PlayerItemUseInfo Parse(string s)
	{
		return Parse(s, new PlayerItemUseInfo());
	}

	public static PlayerItemUseInfo Parse(string s, PlayerItemUseInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PlayerItemUseInfo Parse(IDictionary d)
	{
		return Parse(d, new PlayerItemUseInfo());
	}

	public static PlayerItemUseInfo Parse(IDictionary d, PlayerItemUseInfo o)
	{
		o.ItemIndex = BaseJsonMarshaler.ParseValueInt(d, "ItemIndex");
		o.OutItemIndex = BaseJsonMarshaler.ParseValueInt(d, "OutItemIndex");
		o.UseCount = BaseJsonMarshaler.ParseValueInt(d, "UseCount");
		o.CompletedTime = BaseJsonMarshaler.ParseValueString(d, "CompletedTime");
		return o;
	}

	public static string EncodeJsonString(PlayerItemUseInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PlayerItemUseInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ItemIndex", o.ItemIndex.ToString());
		dictionary.Add("OutItemIndex", o.OutItemIndex.ToString());
		dictionary.Add("UseCount", o.UseCount.ToString());
		if (o.CompletedTime != null)
		{
			dictionary.Add("CompletedTime", o.CompletedTime);
		}
		return dictionary;
	}
}
