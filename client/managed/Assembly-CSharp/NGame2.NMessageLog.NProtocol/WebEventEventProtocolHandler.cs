using NGame2.NLogin;
using NGame2.NUI.NManager.Mission;
using NShared;
using NShared.NMessage.NProtocol;
using NVespa.NSingleton;

namespace NGame2.NMessageLog.NProtocol;

public class WebEventEventProtocolHandler : MessageProtocolHandler<WebEventEvent>
{
	public override void Run(long senderId, WebEventEvent protocolData)
	{
		if (protocolData == null || LoginData.WebEvent == null || LoginData.WebEvent.WebContents == null)
		{
			return;
		}
		bool flag = false;
		WebContentInfo[] webContents = LoginData.WebEvent.WebContents;
		foreach (WebContentInfo webContentInfo in webContents)
		{
			if (webContentInfo.Index == protocolData.Index)
			{
				webContentInfo.HasAReward = protocolData.HasAReward;
				webContentInfo.IsComplete = protocolData.IsComplete;
				flag = true;
			}
		}
		if (flag && SceneAutoBehaviour<MissionManagement>.isValidInstance)
		{
			SceneAutoBehaviour<MissionManagement>.instance.RefreshQuestList();
		}
	}
}
