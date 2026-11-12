using NGame2.NGuild;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NUI.NWorldMap.NMapLayout.NBuilding;

public class GuildBuildingOneModel : BaseBuilding
{
	public override string PrevIdleAnimationKey
	{
		get
		{
			int level = ((base.Level <= 1) ? 1 : (base.Level - 1));
			return GameRoot.Instance.GuildBuildingDataContainer.GetData(base.Index, level)?.IdleAnimationKey;
		}
	}

	public override string IdleAnimationKey => GameRoot.Instance.GuildBuildingDataContainer.GetData(base.Index, base.Level)?.IdleAnimationKey;

	public override string UpgradeAnimationKey => GameRoot.Instance.GuildBuildingDataContainer.GetData(base.Index, base.Level)?.UpgradeAnimationKey;

	public override int GetLevel()
	{
		return GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildBuildingManager.GetGuildBuildingLevel(base.Index);
	}

	public override BaseBuilding GetInstance()
	{
		return new GuildBuildingOneModel();
	}

	public override string GetModelName(int level)
	{
		return GameRoot.Instance.WorldMapBuildDataContainer.GetData(base.Index)?.ModelName;
	}

	public override bool GetReservedUpgrade()
	{
		return GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildBuildingManager.GetGuildBuildingReservedUpgrade(base.Index);
	}

	public override void CompletedUpgradeAnim()
	{
		GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildBuildingManager.CompletedUpgradeAnim(base.Index);
	}

	public override void UpgradePrepare()
	{
		SetIdleAnimation(GetModel(), PrevIdleAnimationKey);
	}

	public override void Upgrade()
	{
		SetActionAnimation(GetModel(), UpgradeAnimationKey);
		SetIdleAnimation(GetModel(), IdleAnimationKey);
	}

	public override void UpgradeComplete()
	{
	}
}
