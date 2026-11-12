using UnityEngine;

namespace NGameAsset.NCreature;

public class ClipMappingData
{
	public AnimationKeyData Key;

	public IAsyncLoad<AnimationClip> ClipAsync;

	public AnimationClip Clip;

	public string ClipName;

	public bool Registered;

	public float Frame;

	public float Weight;
}
