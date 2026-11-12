using NGameAsset;
using NGameAsset.NCreature;

public struct CreatureInUIVisualData
{
	public AnimationKeyData DefalutIdleKey;

	public CreatureStock.ELoadLevel LoadLevel;

	public CreatureInUIVisualData(AnimationKeyData animationKey, CreatureStock.ELoadLevel loadLevel)
	{
		DefalutIdleKey = animationKey;
		LoadLevel = loadLevel;
	}
}
