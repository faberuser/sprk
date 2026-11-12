namespace NGame2.NUI.NRender3D.NCinema;

public class LoadingCinemaState : BaseCinemaState
{
	public override ECinemaState State => ECinemaState.Loading;

	public override bool OnStart(ICinemaOwner owner)
	{
		if (!base.OnStart(owner))
		{
			return false;
		}
		if (base._cinema == null)
		{
			DebugError("[OnStart] Cinema is Null");
			return false;
		}
		IDataProvider dataProvider = owner.DataProvider;
		base._cinema.LoadNodeTree(dataProvider.NodeTreePack, dataProvider.NodeTreeName);
		return true;
	}

	public override void ChainUpdate()
	{
		if (base._cinema == null)
		{
			DebugError("[ChainUpdate] Cinema is Null");
		}
		else if (base._cinema.IsCompletedLoad())
		{
			OnEnd();
			NextState();
		}
	}

	public override void NextState()
	{
		SetNextState(new StartCinemaState());
	}
}
