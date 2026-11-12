using System.Text;

namespace NShared.BuyCustomizingCostumes;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "hero/buy_customizing_costumes";

	public int HeroIndex { get; set; }

	public string AccessoryCostumePositionInfo { get; set; }

	public int[] UnsetAccessoryCostumeIndices { get; set; }

	public int CostumeIndex { get; set; }

	public int WeaponCostumeIndex { get; set; }

	public byte HideUniqueWeapon { get; set; }

	public int HairCostumeIndex { get; set; }

	public int BuyGem { get; set; }

	public int BuyGold { get; set; }

	public string Log()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine($"HeroIndex ({HeroIndex})");
		stringBuilder.AppendLine($"HairCostumeIndex ({HairCostumeIndex})");
		stringBuilder.AppendLine($"CostumeIndex ({CostumeIndex})");
		stringBuilder.AppendLine($"HideUniqueWeapon ({HideUniqueWeapon})");
		stringBuilder.AppendLine($"WeaponCostumeIndex ({WeaponCostumeIndex})");
		if (!string.IsNullOrEmpty(AccessoryCostumePositionInfo))
		{
			string arg = AccessoryCostumePositionInfo.Replace(',', '\n');
			stringBuilder.AppendLine($"AccessoryCostumePositionInfo ({arg})");
		}
		if (UnsetAccessoryCostumeIndices != null && UnsetAccessoryCostumeIndices.Length != 0)
		{
			stringBuilder.AppendLine("UnsetAccessoryCostumeIndices");
			for (int i = 0; i < UnsetAccessoryCostumeIndices.Length; i++)
			{
				stringBuilder.AppendLine($"({UnsetAccessoryCostumeIndices[i]}), ");
			}
		}
		stringBuilder.AppendLine($"BuyGem ({BuyGem})");
		stringBuilder.AppendLine($"BuyGold ({BuyGold})");
		return stringBuilder.ToString();
	}
}
