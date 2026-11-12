using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_FriendshipPointResultInfo
{
	public static FriendshipPointResultInfo Parse(string s)
	{
		return Parse(s, new FriendshipPointResultInfo());
	}

	public static FriendshipPointResultInfo Parse(string s, FriendshipPointResultInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static FriendshipPointResultInfo Parse(IDictionary d)
	{
		return Parse(d, new FriendshipPointResultInfo());
	}

	public static FriendshipPointResultInfo Parse(IDictionary d, FriendshipPointResultInfo o)
	{
		o.AddValue = BaseJsonMarshaler.ParseValueLong(d, "AddValue");
		o.AddDailyAccValue = BaseJsonMarshaler.ParseValueLong(d, "AddDailyAccValue");
		o.NewValue = BaseJsonMarshaler.ParseValueLong(d, "NewValue");
		o.NewDailyAccValue = BaseJsonMarshaler.ParseValueLong(d, "NewDailyAccValue");
		return o;
	}

	public static string EncodeJsonString(FriendshipPointResultInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(FriendshipPointResultInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("AddValue", o.AddValue.ToString());
		dictionary.Add("AddDailyAccValue", o.AddDailyAccValue.ToString());
		dictionary.Add("NewValue", o.NewValue.ToString());
		dictionary.Add("NewDailyAccValue", o.NewDailyAccValue.ToString());
		return dictionary;
	}
}
