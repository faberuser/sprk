namespace NShared;

[JsonSerializable]
public class EquipOptionChangeOnUpgradeTierData : IDataKeySelector<EquipOptionChangeOnUpgradeTierDataKey>
{
	public int OptionIndex { get; set; }

	public int BeforeTier { get; set; }

	public int AfterTier { get; set; }

	public int ResultOptionIndex { get; set; }

	EquipOptionChangeOnUpgradeTierDataKey IDataKeySelector<EquipOptionChangeOnUpgradeTierDataKey>.Key => new EquipOptionChangeOnUpgradeTierDataKey(OptionIndex, BeforeTier, AfterTier);

	public override string ToString()
	{
		return "OptionIndex=" + OptionIndex + " ResultOptionIndex = " + ResultOptionIndex;
	}
}
