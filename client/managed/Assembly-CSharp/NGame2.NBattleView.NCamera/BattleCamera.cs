namespace NGame2.NBattleView.NCamera;

public class BattleCamera : CameraBase
{
	public int WaveIndex;

	public int GroupIndex;

	protected override bool IsAcceptable(Creature creature)
	{
		if (WaveIndex < creature.CreateWaveIndex)
		{
			return false;
		}
		if (creature.IsDead && !creature.IsDying)
		{
			return false;
		}
		if (creature.CreateInfo.IsPlayer && creature.CreateInfo.GroupIndex != GroupIndex)
		{
			return false;
		}
		return true;
	}
}
