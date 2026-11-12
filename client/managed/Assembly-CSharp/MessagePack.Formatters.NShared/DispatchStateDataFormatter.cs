using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class DispatchStateDataFormatter : IMessagePackFormatter<DispatchStateData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, DispatchStateData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(3);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<DispatchState>().Serialize(ref writer, value.DispatchState, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.StateDescKey, options);
	}

	public DispatchStateData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		DispatchStateData dispatchStateData = new DispatchStateData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				dispatchStateData.Index = reader.ReadInt32();
				break;
			case 1:
				dispatchStateData.DispatchState = resolver.GetFormatterWithVerify<DispatchState>().Deserialize(ref reader, options);
				break;
			case 2:
				dispatchStateData.StateDescKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return dispatchStateData;
	}
}
