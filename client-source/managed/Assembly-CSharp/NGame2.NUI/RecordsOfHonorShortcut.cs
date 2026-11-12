using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow;
using NShared;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI;

public class RecordsOfHonorShortcut : MonoBehaviour
{
	public interface ServerTypeProvider
	{
		HonoraryChamber.ServerType GetCurrentServerType();
	}

	public UIButton Button;

	public RecordsOfHonorContentType ContentType;

	public HonoraryChamber.ServerType ServerType;

	public BaseWindow BaseWindowServerProvider;

	private void Start()
	{
		if (!(Button != null))
		{
			return;
		}
		Button.onClick.Clear();
		Button.onClick.Add(new EventDelegate(delegate
		{
			LobbyManagement instance = SceneAutoBehaviour<LobbyManagement>.instance;
			if (!(instance == null))
			{
				HonoraryChamber.ServerType serverType = ((BaseWindowServerProvider is ServerTypeProvider serverTypeProvider) ? serverTypeProvider.GetCurrentServerType() : ServerType);
				instance.OpenHonoraryChamber(ContentType, serverType);
			}
		}));
	}
}
