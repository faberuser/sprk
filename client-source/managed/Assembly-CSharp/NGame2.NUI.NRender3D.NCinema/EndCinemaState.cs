namespace NGame2.NUI.NRender3D.NCinema;

public class EndCinemaState : BaseCinemaState
{
	public override ECinemaState State => ECinemaState.End;

	public override bool OnStart(ICinemaOwner owner)
	{
		return base.OnStart(owner);
	}

	public override void ChainUpdate()
	{
		if (base._cinema == null)
		{
			DebugError("[Update] Cinema is Null");
		}
		if (_owner.HasNextNodeTree)
		{
			if (!base._cinema.IsEnd)
			{
				Vespa_System_Action_Extension_Method.fire(_owner.OnEnd);
			}
			OnEnd();
			NextState();
			_owner.HasNextNodeTree = false;
		}
	}

	public override void NextState()
	{
		SetNextState(new CreateCinemaState());
	}
}
