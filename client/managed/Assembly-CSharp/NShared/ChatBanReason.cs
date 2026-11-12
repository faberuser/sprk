namespace NShared;

[JsonSerializable(Javascript = true)]
public enum ChatBanReason
{
	None,
	CurseWord,
	Obscene,
	Advertised,
	Deal,
	ReleaseBan
}
