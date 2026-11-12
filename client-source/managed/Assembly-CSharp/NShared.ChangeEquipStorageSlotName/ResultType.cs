namespace NShared.ChangeEquipStorageSlotName;

public enum ResultType
{
	Fail,
	Success,
	InvalidEquipStorageSlotIndex,
	EquipSlotDataNotFound,
	[Comment("슬롯명이 너무 깁니다. 10자 이내로 입력하세요.")]
	NameTooLong
}
