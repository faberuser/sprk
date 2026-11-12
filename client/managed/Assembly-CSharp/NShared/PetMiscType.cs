namespace NShared;

[JsonSerializable(Javascript = true)]
public enum PetMiscType
{
	None,
	AvatarPetIndex,
	PetLayout,
	PetGachaCount,
	PetCollaborationGachaCount,
	PetIncubatorOpenTime,
	UsePetIncubatorCount,
	PetUpgradeTierCount,
	CompleteAdventureCount
}
