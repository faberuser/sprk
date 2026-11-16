using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using MiniJSON;
using NGame2.NAccount;
using NGame2.NUI.NWindow;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using NVespa.NWeb;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Offers a new player, before the lobby loads, to skip the scripted opening
/// (prologue through the guided 1-3 battle). The server decides who is offered
/// the skip and completes the opening tutorials with their normal rewards.
/// </summary>
public static class SprkOpeningSkip
{
	// Matches the server's last opening tutorial (the Special Shop unlock at node 1-4).
	private const int OpeningLastTutorial = 10200;

	// The node where the skipped opening leaves the player, as after walking there.
	private const int StartChapter = 1;

	private const int StartNode = 4;

	private static bool offered;

	public static void OnLogin(NShared.Login.Response response)
	{
		offered = response != null && response.SprkOpeningSkipOffer;
	}

	public static IEnumerator Offer()
	{
		AccountManager account = GlobalAutoBehaviour<AccountManager>.instance;
		if (!offered || account == null || account.IsTutorialSkip || account.TutorialManager.IsComplete(OpeningLastTutorial))
		{
			offered = false;
			yield break;
		}
		offered = false;
		int choice = 0;
		string title = SprkText.Get("Skip Tutorial");
		MessagePopup.Open(title, SprkText.Get("Skip the opening tutorial and continue from Chapter 1-3?\nYou keep every hero and reward it gives."), delegate
		{
			choice = 1;
		}, delegate
		{
			choice = 2;
		});
		AddNoButton(title);
		while (choice == 0)
		{
			yield return null;
		}
		if (choice != 1)
		{
			yield break;
		}
		IList responses = null;
		object waiter = new object();
		WaitingPopup.Open(waiter);
		WebServiceRequester requester = SingletonUpdater<WebServiceManager>.instance.Requester;
		try
		{
			string body = "SessionKey=" + UnityWebRequest.EscapeURL(requester.SessionKey);
			using UnityWebRequest request = new UnityWebRequest(requester.Host.TrimEnd('/') + "/tutorial/skip_opening", "POST");
			request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
			request.downloadHandler = new DownloadHandlerBuffer();
			request.timeout = 30;
			request.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
			yield return request.SendWebRequest();
			if (!request.isNetworkError && !request.isHttpError && Json.Deserialize(request.downloadHandler.text) is IDictionary reply && Convert.ToString(reply["Result"]) == "Success")
			{
				responses = reply["Responses"] as IList;
			}
		}
		finally
		{
			WaitingPopup.Close(waiter);
		}
		if (responses == null || !Apply(account, responses))
		{
			bool closed = false;
			GlobalMessagePopup.OpenGlobal(SprkText.Get("Skip Tutorial"), SprkText.Get("The tutorial could not be skipped. It will start normally."), delegate
			{
				closed = true;
			}, retryButton: false);
			while (!closed)
			{
				yield return null;
			}
		}
	}

	/// <summary>
	/// This build's message popup prefab has only a Yes button (No is the X), and
	/// its two-button prefab is missing. Split Yes in half and add a cloned No.
	/// </summary>
	private static void AddNoButton(string title)
	{
		MessagePopup popup = null;
		foreach (MessagePopup candidate in UnityEngine.Object.FindObjectsOfType<MessagePopup>())
		{
			if (candidate.gameObject.activeInHierarchy && candidate.Title == title)
			{
				popup = candidate;
			}
		}
		if (popup == null || popup.ButtonYes == null || (popup.ButtonNo != null && popup.ButtonNo.gameObject.activeInHierarchy))
		{
			return;
		}
		GameObject yes = popup.ButtonYes.gameObject;
		GameObject no = UnityEngine.Object.Instantiate(yes, yes.transform.parent, worldPositionStays: false);
		no.name = "Button_No";
		UIButton button = no.GetComponent<UIButton>();
		button.onClick = new List<EventDelegate> { new EventDelegate(popup.OnClickNo) };
		UILabel label = no.GetComponentInChildren<UILabel>(includeInactive: true);
		if (label != null)
		{
			label.text = LocalizationManager.GetString(NShared.NLocalization.Common.COMMON_NO);
		}
		UIWidget yesWidget = yes.GetComponent<UIWidget>();
		UIWidget noWidget = no.GetComponent<UIWidget>();
		if (yesWidget == null || noWidget == null)
		{
			return;
		}
		const int gap = 24;
		int half = (yesWidget.width - gap) / 2;
		float offset = (half + gap) / 2f;
		Vector3 center = yes.transform.localPosition;
		foreach (UIWidget widget in new[] { yesWidget, noWidget })
		{
			widget.SetAnchor((GameObject)null);
			widget.width = half;
			NGUITools.UpdateWidgetCollider(widget.gameObject);
		}
		// No on the left, Yes on the right, matching the game's other confirmations.
		no.transform.localPosition = center - new Vector3(offset, 0f, 0f);
		yes.transform.localPosition = center + new Vector3(offset, 0f, 0f);
	}

	/// <summary>Applies each completion receipt exactly as the tutorial would have.</summary>
	private static bool Apply(AccountManager account, IList responses)
	{
		if (!GlobalAutoBehaviour<AccountManager>.isValidInstance || (object)account != GlobalAutoBehaviour<AccountManager>.instance)
		{
			return false;
		}
		ChapterDungeonInfo last = null;
		foreach (object item in responses)
		{
			if (!(item is IDictionary data))
			{
				return false;
			}
			NShared.CompleteTutorial.Response response = JM_NShared_CompleteTutorial_Response.Parse(data);
			if (response == null || response.BaseResult != BaseResultType.Success || response.Result != NShared.CompleteTutorial.ResultType.Success || response.Info == null)
			{
				return false;
			}
			account.TutorialManager.Apply(response.Info);
			account.UserManager.Apply(response.CurrencyResults);
			account.CampaignManager.Apply(response.DungeonInfos);
			account.ItemManager.Apply(response.ItemResults);
			account.EquipItemManager.Apply(response.EquipItemResults);
			account.ApplyAddHeroForTutorial(response.HeroInfos, response.TeamExpResultInfos, reserveWindow: false);
			account.UserManager.Apply(response.StaminaResultInfos);
			if (response.DungeonInfos != null && response.DungeonInfos.Length > 0)
			{
				last = response.DungeonInfos[response.DungeonInfos.Length - 1];
			}
		}
		if (last != null)
		{
			account.CampaignManager.SaveLatelyBattleNodePos(last.ChapterIndex, last.DungeonIndex);
		}
		if (!account.TutorialManager.IsComplete(OpeningLastTutorial))
		{
			return false;
		}
		// The lobby places the player at the stored node; without this it stays at Orvel.
		NGame2.NAccount.UserCampaignManager.SetChapterDungeonIndex(StartChapter, StartNode, isForce: true);
		return true;
	}
}
