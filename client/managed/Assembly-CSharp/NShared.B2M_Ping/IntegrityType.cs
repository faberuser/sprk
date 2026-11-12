namespace NShared.B2M_Ping;

[JsonSerializable(Javascript = true)]
public enum IntegrityType
{
	Sync,
	Async,
	All
}
