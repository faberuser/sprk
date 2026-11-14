using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NGame2;
using NGame2.NAccount;
using NGame2.NBattleContext;
using NGame2.NBattleField.NManager.NPlay;
using NGame2.NParty;
using NGame2.NSocket;
using NGame2.NUI;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NShared;
using NShared.BeginCampaign;
using NShared.NLocalization;
using NShared.NMessage.NProtocol;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using NVespa.NSocket;
using UnityEngine;

public static class SprkPartyRetry
{
	private static MessagePopup3Button votePopup;

	private static int openVote;

	private static CampaignContext Context()
	{
		if (!SingletonAuto<BattleContext>.isValidInstance)
		{
			return null;
		}
		return SingletonAuto<BattleContext>.instance.DataProvider as CampaignContext;
	}

	private static bool Eligible(CampaignContext context)
	{
		if (context == null)
		{
			return false;
		}
		try
		{
			return SprkPartyRetryPolicy.Eligible(context.IsMultiplay(), (int)((IDataProvider)context).GetBattleType());
		}
		catch (Exception)
		{
			return false;
		}
	}

	public static EBattleMenu Menu(EBattleMenu menu, CampaignContext context)
	{
		if (!Eligible(context))
		{
			return menu;
		}
		return menu | EBattleMenu.PartyExitButton;
	}

	private static string Text(string key, string fallback)
	{
		try
		{
			if (LocalizationManager.GetStringDirect(key, out var value) && !string.IsNullOrEmpty(value))
			{
				return value;
			}
		}
		catch (Exception)
		{
		}
		return fallback;
	}

	private static string Failure(string code)
	{
		try
		{
			if (Enum.TryParse<ResultType>(code, out var result))
			{
				return LocalizationManager.GetString(result);
			}
		}
		catch (Exception)
		{
		}
		return code;
	}

	private static void Notice(string message)
	{
		try
		{
			CenterMessage.Open(message);
		}
		catch (Exception)
		{
		}
	}

