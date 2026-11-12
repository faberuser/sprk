namespace NShared;

[JsonSerializable(Javascript = true)]
public enum AttackAttrType
{
	None,
	Swing,
	Strike,
	Pierce,
	Explode,
	Fire,
	Water,
	Nature,
	Light,
	Darkness
}
