namespace NShared;

public class EquipOptionChangeOnUpgradeTierDataKey : Tuple<int, int, int>
{
	public EquipOptionChangeOnUpgradeTierDataKey()
		: base(0, 0, 0)
	{
	}

	public EquipOptionChangeOnUpgradeTierDataKey(int OptionIndex, int BeforeTier, int AfterTier)
		: base(OptionIndex, BeforeTier, AfterTier)
	{
	}
}
