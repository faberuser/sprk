using NShared;
using NVespa.NGlobal;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class ServerListComponent : LinkFactoryComponent, InitOnCreate
{
	public interface Listener
	{
		void OnSelectServer(AppServerInfo serverData);
	}

	public const string PrefabPath = "Prefab2/Component/Login/ServerListComponent";

	public static PrefabAuto<ServerListComponent> PrefabAuto = new PrefabAuto<ServerListComponent>("Prefab2/Component/Login/ServerListComponent");

	public UILabel NameLabel;

	private Listener _listener;

	private AppServerInfo _serverData;

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		_listener = null;
		_serverData = null;
	}

	public void Open(Listener listener, AppServerInfo serverData)
	{
		if (serverData != null)
		{
			_listener = listener;
			_serverData = serverData;
			UIHelper.SetLabelText(NameLabel, LocalizationManager.GetStringDirect(serverData.Name));
		}
	}

	public void OnSelectServer()
	{
		if (_listener != null)
		{
			_listener.OnSelectServer(_serverData);
		}
	}
}
