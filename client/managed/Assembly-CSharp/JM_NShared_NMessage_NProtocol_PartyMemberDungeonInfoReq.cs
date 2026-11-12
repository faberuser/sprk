using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_PartyMemberDungeonInfoReq
{
	public static PartyMemberDungeonInfoReq Parse(string s)
	{
		return Parse(s, new PartyMemberDungeonInfoReq());
	}

	public static PartyMemberDungeonInfoReq Parse(string s, PartyMemberDungeonInfoReq o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PartyMemberDungeonInfoReq Parse(IDictionary d)
	{
		return Parse(d, new PartyMemberDungeonInfoReq());
	}

	public static PartyMemberDungeonInfoReq Parse(IDictionary d, PartyMemberDungeonInfoReq o)
	{
		return o;
	}

	public static string EncodeJsonString(PartyMemberDungeonInfoReq o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PartyMemberDungeonInfoReq o)
	{
		return new Dictionary<string, object>();
	}
}
