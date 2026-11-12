using System.Collections;
using NShared;
using NVespa;
using NVespa.NGlobal;

namespace NGame2.NUI.NWindow;

public class ChallengeRaidGuide : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/Raid/ChallengeRaidGuide";

	public UILabel Lbl_Battle;

	public UILabel Lbl_Ranking;

	public UITable Table_Root;

	private const string CHALLENGERAID_BATTLE_DESC = "CHALLENGERAID_BATTLE_DESC_{0}";

	private const string CHALLENGERAID_RANKING_DESC = "CHALLENGERAID_RANKING_DESC_{0}";

	public void Open(int challengeRaidIndex)
	{
		if (challengeRaidIndex == 0)
		{
			VespaDebug.LogError("challengeRaidIndex == 0");
			return;
		}
		base.Open();
		Init(challengeRaidIndex);
	}

	private void Init(int challengeRaidIndex)
	{
		ChallengeRaidData data = GameRoot.Instance.ChallengeRaidDataContainer.GetData(challengeRaidIndex);
		if (data != null)
		{
			UIHelper.SetLabelText(Lbl_Battle, GetBattleGuide(challengeRaidIndex));
			UIHelper.SetLabelText(Lbl_Ranking, LocalizationManager.GetStringDirect(data.GuideRanking));
			ExtensionMethod_VespaCoroutine.StartCoroutineThis(this, RepostionNextUpdate());
		}
	}

	private string GetBattleGuide(int challengeRaidIndex)
	{
		ChallengeRaidData data = GameRoot.Instance.ChallengeRaidDataContainer.GetData(challengeRaidIndex);
		if (data != null)
		{
			RaidData data2 = GameRoot.Instance.RaidDataContainer.GetData(data.RaidIndex, 1);
			if (data2 != null)
			{
				return LocalizationManager.GetStringDirect(data.GuideBattle, GetBossName(data2), data2.DeckCount, "", "", "", data2.PlayerCount, data2.DeckCount, data2.DeckCountPerPlayer);
			}
		}
		return "";
	}

	private string GetBossName(RaidData raidData)
	{
		if (raidData != null)
		{
			CampaignWaveData data = GameRoot.Instance.CampaignWaveDataContainer.GetData(raidData.ChapterIndex, raidData.DungeonIndex, DungeonDifficultyType.Easy, scenario: false, 1);
			if (data != null)
			{
				CreatureVisualData data2 = GameRoot.Instance.CreatureVisualDataContainer.GetData(data.BossVisualInfo);
				if (data2 != null)
				{
					return LocalizationManager.GetStringDirect(data2.NamePrefixCode) + " " + LocalizationManager.GetStringDirect(data2.NameCode);
				}
			}
		}
		return "";
	}

	private IEnumerator RepostionNextUpdate()
	{
		yield return null;
		if ((bool)Table_Root)
		{
			Table_Root.repositionNow = true;
		}
	}
}
