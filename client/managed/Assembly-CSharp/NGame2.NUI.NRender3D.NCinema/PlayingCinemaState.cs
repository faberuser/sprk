namespace NGame2.NUI.NRender3D.NCinema;

public class PlayingCinemaState : BaseCinemaState
{
	public override ECinemaState State => ECinemaState.Playing;

	public override bool OnStart(ICinemaOwner owner)
	{
		return base.OnStart(owner);
	}

	public override void ChainUpdate()
	{
		if (base._cinema == null)
		{
			DebugError("[Update] Cinema is Null");
			return;
		}
		if (base._cinema.IsEnd)
		{
			OnEnd();
			NextState();
		}
		if (_owner.HasNextNodeTree)
		{
			OnEnd();
			SetNextState(new CreateCinemaState());
			_owner.HasNextNodeTree = false;
		}
	}

	public override void NextState()
	{
		SetNextState(new EndCinemaState());
	}
}
