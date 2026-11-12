using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_WaveReadyBattleLog_CreatureReadyPosition
{
	public static WaveReadyBattleLog.CreatureReadyPosition Parse(string s)
	{
		return Parse(s, new WaveReadyBattleLog.CreatureReadyPosition());
	}

	public static WaveReadyBattleLog.CreatureReadyPosition Parse(string s, WaveReadyBattleLog.CreatureReadyPosition o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static WaveReadyBattleLog.CreatureReadyPosition Parse(IDictionary d)
	{
		return Parse(d, new WaveReadyBattleLog.CreatureReadyPosition());
	}

	public static WaveReadyBattleLog.CreatureReadyPosition Parse(IDictionary d, WaveReadyBattleLog.CreatureReadyPosition o)
	{
		o.CreatureUid = BaseJsonMarshaler.ParseValueInt(d, "CreatureUid");
		o.ReadyPosition = BaseJsonMarshaler.ParseObject(d, "ReadyPosition", JM_NShared_NUtil_Vector3.Parse);
		return o;
	}

	public static string EncodeJsonString(WaveReadyBattleLog.CreatureReadyPosition o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(WaveReadyBattleLog.CreatureReadyPosition o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("CreatureUid", o.CreatureUid.ToString());
		dictionary.Add("ReadyPosition", JM_NShared_NUtil_Vector3.EncodeJsonObject(o.ReadyPosition));
		return dictionary;
	}
}
