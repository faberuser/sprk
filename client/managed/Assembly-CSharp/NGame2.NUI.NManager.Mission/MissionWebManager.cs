using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NWindow;
using NShared;
using NShared.CompleteSubQuest;
using NVespa;
using NVespa.NSingleton;
using NVespa.NWeb;

namespace NGame2.NUI.NManager.Mission;

public class MissionWebManager
{
	private MissionManagement _management;

	public void Init(MissionManagement t)
	{
		_management = t;
	}

	public void RequestCompleteSubQuest(List<int> list, Action<NShared.CompleteSubQuest.Response, float> responseComplete, float openDelayTime = 0f)
	{
		NShared.CompleteSubQuest.Request request = new NShared.CompleteSubQuest.Request
		{
			SubQuestIndices = list.ToArray()
		};
		SingletonUpdater<WebServiceRequester>.instance.Request(request, delegate(NShared.CompleteSubQuest.Response response)
		{
			if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			else if (response.Result != ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.QuestManager.Apply(response.SucceededSubQuestInfos, isComplete: true);
				GlobalAutoBehaviour<AccountManager>.instance.ApplyRewardResult(response.RewardResults);
				if (responseComplete != null)
				{
					Vespa_System_Action_Extension_Method.fire(responseComplete, response, openDelayTime);
				}
			}
		});
	}
}
