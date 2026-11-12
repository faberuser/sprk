namespace NShared.UseNickChangeItem;

public enum ResultType
{
	Fail,
	Success,
	ItemNotOwned,
	ItemDataNotFound,
	ItemTypeMismatch,
	InvalidName,
	AlreadyExistName,
	TooShortName,
	TooLongName,
	CensoredWord,
	SensitiveWord
}
