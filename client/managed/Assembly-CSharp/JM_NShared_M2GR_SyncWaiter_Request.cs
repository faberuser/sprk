using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.M2GR_SyncWaiter;

public static class JM_NShared_M2GR_SyncWaiter_Request
{
	public static NShared.M2GR_SyncWaiter.Request Parse(string s)
	{
		return Parse(s, new NShared.M2GR_SyncWaiter.Request());
	}

	public static NShared.M2GR_SyncWaiter.Request Parse(string s, NShared.M2GR_SyncWaiter.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.M2GR_SyncWaiter.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.M2GR_SyncWaiter.Request());
	}

	public static NShared.M2GR_SyncWaiter.Request Parse(IDictionary d, NShared.M2GR_SyncWaiter.Request o)
	{
		o.__Header__ = BaseJsonMarshaler.ParseObject(d, "__Header__", JM_NShared_M2GR_SyncWaiter_Header.Parse);
		o.MatchServerHost = BaseJsonMarshaler.ParseValueString(d, "MatchServerHost");
		o.ArenaType = BaseJsonMarshaler.ParseEnum<MatchArenaType>(d, "ArenaType");
		o.AccountInfosForRegister = BaseJsonMarshaler.ParseArray(d, "AccountInfosForRegister", JM_NShared_MatchedAccountInfo.Parse);
		o.AccountIdsForTouch = BaseJsonMarshaler.ParseArrayCast(d, "AccountIdsForTouch", BaseJsonMarshaler.CastToLong);
		o.CanceledAccountIds = BaseJsonMarshaler.ParseArrayCast(d, "CanceledAccountIds", BaseJsonMarshaler.CastToLong);
		o.PausedAccountIds = BaseJsonMarshaler.ParseArrayCast(d, "PausedAccountIds", BaseJsonMarshaler.CastToLong);
		return o;
	}

	public static string EncodeJsonString(NShared.M2GR_SyncWaiter.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.M2GR_SyncWaiter.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("__Header__", JM_NShared_M2GR_SyncWaiter_Header.EncodeJsonObject(o.__Header__));
		if (o.MatchServerHost != null)
		{
			dictionary.Add("MatchServerHost", o.MatchServerHost);
		}
		dictionary.Add("ArenaType", o.ArenaType.ToString());
		dictionary.Add("AccountInfosForRegister", BaseJsonMarshaler.EncodeArray(o.AccountInfosForRegister, JM_NShared_MatchedAccountInfo.EncodeJsonObject));
		dictionary.Add("AccountIdsForTouch", BaseJsonMarshaler.EncodeArray(o.AccountIdsForTouch));
		dictionary.Add("CanceledAccountIds", BaseJsonMarshaler.EncodeArray(o.CanceledAccountIds));
		dictionary.Add("PausedAccountIds", BaseJsonMarshaler.EncodeArray(o.PausedAccountIds));
		return dictionary;
	}
}
