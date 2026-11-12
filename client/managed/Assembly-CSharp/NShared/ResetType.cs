namespace NShared;

[JsonSerializable(Javascript = true)]
public enum ResetType
{
	None,
	Daily,
	Weekly,
	Monthly
}
