namespace NShared.UseGuildNameChangeItem;

public enum ResultType
{
	Fail,
	Success,
	ItemNotOwned,
	ItemDataNotFound,
	ItemTypeMismatch,
	EmptyName,
	InvalidName,
	AlreadyExistGuildName,
	TooShortName,
	TooLongName,
	CensoredWord,
	SensitiveWord
}
