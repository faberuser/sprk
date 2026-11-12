using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NLobby;
using NVespa.NSingleton;
using NVespa.NTask;
using NVespa.NTask.NRuntime;

namespace NGame2.NLobby.NTask;

public class OpenPortalSelect : ILobbyTaskCreator
{
	ITask ILobbyTaskCreator.Create(string[] values)
	{
		if (!SceneAutoBehaviour<LobbyManagement>.isValidInstance)
		{
			return null;
		}
		return new DelegateTask(delegate
		{
			SceneAutoBehaviour<PortalManagement>.instance.OpenPortalSelectWindow();
		});
	}
}
