namespace NShared;

[JsonSerializable(Javascript = true)]
public class OrdealArenaStatDataBase : OrdealArenaBuffDataBase, IConditionalExtraStatData
{
	public OperationType OptionType { get; set; }

	public int OptionValue { get; set; }

	OperationType IConditionalExtraStatData.OptionType => OptionType;

	int IConditionalExtraStatData.OptionValue => OptionValue;

	bool IConditionalExtraStatData.CheckActiveCondition(CampaignDungeonData dungeonData)
	{
		return CheckActiveCondition(dungeonData);
	}
}
