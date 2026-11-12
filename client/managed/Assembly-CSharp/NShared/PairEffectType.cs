namespace NShared;

[JsonSerializable(Javascript = true)]
public enum PairEffectType
{
	None,
	StartPair,
	LoopPair,
	EndPair
}
