using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NBattle;
using NShared;
using NShared.ReceiveAllMail;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NSingleton;
using UnityEngine;

public static class SprkMailClaimAll
{
	private const int BatchSize = 50;

	private static bool active;

	private static bool waiting;

	private static bool cancelled;

	private static int run;

	private static float requested;

	private static string session;

	private static MailManagement manager;

	private static MailListPopup popup;

	private static readonly List<MailReceiveResult> receipts = new List<MailReceiveResult>();

	public static Dictionary<string, object> Encode(Dictionary<string, object> fields)
	{
		fields["ClaimUntilFull"] = "true";
		return fields;
	}

	private static string CurrentSession()
	{
		WebServiceManager instance = SingletonUpdater<WebServiceManager>.instance;
		if (instance.Requester != null)
		{
			return instance.Requester.SessionKey;
		}
		return null;
	}

	public static void Caption(MailListPopup window)
	{
		UILocalizeEx[] componentsInChildren = window.GetComponentsInChildren<UILocalizeEx>(includeInactive: true);
		foreach (UILocalizeEx uILocalizeEx in componentsInChildren)
		{
			if (uILocalizeEx.LocalizeLink != null && !(uILocalizeEx.LocalizeLink.Key != "MAIL_LIMITED_MESSAGE"))
			{
				uILocalizeEx.enabled = false;
				UILabel component = uILocalizeEx.GetComponent<UILabel>();
				if (component != null)
				{
					component.text = (active ? ("Claiming mail... " + receipts.Count + " claimed") : "Claims continue until inventory is full.");
				}
			}
		}
	}

	public static void Start(MailManagement owner, MailListPopup window, int totalCount)
	{
		if (!active && !(window == null) && !string.IsNullOrEmpty(CurrentSession()))
		{
			manager = owner;
			popup = window;
			session = CurrentSession();
			active = true;
			waiting = (cancelled = false);
			run++;
			receipts.Clear();
			if (owner.GetComponent<SprkMailClaimDriver>() == null)
			{
				owner.gameObject.AddComponent<SprkMailClaimDriver>();
			}
			Caption(window);
			if (window.ReceiveAllButton != null)
			{
				window.ReceiveAllButton.isEnabled = false;
			}
		}
	}

	public static void Cancel(MailListPopup window)
	{
		if (active && window == popup)
		{
			cancelled = true;
		}
	}

	public static void Pump()
	{
		if (!active)
		{
			return;
		}
		if (CurrentSession() != session)
		{
			Abandon();
			return;
		}
		if (popup != null && popup.ReceiveAllButton != null)
		{
			popup.ReceiveAllButton.isEnabled = false;
		}
		if (waiting)
		{
			if (Time.realtimeSinceStartup - requested >= 60f)
			{
				Finish("The mail request timed out. Reopen the mailbox; relog if reward counts have not refreshed.");
			}
			return;
		}
		if (cancelled || popup == null || !popup.IsOpen())
		{
			Finish(null);
			return;
		}
		waiting = true;
		requested = Time.realtimeSinceStartup;
		int generation = run;
		try
		{
			NShared.ReceiveAllMail.Request request = new NShared.ReceiveAllMail.Request
			{
				MaxCount = 50
			};
			bool completed = false;
			SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.ReceiveAllMail.Response response)
			{
				if (!completed)
				{
					completed = true;
					if (active && generation == run)
					{
						if (CurrentSession() != session)
						{
							Abandon();
						}
						else
						{
							waiting = false;
							if (response != null && response.BaseResult == BaseResultType.Success && response.Result == ResultType.Success)
							{
								try
								{
									MailReceiveResult[] array = response.MailReceiveResults ?? new MailReceiveResult[0];
									MailReceiveResult[] array2 = array;
									foreach (MailReceiveResult mailReceiveResult in array2)
									{
										GlobalAutoBehaviour<AccountManager>.instance.ApplyReceiveMail(mailReceiveResult);
										receipts.Add(mailReceiveResult);
									}
									Caption(popup);
									if (!string.IsNullOrEmpty(response.InternalErrorMessage))
									{
										Finish(response.InternalErrorMessage);
									}
									else if (array.Length < 50 || cancelled)
									{
										Finish(null);
									}
									return;
								}
								catch (Exception ex2)
								{
									Debug.LogWarning("SPRK mail claim application: " + ex2.GetType().Name);
									Finish("Mail rewards were received. Please relog to refresh inventory counts before continuing.");
									return;
								}
							}
							Finish("Claiming stopped because the request failed. Remaining mail has been kept.");
						}
					}
				}
			});
		}
		catch (Exception ex)
		{
			Debug.LogWarning("SPRK mail claim request: " + ex.GetType().Name);
			waiting = false;
			Finish("Claiming stopped because the request could not be sent. Remaining mail has been kept.");
		}
	}

	private static void RestoreButton()
	{
		if (!(popup == null))
		{
			if (popup.ReceiveAllButton != null)
			{
				popup.ReceiveAllButton.isEnabled = true;
			}
			Caption(popup);
		}
	}

	private static void Abandon()
	{
		active = (waiting = false);
		run++;
		receipts.Clear();
		RestoreButton();
	}

	private static void Finish(string reason)
	{
		active = (waiting = false);
		RestoreButton();
		SingletonAuto<MailInfoContainer>.instance.OnReceiveAll();
		bool flag = !string.IsNullOrEmpty(reason) && reason.IndexOf("Equipment bag is full", StringComparison.Ordinal) >= 0;
		if (receipts.Count > 0)
		{
			string title = "Claimed " + receipts.Count + " mails" + (flag ? " (inventory full)" : "");
			BaseRewardPopup.PopupOpenData popupOpenData = new BaseRewardPopup.PopupOpenData().SetTitle(title);
			foreach (MailReceiveResult receipt in receipts)
			{
				popupOpenData.AddItemResultInfos(receipt.ItemResults).AddEquipResultInfos(receipt.EquipItemResults).AddStaminaResultInfos(receipt.StaminaResults)
					.AddGuildPointResult(receipt.GuildPointResult)
					.AddCurrencyResultInfos3(receipt.CurrencyResults)
					.AddPetInfo(receipt.PetResult);
			}
			popupOpenData.IsSortForMail = true;
			manager.rewardPopup = BaseRewardPopup.OpenPopup(popupOpenData, GlobalAutoBehaviour<GlobalManagement>.instance.WindowOpenedUI3DUI());
		}
		if (!string.IsNullOrEmpty(reason) && (!flag || receipts.Count == 0))
		{
			MessagePopup.Open("Mailbox", reason);
		}
		receipts.Clear();
	}
}
