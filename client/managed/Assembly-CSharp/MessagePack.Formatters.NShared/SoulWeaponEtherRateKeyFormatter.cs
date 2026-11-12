using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class SoulWeaponEtherRateKeyFormatter : IMessagePackFormatter<SoulWeaponEtherRateKey>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, SoulWeaponEtherRateKey value, MessagePackSerializerOptions options)
	{
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(2);
		writer.Write(value.ReinforceLevel);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ItemCode, options);
	}

	public SoulWeaponEtherRateKey Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		int reinforceLevel = 0;
		string itemCode = null;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				reinforceLevel = reader.ReadInt32();
				break;
			case 1:
				itemCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		SoulWeaponEtherRateKey result = new SoulWeaponEtherRateKey(reinforceLevel, itemCode);
		reader.Depth--;
		return result;
	}
}
