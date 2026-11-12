using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class DeviceModelDataFormatter : IMessagePackFormatter<DeviceModelData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, DeviceModelData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(6);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.DeviceModel, options);
		writer.Write(value.UseRegularExpression);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ForceMemorySpec, options);
		resolver.GetFormatterWithVerify<DeviceStyleType>().Serialize(ref writer, value.DeviceStyle, options);
		resolver.GetFormatterWithVerify<float[]>().Serialize(ref writer, value.ScreenRate, options);
	}

	public DeviceModelData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		DeviceModelData deviceModelData = new DeviceModelData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				deviceModelData.Index = reader.ReadInt32();
				break;
			case 1:
				deviceModelData.DeviceModel = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 2:
				deviceModelData.UseRegularExpression = reader.ReadBoolean();
				break;
			case 3:
				deviceModelData.ForceMemorySpec = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 4:
				deviceModelData.DeviceStyle = resolver.GetFormatterWithVerify<DeviceStyleType>().Deserialize(ref reader, options);
				break;
			case 5:
				deviceModelData.ScreenRate = resolver.GetFormatterWithVerify<float[]>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return deviceModelData;
	}
}
