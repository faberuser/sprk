namespace NShared.ValanceCraft;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "valance/valance_craft";

	public EquipItemSetGroupType SetType { get; set; }

	public CreatureTagType CreatureTagType { get; set; }

	public EquipPartType PartType { get; set; }

	public EquipItemSubType SubPartType { get; set; }

	public int[] SelectEquipOptionIndices { get; set; }
}
