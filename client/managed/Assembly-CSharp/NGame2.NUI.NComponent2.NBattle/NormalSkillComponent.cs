using NNguiExtension;
using NShared;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NBattle;

public class NormalSkillComponent : BaseSkillComponent
{
	public const string PrefabPath = "Prefab2/Component/Battle/NormalSkillComponent";

	public static PrefabAuto<NormalSkillComponent> PrefabAuto = new PrefabAuto<NormalSkillComponent>("Prefab2/Component/Battle/NormalSkillComponent");

	public UISpriteSelector SpriteSelectorManaGauge;

	private float _lastUpdateReqMp;

	protected override void SetIconLocked(bool isLocked)
	{
		UIHelper.EnableGameObject(SpriteSelectorReqLevel, isLocked);
		if (isLocked)
		{
			StopAllEffect();
			if (SpriteSelectorReqLevel != null)
			{
				SpriteSelectorReqLevel.SelectInt(base.SkillSlotIndex);
			}
		}
	}

	protected override void SetDisableSkillSlot(bool isDisable)
	{
		UIHelper.EnableGameObject(SpriteDisableSkillSlot, isDisable);
		if (isDisable)
		{
			SetCachedLastSkillReady(isReady: false);
			SetButtonScale(Vector3.one);
			StopAllEffect();
		}
	}

	protected override bool IsDisableSlot()
	{
		return GetSyncSkillUseResult() == SkillUseResult.DisableSkillSlot;
	}

	public override void UpdateExtra()
	{
		UpdateReqMp();
	}

	private void UpdateReqMp()
	{
		long maxMp = _creatureCache.MaxMp;
		int tableReqMp = _syncCanUseSkill.TableReqMp;
		if (_toolTipTextInfo != null)
		{
			_toolTipTextInfo.InitReqManaGauge(tableReqMp, (int)maxMp);
			_toolTipTextInfo.CooltimeMs = _syncCanUseSkill.TableCooltimeMs;
		}
		int value = ((!base.SkillInfo.IsLocked) ? Mathf.RoundToInt((float)tableReqMp * 0.001f) : 0);
		if (SpriteSelectorManaGauge != null)
		{
			bool enable = SpriteSelectorManaGauge.SelectInt(value);
			UIHelper.EnableGameObject(SpriteSelectorManaGauge, enable);
		}
	}
}
