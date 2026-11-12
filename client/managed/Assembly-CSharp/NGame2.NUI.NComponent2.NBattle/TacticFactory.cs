using NGame2.NUI.NComponent2.NBattle.NTactic;
using NShared;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NBattle;

public class TacticFactory
{
	public static TacticComponent CreateTactic(BattleTacticType tacticType, string componentName, Transform root)
	{
		return tacticType switch
		{
			BattleTacticType.BossCcGauge => TacticComponent.GetPrefabAuto<BossCcGaugeTacticComponent>(componentName).Factory.Create(root), 
			BattleTacticType.TimerSkill => TacticComponent.GetPrefabAuto<TimerSkillTacticComponent>(componentName).Factory.Create(root), 
			_ => null, 
		};
	}
}
