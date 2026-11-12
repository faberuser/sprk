using System.Collections.Generic;

namespace NGameAsset.NCreature;

public interface IAnimationData
{
	float TimeLength { get; }

	AnimationKeyData GetKey();

	bool GetNodeVisible(string nodeName, float frame, out bool isNullNode);

	FacialAnimationData GetFacialAnimation();

	List<EventSoundFx> GetSoundFxList();
}
