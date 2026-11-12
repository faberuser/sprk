using System.Collections.Generic;

namespace NShared;

public class TranscendSkillPageInfo
{
	public List<CreatureData.TranscendSkill> TranscendSkillDatas = new List<CreatureData.TranscendSkill>();

	public int Page { get; set; }

	public CreatureData.TranscendSkill FindSkill(int transcended, int index)
	{
		return TranscendSkillDatas.Find((CreatureData.TranscendSkill skill) => skill.Transcended == transcended && skill.Index == index);
	}
}
