using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow;
using NShared;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NComponent2;

public class RaidRoomComponent : LinkFactoryComponent, NVespa.NPrefab2.InitOnCreate
{
	public interface Listener
	{
		void OnClickEnterRaid(RaidRoomInfo roomInfo);

		void OpenAffixSelectSetting(int raidindex, int raidLevel, int[] affixIndices);
	}

	public const string PrefabPath = "Prefab2/Component/Raid/RaidRoomComponent";

	public static PrefabAuto<RaidRoomComponent> PrefabAuto = new PrefabAuto<RaidRoomComponent>("Prefab2/Component/Raid/RaidRoomComponent");

	public UIBundleLoader PortraitBundleTex;

	public UILabel RaidLevelLabel;

	public UILabel HardModeLabel;

	public UILabel TitleLable;

	public UILabel RoomLeaderLabel;

	public UILabel RoomMemberCountLabel;

	public UIButton EnterRaid;

	protected Listener _Listener;

	protected RaidRoomInfo RoomInfo;

	private const string CHALLENGE_RAID_TIME_OUT_MSG = "RAID_CHALLENGE_TIME_OUT_MSG";

	public int Level => RoomInfo.RaidLevel;

	public long Index => RoomInfo.RoomNo;

	void NVespa.NPrefab2.InitOnCreate.OnCreate()
	{
	}

	void NVespa.NPrefab2.InitOnCreate.OnRelease()
	{
		ClosePortrait();
	}

	public virtual void Open(RaidRoomInfo roomInfo, Listener listener)
	{
		RoomInfo = roomInfo;
		_Listener = listener;
		RaidData data = GameRoot.Instance.RaidDataContainer.GetData(roomInfo.RaidIndex, roomInfo.RaidLevel);
		if (data != null)
		{
			SetMemberCount(roomInfo.CurPartyMember, data.PlayerCount);
			string raidRoomTypeString = RaidManagement.GetRaidRoomTypeString((RaidRoomType)roomInfo.GoalIndex, data, isTitle: true);
			UIHelper.SetLabelText(TitleLable, raidRoomTypeString);
			UIHelper.SetLabelText(RoomLeaderLabel, roomInfo.MasterNick);
			InitPortrait(data);
			SetRaidRoomStep(data);
			SetEnableEnter(data);
		}
	}

	private void SetRaidRoomStep(RaidData raidData)
	{
		bool flag = raidData.Type == RaidType.DragonNormal || raidData.Type == RaidType.DragonHard || raidData.Type == RaidType.FieldRaid || raidData.Type == RaidType.TechnoMagic || raidData.Type == RaidType.TechnoEnchant;
		UIHelper.EnableGameObject(RaidLevelLabel, flag);
		UIHelper.EnableGameObject(HardModeLabel, raidData.Type == RaidType.Challenge);
		string stringDirect = LocalizationManager.GetStringDirect(raidData.Step_Name);
		if (flag)
		{
			UIHelper.SetLabelText(RaidLevelLabel, stringDirect);
		}
		else if (raidData.Type == RaidType.Challenge)
		{
			ChallengeRaidDiff challengeRaidDiff = RaidHelper.ChallengeRaidDiffClamp(RoomInfo.RaidLevel);
			if (SceneAutoBehaviour<RaidManagement>.instance.IsAbleEnter())
			{
				UIHelper.SetLabelText(HardModeLabel, challengeRaidDiff.ToString());
				string roomTitle = raidData.Name;
				int challengeRaidIndex = SceneAutoBehaviour<RaidManagement>.instance.GetChallengeRaidSeasonInfo().ChallengeRaidIndex;
				ChallengeRaidData data = GameRoot.Instance.ChallengeRaidDataContainer.GetData(challengeRaidIndex);
				if (data != null && !String_Extension_Method.IsNullOrEmpty(data.RoomTitle))
				{
					roomTitle = data.RoomTitle;
				}
				UIHelper.SetLabelText(TitleLable, LocalizationManager.GetStringDirect(roomTitle));
			}
			else
			{
				UIHelper.SetLabelText(HardModeLabel, challengeRaidDiff.ToString());
				UIHelper.SetLabelText(TitleLable, LocalizationManager.GetStringDirect("RAID_CHALLENGE_TIME_OUT_MSG"));
			}
		}
		else
		{
			UIHelper.SetLabelText(HardModeLabel, "Hard");
		}
	}

	private void SetEnableEnter(RaidData raidData)
	{
		if (raidData.Type != RaidType.Challenge)
		{
			UIHelper.EnableButton(EnterRaid, enable: true, grayscaleIfDisable: true);
			return;
		}
		if (!SceneAutoBehaviour<RaidManagement>.instance.IsAbleEnter())
		{
			UIHelper.EnableButton(EnterRaid, enable: false, grayscaleIfDisable: true);
			return;
		}
		bool enable = true;
		if (!RaidHelper.CanEnterRaid(raidData))
		{
			enable = false;
		}
		UIHelper.EnableButton(EnterRaid, enable, grayscaleIfDisable: true);
	}

	private void SetMemberCount(int curr, int max)
	{
		UIHelper.SetLabelText(RoomMemberCountLabel, $"{curr}/{max}");
	}

	private void SetLevelString(UILabel label, int level)
	{
		UIHelper.SetLabelText(label, "LV" + level);
	}

	public void OnClickEnterRaid()
	{
		if (SceneAutoBehaviour<MultiPartyManagement>.instance.CheckEnterBattle() && _Listener != null)
		{
			_Listener.OnClickEnterRaid(RoomInfo);
		}
	}

	private void InitPortrait(RaidData raidData)
	{
		RaidHelper.InitPortrait(this, PortraitBundleTex, raidData);
	}

	private void ClosePortrait()
	{
		UIHelper.SetBundleTexture(PortraitBundleTex, string.Empty);
	}
}
