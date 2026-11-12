namespace NGame2.NUI.NRender3D.NCinema;

public class StartCinemaState : BaseCinemaState
{
	public override ECinemaState State => ECinemaState.Start;

	public override bool OnStart(ICinemaOwner owner)
	{
		if (!base.OnStart(owner))
		{
			return false;
		}
		if (base._cinema == null)
		{
			DebugError("[OnStart] Cinema Is Null");
			return false;
		}
		Vespa_System_Action_Extension_Method.fire(_owner.OnStart);
		if (!base._cinema.StartNodeTree(_owner.OnEnd))
		{
			DebugError("[OnStart] NodeTree Start Fail");
			return false;
		}
		_owner.SetRenderTexture();
		return true;
	}

	public override void ChainUpdate()
	{
		if (base._cinema == null)
		{
			DebugError("[Update] Cinema is Null");
			return;
		}
		if (_owner.HasNextNodeTree)
		{
			OnEnd();
			SetNextState(new CreateCinemaState());
			_owner.HasNextNodeTree = false;
		}
		if (base._cinema.IsPlaying)
		{
			OnEnd();
			NextState();
		}
	}

	public override void NextState()
	{
		SetNextState(new PlayingCinemaState());
	}
}
