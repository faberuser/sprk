using System.Collections;
using NGame2.NGuild;
using NGame2.NRender;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NUI.NWorldMap.NMapLayout.NBuilding;

public class GuildBuildingPartialModel : BaseBuilding
{
	private int _level;

	private RenderFacade _curLvBuilding;

	private RenderFacade _prevLvBuiliding;

	private RenderFacade _subBuilding;

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

	private void ResetInitBuildingLv()
	{
		_level = 0;
		GetLevel();
	}

	public override int GetLevel()
	{
		if (_level == 0)
		{
			_level = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildBuildingManager.GetGuildBuildingLevel(base.Index);
			return _level;
		}
		return GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildBuildingManager.GetGuildBuildingLevel(base.Index);
	}

	public override BaseBuilding GetInstance()
	{
		return new GuildBuildingPartialModel();
	}

	public override string GetModelName(int level)
	{
		return GameRoot.Instance.GuildBuildingDataContainer.GetData(base.Index, level)?.ModelName;
	}

	public override bool GetReservedUpgrade()
	{
		return GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildBuildingManager.GetGuildBuildingReservedUpgrade(base.Index);
	}

	public override void CompletedUpgradeAnim()
	{
		GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildBuildingManager.CompletedUpgradeAnim(base.Index);
	}

	public override void Update()
	{
		base.Update();
		if (_subBuilding != null)
		{
			_subBuilding.ChainUpdate();
		}
	}

	public override void UpgradeCancel()
	{
		string modelName = GetModelName(_level);
		string modelName2 = GetModelName(GetLevel());
		if (!modelName.Equals(modelName2))
		{
			RenderFacade model = RenderFacadeBulid(modelName2);
			ResetInitBuildingLv();
			ChangeModel(model);
		}
	}

	public override void UpgradePrepare()
	{
		string modelName = GetModelName(_level);
		string modelName2 = GetModelName(GetLevel());
		string modelName3 = GetModelName(GetLevel() - 1);
		bool flag = modelName3 != modelName;
		bool flag2 = modelName2 != modelName;
		bool num = flag2 && flag;
		_prevLvBuiliding = (flag ? RenderFacadeBulid(modelName3) : GetModel());
		_curLvBuilding = (flag2 ? RenderFacadeBulid(modelName2) : GetModel());
		if (num)
		{
			GetModel().Close();
		}
		SetExtraAlpha(_curLvBuilding, 0f);
		SetIdleAnimation(_prevLvBuiliding, PrevIdleAnimationKey);
		SwitchModel(_prevLvBuiliding);
		_subBuilding = _curLvBuilding;
	}

	public override void Upgrade()
	{
		SetActionAnimation(_prevLvBuiliding, UpgradeAnimationKey);
		VespaCoroutine.Start(coCheckEndUpgradAni());
	}

	private IEnumerator coCheckEndUpgradAni()
	{
		if (!(_prevLvBuiliding == null))
		{
			while (!_prevLvBuiliding.Action.IsActionEnd())
			{
				yield return null;
			}
			SetExtraAlpha(_prevLvBuiliding, 0f, changeNow: true);
			SetExtraAlpha(_curLvBuilding, 1f, changeNow: true);
			if (GetModel() != _curLvBuilding)
			{
				ResetInitBuildingLv();
				ChangeModel(_curLvBuilding);
			}
			_prevLvBuiliding = null;
			_curLvBuilding = null;
			_subBuilding = null;
		}
	}

	public override void UpgradeComplete()
	{
	}
}
