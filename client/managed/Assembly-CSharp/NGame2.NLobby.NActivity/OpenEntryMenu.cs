using System.Collections;
using NGame2.NUI.NManager;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NLobby.NActivity;

public class OpenEntryMenu : IActivity
{
	private int _chapterIndex;

	private int _dungeonIndex;

	private bool _skipAni;

	private bool _playNpcSound;

	public static IActivity Create(string[] values)
	{
		int valueInt = ArgParser.GetValueInt(values, 0);
		int valueInt2 = ArgParser.GetValueInt(values, 1);
		bool valueBool = ArgParser.GetValueBool(values, 2);
		bool valueBool2 = ArgParser.GetValueBool(values, 3);
		return new OpenEntryMenu(valueInt, valueInt2, valueBool, valueBool2);
	}

	public OpenEntryMenu(int chapterIndex, int dungeonIndex, bool skipAni, bool playNpcSound)
	{
		_chapterIndex = chapterIndex;
		_dungeonIndex = dungeonIndex;
		_skipAni = skipAni;
		_playNpcSound = playNpcSound;
	}

	IEnumerator IActivity.coStart()
	{
		bool isComplete = false;
		SceneAutoBehaviour<EntryMenuManagement>.instance.OpenEntryMenuReal(_chapterIndex, _dungeonIndex, _skipAni, _playNpcSound, isIgnoreTrigger: false, delegate
		{
			isComplete = true;
		});
		while (!isComplete)
		{
			yield return null;
		}
	}

	void IActivity.OnRequestStop()
	{
	}
}
