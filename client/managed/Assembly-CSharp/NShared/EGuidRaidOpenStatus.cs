namespace NShared;

[JsonSerializable(Javascript = true)]
public enum EGuidRaidOpenStatus
{
	Complete,
	Initializing,
	Error
}
