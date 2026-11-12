using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class SoulWeaponEtherDataFormatter : IMessagePackFormatter<SoulWeaponEtherData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, SoulWeaponEtherData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(3);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ItemCode, options);
		writer.Write(value.ExpPoints);
		writer.Write(value.ReqGold);
	}

	public SoulWeaponEtherData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		SoulWeaponEtherData soulWeaponEtherData = new SoulWeaponEtherData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				soulWeaponEtherData.ItemCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 1:
				soulWeaponEtherData.ExpPoints = reader.ReadInt32();
				break;
			case 2:
				soulWeaponEtherData.ReqGold = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return soulWeaponEtherData;
	}
}
