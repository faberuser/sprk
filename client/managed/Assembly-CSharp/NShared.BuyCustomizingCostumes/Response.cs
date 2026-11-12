using System.Text;

namespace NShared.BuyCustomizingCostumes;

[Response]
public class Response : GenericResponse<ResultType>
{
	public CurrencyResultInfo3[] CurrencyResults { get; set; }

	public CostumeInfo[] CostumeInfos { get; set; }

	public PlayerAccessoryCostumeInfo[] PlayerAccessoryCostumeInfos { get; set; }

	public WeaponCostumeInfo WeaponCostumeInfo { get; set; }

	public HairCostumeInfo HairCostumeInfo { get; set; }

	public int[] UnsetAccessoryCostumeIndices { get; set; }

	public HeroCostumeResultInfo HeroCostumeResultInfo { get; set; }

	public string Log()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine($"HeroIndex ({HeroCostumeResultInfo.HeroIndex})");
		stringBuilder.AppendLine($"HairCostumeIndex ({HeroCostumeResultInfo.HairCostumeIndex})");
		stringBuilder.AppendLine($"CostumeIndex ({HeroCostumeResultInfo.CostumeIndex})");
		stringBuilder.AppendLine($"HideUniqueWeapon ({HeroCostumeResultInfo.HideUniqueWeapon})");
		stringBuilder.AppendLine($"WeaponCostumeIndex ({HeroCostumeResultInfo.WeaponCostumeIndex})");
		stringBuilder.AppendLine($"AccessoryCostumeIndex1 ({HeroCostumeResultInfo.AccessoryCostumeIndex1})");
		stringBuilder.AppendLine($"AccessoryCostumeIndex2 ({HeroCostumeResultInfo.AccessoryCostumeIndex2})");
		stringBuilder.AppendLine($"AccessoryCostumeIndex3 ({HeroCostumeResultInfo.AccessoryCostumeIndex3})");
		stringBuilder.AppendLine($"AccessoryCostumeIndex4 ({HeroCostumeResultInfo.AccessoryCostumeIndex4})");
		if (PlayerAccessoryCostumeInfos != null && PlayerAccessoryCostumeInfos.Length != 0)
		{
			for (int i = 0; i < PlayerAccessoryCostumeInfos.Length; i++)
			{
				string arg = PlayerAccessoryCostumeInfos[i].PositionInfo.Replace(',', '\n');
				stringBuilder.AppendLine($"AccessoryIndex {PlayerAccessoryCostumeInfos[i].AccessoryCostumeIndex} \n PositionInfo {arg}");
			}
		}
		return stringBuilder.ToString();
	}
}
