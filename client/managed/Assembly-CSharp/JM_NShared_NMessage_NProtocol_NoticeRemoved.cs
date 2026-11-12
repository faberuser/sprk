using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_NoticeRemoved
{
	public static NoticeRemoved Parse(string s)
	{
		return Parse(s, new NoticeRemoved());
	}

	public static NoticeRemoved Parse(string s, NoticeRemoved o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NoticeRemoved Parse(IDictionary d)
	{
		return Parse(d, new NoticeRemoved());
	}

	public static NoticeRemoved Parse(IDictionary d, NoticeRemoved o)
	{
		o.Id = BaseJsonMarshaler.ParseValueInt(d, "Id");
		return o;
	}

	public static string EncodeJsonString(NoticeRemoved o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NoticeRemoved o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Id", o.Id.ToString());
		return dictionary;
	}
}
