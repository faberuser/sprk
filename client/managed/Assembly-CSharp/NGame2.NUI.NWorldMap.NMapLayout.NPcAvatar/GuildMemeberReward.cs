using System;
using NGame2.NAccount;
using NGame2.NGuild;
using NShared;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NUI.NWorldMap.NMapLayout.NPcAvatar;

public class GuildMemeberReward : IInteractiveAction
{
	private int _rewardIndex;

	private float _rewardCoolTime;

	private float _rewardCheckInteval = 1f;

	private DateTime _lastRewardCheckTime;

	private bool _IsPendingRequestReward;

	private bool _canAction;

	private AiInteractionComponent _aiInteractionComponent;

	void IInteractiveAction.Init(PcAvatarData data, AiInteractionComponent aiInteractionComponent)
	{
		_rewardIndex = data.Index;
		_rewardCoolTime = data.RewardCoolTime;
		_IsPendingRequestReward = false;
		_canAction = false;
		_aiInteractionComponent = aiInteractionComponent;
	}

	bool IInteractiveAction.CanAction()
	{
		return _canAction;
	}

	void IInteractiveAction.DoAction()
	{
		VespaDebug.LogError("DoAction");
		_IsPendingRequestReward = true;
		_canAction = false;
		GlobalAutoBehaviour<GuildManager>.instance.RequestGiveRewardGuildMember(_rewardIndex, delegate
		{
			VespaDebug.LogError("DoAction Done");
			_aiInteractionComponent.ShowReward(isShow: false);
			_IsPendingRequestReward = false;
		});
	}

	void IInteractiveAction.Update()
	{
		if (_IsPendingRequestReward)
		{
			return;
		}
		DateTime currentServerUTCTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime();
		if (currentServerUTCTime >= _lastRewardCheckTime.AddSeconds(_rewardCheckInteval))
		{
			_lastRewardCheckTime = currentServerUTCTime;
			DateTime outRewardTime = DateTime.MinValue;
			if (!TryGetRewardTime(out outRewardTime))
			{
				SetCanAction(canAction: false);
			}
			else
			{
				SetCanAction(currentServerUTCTime > outRewardTime);
			}
		}
	}

	private void SetCanAction(bool canAction)
	{
		if (_canAction != canAction)
		{
			_aiInteractionComponent.ShowReward(canAction);
		}
		_canAction = canAction;
	}

	private bool TryGetRewardTime(out DateTime outRewardTime)
	{
		outRewardTime = DateTime.MinValue;
		string guildMemberRewardedTime = GlobalAutoBehaviour<AccountManager>.instance.UserManager.MiscInfo.GuildMemberRewardedTime;
		DateTime o_dateTime = DateTime.MinValue;
		if (!Vespa_String_Extension_Method2.TryParseNetworkTime2(guildMemberRewardedTime, out o_dateTime))
		{
			return false;
		}
		outRewardTime = o_dateTime.AddSeconds(_rewardCoolTime);
		return true;
	}
}
