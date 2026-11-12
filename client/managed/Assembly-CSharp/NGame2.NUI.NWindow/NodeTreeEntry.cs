using NGame2.NUI.NRender3D.NCinema;
using NVespa;

namespace NGame2.NUI.NWindow;

public class NodeTreeEntry : BaseWindow, IInspectorGuiDrawer
{
	public const string PrefabPath = "Resources/Prefab2/Window/EntryMenu/NodeTreeMenu";

	private bool _isInit;

	private CinemaOwner _cinemaOwner;

	private EntryDataProvider _provider;

	public UITexture NodeTreeTexture;

	public void Open(CinemaOwner cinemaOwner, EntryDataProvider entryDataProvider)
	{
		Open(delegate
		{
			Init(cinemaOwner, entryDataProvider);
		}, delegate
		{
		});
	}

	public void Init(CinemaOwner cinemaOwner, EntryDataProvider entryDataProvider)
	{
		_provider = entryDataProvider;
		_provider.SetTexture(NodeTreeTexture);
		_cinemaOwner = cinemaOwner;
		_cinemaOwner.SetCinemaTexture();
	}

	public override void OnSystemBackButton()
	{
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		if (!_isInit && _cinemaOwner != null && (NodeTreeTexture.width != _provider.TextureWidth || NodeTreeTexture.height != _provider.TextureHeight))
		{
			_cinemaOwner.SetCinemaTexture();
			_isInit = true;
		}
	}

	public override void OnOtherWindowOpened()
	{
		base.OnOtherWindowOpened();
		_ = _provider;
	}

	public override void ReOpen()
	{
		base.ReOpen();
		_ = _provider;
	}

	public override void OnClosing()
	{
		base.OnClosing();
		_cinemaOwner.Close();
		_cinemaOwner = null;
		_provider.RemoveTexture(NodeTreeTexture);
		_provider = null;
	}
}
