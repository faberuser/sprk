namespace NShared.ChangeHeroStorageSlotName;

public enum ResultType
{
	Fail,
	Success,
	InvalidHeroStorageKey,
	[Comment("슬롯명이 너무 깁니다. 12자 이내로 입력하세요.")]
	HeroStorageSlotNameTooLong,
	[Comment("슬롯명이 너무 짧습니다. 2자 이상 입력하세요.")]
	HeroStorageSlotNameTooShort,
	[Comment("사용할 수 없는 언어가 포함되어 있습니다.")]
	HeroStorageSlotNameInvalidThisServer,
	[Comment("사용할 수 없는 단어가 포함되어 있습니다.")]
	HeroStorageSlotNameCensoredWord,
	[Comment("사용할 수 없는 문자가 포함되어 있습니다.")]
	HeroStorageSlotNameInvalid
}
