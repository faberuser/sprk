using NShared.NUtil;

namespace NShared;

[JsonSerializable]
public class CreatureManualMoveBattleLog : IBattleLogRuntime
{
	public int CreatureUid { get; set; }

	public Vector3 Position { get; set; }

	BattleLogType IBattleLogRuntime.GetBattleLogType()
	{
		return BattleLogType.CreatureManualMove;
	}

	void IBattleLogRuntime.Run(BattleInstance battleInstance, BattleLog battleLog)
	{
		BattleCreature creature = battleInstance.CreatureContainer.GetCreature(CreatureUid);
		if (creature != null)
		{
			Vector3 position = Position;
			battleInstance.BattleArea.ValidateByGroupIndex(ref position, creature.CreateInfo.GroupIndex);
			creature.ManualMove(position);
		}
	}
}
