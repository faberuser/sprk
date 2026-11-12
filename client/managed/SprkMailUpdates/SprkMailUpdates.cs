using System;
using NGame2.NAccount;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow;
using NShared;
using NShared.CheckNewMail;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

public static class SprkMailUpdates
{
	private static string session;

	private static bool pending;

	private static bool inFlight;

	private static float nextPoll;

	private static float nextPush;

	private static float requested;

	public static void Arrived()
	{
		pending = true;
	}

	public static void Tick(LobbyManagement lobby)
	{
		AccountManager instance = GlobalAutoBehaviour<AccountManager>.instance;
		WebServiceManager instance2 = SingletonUpdater<WebServiceManager>.instance;
		if (instance == null || !instance.LoginResponseComplete || instance2.Requester == null || string.IsNullOrEmpty(instance2.Requester.SessionKey))
		{
			return;
		}
		if (session != instance2.Requester.SessionKey)
		{
			session = instance2.Requester.SessionKey;
			pending = true;
			inFlight = false;
			nextPoll = (nextPush = 0f);
		}
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		if ((inFlight && realtimeSinceStartup - requested < 30f) || (realtimeSinceStartup < nextPoll && (!pending || realtimeSinceStartup < nextPush)))
		{
			return;
		}
		pending = false;
		inFlight = true;
		requested = realtimeSinceStartup;
		nextPoll = realtimeSinceStartup + 60f;
		nextPush = realtimeSinceStartup + 1f;
		try
		{
			lobby.RequestCheckNewMailArrived();
		}
		catch (Exception ex)
		{
			inFlight = false;
			nextPoll = realtimeSinceStartup + 15f;
			Debug.LogWarning("SPRK mailbox refresh: " + ex.GetType().Name);
		}
	}

	public static void Finished(NShared.CheckNewMail.Response response)
	{
		inFlight = false;
		if (response != null && response.BaseResult == BaseResultType.Success && response.Result == ResultType.Success && (response.NewMailArrived || response.TotalMailCount != MailManagement.MailCount) && GlobalAutoBehaviour<GlobalManagement>.isValidInstance && GlobalAutoBehaviour<GlobalManagement>.instance.WindowOpened<MailListPopup>() && SceneAutoBehaviour<MailManagement>.isValidInstance)
		{
			SingletonAuto<MailInfoContainer>.instance.UpdateMail();
		}
	}
}
