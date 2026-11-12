using NGameAsset.NCreature;

namespace NGameAsset;

public struct AnimationStock
{
	public AnimationKeyData Key;

	public bool IsMain;

	public float Frame;

	public float Weight;

	public FacialAnimationData FacialAnimation;

	public FacialAnimationData PrevFacialAnimation;
}
