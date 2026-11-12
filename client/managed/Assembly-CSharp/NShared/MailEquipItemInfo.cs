namespace NShared;

[JsonSerializable(Javascript = true)]
public class MailEquipItemInfo
{
	public int ItemIndex { get; set; }

	public int Star { get; set; }

	public int Level { get; set; }

	public int OptionIndex1 { get; set; }

	public int OptionIndex2 { get; set; }

	public int OptionIndex3 { get; set; }

	public int OptionIndex4 { get; set; }

	public int OptionIndex5 { get; set; }
}
