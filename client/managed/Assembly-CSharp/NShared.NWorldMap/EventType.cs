namespace NShared.NWorldMap;

[JsonSerializable(Javascript = true)]
public enum EventType
{
	None,
	FieldBoss,
	GoblinShop,
	QuestDungeon,
	GuerrillaDungeon,
	EventDungeon,
	StageBuff,
	TreasureBox,
	MapCoin,
	Excavation
}
