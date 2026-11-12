using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class ClientCustomizingCameraData : IDataKeySelector<AccessoryPartType>
{
	[Key(0)]
	public AccessoryPartType PartType { get; set; }

	[Key(1)]
	public string TransformName { get; set; }

	[Key(2)]
	public float CreatureRotation { get; set; }

	[Key(3)]
	public float ZoomRate { get; set; }

	[Key(4)]
	public float[] ExtraOffset { get; set; }

	AccessoryPartType IDataKeySelector<AccessoryPartType>.Key => PartType;
}
