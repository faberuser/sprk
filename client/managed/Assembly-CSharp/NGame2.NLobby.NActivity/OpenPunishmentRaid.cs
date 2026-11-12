using System.Collections;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NLobby;
using NShared;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NLobby.NActivity;

public class OpenPunishmentRaid : IActivity
{
	private int _chapterIndex;

	private int _dungeonIndex;

	public static IActivity Create(string[] values)
	{
		if (!SceneAutoBehaviour<LobbyManagement>.isValidInstance)
		{
			return null;
		}
		int valueInt = ArgParser.GetValueInt(values, 0);
		int valueInt2 = ArgParser.GetValueInt(values, 1);
		ArgParser.GetValueInt(values, 0);
		return new OpenPunishmentRaid(valueInt, valueInt2);
	}

	private OpenPunishmentRaid(int chapterIndex, int dungeonIndex)
	{
		_chapterIndex = chapterIndex;
		_dungeonIndex = dungeonIndex;
	}

	IEnumerator IActivity.coStart()
	{
		if (GameRoot.Instance.CampaignNodeDataContainer.GetData(_chapterIndex, _dungeonIndex) == null)
		{
			VespaDebug.LogError("NodeData is Null" + _chapterIndex + "_" + _dungeonIndex);
			SceneAutoBehaviour<PortalManagement>.instance.OpenPortalRenewalWindow(PortalRenewalCategory.PunishmentRaid);
		}
		else
		{
			SceneAutoBehaviour<PunishManagment>.instance.OpenPunishMainWindow();
		}
		yield return null;
	}

	void IActivity.OnRequestStop()
	{
	}
}
