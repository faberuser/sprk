using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class ClientCustomizingPivotData : IDataKeySelector<ClientCustomizingPivotDataKey>
{
	ClientCustomizingPivotDataKey IDataKeySelector<ClientCustomizingPivotDataKey>.Key => new ClientCustomizingPivotDataKey(HeroIndex, PivotType);

	[Key(0)]
	public int HeroIndex { get; set; }

	[Key(1)]
	public AccessoryPartPivotType PivotType { get; set; }

	[Key(2)]
	public int PresetHeroIndex { get; set; }

	[Key(3)]
	public string TransformName1 { get; set; }

	[Key(4)]
	public string TransformName2 { get; set; }

	[Key(5)]
	public float[] InitPosition { get; set; }

	[Key(6)]
	public float[] InitRotation { get; set; }

	[Key(7)]
	public float InitScale { get; set; }

	[Key(8)]
	public float[] PositionVariation { get; set; }

	[Key(9)]
	public float[] RotationVariation { get; set; }

	[Key(10)]
	public float ScaleVariation { get; set; }
}
