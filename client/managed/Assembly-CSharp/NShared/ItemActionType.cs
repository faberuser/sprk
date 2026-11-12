namespace NShared;

[JsonSerializable(Javascript = true)]
public enum ItemActionType
{
	None,
	RecoverStamina,
	RecoverStaminaPercent,
	GetHeroExp,
	RecoverGem,
	RecoverHideoutKey,
	ExtendEquip,
	CompleteArchive,
	CompleteAchievement,
	BuyEquipStorageSlot,
	ExtendChest,
	AddTranscendPoint,
	RecoverConquestKey,
	ExtendHousingInven
}
