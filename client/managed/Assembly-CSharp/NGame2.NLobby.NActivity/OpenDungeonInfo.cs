using System.Collections;
using NGame2.NAccount;
using NGame2.NUI.NManager.NLobby;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NLobby.NActivity;

public class OpenDungeonInfo : IActivity
{
	private int _chapterIndex;

	private int _dungeonIndex;

	public static IActivity Create(string[] values)
	{
		int valueInt = ArgParser.GetValueInt(values, 0);
		int valueInt2 = ArgParser.GetValueInt(values, 1);
		return new OpenDungeonInfo(valueInt, valueInt2);
	}

	public OpenDungeonInfo(int chapterIndex, int dungeonIndex)
	{
		_chapterIndex = chapterIndex;
		_dungeonIndex = dungeonIndex;
	}

	IEnumerator IActivity.coStart()
	{
		int linkedDungeonIndex = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.GetLinkedDungeonIndex(_chapterIndex, _dungeonIndex);
		SceneAutoBehaviour<LobbyCampaignDungeon>.instance.OpenDungeonInfo(_chapterIndex, _dungeonIndex, linkedDungeonIndex);
		yield return null;
	}

	void IActivity.OnRequestStop()
	{
	}
}
