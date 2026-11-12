using System;
using System.Collections;
using NGame2.NAccount;
using NGame2.NLogin;
using NGame2.NUI.NManager;
using NShared;
using NShared.NLocalization;
using NShared.QueryNick;
using NShared.UseNickChangeItem;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow;

public class NickChangeMessagePopup : BaseWindow
{
	public const string PrefabPath = "Prefab2/Window/MessagePopup/NickChangeMessagePopup";

	public UIInput NickInput;

	public UILabel DescLabel;

	private ItemData _itemData;

	public static void Open(ItemData itemData)
	{
		WindowPrefab<NickChangeMessagePopup> windowPrefab = new WindowPrefab<NickChangeMessagePopup>("Prefab2/Window/MessagePopup/NickChangeMessagePopup", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(itemData);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	public void OpenInternal(ItemData itemData)
	{
		base.Open();
		Init(itemData);
	}

	public void Init(ItemData itemData)
	{
		_itemData = itemData;
	}

	public override void Reset()
	{
		base.Reset();
		_itemData = null;
		if ((bool)NickInput)
		{
			NickInput.value = "";
		}
	}

	public override void OnSystemBackButton()
	{
		OnClickClose();
	}

	public void OnClickClose()
	{
		Close();
	}

	private void ShowResult(NShared.QueryNick.ResultType result)
	{
		if (result != NShared.QueryNick.ResultType.Success)
		{
			string text = LocalizationManager.GetString(result);
			UIHelper.SetLabelText(DescLabel, "[FF0000]" + text);
		}
		else
		{
			UIHelper.SetLabelText(DescLabel, LocalizationManager.GetString(Nick.NICK_VALID_MESSAGE));
		}
	}

	public void OnClickConfirm()
	{
		if (!(NickInput == null))
		{
			VespaCoroutine.Start(this, coUseNick());
		}
	}

	private IEnumerator coUseNick()
	{
		string nick = NickInput.value;
		bool isComplete = false;
		NShared.QueryNick.ResultType validNickResult = NShared.QueryNick.ResultType.Fail;
		IsValidNick(_itemData, nick, delegate(NShared.QueryNick.ResultType result)
		{
			isComplete = true;
			validNickResult = result;
		});
		while (!isComplete)
		{
			yield return null;
		}
		ShowResult(validNickResult);
		if (validNickResult == NShared.QueryNick.ResultType.Success)
		{
			string text = LocalizationManager.GetString(Nick.NICK_VALID_MESSAGE);
			UIHelper.SetLabelText(DescLabel, text);
			string text2 = LocalizationManager.GetString(Nick.NICK_CONFIRM_TITLE);
			string content = LocalizationManager.GetString(Nick.NICK_CONFIRM_MESSAGE, nick);
			isComplete = false;
			bool useThisNick = false;
			OpenConfirmPopup(text2, content, delegate
			{
				isComplete = true;
				useThisNick = true;
			}, delegate
			{
				isComplete = true;
				useThisNick = false;
			});
			while (!isComplete)
			{
				yield return null;
			}
			if (useThisNick)
			{
				UseNick(_itemData, nick, OnUseNick);
			}
		}
	}

	private void IsValidNick(ItemData itemData, string nick, Action<NShared.QueryNick.ResultType> onCheck)
	{
		NShared.QueryNick.ResultType resultType = SingletonAuto<NickChecker>.instance.Check(nick);
		if (resultType != NShared.QueryNick.ResultType.Success)
		{
			Vespa_System_Action_Extension_Method.fire(onCheck, resultType);
			return;
		}
		NShared.QueryNick.Request request = new NShared.QueryNick.Request
		{
			Nick = nick
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.QueryNick.Response response)
		{
			if (response == null || response.BaseResult != BaseResultType.Success)
			{
				Vespa_System_Action_Extension_Method.fire(onCheck, NShared.QueryNick.ResultType.Fail);
			}
			else
			{
				Vespa_System_Action_Extension_Method.fire(onCheck, response.Result);
			}
		});
	}

	private void UseNick(ItemData itemData, string nick, Action<NShared.UseNickChangeItem.ResultType> onUse)
	{
		if (itemData == null)
		{
			Vespa_System_Action_Extension_Method.fire(onUse, NShared.UseNickChangeItem.ResultType.Fail);
			return;
		}
		NShared.UseNickChangeItem.Request request = new NShared.UseNickChangeItem.Request
		{
			Nick = nick,
			ItemIndex = itemData.Index
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.UseNickChangeItem.Response response)
		{
			if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else if (response.Result != NShared.UseNickChangeItem.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				if (GlobalAutoBehaviour<AccountManager>.isValidInstance)
				{
					GlobalAutoBehaviour<AccountManager>.instance.UserManager.SetNick(nick);
					GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResult);
				}
				Vespa_System_Action_Extension_Method.fire(onUse, response.Result);
				CenterMessage.Open(LocalizationManager.GetString(Item.USE_NICK_CHANGE_MSG, nick));
			}
		});
	}

	private void OpenConfirmPopup(string title, string content, Action onYes, Action onNo)
	{
		LoginMethodType lastLoginMethod = LoginData.LastLoginMethod;
		NickMessagePopup.Open(title, content, lastLoginMethod, onYes, onNo);
	}

	private void OnUseNick(NShared.UseNickChangeItem.ResultType result)
	{
		Close();
	}
}
