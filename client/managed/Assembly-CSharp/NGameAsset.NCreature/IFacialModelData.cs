namespace NGameAsset.NCreature;

public interface IFacialModelData
{
	string ModelName { get; }

	int GetBlendShapeCount();

	string GetBlendShapeName(int index);
}
