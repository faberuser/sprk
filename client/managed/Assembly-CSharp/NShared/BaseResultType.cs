namespace NShared;

public enum BaseResultType
{
	Fail,
	Success,
	Timeout,
	SessionError,
	SequenceError,
	AccountError,
	Unhandled,
	Exception,
	HttpError,
	InternalError,
	InternalServerError,
	ParsingError,
	EmptyHost,
	Disabled,
	Offline,
	InvalidAppError,
	NoGuild
}
