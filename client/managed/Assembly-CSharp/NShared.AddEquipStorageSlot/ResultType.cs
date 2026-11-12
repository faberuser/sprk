namespace NShared.AddEquipStorageSlot;

public enum ResultType
{
	Fail,
	Success,
	NoHero,
	HeroDataNotFound,
	[Comment("잘못된 장비 저장 슬롯 번호입니다.")]
	InvalidEquipStorageSlotIndex,
	[Comment("슬롯명이 너무 깁니다. 10자 이내로 입력하세요.")]
	NameTooLong,
	[Comment("장착한 장비 리스트 크기가 올바르지 않습니다. (Only 5개)")]
	InvalidEquipItemSlotIndex,
	InvalidIndexArraySize,
	InvalidPartIndex,
	DuplicatedPartIndex,
	[Comment("이미 사용중인 EquipStorageSlotIndex")]
	AlreadyUsedEquipStorageSlotIndex
}
