namespace NShared.Login;

public enum ResultType
{
	Fail,
	Success,
	VersionError,
	Retry,
	NotImplementedMethod,
	ServerClosing,
	NotEncrypted,
	SaltError,
	NoSecureKey,
	SaltNotDecrypted,
	KeyNotDecrypted,
	SaltMismatch,
	LoginIdNotDecrypted,
	DecryptFail,
	Blocked,
	LowVersion,
	ServerClosed,
	InSignOutProcessing,
	ContentsDisabled,
	[Comment("로그인 인증에 실패하였습니다.")]
	AuthFail,
	[Comment("이전 데이터의 복원이 필요합니다.")]
	InactiveAccount,
	[Comment("허용되지 않은 로그인 매체 입니다.")]
	NotAllowedLoginMethod,
	PtsSelectUser,
	PtsExceededAllowedCcu,
	PtsNotOpen,
	PtsInsertingUserData,
	DuplicateUserInfos
}
