using System;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NWindow;
using NShared;
using NShared.ReinforceTeamLevelBuff;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NUI.NManager;

public class TeamLevelBuffManagement : BaseManagement<TeamLevelBuffManagement>, TeamLevelBuffWindow.IListener
{
	protected override void Awake()
	{
		base.Awake();
		AddInstance<TeamLevelBuffWindow>(TeamLevelBuffWindow.PrefabPath);
	}

	public void OpenTeamLevelBuffWindow()
	{
		TeamLevelBuffWindow teamLevelBuffWindow = GetInstance<TeamLevelBuffWindow>();
		if (!teamLevelBuffWindow.IsOpen())
		{
			teamLevelBuffWindow.Open(this);
		}
	}

	void TeamLevelBuffWindow.IListener.RequestReinforceTeamLevelBuff(TeamLevelReinforceData reinforceData, Action onComplete)
	{
		TeamLevelData data = GameRoot.Instance.TeamLevelDataContainer.GetData(reinforceData.TeamLevel);
		if (data == null)
		{
			return;
		}
		NShared.ReinforceTeamLevelBuff.Request request = new NShared.ReinforceTeamLevelBuff.Request();
		request.BuffTeamLevel = reinforceData.TeamLevel;
		request.OptionIndex = data.TeamLevelBuffOptionIndex;
		request.BonusOptionIndex = data.TeamLevelBuffBonusOptionIndex;
		request.Reinforce = reinforceData.Reinforce;
		request.ReinforcePrice = reinforceData.CurrencyPrice;
		request.ReinforcePriceType = reinforceData.CurrencyPriceType.ToString();
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.ReinforceTeamLevelBuff.Response response)
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
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResults);
				GlobalAutoBehaviour<AccountManager>.instance.TeamLevelBuffManager.Apply(response.TeamLevelBuffInfo);
				if (onComplete != null)
				{
					onComplete();
				}
			}
		});
	}
}
