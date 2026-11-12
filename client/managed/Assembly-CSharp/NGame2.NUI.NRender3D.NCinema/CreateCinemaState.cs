namespace NGame2.NUI.NRender3D.NCinema;

public class CreateCinemaState : BaseCinemaState
{
	public override ECinemaState State => ECinemaState.Create;

	public override bool OnStart(ICinemaOwner owner)
	{
		if (!base.OnStart(owner))
		{
			return false;
		}
		IDataProvider dataProvider = _owner.DataProvider;
		if (base._cinema == null)
		{
			_owner.Cinema = Cinema.CreateInstance(dataProvider);
		}
		else
		{
			if (_owner.IsNextOncePlay)
			{
				base._cinema.Close();
				base._cinema.Open(dataProvider);
			}
			NextState();
		}
		return base._cinema != null;
	}

	public override void NextState()
	{
		SetNextState(new LoadingCinemaState());
	}

	public override void ChainUpdate()
	{
		if (base._cinema != null)
		{
			OnEnd();
			NextState();
		}
	}
}
