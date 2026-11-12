using System.Collections.Generic;
using NShared;

namespace NGame2.NUI.NWindow;

public class HeroTooltipTextInfo : TooltipTextInfo
{
	public CreatureTagType CreatureTagType;

	public List<SkillAttrType> ListAttrTypes = new List<SkillAttrType>();

	public List<SkillAttrType> ListWeakTypes = new List<SkillAttrType>();

	public bool DisplayCreatureAttrDefect
	{
		get
		{
			if (ListAttrTypes.Count <= 0)
			{
				return ListWeakTypes.Count > 0;
			}
			return true;
		}
	}

	public override void Reset()
	{
		base.Reset();
		CreatureTagType = CreatureTagType.None;
		ListAttrTypes.Clear();
		ListWeakTypes.Clear();
	}
}
