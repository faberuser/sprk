using NGame2.NAccount;
using NGame2.NUI.NWindow;
using NShared;
using NShared.GetPushReward;
using NShared.ReceivePushReward;
using NVespa;
using NVespa.NSingleton;
using NVespa.NWeb;

namespace NGame2.NUI.NManager.NLobby;

public class DailyGiftBoxWebManager : WebManager
{
	public void RequestGetPushReward(IListener<NShared.GetPushReward.Response> iListener)
	{
		if (iListener == null)
		{
			return;
		}
		NShared.GetPushReward.Request request = new NShared.GetPushReward.Request();
		SingletonUpdater<WebServiceRequester>.instance.Request(request, delegate(NShared.GetPushReward.Response response)
		{
			if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
				iListener.Fail(null);
			}
			else if (response.Result != NShared.GetPushReward.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
				iListener.Fail(null);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.DailyGiftBoxInfo = response.PushRewardInfo;
				iListener.Success(null);
			}
		});
	}

	public void RequestReceivePushReward(IListener<NShared.ReceivePushReward.Response> iListener)
	{
		NShared.ReceivePushReward.Request request = new NShared.ReceivePushReward.Request();
		SingletonUpdater<WebServiceRequester>.instance.Request(request, delegate(NShared.ReceivePushReward.Response response)
		{
			if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
				iListener.Fail(null);
			}
			else if (response.Result != NShared.ReceivePushReward.ResultType.Success)
			{
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.DailyGiftBoxInfo = null;
				MessagePopup.OpenError(response.Result);
				if (response.Result != NShared.ReceivePushReward.ResultType.RewardTimeEnded)
				{
					iListener.Fail(null);
				}
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.DailyGiftBoxInfo = null;
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResultInfos);
				GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResultInfos);
				GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Apply(response.EquipItemResultInfos);
				GlobalAutoBehaviour<AccountManager>.instance.ApplyAddHero(response.HeroResultInfos, response.ExpResultInfos);
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.StaminaResultInfos);
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.FriendshipPointResultInfos);
				iListener.Success(response);
			}
		});
	}
}
