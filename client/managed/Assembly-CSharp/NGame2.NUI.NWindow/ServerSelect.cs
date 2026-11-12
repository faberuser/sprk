using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;

namespace NGame2.NUI.NWindow;

public class ServerSelect : BaseWindow, ServerListComponent.Listener
{
	public interface Listener
	{
		void OnSelect(ServerSelect caller, string serverName);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Login/ServerSelect";

	public UIScrollGrid ServerGrid;

	private Listener _listener;

	private AppServerInfo[] _serverDatas;

	private List<ServerListComponent> _componentList = new List<ServerListComponent>();

	public void Open(Listener listener, AppServerInfo[] serverDatas)
	{
		base.Open();
		_listener = listener;
		_serverDatas = serverDatas;
		InitServerList();
	}

	public override void Reset()
	{
		base.Reset();
		FactoryBase.ReleaseAndClearList(_componentList);
		_listener = null;
	}

	private void InitServerList()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_serverDatas); i++)
		{
			AppServerInfo serverData = _serverDatas[i];
			ServerListComponent serverListComponent = UIHelper.AddGridItem(ServerGrid, ServerListComponent.PrefabAuto);
			if (!(serverListComponent == null))
			{
				serverListComponent.Open(this, serverData);
				_componentList.Add(serverListComponent);
			}
		}
		UIHelper.RepositionGrid(ServerGrid);
	}

	void ServerListComponent.Listener.OnSelectServer(AppServerInfo serverData)
	{
		if (serverData != null)
		{
			string serverName = serverData.Name;
			if (_listener != null)
			{
				_listener.OnSelect(this, serverName);
			}
		}
	}
}
