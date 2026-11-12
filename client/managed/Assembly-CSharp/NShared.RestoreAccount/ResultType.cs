namespace NShared.RestoreAccount;

public enum ResultType
{
	Fail,
	Success,
	Retry,
	NotImplementedMethod,
	ServerClosing,
	NotEncrypted,
	SaltError,
	NoSecureKey,
	SaltNotDecrypted,
	SaltMismatch,
	LoginIdNotDecrypted,
	DecryptFail,
	Blocked,
	LowVersion,
	ServerClosed,
	ContentsDisabled,
	[Comment("로그인 인증에 실패하였습니다.")]
	AuthFail,
	[Comment("게스트계정은 복원할 수 없습니다.")]
	CannotRestoreGuest,
	[Comment("계정 복원을 실패하였습니다.")]
	RestoreAccountFail,
	[Comment("복원할 계정을 찾을 수 없습니다.")]
	RestoreAccountNotFound
}
