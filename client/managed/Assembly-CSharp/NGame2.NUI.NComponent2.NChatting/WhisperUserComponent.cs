using NGame2.NUI.NManager.NLobby;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NChatting;

public class WhisperUserComponent : LinkFactoryComponent, UICamera.IOnClickListener
{
	public interface Listener
	{
		void OnClick(WhisperUserComponent component);

		void OnClose(WhisperUserComponent component);
	}

	public static PrefabAuto<WhisperUserComponent> PrefabAuto = new PrefabAuto<WhisperUserComponent>("Prefab2/Component/Chatting/WhisperUserComponent");

	public static PrefabAuto<WhisperUserComponent> BattlePrefabAuto = new PrefabAuto<WhisperUserComponent>("Prefab2/Component/BattleChatting/BattleWhisperUserComponent");

	public UILabel NameLabel;

	public GameObject SelectedObject;

	public GameObject AlertObject;

	public UILabel AlertCountLabel;

	private Listener _listener;

	public long AccountId { get; private set; }

	public string UserName { get; private set; }

	public int AlertCount { get; private set; }

	public static PrefabAuto<WhisperUserComponent> GetPrefab()
	{
		if (SceneAutoBehaviour<LobbyManagement>.isValidInstance)
		{
			return PrefabAuto;
		}
		return BattlePrefabAuto;
	}

	public bool Open(long accountId, string userName, Listener listener)
	{
		AccountId = accountId;
		UserName = userName;
		_listener = listener;
		UIHelper.SetLabelText(NameLabel, userName);
		SetSelected(selected: false);
		AlertCount = -1;
		return true;
	}

	public void OnClick()
	{
		if (_listener != null)
		{
			_listener.OnClick(this);
		}
	}

	public void SetSelected(bool selected)
	{
		UIHelper.EnableGameObject(SelectedObject, selected);
	}

	public void SetAlertCount(int count)
	{
		UIHelper.EnableGameObject(AlertObject, count > 0);
		UIHelper.SetLabelText(AlertCountLabel, count);
		AlertCount = count;
	}

	public void OnClickClose()
	{
		if (_listener != null)
		{
			_listener.OnClose(this);
		}
	}
}
