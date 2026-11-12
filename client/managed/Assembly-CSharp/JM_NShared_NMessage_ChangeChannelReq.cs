using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage;

public static class JM_NShared_NMessage_ChangeChannelReq
{
	public static ChangeChannelReq Parse(string s)
	{
		return Parse(s, new ChangeChannelReq());
	}

	public static ChangeChannelReq Parse(string s, ChangeChannelReq o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static ChangeChannelReq Parse(IDictionary d)
	{
		return Parse(d, new ChangeChannelReq());
	}

	public static ChangeChannelReq Parse(IDictionary d, ChangeChannelReq o)
	{
		o.ChannelNo = BaseJsonMarshaler.ParseValueInt(d, "ChannelNo");
		o.RequestId = BaseJsonMarshaler.ParseValueInt(d, "RequestId");
		return o;
	}

	public static string EncodeJsonString(ChangeChannelReq o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(ChangeChannelReq o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ChannelNo", o.ChannelNo.ToString());
		dictionary.Add("RequestId", o.RequestId.ToString());
		return dictionary;
	}
}
