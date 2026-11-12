using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class ClientCustomizingPivotDataFormatter : IMessagePackFormatter<ClientCustomizingPivotData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, ClientCustomizingPivotData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(11);
		writer.Write(value.HeroIndex);
		resolver.GetFormatterWithVerify<AccessoryPartPivotType>().Serialize(ref writer, value.PivotType, options);
		writer.Write(value.PresetHeroIndex);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.TransformName1, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.TransformName2, options);
		resolver.GetFormatterWithVerify<float[]>().Serialize(ref writer, value.InitPosition, options);
		resolver.GetFormatterWithVerify<float[]>().Serialize(ref writer, value.InitRotation, options);
		writer.Write(value.InitScale);
		resolver.GetFormatterWithVerify<float[]>().Serialize(ref writer, value.PositionVariation, options);
		resolver.GetFormatterWithVerify<float[]>().Serialize(ref writer, value.RotationVariation, options);
		writer.Write(value.ScaleVariation);
	}

	public ClientCustomizingPivotData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		ClientCustomizingPivotData clientCustomizingPivotData = new ClientCustomizingPivotData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				clientCustomizingPivotData.HeroIndex = reader.ReadInt32();
				break;
			case 1:
				clientCustomizingPivotData.PivotType = resolver.GetFormatterWithVerify<AccessoryPartPivotType>().Deserialize(ref reader, options);
				break;
			case 2:
				clientCustomizingPivotData.PresetHeroIndex = reader.ReadInt32();
				break;
			case 3:
				clientCustomizingPivotData.TransformName1 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 4:
				clientCustomizingPivotData.TransformName2 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 5:
				clientCustomizingPivotData.InitPosition = resolver.GetFormatterWithVerify<float[]>().Deserialize(ref reader, options);
				break;
			case 6:
				clientCustomizingPivotData.InitRotation = resolver.GetFormatterWithVerify<float[]>().Deserialize(ref reader, options);
				break;
			case 7:
				clientCustomizingPivotData.InitScale = reader.ReadSingle();
				break;
			case 8:
				clientCustomizingPivotData.PositionVariation = resolver.GetFormatterWithVerify<float[]>().Deserialize(ref reader, options);
				break;
			case 9:
				clientCustomizingPivotData.RotationVariation = resolver.GetFormatterWithVerify<float[]>().Deserialize(ref reader, options);
				break;
			case 10:
				clientCustomizingPivotData.ScaleVariation = reader.ReadSingle();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return clientCustomizingPivotData;
	}
}
