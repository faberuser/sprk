using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class ClientCustomizingPivotDataKeyFormatter : IMessagePackFormatter<ClientCustomizingPivotDataKey>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, ClientCustomizingPivotDataKey value, MessagePackSerializerOptions options)
	{
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(2);
		writer.Write(value.HeroIndex);
		resolver.GetFormatterWithVerify<AccessoryPartPivotType>().Serialize(ref writer, value.PivotType, options);
	}

	public ClientCustomizingPivotDataKey Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		int heroIndex = 0;
		AccessoryPartPivotType pivotType = AccessoryPartPivotType.None;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				heroIndex = reader.ReadInt32();
				break;
			case 1:
				pivotType = resolver.GetFormatterWithVerify<AccessoryPartPivotType>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		ClientCustomizingPivotDataKey result = new ClientCustomizingPivotDataKey(heroIndex, pivotType);
		reader.Depth--;
		return result;
	}
}
