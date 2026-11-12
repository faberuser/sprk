using System.Collections.Generic;
using NGame2.NUI.NManager;
using NShared;
using NVespa.NSingleton;
using NVespa.NTask;
using NVespa.NTask.NRuntime;

namespace NGame2.NLobby.NTask;

public class OpenEntryMenu : ILobbyTaskCreator
{
	ITask ILobbyTaskCreator.Create(string[] values)
	{
		int valueInt = ArgParser.GetValueInt(values, 0);
		int valueInt2 = ArgParser.GetValueInt(values, 1);
		bool valueBool = ArgParser.GetValueBool(values, 2);
		bool valueBool2 = ArgParser.GetValueBool(values, 3);
		return new DelegateTask(Run(valueInt, valueInt2, valueBool, valueBool2));
	}

	private IEnumerator<TaskCurrentData> Run(int chapterIndex, int dungeonIndex, bool skipAni, bool playNpcSound)
	{
		bool isComplete = false;
		SceneAutoBehaviour<EntryMenuManagement>.instance.OpenEntryMenuReal(chapterIndex, dungeonIndex, skipAni, playNpcSound, isIgnoreTrigger: false, delegate
		{
			isComplete = true;
		});
		while (!isComplete)
		{
			yield return TaskCurrentData.RUNNING;
		}
		yield return TaskCurrentData.SUCCESS;
	}
}
