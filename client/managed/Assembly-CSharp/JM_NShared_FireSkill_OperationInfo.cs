using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_FireSkill_OperationInfo
{
	public static string EncodeJsonString(FireSkill.OperationInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(FireSkill.OperationInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("OperationUid", o.OperationUid.ToString());
		dictionary.Add("OperationNo", o.OperationNo.ToString());
		dictionary.Add("TargetCreatureUid", o.TargetCreatureUid.ToString());
		dictionary.Add("TargetPosition", JM_NShared_NUtil_Vector3.EncodeJsonObject(o.TargetPosition));
		dictionary.Add("SoloTimeMs", o.SoloTimeMs.ToString());
		dictionary.Add("IsIgnoreSoloTime", o.IsIgnoreSoloTime.ToString());
		return dictionary;
	}
}
