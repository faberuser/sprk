using System.Collections.Generic;
using NGame2.NUtil;
using NShared;

namespace NGame2.NUI.NWindow;

public class ItemTooltipTextInfo : TooltipTextInfo
{
	public StringProxy Desc1 = new StringProxy();

	public StringProxy Desc2 = new StringProxy();

	public string CreatureLimit = string.Empty;

	public List<CreatureTagType> EquipClassTags = new List<CreatureTagType>();

	public List<RuneSlotType> EquipRuneSlotTypes = new List<RuneSlotType>();

	public bool EquipAllClassTag;

	public bool DisplayEquipClassTags
	{
		get
		{
			if (EquipClassTags.Count <= 0 && !EquipAllClassTag)
			{
				return !String_Extension_Method.IsNullOrEmpty(Desc2.Value);
			}
			return true;
		}
	}

	public bool DisplayEquipCreatureLimit => !string.IsNullOrEmpty(CreatureLimit);

	public override void Reset()
	{
		base.Reset();
		Desc1.Reset();
		Desc2.Reset();
		CreatureLimit = string.Empty;
		EquipClassTags.Clear();
		EquipAllClassTag = false;
	}
}
