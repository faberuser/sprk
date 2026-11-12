namespace NGame2.NUI.NRender3D.NCinema;

public interface ICinemaState
{
	ECinemaState State { get; }

	bool OnStart(ICinemaOwner onwer);

	void NextState();

	void ChainUpdate();

	void OnEnd();
}
