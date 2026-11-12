namespace NShared.NWorldMap;

[JsonSerializable(Javascript = true)]
public enum EventTriggerType
{
	None,
	EndCampaign,
	Excavation,
	AcceptQuest,
	ReceiveWorldMapEventReward
}
