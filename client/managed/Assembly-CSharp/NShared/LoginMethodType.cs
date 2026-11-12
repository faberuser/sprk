namespace NShared;

[JsonSerializable(Javascript = true)]
public enum LoginMethodType
{
	None,
	Guest,
	Steam,
	GooglePlus,
	GameCenter,
	Apple,
	Twitter,
	Line,
	Masang
}
