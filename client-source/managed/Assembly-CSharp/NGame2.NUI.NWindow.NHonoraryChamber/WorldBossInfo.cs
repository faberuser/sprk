using System;
using NShared;
using NVespa.NGlobal;

namespace NGame2.NUI.NWindow.NHonoraryChamber;

[Serializable]
public class WorldBossInfo : HonoraryInfoBase
{
	public UITexture Texture;

	public UIBundleLoader TextureWorldBoss;

	public UILabel LabelNameLabel;

	public UILabel LabelName;

	public UILabel LabelRankLabel;

	public UILabel LabelRank;

	public UILabel LabelScoreLabel;

	public UILabel LabelScore;

	public override void Release()
	{
		base.Release();
	}

	public override bool IsActiveCondition(RecordsOfHonorRankingInfo info)
	{
		return info.ContentType == RecordsOfHonorContentType.WorldBoss;
	}

	public override void SetInfo(RecordsOfHonorRankingInfo info)
	{
		base.SetInfo(info);
		GetBossCreatureData(info.Index, out var creatureData, out var waveData);
		UIHelper.SetLabelText(LabelName, GetName(creatureData, waveData));
		string locale = GetLocale("HONOR_RANK_NUM", info.Rank + 1);
		UIHelper.SetLabelText(LabelRank, locale);
		UIHelper.SetLabelText(LabelScore, Vespa_Long_Extension_Method.ToCommaString(info.Score));
		TextureWorldBoss.SetAssetPath(GetPortraitPath(creatureData));
	}

	private string GetName(CreatureData creatureData, CampaignWaveData waveData)
	{
		if (creatureData == null)
		{
			return string.Empty;
		}
		string creatureName = string.Empty;
		string creatureNamePrefix = string.Empty;
		string visualInfo = waveData?.BossVisualInfo;
		GameRoot.Instance.CreatureVisualDataContainer.GetCreatureName(creatureData, visualInfo, LocalizationManager.GetStringDirect, out creatureName, out creatureNamePrefix);
		return creatureName;
	}

	private string GetPortraitPath(CreatureData creatureData)
	{
		if (creatureData == null)
		{
			return string.Empty;
		}
		return UIHelper.GetCreatureIconPath(creatureData.CodeName);
	}

	private bool GetBossCreatureData(int worldBossIndex, out CreatureData creatureData, out CampaignWaveData waveData)
	{
		creatureData = null;
		waveData = null;
		WorldBossData data = GameRoot.Instance.WorldBossDataContainer.GetData(worldBossIndex);
		if (data == null)
		{
			return false;
		}
		int chapterIndex = data.ChapterIndex;
		int dungeonIndex = data.DungeonIndex;
		CampaignWaveKey2 key = new CampaignWaveKey2(chapterIndex, dungeonIndex, DungeonDifficultyType.Easy, scenario: false, 1);
		CampaignWaveData data2 = GameRoot.Instance.CampaignWaveDataContainer.GetData(key);
		if (data2 == null)
		{
			return false;
		}
		CreatureData data3 = GameRoot.Instance.CreatureDataContainer.GetData(data2.BossCreatureIndex);
		creatureData = data3;
		waveData = data2;
		return true;
	}
}
