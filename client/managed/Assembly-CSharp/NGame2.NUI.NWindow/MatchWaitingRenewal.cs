using System.Collections;
using System.Linq;
using NGame2.N3D;
using NGame2.NAccount;
using NGame2.NMatch;
using NGame2.NUI.NManager.NLobby;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class MatchWaitingRenewal : MatchWaitingBase
{
	public const string PrefabPath = "Resources/Prefab2/Window/Match/MatchWaitingRenewal";

	public PlayerInfo Player1;

	public UILabel CancelLabel;

	public UIButton CancelButton;

	public Transform Waiting;

	public Transform Challenger;

	public TweenAlpha[] TweenAlphaArray;

	public TweenScale[] TweenScaleArray;

	private float startTime;

	private float _limitWaitingTime;

	private bool isTimeOut;

	private bool isWaitingOpponentExpireTime;

	public void OpenMatchWaiting()
	{
		base.Open();
		UIHelper.EnableGameObject(SceneAutoBehaviour<LobbyManagement>.instance.GetInstance<LobbyTopmostMenu>(), enable: false);
		SceneAutoBehaviour<LobbyManagement>.instance.GetInstance<LobbyTopmostMenu>().gameObject.SetActive(value: false);
		string nick = GlobalAutoBehaviour<AccountManager>.instance.UserManager.Nick;
		int teamLevel = GlobalAutoBehaviour<AccountManager>.instance.UserManager.TeamLevel;
		int avatarHeroIndex = GlobalAutoBehaviour<AccountManager>.instance.UserManager.AvatarHeroIndex;
		SetupPlayer(Player1, nick, teamLevel, avatarHeroIndex);
		GameUIRoot.instance.GetLayerInfo(GameUIRoot.ELayerType.UI3DUI);
		startTime = Time.realtimeSinceStartup;
		_limitWaitingTime = GameRoot.Instance.ConstantDataContainer.GetValueInt("MatchingBanPickWaitingLimit_Client");
		UIHelper.SetLabelText(CancelLabel, LocalizationManager.GetStringDirect("COMMON_CANCEL"));
	}

	public override void OnSystemBackButton()
	{
		if (IsMatchWaitingCanceled())
		{
			VespaDebug.LogError("MatchWaiting Close :: [ IsCancelWaiting() ] ");
			Close();
		}
		else
		{
			CancelWaiting();
		}
	}

	private bool IsMatchWaitingCanceled()
	{
		if (!SingletonUpdater<MatchManager>.instance.IsWaitingMatchPvp)
		{
			return SingletonUpdater<MatchManager>.instance.MatchInfo == null;
		}
		return false;
	}

	private bool IsMatchingSuccess()
	{
		if (!SingletonUpdater<MatchManager>.instance.IsWaitingMatchPvp)
		{
			return SingletonUpdater<MatchManager>.instance.MatchInfo != null;
		}
		return false;
	}

	public override void Close()
	{
		base.Close();
		UIHelper.EnableGameObject(SceneAutoBehaviour<LobbyManagement>.instance.GetInstance<LobbyTopmostMenu>(), enable: true);
	}

	public override void Reset()
	{
		base.Reset();
		if (Player1.renderFacade != null)
		{
			RenderFacadeBuilder.ReleaseRenderFacade(Player1.renderFacade);
			Player1.renderFacade = null;
		}
		UIHelper.EnableGameObject(CancelButton, enable: true);
		UIHelper.EnableButton(CancelButton, enable: true, grayscaleIfDisable: true);
		UIHelper.EnableGameObject(Waiting, enable: true);
		SetNewChallenger(active: false);
		ResetTween();
		isTimeOut = false;
		isWaitingOpponentExpireTime = false;
	}

	private void ResetTween()
	{
		if (!(Waiting == null))
		{
			UITweener[] componentsInChildren = Waiting.GetComponentsInChildren<UITweener>();
			foreach (UITweener obj in componentsInChildren)
			{
				obj.ResetToBeginning();
				obj.PlayForward();
			}
		}
	}

	private void Update()
	{
		if (Player1.renderFacade != null)
		{
			Player1.renderFacade.RenderGroupModifier.SetExtraAlpha(1f);
			Player1.renderFacade.ChainUpdate();
		}
		if (IsMatchingSuccess() && !isWaitingOpponentExpireTime)
		{
			isWaitingOpponentExpireTime = true;
			UIHelper.SetLabelText(CancelLabel, LocalizationManager.GetStringDirect("COMMON_WAITING"));
			UIHelper.EnableButton(CancelButton, enable: false, grayscaleIfDisable: true);
		}
		if (!isTimeOut && !isWaitingOpponentExpireTime && Time.realtimeSinceStartup - startTime > _limitWaitingTime)
		{
			isTimeOut = true;
			UIHelper.EnableButton(CancelButton, enable: false, grayscaleIfDisable: true);
			SingletonUpdater<MatchManager>.instance.CancelReserve(timeOut: true);
		}
	}

	private void SetNewChallenger(bool active)
	{
		UIHelper.EnableGameObject(Challenger, active);
		if (active)
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(TweenAlphaArray); i++)
			{
				if (!(TweenAlphaArray[i] == null))
				{
					TweenAlphaArray[i].PlayForwardReset();
				}
			}
			for (int j = 0; j < __Extension_Method_ICollection__.COUNT(TweenScaleArray); j++)
			{
				if (!(TweenScaleArray[j] == null))
				{
					TweenScaleArray[j].PlayForwardReset();
				}
			}
			return;
		}
		for (int k = 0; k < __Extension_Method_ICollection__.COUNT(TweenAlphaArray); k++)
		{
			if (!(TweenAlphaArray[k] == null))
			{
				TweenAlphaArray[k].ResetToBeginning();
			}
		}
		for (int l = 0; l < __Extension_Method_ICollection__.COUNT(TweenScaleArray); l++)
		{
			if (!(TweenScaleArray[l] == null))
			{
				TweenScaleArray[l].ResetToBeginning();
			}
		}
	}

	public void OnClickCancel()
	{
		CancelWaiting();
	}

	private void CancelWaiting()
	{
		SingletonUpdater<MatchManager>.instance.CancelReserve();
		UIHelper.EnableButton(CancelButton, enable: false, grayscaleIfDisable: true);
	}

	public void SetOpponent(MatchedAccountInfo npcAccountInfo)
	{
		UIHelper.EnableGameObject(CancelButton, enable: false);
		UIHelper.EnableGameObject(Waiting, enable: false);
		SetNewChallenger(active: true);
		if (npcAccountInfo == null)
		{
			VespaDebug.LogError("Error : npc info null");
		}
	}

	public void SetOpponent(MatchedAccountInfo[] accountInfos)
	{
		UIHelper.EnableGameObject(CancelButton, enable: false);
		UIHelper.EnableGameObject(Waiting, enable: false);
		SetNewChallenger(active: true);
		MatchedAccountInfo matchedAccountInfo = accountInfos.FirstOrDefault((MatchedAccountInfo e) => e.AccountId == GlobalAutoBehaviour<AccountManager>.instance.UserManager.Id);
		MatchedAccountInfo matchedAccountInfo2 = accountInfos.FirstOrDefault((MatchedAccountInfo e) => e.AccountId != GlobalAutoBehaviour<AccountManager>.instance.UserManager.Id);
		if (matchedAccountInfo == null || matchedAccountInfo2 == null)
		{
			VespaDebug.LogError("Error : info null");
		}
	}

	public IEnumerator PlayFlagEffect_Co()
	{
		yield return new WaitForSeconds(1f);
		Player1.Flag.Play(null);
	}
}
