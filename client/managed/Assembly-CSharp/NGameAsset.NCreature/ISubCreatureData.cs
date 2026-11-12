using NVespa;

namespace NGameAsset.NCreature;

public interface ISubCreatureData
{
	string CreatureCode { get; }

	string PivotName { get; }

	TransformData GetExtraTransform();

	AnimationKeyData GetAnimationKey(AnimationKeyData parentKey);
}
