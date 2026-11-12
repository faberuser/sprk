using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class SoulWeaponUpgradePriceDataFormatter : IMessagePackFormatter<SoulWeaponUpgradePriceData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, SoulWeaponUpgradePriceData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(5);
		writer.Write(value.ItemIndex);
		writer.Write(value.Grade);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.MaterialItemCode, options);
		writer.Write(value.MaterialItemCount);
		writer.Write(value.ReqGold);
	}

	public SoulWeaponUpgradePriceData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		SoulWeaponUpgradePriceData soulWeaponUpgradePriceData = new SoulWeaponUpgradePriceData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				soulWeaponUpgradePriceData.ItemIndex = reader.ReadInt32();
				break;
			case 1:
				soulWeaponUpgradePriceData.Grade = reader.ReadInt32();
				break;
			case 2:
				soulWeaponUpgradePriceData.MaterialItemCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 3:
				soulWeaponUpgradePriceData.MaterialItemCount = reader.ReadInt32();
				break;
			case 4:
				soulWeaponUpgradePriceData.ReqGold = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return soulWeaponUpgradePriceData;
	}
}
