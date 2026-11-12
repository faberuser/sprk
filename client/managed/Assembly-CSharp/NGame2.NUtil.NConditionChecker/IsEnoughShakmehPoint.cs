using NGame2.NUI.NManager;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NUtil.NConditionChecker;

public class IsEnoughShakmehPoint : ConditionChecker.ConditionBase
{
	public override bool Check(string[] values)
	{
		NGame2.NUI.NManager.ShakmehDungeonManagement instance = SceneAutoBehaviour<NGame2.NUI.NManager.ShakmehDungeonManagement>.instance;
		if (instance == null)
		{
			return false;
		}
		bool isEnoughShakmehBossGauge = instance.IsEnoughShakmehBossGauge;
		bool valueBool = ArgParser.GetValueBool(values, 1);
		return isEnoughShakmehBossGauge == valueBool;
	}
}
