namespace NShared.EnchantEquip;

public enum ResultType
{
	Fail,
	Success,
	EquipNotOwned,
	EquipDataNotFound,
	NotEnoughGold,
	InvalidMaterialItem,
	MaterialItemDataNotFound,
	NotEnoughMaterialItem,
	ItemDataNotFound,
	EquipEnchantOptionItemDataNotFound,
	InvalidReqGold,
	ContentsDisabled,
	ItemDataOfEnchantEquipNotFound,
	EnchantableConditionNotFound,
	WrongEnchantItemCount,
	NotEnoughEnchantItem,
	[Comment("인챈트 옵션 생성에 실패 하였습니다.")]
	EnchantOptionGenerateFailed,
	[Comment("더이상 인챈트를 할 수 없는 상태입니다.")]
	CanNotTryEnchant
}
