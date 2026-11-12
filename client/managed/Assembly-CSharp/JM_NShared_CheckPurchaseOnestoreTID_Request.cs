using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.CheckPurchaseOnestoreTID;

public static class JM_NShared_CheckPurchaseOnestoreTID_Request
{
	public static NShared.CheckPurchaseOnestoreTID.Request Parse(string s)
	{
		return Parse(s, new NShared.CheckPurchaseOnestoreTID.Request());
	}

	public static NShared.CheckPurchaseOnestoreTID.Request Parse(string s, NShared.CheckPurchaseOnestoreTID.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.CheckPurchaseOnestoreTID.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.CheckPurchaseOnestoreTID.Request());
	}

	public static NShared.CheckPurchaseOnestoreTID.Request Parse(IDictionary d, NShared.CheckPurchaseOnestoreTID.Request o)
	{
		o.TID = BaseJsonMarshaler.ParseValueString(d, "TID");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.CheckPurchaseOnestoreTID.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.CheckPurchaseOnestoreTID.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.TID != null)
		{
			dictionary.Add("TID", o.TID);
		}
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
