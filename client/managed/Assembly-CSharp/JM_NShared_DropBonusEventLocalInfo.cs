using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_DropBonusEventLocalInfo
{
	public static DropBonusEventLocalInfo Parse(string s)
	{
		return Parse(s, new DropBonusEventLocalInfo());
	}

	public static DropBonusEventLocalInfo Parse(string s, DropBonusEventLocalInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static DropBonusEventLocalInfo Parse(IDictionary d)
	{
		return Parse(d, new DropBonusEventLocalInfo());
	}

	public static DropBonusEventLocalInfo Parse(IDictionary d, DropBonusEventLocalInfo o)
	{
		o.Id = BaseJsonMarshaler.ParseValueInt(d, "Id");
		o.Language = BaseJsonMarshaler.ParseValueString(d, "Language");
		o.Title = BaseJsonMarshaler.ParseValueString(d, "Title");
		o.Message = BaseJsonMarshaler.ParseValueString(d, "Message");
		return o;
	}

	public static string EncodeJsonString(DropBonusEventLocalInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(DropBonusEventLocalInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Id", o.Id.ToString());
		if (o.Language != null)
		{
			dictionary.Add("Language", o.Language);
		}
		if (o.Title != null)
		{
			dictionary.Add("Title", o.Title);
		}
		if (o.Message != null)
		{
			dictionary.Add("Message", o.Message);
		}
		return dictionary;
	}
}
