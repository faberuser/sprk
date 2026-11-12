using NGameAsset.NCreature;
using UnityEngine;

namespace NGameAsset;

public interface IAnimationOpenData
{
	int GetAnimationCount();

	IAnimationData GetAnimationData(int index);

	IAsyncLoad<AnimationClip> GetAnimationClip(int index);

	bool IsAnimationClipLoaded(int index);
}
