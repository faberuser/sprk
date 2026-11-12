using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.PatchNote;

public static class JM_NShared_PatchNote_Request
{
	public static NShared.PatchNote.Request Parse(string s)
	{
		return Parse(s, new NShared.PatchNote.Request());
	}

	public static NShared.PatchNote.Request Parse(string s, NShared.PatchNote.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.PatchNote.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.PatchNote.Request());
	}

	public static NShared.PatchNote.Request Parse(IDictionary d, NShared.PatchNote.Request o)
	{
		o.PatchNoteIndex = BaseJsonMarshaler.ParseValueInt(d, "PatchNoteIndex");
		o.Language = BaseJsonMarshaler.ParseValueString(d, "Language");
		o.BeforeNext = BaseJsonMarshaler.ParseValueByte(d, "BeforeNext");
		o.IsRead = BaseJsonMarshaler.ParseValueBool(d, "IsRead");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.PatchNote.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.PatchNote.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("PatchNoteIndex", o.PatchNoteIndex.ToString());
		if (o.Language != null)
		{
			dictionary.Add("Language", o.Language);
		}
		dictionary.Add("BeforeNext", o.BeforeNext.ToString());
		dictionary.Add("IsRead", o.IsRead.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
