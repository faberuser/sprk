namespace NGameAsset.NCreature;

public interface IAnimationBlendData
{
	bool TryBlendTime(AnimationKeyData motionFrom, AnimationKeyData motionTo, out float blendTime);
}
