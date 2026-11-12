using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_ChangePartyRoomMaster
{
	public static ChangePartyRoomMaster Parse(string s)
	{
		return Parse(s, new ChangePartyRoomMaster());
	}

	public static ChangePartyRoomMaster Parse(string s, ChangePartyRoomMaster o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static ChangePartyRoomMaster Parse(IDictionary d)
	{
		return Parse(d, new ChangePartyRoomMaster());
	}

	public static ChangePartyRoomMaster Parse(IDictionary d, ChangePartyRoomMaster o)
	{
		o.NewMasterAccountId = BaseJsonMarshaler.ParseValueLong(d, "NewMasterAccountId");
		return o;
	}

	public static string EncodeJsonString(ChangePartyRoomMaster o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(ChangePartyRoomMaster o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("NewMasterAccountId", o.NewMasterAccountId.ToString());
		return dictionary;
	}
}
