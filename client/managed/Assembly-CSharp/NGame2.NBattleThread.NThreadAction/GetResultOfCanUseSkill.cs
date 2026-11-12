using NShared;
using NShared.NCampaign;
using UnityEngine;

namespace NGame2.NBattleThread.NThreadAction;

public class GetResultOfCanUseSkill : IThreadAction
{
	public enum SkillType
	{
		Normal,
		SoulWeapon,
		DungeonSkill
	}

	public static SkillUseOption DefaultSkillUseOption = new SkillUseOption(checkOnly: true);

	public int CreatureUid;

	public SkillData SkillData;

	public SkillUseOption SkillUseOption;

	public SkillUseResult SkillUseResult = SkillUseResult.Fail;

	public float CooltimeFillAmount = 1f;

	public int CooltimeDuratiomMs;

	public int CooltimeElapsedMs;

	public int TableCooltimeMs;

	public int TableReqMp;

	private SkillType _skillType;

	public bool IsWaiting { get; set; }

	public GetResultOfCanUseSkill(int creatureUid, SkillData skillData, SkillUseOption skillUseOption = null, SkillType skillType = SkillType.Normal)
	{
		CreatureUid = creatureUid;
		SkillData = skillData;
		if (skillUseOption == null)
		{
			SkillUseOption = DefaultSkillUseOption;
		}
		else
		{
			SkillUseOption = skillUseOption;
		}
		TableReqMp = SkillData.Mp;
		TableCooltimeMs = SkillData.CoolTimeMs;
		_skillType = skillType;
	}

	void IThreadAction.Action(BattleInstance battleInstance, CampaignUpdater campaignUpdater)
	{
		BattleCreature battleCreature = GetBattleCreature(battleInstance, _skillType);
		if (battleCreature != null)
		{
			if (_skillType == SkillType.Normal)
			{
				SkillUseResult = battleCreature.SkillController.CanUseSkill(SkillData, SkillUseOption, out var _);
			}
			else
			{
				SkillUseResult = GetOtherSkillUseResult(battleCreature, _skillType);
			}
			if (battleCreature.SkillController.CooltimeManager.GetCooltimeInfo(SkillData.Index, out CooltimeDuratiomMs, out CooltimeElapsedMs))
			{
				CooltimeFillAmount = ((CooltimeDuratiomMs > 0) ? Mathf.Min((float)CooltimeElapsedMs / (float)CooltimeDuratiomMs, 1f) : 0f);
			}
			else
			{
				CooltimeFillAmount = 1f;
			}
			TableReqMp = battleCreature.SkillController.ModifierContainer.GetModifiedInt(SkillData.Index, 0, OperationFieldType.SkillMp, 0, SkillData.Mp);
			TableCooltimeMs = battleCreature.SkillController.ModifierContainer.GetModifiedInt(SkillData.Index, 0, OperationFieldType.SkillCoolTimeMs, 0, SkillData.CoolTimeMs);
		}
	}

	private BattleCreature GetBattleCreature(BattleInstance battleInstance, SkillType skillType)
	{
		if (skillType == SkillType.DungeonSkill)
		{
			return battleInstance.GetEnvironmentCreatureOfTeam(TeamType.Left);
		}
		return battleInstance.CreatureContainer.GetCreature(CreatureUid);
	}

	private SkillUseResult GetOtherSkillUseResult(BattleCreature battleCreature, SkillType skillType)
	{
		return skillType switch
		{
			SkillType.SoulWeapon => battleCreature.SoulWeaponSkillController.SkillUseResult, 
			SkillType.DungeonSkill => battleCreature.DungeonSkillController.CanUseSkill(SkillData.Index), 
			_ => SkillUseResult.Fail, 
		};
	}
}
