using System;
using System.Collections;
using NShared;
using NShared.CheckGuildName;
using NShared.NLocalization;
using NShared.UseGuildNameChangeItem;
using NVespa.NGlobal;

namespace NGame2.NUI.NWindow;

public class GuildNameChangeMessagePopup : BaseWindow
{
	public interface Listener
	{
		void IsValidGuildName(ItemData itemData, string name, Action<NShared.CheckGuildName.ResultType> onCheck);

		void UseGuildName(ItemData itemData, string name, Action<NShared.UseGuildNameChangeItem.ResultType> onUse);

		void OpenConfirmPopup(string title, string content, Action onYes, Action onNo);
	}

	public const string PrefabPath = "Prefab2/Window/MessagePopup/GuildNameChangeMessagePopup";

	public UIInput NameInput;

	public UILabel DescLabel;

	private Listener _listener;

	private ItemData _itemData;

	public void Open(Listener listener, ItemData itemData)
	{
		Open(delegate
		{
			Init(listener, itemData);
		}, null);
	}

	public void Init(Listener listener, ItemData itemData)
	{
		_listener = listener;
		_itemData = itemData;
	}

	public override void Reset()
	{
		base.Reset();
		_listener = null;
		_itemData = null;
		if ((bool)NameInput)
		{
			NameInput.value = "";
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

	private void ShowResult(NShared.CheckGuildName.ResultType result)
	{
		if (result != NShared.CheckGuildName.ResultType.Success)
		{
			string text = LocalizationManager.GetString(result);
			UIHelper.SetLabelText(DescLabel, "[FF0000]" + text);
		}
		else
		{
			UIHelper.SetLabelText(DescLabel, LocalizationManager.GetString(Guild.GUILD_NAME_VALID_MESSAGE));
		}
	}

	public void OnClickConfirm()
	{
		if (!(NameInput == null) && _listener != null)
		{
			VespaCoroutine.Start(this, coUseGuildName());
		}
	}

	private IEnumerator coUseGuildName()
	{
		string name = NameInput.value;
		bool isComplete = false;
		NShared.CheckGuildName.ResultType validNameResult = NShared.CheckGuildName.ResultType.Fail;
		_listener.IsValidGuildName(_itemData, name, delegate(NShared.CheckGuildName.ResultType result)
		{
			isComplete = true;
			validNameResult = result;
		});
		while (!isComplete)
		{
			yield return null;
		}
		ShowResult(validNameResult);
		if (validNameResult == NShared.CheckGuildName.ResultType.Success)
		{
			string text = LocalizationManager.GetString(Guild.GUILD_NAME_VALID_MESSAGE);
			UIHelper.SetLabelText(DescLabel, text);
			string text2 = LocalizationManager.GetString(Guild.GUILD_NAME_CONFIRM_TITLE);
			string content = LocalizationManager.GetString(Guild.GUILD_NAME_CONFIRM_MESSAGE, name);
			isComplete = false;
			bool useThisName = false;
			_listener.OpenConfirmPopup(text2, content, delegate
			{
				isComplete = true;
				useThisName = true;
			}, delegate
			{
				isComplete = true;
				useThisName = false;
			});
			while (!isComplete)
			{
				yield return null;
			}
			if (useThisName)
			{
				_listener.UseGuildName(_itemData, name, UseGuildName);
			}
		}
	}

	private void UseGuildName(NShared.UseGuildNameChangeItem.ResultType result)
	{
		Close();
	}
}
