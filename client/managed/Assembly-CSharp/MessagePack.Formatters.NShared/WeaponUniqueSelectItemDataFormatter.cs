using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class WeaponUniqueSelectItemDataFormatter : IMessagePackFormatter<WeaponUniqueSelectItemData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, WeaponUniqueSelectItemData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(7);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ItemCode, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.ItemIndices, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.TitleKey, options);
		writer.Write(value.SelectOptionCount);
		writer.Write(value.Star);
		writer.Write(value.Level);
		writer.Write(value.SelectUniqueOption);
	}

	public WeaponUniqueSelectItemData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		WeaponUniqueSelectItemData weaponUniqueSelectItemData = new WeaponUniqueSelectItemData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				weaponUniqueSelectItemData.ItemCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 1:
				weaponUniqueSelectItemData.ItemIndices = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 2:
				weaponUniqueSelectItemData.TitleKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 3:
				weaponUniqueSelectItemData.SelectOptionCount = reader.ReadInt32();
				break;
			case 4:
				weaponUniqueSelectItemData.Star = reader.ReadInt32();
				break;
			case 5:
				weaponUniqueSelectItemData.Level = reader.ReadInt32();
				break;
			case 6:
				weaponUniqueSelectItemData.SelectUniqueOption = reader.ReadBoolean();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return weaponUniqueSelectItemData;
	}
}
