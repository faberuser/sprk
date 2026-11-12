namespace NShared;

[JsonSerializable(Javascript = true)]
public class DefaultExtraStatData : IConditionalExtraStatData
{
	public OperationType OptionType { get; set; }

	public int OptionValue { get; set; }

	bool IConditionalExtraStatData.CheckActiveCondition(CampaignDungeonData dungeonData)
	{
		return true;
	}
}
