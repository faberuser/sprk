namespace NGame2.NUI.NManager;

public class CommonManagement : BaseManagement<CommonManagement>
{
	protected override void Awake()
	{
		base.Awake();
		_showLobbyMenu = true;
	}

	public override void ReorderDepth(EDepthOrder depthOrder, ref int baseDepth)
	{
		base.ReorderDepth(depthOrder, ref baseDepth);
	}
}
