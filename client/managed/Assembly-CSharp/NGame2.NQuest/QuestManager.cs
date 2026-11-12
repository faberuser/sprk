using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NScenarioQuest;
using NGame2.NTutorial;
using NGame2.NUI.NWindow;
using NShared;
using NShared.ProgressClientMainQuest;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NQuest;

public class QuestManager : SingletonUpdater<QuestManager>, IUserObserver<MainQuestInfo>, IUserObserver
{
	private readonly ScenarioQuestFactory _runtimeFactory = new ScenarioQuestFactory();

	private IScenarioQuestRuntime _currentRuntime;

	public QuestManager()
	{
		ProcessInClient();
	}

	private void ProcessInClient()
	{
		if (!AccountManager.IsLogined())
		{
			return;
		}
		MainQuestInfo mainQuestInfo = GlobalAutoBehaviour<AccountManager>.instance.QuestManager.MainQuestInfo;
		if (mainQuestInfo == null)
		{
			return;
		}
		MainQuestData dataByStep = GameRoot.Instance.MainQuestDataContainer.GetDataByStep(mainQuestInfo.Step);
		if (dataByStep != null)
		{
			IScenarioQuestRuntime scenarioQuestRuntime = _runtimeFactory.Create(dataByStep.StateType);
			if (scenarioQuestRuntime != null)
			{
				_currentRuntime = scenarioQuestRuntime;
				_currentRuntime.Run(dataByStep.StateValue);
			}
		}
	}

	public override void ChainUpdate()
	{
		if ((!SingletonUpdater<TutorialManager>.isValidInstance || !SingletonUpdater<TutorialManager>.instance.IsProgressing) && _currentRuntime != null && _currentRuntime.IsEnd())
		{
			RequestClientMainQuest();
			_currentRuntime = null;
		}
	}

	private void RequestClientMainQuest()
	{
		NShared.ProgressClientMainQuest.Request request = new NShared.ProgressClientMainQuest.Request();
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.ProgressClientMainQuest.Response response)
		{
			if (response != null && response.BaseResult == BaseResultType.Success && response.Result == ResultType.Success && AccountManager.IsLogined())
			{
				GlobalAutoBehaviour<AccountManager>.instance.QuestManager.Apply(response.ReservedMainQuestInfo);
			}
		});
	}

	public void OpenClearMainQuestConfirmNotiPopup(Action<int> reservedMainQuestClearPopup = null)
	{
		if (!AccountManager.IsLogined())
		{
			return;
		}
		if (!GlobalAutoBehaviour<AccountManager>.instance.QuestManager.CanOpenMainQuestNotiPopup())
		{
			Vespa_Generic_ICollection_Extension_Method.CLEAR(GlobalAutoBehaviour<AccountManager>.instance.QuestManager.ClearMainQuestStepList);
			return;
		}
		NGame2.NAccount.UserQuestManager questManager = GlobalAutoBehaviour<AccountManager>.instance.QuestManager;
		List<int> clearMainQuestStepList = questManager.ClearMainQuestStepList;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(clearMainQuestStepList); i++)
		{
			int num = clearMainQuestStepList[i];
			if (reservedMainQuestClearPopup != null)
			{
				Vespa_System_Action_Extension_Method.fire(reservedMainQuestClearPopup, num);
			}
			else
			{
				OpenConfirmNotiPopupByStep(num);
			}
		}
		questManager.ClearMainQuestStepList.Clear();
	}

	public void OpenConfirmNotiPopupByStep(int step)
	{
		MainQuestData dataByStep = GameRoot.Instance.MainQuestDataContainer.GetDataByStep(step);
		if (dataByStep != null)
		{
			AddClearQuestPopupQueue(dataByStep);
		}
	}

	public void OpenSubQuestClearPopup(SubQuestInfo info)
	{
		if (GlobalAutoBehaviour<AccountManager>.isValidInstance && AccountManager.IsLogined() && info != null)
		{
			SubQuestData subQuestData = GameRoot.Instance.SubQuestDataContainer.GetSubQuestData(info.SubQuestIndex, info.LastStep + 1);
			if (subQuestData != null)
			{
				AddClearQuestPopupQueue(subQuestData);
			}
		}
	}

	private void AddClearQuestPopupQueue(IQuestData data)
	{
		if (data != null)
		{
			string localizeMissionText = GetLocalizeMissionText(data.Title, data.Variable1);
			string localizeMissionText2 = GetLocalizeMissionText(data.Goal, data.Variable3);
			string path = ((data is MainQuestData) ? (data as MainQuestData).BackgroundPath : "");
			ConfirmNotiPopup.Open(localizeMissionText, localizeMissionText2, path, data.QuestIndex);
		}
	}

	bool IUserObserver.IsGlobal()
	{
		return true;
	}

	void IUserObserver<MainQuestInfo>.OnChanged(MainQuestInfo userStatInfo)
	{
		ProcessInClient();
	}

	public string GetLocalizeQuestTitle(IQuestData data)
	{
		if (data == null)
		{
			return string.Empty;
		}
		string title = data.Title;
		object[] variable = data.Variable1;
		return LocalizationManager.GetStringDirect(title, variable);
	}

	public string GetLocalizeQuestDesc(IQuestData data)
	{
		if (data == null)
		{
			return string.Empty;
		}
		string desc = data.Desc;
		object[] variable = data.Variable2;
		return LocalizationManager.GetStringDirect(desc, variable);
	}

	public string GetLocalizeQuestGoal(IQuestData data)
	{
		if (data == null)
		{
			return string.Empty;
		}
		string goal = data.Goal;
		object[] variable = data.Variable3;
		return LocalizationManager.GetStringDirect(goal, variable);
	}

	public string GetLocalizeMissionText(string title, string[] variable)
	{
		if (variable == null || variable.Length == 0)
		{
			return LocalizationManager.GetStringDirect(title);
		}
		string[] array = new string[variable.Length];
		for (int i = 0; i < variable.Length; i++)
		{
			array[i] = LocalizationManager.GetStringDirect(variable[i]);
		}
		object[] args = array;
		return LocalizationManager.GetStringDirect(title, args);
	}
}
