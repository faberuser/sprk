using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class CutSceneForegroundDataFormatter : IMessagePackFormatter<CutSceneForegroundData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, CutSceneForegroundData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(2);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.DecorateWidget, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.AssetPath, options);
	}

	public CutSceneForegroundData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		CutSceneForegroundData cutSceneForegroundData = new CutSceneForegroundData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				cutSceneForegroundData.DecorateWidget = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 1:
				cutSceneForegroundData.AssetPath = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return cutSceneForegroundData;
	}
}
