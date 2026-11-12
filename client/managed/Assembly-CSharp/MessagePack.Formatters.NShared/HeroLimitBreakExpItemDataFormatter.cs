using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class HeroLimitBreakExpItemDataFormatter : IMessagePackFormatter<HeroLimitBreakExpItemData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, HeroLimitBreakExpItemData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(2);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ItemCode, options);
		writer.Write(value.ExpAmount);
	}

	public HeroLimitBreakExpItemData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		HeroLimitBreakExpItemData heroLimitBreakExpItemData = new HeroLimitBreakExpItemData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				heroLimitBreakExpItemData.ItemCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 1:
				heroLimitBreakExpItemData.ExpAmount = reader.ReadInt64();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return heroLimitBreakExpItemData;
	}
}
