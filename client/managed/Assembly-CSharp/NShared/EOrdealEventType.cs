namespace NShared;

[JsonSerializable(Javascript = true)]
public enum EOrdealEventType
{
	SelectClearBuff,
	GetPoint,
	ReviveAllCharacter,
	GetOpponentChange
}
