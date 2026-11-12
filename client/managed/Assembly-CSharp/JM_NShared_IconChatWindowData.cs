using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_IconChatWindowData
{
	public static IconChatWindowData Parse(string s)
	{
		return Parse(s, new IconChatWindowData());
	}

	public static IconChatWindowData Parse(string s, IconChatWindowData o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static IconChatWindowData Parse(IDictionary d)
	{
		return Parse(d, new IconChatWindowData());
	}

	public static IconChatWindowData Parse(IDictionary d, IconChatWindowData o)
	{
		o.Index = BaseJsonMarshaler.ParseValueInt(d, "Index");
		o.Order = BaseJsonMarshaler.ParseValueInt(d, "Order");
		o.UIPanelType = BaseJsonMarshaler.ParseEnum<RankPanelType>(d, "UIPanelType");
		o.ViewType = BaseJsonMarshaler.ParseEnum<RankViewType>(d, "ViewType");
		o.IconFile = BaseJsonMarshaler.ParseValueString(d, "IconFile");
		return o;
	}

	public static string EncodeJsonString(IconChatWindowData o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(IconChatWindowData o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Index", o.Index.ToString());
		dictionary.Add("Order", o.Order.ToString());
		dictionary.Add("UIPanelType", o.UIPanelType.ToString());
		dictionary.Add("ViewType", o.ViewType.ToString());
		if (o.IconFile != null)
		{
			dictionary.Add("IconFile", o.IconFile);
		}
		return dictionary;
	}
}
