namespace NShared;

public interface IConditionalExtraStatData
{
	OperationType OptionType { get; }

	int OptionValue { get; }

	bool CheckActiveCondition(CampaignDungeonData dungeonData);
}
