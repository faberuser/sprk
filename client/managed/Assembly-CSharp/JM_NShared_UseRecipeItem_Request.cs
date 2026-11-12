using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.UseRecipeItem;

public static class JM_NShared_UseRecipeItem_Request
{
	public static NShared.UseRecipeItem.Request Parse(string s)
	{
		return Parse(s, new NShared.UseRecipeItem.Request());
	}

	public static NShared.UseRecipeItem.Request Parse(string s, NShared.UseRecipeItem.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.UseRecipeItem.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.UseRecipeItem.Request());
	}

	public static NShared.UseRecipeItem.Request Parse(IDictionary d, NShared.UseRecipeItem.Request o)
	{
		o.ItemIndex = BaseJsonMarshaler.ParseValueInt(d, "ItemIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.UseRecipeItem.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.UseRecipeItem.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ItemIndex", o.ItemIndex.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
