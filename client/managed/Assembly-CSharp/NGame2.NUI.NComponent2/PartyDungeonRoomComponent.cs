using NGame2.NUI.NManager.NLobby;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NComponent2;

public class PartyDungeonRoomComponent : LinkFactoryComponent, NVespa.NPrefab2.InitOnCreate
{
	public interface Listener
	{
		void OnClickEnterDungeon(RaidRoomInfo roomInfo);
	}

	public const string PrefabPath = "Prefab2/Component/PartyDungeon/PartyDungeonRoomComponent";

	public static PrefabAuto<PartyDungeonRoomComponent> PrefabAuto = new PrefabAuto<PartyDungeonRoomComponent>("Prefab2/Component/PartyDungeon/PartyDungeonRoomComponent");

	public UIBundleLoader PortraitBundleTex;

	public UILabel Label_Title;

	public UILabel Label_RoomLeader;

	public UILabel Label_RoomMemberCount;

	public UILabel Label_DungeonType;

	public UIButton EnterRaid;

	private Listener _Listener;

	private RaidRoomInfo _roomInfo;

	void NVespa.NPrefab2.InitOnCreate.OnCreate()
	{
	}

	void NVespa.NPrefab2.InitOnCreate.OnRelease()
	{
		ClosePortrait();
	}

	public void Open(RaidRoomInfo roomInfo, Listener listener, bool enterAble)
	{
		_roomInfo = roomInfo;
		_Listener = listener;
		if (roomInfo == null)
		{
			VespaDebug.LogError("roomInfo == null");
			return;
		}
		PartyDungeonData data = GameRoot.Instance.PartyDungeonDataContainer.GetData(_roomInfo.ChapterIndex, _roomInfo.DungeonIndex);
		if (data == null)
		{
			VespaDebug.LogError("partyDungeonData == null / chapterIndex : " + _roomInfo.ChapterIndex + " / dungeonIndex = " + _roomInfo.DungeonIndex);
			return;
		}
		SetMemberCount(_roomInfo.CurPartyMember, data.PlayerCount);
		string title = GetTitle(_roomInfo.ChapterIndex, _roomInfo.DungeonIndex);
		CampaignChapterData data2 = GameRoot.Instance.CampaignChapterDataContainer.GetData(_roomInfo.ChapterIndex);
		string compareString = ((data2 != null) ? LocalizationManager.GetStringDirect(data2.Name) : string.Empty);
		title = UIHelper.TextInsertWrapCharacter(title, compareString);
		UIHelper.SetLabelText(Label_Title, title);
		UIHelper.SetLabelText(Label_RoomLeader, _roomInfo.MasterNick);
		UIHelper.SetLabelText(Label_DungeonType, UIHelper.PartyDungeonTypeLocale(_roomInfo.DungeonType));
		InitPortrait(data);
		UIHelper.EnableButton(EnterRaid, enterAble, grayscaleIfDisable: true);
	}

	private void SetMemberCount(int curr, int max)
	{
		UIHelper.SetLabelText(Label_RoomMemberCount, $"{curr}/{max}");
	}

	private void SetLevelString(UILabel label, int level)
	{
		UIHelper.SetLabelText(label, "LV" + level);
	}

	private string GetTitle(int chapterIndex, int dungeonIndex)
	{
		CampaignNodeData data = GameRoot.Instance.CampaignNodeDataContainer.GetData(chapterIndex, dungeonIndex);
		if (data == null)
		{
			_ = $"{chapterIndex} - {dungeonIndex}";
		}
		return UIHelper.GetNodeDungeonName(data);
	}

	private void InitPortrait(PartyDungeonData partyydungeonData)
	{
		string portraitPath = partyydungeonData.PortraitPath;
		if (String_Extension_Method.IsNullOrEmpty(portraitPath))
		{
			VespaDebug.LogError("portraitPath.IsNullOrEmpty()");
		}
		UIHelper.SetBundleTexture(PortraitBundleTex, portraitPath);
	}

	private void ClosePortrait()
	{
		UIHelper.SetBundleTexture(PortraitBundleTex, string.Empty);
	}

	public void OnClickEnterRaid()
	{
		if (SceneAutoBehaviour<MultiPartyManagement>.instance.CheckEnterBattle() && _Listener != null)
		{
			_Listener.OnClickEnterDungeon(_roomInfo);
		}
	}
}
