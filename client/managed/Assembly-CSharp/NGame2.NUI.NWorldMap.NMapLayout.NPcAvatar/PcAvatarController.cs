using NVespa;

namespace NGame2.NUI.NWorldMap.NMapLayout.NPcAvatar;

public class PcAvatarController : AvatarController
{
	public PcAvatarController(IListener listener)
		: base(listener)
	{
	}

	public override void UpdateAvatarIdleDirection(float deltaTime)
	{
		if (!(Math.Rotate(270f) == _listener.GetAvatar().CurrentDirection))
		{
			_listener.GetAvatar().SetDirection(270f);
		}
	}

	public override void ChainUpdate()
	{
		float deltaTime = MainTimer.deltaTime;
		if (IsAvatarMoving())
		{
			UpdateAvatarMoving(deltaTime);
			UpdateAnimation(deltaTime);
		}
		else
		{
			UpdateAvatarIdleDirection(deltaTime);
		}
	}
}
