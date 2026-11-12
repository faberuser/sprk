namespace NGameAsset.NCreature;

public interface IWeaponData
{
	string Name { get; set; }

	string AttachPivot { get; set; }

	bool IsShow { get; set; }

	bool AnimationFollowBodySelection { get; set; }

	AnimationKeyData DefaultAnimationKey { get; set; }
}
