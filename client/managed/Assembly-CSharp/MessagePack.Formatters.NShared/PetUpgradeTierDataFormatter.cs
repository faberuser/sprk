using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class PetUpgradeTierDataFormatter : IMessagePackFormatter<PetUpgradeTierData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, PetUpgradeTierData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(4);
		writer.Write(value.Grade);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ResultPetGroupCode, options);
		writer.Write(value.ItemCount);
		writer.Write(value.ReqGold);
	}

	public PetUpgradeTierData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		PetUpgradeTierData petUpgradeTierData = new PetUpgradeTierData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				petUpgradeTierData.Grade = reader.ReadInt32();
				break;
			case 1:
				petUpgradeTierData.ResultPetGroupCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 2:
				petUpgradeTierData.ItemCount = reader.ReadInt32();
				break;
			case 3:
				petUpgradeTierData.ReqGold = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return petUpgradeTierData;
	}
}
