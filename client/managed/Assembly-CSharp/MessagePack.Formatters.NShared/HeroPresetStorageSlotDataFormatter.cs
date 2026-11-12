using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class HeroPresetStorageSlotDataFormatter : IMessagePackFormatter<HeroPresetStorageSlotData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, HeroPresetStorageSlotData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(5);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.NameKey, options);
		writer.Write(value.IsPaid);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.StorageKey, options);
		writer.Write(value.BuyGem);
	}

	public HeroPresetStorageSlotData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		HeroPresetStorageSlotData heroPresetStorageSlotData = new HeroPresetStorageSlotData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				heroPresetStorageSlotData.Index = reader.ReadInt32();
				break;
			case 1:
				heroPresetStorageSlotData.NameKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 2:
				heroPresetStorageSlotData.IsPaid = reader.ReadBoolean();
				break;
			case 3:
				heroPresetStorageSlotData.StorageKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 4:
				heroPresetStorageSlotData.BuyGem = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return heroPresetStorageSlotData;
	}
}
