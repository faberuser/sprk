using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_PatchNoteInfo
{
	public static PatchNoteInfo Parse(string s)
	{
		return Parse(s, new PatchNoteInfo());
	}

	public static PatchNoteInfo Parse(string s, PatchNoteInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PatchNoteInfo Parse(IDictionary d)
	{
		return Parse(d, new PatchNoteInfo());
	}

	public static PatchNoteInfo Parse(IDictionary d, PatchNoteInfo o)
	{
		o.Index = BaseJsonMarshaler.ParseValueInt(d, "Index");
		o.Title = BaseJsonMarshaler.ParseValueString(d, "Title");
		o.Message = BaseJsonMarshaler.ParseValueString(d, "Message");
		o.EndTime = BaseJsonMarshaler.ParseValueString(d, "EndTime");
		return o;
	}

	public static string EncodeJsonString(PatchNoteInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PatchNoteInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Index", o.Index.ToString());
		if (o.Title != null)
		{
			dictionary.Add("Title", o.Title);
		}
		if (o.Message != null)
		{
			dictionary.Add("Message", o.Message);
		}
		if (o.EndTime != null)
		{
			dictionary.Add("EndTime", o.EndTime);
		}
		return dictionary;
	}
}
