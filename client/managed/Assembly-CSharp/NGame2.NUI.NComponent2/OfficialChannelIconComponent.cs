using NGame2.NContentsDefine;
using NGame2.NLogin;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow;
using NShared;
using NShared.NContentsDefine;
using NVespa;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NComponent2;

public class OfficialChannelIconComponent : LinkFactoryComponent
{
	public const string PrefabPath = "Resources/Prefab2/Lobby/OfficialChannelIconComponent";

	public static PrefabAuto<OfficialChannelIconComponent> PrefabAuto = new PrefabAuto<OfficialChannelIconComponent>("Resources/Prefab2/Lobby/OfficialChannelIconComponent");

	public static LocalStorageInt LastPatchNoteIndex = new LocalStorageInt(0, "OfficialChannelIconComponent_LastPatchNoteIndex");

	public UISprite NewMarkSprite;

	private int _patchNoteIndex;

	public static OfficialChannelIconComponent CreateComponent(UITable table, UIScrollView scrollView)
	{
		OfficialChannelIconComponent officialChannelIconComponent = PrefabAuto.Factory.Create(table.transform);
		if (officialChannelIconComponent == null)
		{
			return null;
		}
		officialChannelIconComponent.Open();
		return officialChannelIconComponent;
	}

	public void Open()
	{
		_patchNoteIndex = NGame2.NContentsDefine.OfficialChannel.GetPatchNoteIndex();
		bool flag = false;
		if (_patchNoteIndex > 0 && _patchNoteIndex > LastPatchNoteIndex.Value)
		{
			flag = true;
		}
		string serverName = SingletonUpdater<LoginManager>.instance.ServerName;
		string selectedServer;
		NShared.NContentsDefine.OfficialChannel.Setting setting = ContentsDefine.OfficialChannel.GetSetting(serverName, out selectedServer);
		UIHelper.EnableGameObject(NewMarkSprite, flag && setting.NewMarkInLobbyButton);
	}

	public void OnButtonClick()
	{
		SceneAutoBehaviour<LobbyManagement>.instance.GetInstance<OfficialChannelWindow>().Open();
		if (_patchNoteIndex > 0 && LastPatchNoteIndex.Value < _patchNoteIndex)
		{
			LastPatchNoteIndex.Value = _patchNoteIndex;
		}
		UIHelper.EnableGameObject(NewMarkSprite, enable: false);
	}
}
