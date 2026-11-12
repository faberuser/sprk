namespace NShared;

[JsonSerializable(Javascript = true)]
public enum ChatBanType
{
	None,
	Global,
	Guild,
	Party,
	Whisper,
	All
}
