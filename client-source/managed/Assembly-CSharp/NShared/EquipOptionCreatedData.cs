namespace NShared;

public class EquipOptionCreatedData
{
	public EquipOptionData Data { get; set; }

	public int Star { get; set; }

	public int Step { get; set; }

	public OperationType OptionType => GetOptionType();

	public int OptionValue => GetOptionValue();

	public EquipOptionCreatedData(EquipOptionData data, int star, int step)
	{
		Data = data;
		Star = star;
		Step = step;
	}

	public OperationType GetOptionType()
	{
		if (Data == null)
		{
			return OperationType.None;
		}
		return Data.OptionType;
	}

	public int GetOptionValue()
	{
		if (Data == null)
		{
			return 0;
		}
		return Data.GetOptionValue(Star, Step);
	}
}
