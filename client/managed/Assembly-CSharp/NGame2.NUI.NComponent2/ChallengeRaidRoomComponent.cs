using NGame2.NUI.NHelper.NAffix;
using NGame2.NUI.NWindow;
using NShared;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class ChallengeRaidRoomComponent : RaidRoomComponent
{
	public new const string PrefabPath = "Prefab2/Component/Raid/ChallengeRaidRoomComponent";

	public new static PrefabAuto<ChallengeRaidRoomComponent> PrefabAuto = new PrefabAuto<ChallengeRaidRoomComponent>("Prefab2/Component/Raid/ChallengeRaidRoomComponent");

	public UILabel Label_Point;

	public override void Open(RaidRoomInfo roomInfo, Listener listener)
	{
		base.Open(roomInfo, listener);
		SetLabelPoint();
	}

	private void SetLabelPoint()
	{
		RaidData data = GameRoot.Instance.RaidDataContainer.GetData(RoomInfo.RaidIndex, RoomInfo.RaidLevel);
		if (data != null)
		{
			CampaignDungeonData data2 = GameRoot.Instance.CampaignDungeonDataContainer.GetData(data.ChapterIndex, data.DungeonIndex);
			if (data2 != null)
			{
				DungeonDifficultyType challengeDungeonDifficulty = RaidHelper.GetChallengeDungeonDifficulty();
				int[] affixIndices = RoomInfo.AffixIndices;
				int totalAffixPoint = AffixHelper.GetTotalAffixPoint(data2, challengeDungeonDifficulty, affixIndices);
				UIHelper.SetLabelText(Label_Point, totalAffixPoint);
			}
		}
	}

	public void OnClickAffixInfo()
	{
		if (_Listener != null)
		{
			int[] affixIndices = RoomInfo.AffixIndices;
			_Listener.OpenAffixSelectSetting(RoomInfo.RaidIndex, RoomInfo.RaidLevel, affixIndices);
		}
	}
}
