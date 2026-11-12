using NGame2.NNodeTree;
using NVespa;
using NVespa.NGlobal;

namespace NGame2.NUI.NRender3D.NCinema;

public class NodeTreeLoader
{
	public enum EState
	{
		Loading,
		Completed,
		LoadError
	}

	private EState _state;

	private INodeTreeLoadProvider _provider;

	public EState State => _state;

	public NodeTreeLoader(INodeTreeLoadProvider provider)
	{
		_provider = provider;
	}

	public void LoadStart()
	{
		SingletonUpdater<GCManager>.instance.IPhoneLowMemCollect();
		_provider.PackLoad = SingletonAuto<PackContainer>.instance.GetPack(_provider.NodeTreePack);
		_state = EState.Loading;
	}

	public bool Loading()
	{
		if (_state != EState.Loading)
		{
			return false;
		}
		PackLoad packLoad = _provider.PackLoad;
		if (!packLoad.ISDONE())
		{
			return true;
		}
		if (!packLoad.ISVALID())
		{
			_state = EState.LoadError;
			return false;
		}
		TreeData treeData = (_provider.TreeData = packLoad.Value.GetNodeTree(_provider.NodeTreeName));
		TreeData treeData2 = treeData;
		if (treeData2 == null)
		{
			_state = EState.LoadError;
			return false;
		}
		treeData2.ReserveCache(_provider.Preloader);
		_state = EState.Completed;
		return false;
	}
}
