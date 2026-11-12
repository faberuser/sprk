namespace NShared.ConfirmValanceEnchant;

public enum ResultType
{
	Fail,
	Success,
	[Comment("보유하지 않은 장비입니다.")]
	EquipNotOwned,
	[Comment("인챈트 기록이 존재하지 않습니다.")]
	NotHaveEnchantRecord
}
