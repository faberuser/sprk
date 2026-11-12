using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.SetLockRuneItem;

public static class JM_NShared_SetLockRuneItem_Request
{
	public static NShared.SetLockRuneItem.Request Parse(string s)
	{
		return Parse(s, new NShared.SetLockRuneItem.Request());
	}

	public static NShared.SetLockRuneItem.Request Parse(string s, NShared.SetLockRuneItem.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.SetLockRuneItem.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.SetLockRuneItem.Request());
	}

	public static NShared.SetLockRuneItem.Request Parse(IDictionary d, NShared.SetLockRuneItem.Request o)
	{
		o.RuneItemIndex = BaseJsonMarshaler.ParseValueInt(d, "RuneItemIndex");
		o.Locked = BaseJsonMarshaler.ParseValueByte(d, "Locked");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.SetLockRuneItem.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.SetLockRuneItem.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("RuneItemIndex", o.RuneItemIndex.ToString());
		dictionary.Add("Locked", o.Locked.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
