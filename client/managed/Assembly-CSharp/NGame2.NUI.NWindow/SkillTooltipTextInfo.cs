using UnityEngine;

namespace NGame2.NUI.NWindow;

public class SkillTooltipTextInfo : TooltipTextInfo
{
	public enum SkillTooltipType
	{
		Normal,
		SoulWeapon,
		DungeonSkill
	}

	public bool isActiveSkill;

	public int SkillLevel;

	public int CooltimeMs;

	public int ReqManaGauge;

	public bool IsOnlyDesc;

	public SkillTooltipType TooltipType;

	public void InitReqManaGauge(int reqMp, int maxMp)
	{
		ReqManaGauge = reqMp * 6 / Mathf.Max(1, maxMp);
	}

	public static int GetReqManaGauge(int reqMp, int maxMp)
	{
		return reqMp * 6 / Mathf.Max(1, maxMp);
	}
}
