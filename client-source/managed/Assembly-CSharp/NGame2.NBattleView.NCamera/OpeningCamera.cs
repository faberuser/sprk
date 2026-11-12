using NShared;

namespace NGame2.NBattleView.NCamera;

public class OpeningCamera : CameraBase
{
	protected override bool IsAcceptable(Creature creature)
	{
		TeamType myTeam = CameraBase.GetMyTeam();
		if (creature.CreateInfo.IsAi)
		{
			return false;
		}
		if (creature.TeamType != myTeam)
		{
			return false;
		}
		if (creature.IsDead)
		{
			return false;
		}
		return true;
	}
}
