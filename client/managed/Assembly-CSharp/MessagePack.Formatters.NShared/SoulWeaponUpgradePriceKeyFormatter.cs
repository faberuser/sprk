using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class SoulWeaponUpgradePriceKeyFormatter : IMessagePackFormatter<SoulWeaponUpgradePriceKey>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, SoulWeaponUpgradePriceKey value, MessagePackSerializerOptions options)
	{
		writer.WriteArrayHeader(2);
		writer.Write(value.ItemIndex);
		writer.Write(value.Grade);
	}

	public SoulWeaponUpgradePriceKey Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		int itemIndex = 0;
		int grade = 0;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				itemIndex = reader.ReadInt32();
				break;
			case 1:
				grade = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		SoulWeaponUpgradePriceKey result = new SoulWeaponUpgradePriceKey(itemIndex, grade);
		reader.Depth--;
		return result;
	}
}
