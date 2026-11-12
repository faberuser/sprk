using System;
using NGame2.NAccount;
using NGame2.NUI.NWindow;
using NShared;
using NShared.UseBoosterItem;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using NVespa.NWeb;

public static class SprkHonorBuff
{
	private static bool waiting;

	public static bool TrySelect(ItemData item, int count, Action onComplete)
	{
		if (item == null || item.Index < 110033 || item.Index > 110035)
		{
			return false;
		}
		if (waiting)
		{
			return true;
		}
		if (count < 1 || count > 1000)
		{
			MessagePopup.Open("Server Buff", "Select between 1 and 1,000 items.");
			return true;
		}
		int num = ((item.Index == 110033) ? 5 : ((item.Index == 110034) ? 3 : 2));
		IconConfirmPopup.Open("Use " + count + " " + item.Name + "? This adds " + num * count + "% gold gains for all players until the next Honor rewards calculation.", "Yes", "No", delegate
		{
			Use(item.Index, count, onComplete);
		}).AddItem(item.Index, count);
		return true;
	}

	private static void Use(int index, int count, Action onComplete)
	{
		if (waiting)
		{
			return;
		}
		WebServiceRequester requester = SingletonUpdater<WebServiceManager>.instance.Requester;
		if (requester == null || string.IsNullOrEmpty(requester.SessionKey))
		{
			return;
		}
		string session = requester.SessionKey;
		waiting = true;
		bool completed = false;
		try
		{
			requester.Request(new NShared.UseBoosterItem.Request
			{
				ItemIndex = index,
				ItemCount = count
			}, delegate(NShared.UseBoosterItem.Response response)
			{
				if (!completed)
				{
					completed = true;
					waiting = false;
					if (SingletonUpdater<WebServiceManager>.instance.Requester == requester && !(requester.SessionKey != session))
					{
						if (response == null || response.BaseResult != BaseResultType.Success || response.Result != ResultType.Success)
						{
							MessagePopup.Open("Server Buff", "Activation was not confirmed. Reopen the inventory to check your item count before retrying.");
						}
						else
						{
							GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResult);
							if (onComplete != null)
							{
								onComplete();
							}
							MessagePopup.Open("League of Honor Server Buff", string.IsNullOrEmpty(response.InternalErrorMessage) ? "The server buff was activated." : response.InternalErrorMessage);
						}
					}
				}
			});
		}
		catch (Exception)
		{
			waiting = false;
			MessagePopup.Open("Server Buff", "Unable to send the request. Reopen the inventory and try again.");
		}
	}
}
