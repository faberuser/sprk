using System.Collections;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NLobby;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NLobby.NActivity;

public class OpenRaid : IActivity
{
	private RaidType _raidType;

	public static IActivity Create(string[] values)
	{
		if (!SceneAutoBehaviour<LobbyManagement>.isValidInstance)
		{
			return null;
		}
		return new OpenRaid(ArgParser.GetValueEnum(values, 0, RaidType.DragonNormal));
	}

	private OpenRaid(RaidType raidType)
	{
		_raidType = raidType;
	}

	IEnumerator IActivity.coStart()
	{
		if (_raidType == RaidType.NONE)
		{
			SceneAutoBehaviour<PortalManagement>.instance.OpenPortalRenewalWindow(PortalRenewalCategory.Raid);
		}
		else
		{
			SceneAutoBehaviour<RaidManagement>.instance.OpenRaidList(_raidType);
		}
		yield return null;
	}

	void IActivity.OnRequestStop()
	{
	}
}
