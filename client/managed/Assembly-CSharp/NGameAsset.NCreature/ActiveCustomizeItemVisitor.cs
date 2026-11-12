using NVespa;

namespace NGameAsset.NCreature;

internal class ActiveCustomizeItemVisitor : IGenericVisitor<CustomizeItemInstance>, IVisitor
{
	public bool IsActived = true;

	void IGenericVisitor<CustomizeItemInstance>.Visit(CustomizeItemInstance CustomizeItemInstance)
	{
		CustomizeItemInstance.SetActive(IsActived);
	}
}
