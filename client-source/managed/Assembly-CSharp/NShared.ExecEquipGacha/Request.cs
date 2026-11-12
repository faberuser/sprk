namespace NShared.ExecEquipGacha;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "equip_gacha/exec_equip_gacha";

	public int ItemIndex { get; set; }

	public int GachaIndex { get; set; }

	public int[] HeroIndices { get; set; }

	public HighGachaCategoryType HighGachaCategory { get; set; }

	public bool Free { get; set; }

	public bool Discount { get; set; }

	public int DiscountedCost { get; set; }
}
