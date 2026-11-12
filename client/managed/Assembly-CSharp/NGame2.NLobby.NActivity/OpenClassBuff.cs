using System.Collections;
using NGame2.NUI.NManager.ClassBuff;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NLobby.NActivity;

public class OpenClassBuff : IActivity
{
	private CreatureTagType creatureType;

	public static IActivity Create(string[] values)
	{
		string type = "";
		if (__Extension_Method_ICollection__.COUNT(values) > 0)
		{
			type = values[0];
		}
		return new OpenClassBuff(type);
	}

	public OpenClassBuff(string type)
	{
		if (!BanRuleData.GetCreatureTagType(type.ToString(), out creatureType))
		{
			creatureType = CreatureTagType.Knight;
		}
	}

	IEnumerator IActivity.coStart()
	{
		SceneAutoBehaviour<ClassBuffManagement>.instance.OpenClassBuffMain(creatureType);
		yield return null;
	}

	void IActivity.OnRequestStop()
	{
	}
}
