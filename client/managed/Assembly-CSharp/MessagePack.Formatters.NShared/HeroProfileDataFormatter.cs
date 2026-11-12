using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class HeroProfileDataFormatter : IMessagePackFormatter<HeroProfileData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, HeroProfileData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(15);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Gender, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Race, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Age, options);
		writer.Write(value.Length);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.Birth, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.MonthText, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Constellation, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Like, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Unlike, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Story, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.CVKor, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.CVJap, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.Emoti, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.HeroName, options);
	}

	public HeroProfileData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		HeroProfileData heroProfileData = new HeroProfileData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				heroProfileData.Index = reader.ReadInt32();
				break;
			case 1:
				heroProfileData.Gender = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 2:
				heroProfileData.Race = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 3:
				heroProfileData.Age = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 4:
				heroProfileData.Length = reader.ReadInt32();
				break;
			case 5:
				heroProfileData.Birth = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 6:
				heroProfileData.MonthText = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 7:
				heroProfileData.Constellation = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 8:
				heroProfileData.Like = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 9:
				heroProfileData.Unlike = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 10:
				heroProfileData.Story = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 11:
				heroProfileData.CVKor = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 12:
				heroProfileData.CVJap = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 13:
				heroProfileData.Emoti = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 14:
				heroProfileData.HeroName = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return heroProfileData;
	}
}
