using System.Collections;
using NGame2.NUI.NManager;
using NVespa.NSingleton;

namespace NGame2.NLobby.NActivity;

public class OpenDungeonListPopup : IActivity
{
	private int _chapterIndex;

	private int _dungeonIndex;

	public static IActivity Create(string[] values)
	{
		if (!int.TryParse(values[0], out var result))
		{
			return null;
		}
		if (!int.TryParse(values[1], out var result2))
		{
			return null;
		}
		return new OpenDungeonListPopup(result, result2);
	}

	public OpenDungeonListPopup(int chapterIndex, int dungeonIndex)
	{
		_chapterIndex = chapterIndex;
		_dungeonIndex = dungeonIndex;
	}

	IEnumerator IActivity.coStart()
	{
		SceneAutoBehaviour<PortalManagement>.instance.OpenPortalSelectWindow();
		SceneAutoBehaviour<PortalManagement>.instance.MoveDirectDungeon(_chapterIndex, _dungeonIndex);
		yield return null;
	}

	void IActivity.OnRequestStop()
	{
	}
}
