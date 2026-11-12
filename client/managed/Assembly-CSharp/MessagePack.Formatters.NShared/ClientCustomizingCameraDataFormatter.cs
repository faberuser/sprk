using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class ClientCustomizingCameraDataFormatter : IMessagePackFormatter<ClientCustomizingCameraData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, ClientCustomizingCameraData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(5);
		resolver.GetFormatterWithVerify<AccessoryPartType>().Serialize(ref writer, value.PartType, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.TransformName, options);
		writer.Write(value.CreatureRotation);
		writer.Write(value.ZoomRate);
		resolver.GetFormatterWithVerify<float[]>().Serialize(ref writer, value.ExtraOffset, options);
	}

	public ClientCustomizingCameraData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		ClientCustomizingCameraData clientCustomizingCameraData = new ClientCustomizingCameraData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				clientCustomizingCameraData.PartType = resolver.GetFormatterWithVerify<AccessoryPartType>().Deserialize(ref reader, options);
				break;
			case 1:
				clientCustomizingCameraData.TransformName = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 2:
				clientCustomizingCameraData.CreatureRotation = reader.ReadSingle();
				break;
			case 3:
				clientCustomizingCameraData.ZoomRate = reader.ReadSingle();
				break;
			case 4:
				clientCustomizingCameraData.ExtraOffset = resolver.GetFormatterWithVerify<float[]>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return clientCustomizingCameraData;
	}
}