	private static MessagePopup3Button Popup(string title, string message, Action first, Action second, string firstLabel, string secondLabel, int seconds)
	{
		WindowPrefab<MessagePopup3Button> windowPrefab = new WindowPrefab<MessagePopup3Button>("Resources/Prefab2/Window/MessagePopup/MessagePopup3Button_Battle", GameUIRoot.ELayerType.UI);
		MessagePopup3Button instance = windowPrefab.instance;
		typeof(MessagePopup3Button).GetMethod("OpenInternal", BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[7]
		{
			typeof(string),
			typeof(string),
			typeof(Action),
			typeof(Action),
			typeof(string),
			typeof(string),
			typeof(bool)
		}, null).Invoke(instance, new object[7] { title, message, first, second, firstLabel, secondLabel, true });
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
		if (seconds > 0)
		{
			typeof(MessagePopup3Button).GetField("_remainedTime", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(instance, (float)seconds);
		}
		return instance;
	}

	private static void Send(string message)
	{
		BattleSocketConnector battleSocketConnector = Connector();
		if (battleSocketConnector == null || battleSocketConnector.State != ConnectState.Connected)
		{
			Notice(SprkText.Get("Retry is not available right now."));
			return;
		}
		battleSocketConnector.Request<BattleMessageReq, BattleMessageRes>(new BattleMessageReq
		{
			Message = message
		}, delegate
		{
		});
	}

	private static BattleSocketConnector Connector()
	{
		return GlobalAutoBehaviour<SocketServiceManager>.instance.BattleConnector;
	}

	public static bool OnExit()
	{
		if (!Eligible(Context()))
		{
			return false;
		}
		Popup(Text("COMMON_NOTIFICATION", SprkText.Get("Notice")), SprkText.Get("Retry this battle with your party, or exit to the party room?\nRetry starts when every player agrees."), delegate
		{
			Send(SprkPartyRetryPolicy.Propose());
		}, delegate
		{
			MessagePopup3Button.Open_BattlePopup(Text("COMMON_NOTIFICATION", SprkText.Get("Notice")), SprkText.Get("Leave this battle and return to the party room?\nYour heroes keep fighting under AI control."), ExitToRoom, null, NShared.NLocalization.Common.COMMON_CONFIRM, NShared.NLocalization.Common.COMMON_CANCEL);
		}, SprkText.Get("Retry"), Text("COMMON_EXIT", SprkText.Get("Exit")), 0);
		return true;
	}

	private static void ExitToRoom()
	{
		bool left = false;
		Action leave = delegate
		{
			if (!left)
			{
				left = true;
				CloseVote();
				Context()?.SetOnlineBattleCallbackIsBattleEnd(IsBattleEnd: true);
				BattleSocketConnector battleSocketConnector2 = Connector();
				if (battleSocketConnector2 != null && battleSocketConnector2.State == ConnectState.Connected)
				{
					battleSocketConnector2.Close();
				}
				SingletonAuto<BattleContext>.instance.MoveSceneToLobby(null, EBoolCheck.DontCare);
			}
		};
		BattleSocketConnector battleSocketConnector = Connector();
		if (battleSocketConnector == null || battleSocketConnector.State != ConnectState.Connected)
		{
			leave();
			return;
		}
		battleSocketConnector.Request<WithdrawBattleReq, WithdrawBattleRes>(new WithdrawBattleReq(), delegate
		{
			leave();
		});
		GlobalAutoBehaviour<PartyManager>.instance.StartCoroutine(After(3f, leave));
	}

	private static IEnumerator After(float seconds, Action action)
	{
		yield return new WaitForSecondsRealtime(seconds);
		action();
	}

	public static bool KeepOpen(MessagePopup3Button popup)
	{
		if (popup != null && votePopup != null)
		{
			return (object)popup == votePopup;
		}
		return false;
	}

	private static void PassThroughOutside(MessagePopup3Button popup)
	{
		List<GameObject> list = new List<GameObject>();
		Transform[] componentsInChildren = popup.GetComponentsInChildren<Transform>(includeInactive: true);
		foreach (Transform transform in componentsInChildren)
		{
			if (transform.name == "Outside_ClickClose" && transform.gameObject.activeSelf)
			{
				list.Add(transform.gameObject);
			}
		}
		foreach (GameObject item in list)
		{
			item.SetActive(value: false);
		}
		if (list.Count > 0)
		{
			GlobalAutoBehaviour<PartyManager>.instance.StartCoroutine(ShowOnClose(popup, list));
		}
	}

	private static IEnumerator ShowOnClose(MessagePopup3Button popup, List<GameObject> hidden)
	{
		while (popup != null && popup.IsOpen())
		{
			yield return null;
		}
		foreach (GameObject item in hidden)
		{
			if (item != null)
			{
				item.SetActive(value: true);
			}
		}
	}

	private static void CloseVote()
	{
		try
		{
			if (votePopup != null && votePopup.IsOpen())
			{
				votePopup.Close();
			}
		}
		catch (Exception)
		{
		}
		votePopup = null;
		openVote = 0;
	}

	public static bool OnMessage(BattleMessageNot data)
	{
		if (data == null)
		{
			return false;
		}
		SprkPartyRetryPolicy.Notice notice = SprkPartyRetryPolicy.Parse(data.Type, data.Content);
		if (notice.Kind == SprkPartyRetryPolicy.Kind.None)
		{
			return data.Type == "SprkRetry";
		}
		switch (notice.Kind)
		{
		case SprkPartyRetryPolicy.Kind.Vote:
		{
			CloseVote();
			if (notice.Proposer == GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AccountId)
			{
				Notice(SprkText.Get("Retry requested. Waiting for the other players to agree."));
				break;
			}
			int vote = notice.Vote;
			openVote = vote;
			votePopup = Popup(Text("COMMON_NOTIFICATION", SprkText.Get("Notice")), SprkText.Format("{0} wants to retry this battle.\nDo you want to retry?", notice.Who), delegate
			{
				openVote = 0;
				votePopup = null;
				Send(SprkPartyRetryPolicy.Answer(vote, yes: true));
			}, delegate
			{
				openVote = 0;
				votePopup = null;
				Send(SprkPartyRetryPolicy.Answer(vote, yes: false));
			}, LocalizationManager.GetString(NShared.NLocalization.Common.COMMON_YES), LocalizationManager.GetString(NShared.NLocalization.Common.COMMON_NO), notice.Seconds);
			try
			{
				PassThroughOutside(votePopup);
			}
			catch (Exception)
			{
			}
			break;
		}
		case SprkPartyRetryPolicy.Kind.Cancel:
			if (openVote == 0 || notice.Vote == 0 || notice.Vote == openVote)
			{
				CloseVote();
			}
			Notice(SprkPartyRetryPolicy.CancelMessage(notice, Text, Failure));
			break;
		case SprkPartyRetryPolicy.Kind.Restart:
			CloseVote();
			Restart(notice.Start);
			break;
		}
		return true;
	}

	private static void Restart(string json)
	{
		PartyBattleStart partyBattleStart = JsonMarshaler.Parse<PartyBattleStart>(json);
		Notice(SprkText.Get("All players agreed. Restarting the battle."));
		Context()?.SetOnlineBattleCallbackIsBattleEnd(IsBattleEnd: true);
		BattleSocketConnector battleSocketConnector = Connector();
		if (battleSocketConnector != null && battleSocketConnector.State == ConnectState.Connected)
		{
			battleSocketConnector.Close();
		}
		typeof(PartyManager).GetMethod("Handle", BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[2]
		{
			typeof(long),
			typeof(PartyBattleStart)
		}, null).Invoke(GlobalAutoBehaviour<PartyManager>.instance, new object[2] { 0L, partyBattleStart });
	}
}
