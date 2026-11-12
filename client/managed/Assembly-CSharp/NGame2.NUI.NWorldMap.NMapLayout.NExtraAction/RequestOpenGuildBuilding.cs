using System;
using NGame2.NGuild;
using NGame2.NUI.NManager;
using NVespa.NSingleton;

namespace NGame2.NUI.NWorldMap.NMapLayout.NExtraAction;

public class RequestOpenGuildBuilding : IAction
{
	void IAction.Execute(Action action)
	{
		if (GlobalAutoBehaviour<GuildManager>.instance.MemberManager == null)
		{
			SceneAutoBehaviour<GuildManagement2>.instance.GetAllGuildMemberInfo(delegate
			{
				Request(action);
			});
		}
		else
		{
			Request(action);
		}
	}

	private void Request(Action action)
	{
		GlobalAutoBehaviour<GuildManager>.instance.RequestOpenGuildBuilding(showUpgradeAnim: true, action);
	}
}
