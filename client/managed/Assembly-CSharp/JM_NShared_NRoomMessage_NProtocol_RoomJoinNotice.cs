using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NRoomMessage.NProtocol;

public static class JM_NShared_NRoomMessage_NProtocol_RoomJoinNotice
{
	public static RoomJoinNotice Parse(string s)
	{
		return Parse(s, new RoomJoinNotice());
	}

	public static RoomJoinNotice Parse(string s, RoomJoinNotice o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static RoomJoinNotice Parse(IDictionary d)
	{
		return Parse(d, new RoomJoinNotice());
	}

	public static RoomJoinNotice Parse(IDictionary d, RoomJoinNotice o)
	{
		o.UserInfo = BaseJsonMarshaler.ParseObject(d, "UserInfo", JM_NShared_CachedUserInfo.Parse);
		return o;
	}

	public static string EncodeJsonString(RoomJoinNotice o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(RoomJoinNotice o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("UserInfo", JM_NShared_CachedUserInfo.EncodeJsonObject(o.UserInfo));
		return dictionary;
	}
}
