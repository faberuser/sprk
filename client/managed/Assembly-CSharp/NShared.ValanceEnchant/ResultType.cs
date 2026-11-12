namespace NShared.ValanceEnchant;

public enum ResultType
{
	Fail,
	Success,
	[Comment("보유하지 않은 장비입니다.")]
	EquipNotOwned,
	ItemDataNotFound,
	EquipDataNotFound,
	EnchantableConditionNotFound,
	[Comment("아직 인챈트 옵션이 선택되지 않은 장비가 존재합니다.")]
	IsNotCompletedSelect,
	[Comment("마도공학 장비가 아닙니다.")]
	IsNotTechnoMagicEquip,
	[Comment("미감정 된 장비입니다.")]
	EquipNotIdentified,
	ValanceEnchantDataFound,
	[Comment("인챈트 옵션 생성에 실패하였습니다.")]
	FailGenerateValanceEnchantOption
}
