using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class HeroFriendlyActionDataFormatter : IMessagePackFormatter<HeroFriendlyActionData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, HeroFriendlyActionData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(22);
		writer.Write(value.HeroIndex);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.BaseSpeak, options);
		resolver.GetFormatterWithVerify<FriendlyActionType>().Serialize(ref writer, value.Action1, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.ActionResponse1, options);
		resolver.GetFormatterWithVerify<FriendlyActionType>().Serialize(ref writer, value.Action2, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.ActionResponse2, options);
		resolver.GetFormatterWithVerify<FriendlyActionType>().Serialize(ref writer, value.Action3, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.ActionResponse3, options);
		resolver.GetFormatterWithVerify<FriendlyActionType>().Serialize(ref writer, value.Action4, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.ActionResponse4, options);
		resolver.GetFormatterWithVerify<FriendlyActionType>().Serialize(ref writer, value.Action5, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.ActionResponse5, options);
		resolver.GetFormatterWithVerify<FriendlyActionType>().Serialize(ref writer, value.Action6, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.ActionResponse6, options);
		resolver.GetFormatterWithVerify<FriendlyActionType>().Serialize(ref writer, value.Action1_MAX, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.Response1_MAX, options);
		resolver.GetFormatterWithVerify<FriendlyActionType>().Serialize(ref writer, value.MaxAction1, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.MaxResponse1, options);
		resolver.GetFormatterWithVerify<FriendlyActionType>().Serialize(ref writer, value.MaxAction2, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.MaxResponse2, options);
		resolver.GetFormatterWithVerify<FriendlyActionType>().Serialize(ref writer, value.MaxAction3, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.MaxResponse3, options);
	}

	public HeroFriendlyActionData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		HeroFriendlyActionData heroFriendlyActionData = new HeroFriendlyActionData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				heroFriendlyActionData.HeroIndex = reader.ReadInt32();
				break;
			case 1:
				heroFriendlyActionData.BaseSpeak = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 2:
				heroFriendlyActionData.Action1 = resolver.GetFormatterWithVerify<FriendlyActionType>().Deserialize(ref reader, options);
				break;
			case 3:
				heroFriendlyActionData.ActionResponse1 = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 4:
				heroFriendlyActionData.Action2 = resolver.GetFormatterWithVerify<FriendlyActionType>().Deserialize(ref reader, options);
				break;
			case 5:
				heroFriendlyActionData.ActionResponse2 = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 6:
				heroFriendlyActionData.Action3 = resolver.GetFormatterWithVerify<FriendlyActionType>().Deserialize(ref reader, options);
				break;
			case 7:
				heroFriendlyActionData.ActionResponse3 = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 8:
				heroFriendlyActionData.Action4 = resolver.GetFormatterWithVerify<FriendlyActionType>().Deserialize(ref reader, options);
				break;
			case 9:
				heroFriendlyActionData.ActionResponse4 = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 10:
				heroFriendlyActionData.Action5 = resolver.GetFormatterWithVerify<FriendlyActionType>().Deserialize(ref reader, options);
				break;
			case 11:
				heroFriendlyActionData.ActionResponse5 = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 12:
				heroFriendlyActionData.Action6 = resolver.GetFormatterWithVerify<FriendlyActionType>().Deserialize(ref reader, options);
				break;
			case 13:
				heroFriendlyActionData.ActionResponse6 = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 14:
				heroFriendlyActionData.Action1_MAX = resolver.GetFormatterWithVerify<FriendlyActionType>().Deserialize(ref reader, options);
				break;
			case 15:
				heroFriendlyActionData.Response1_MAX = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 16:
				heroFriendlyActionData.MaxAction1 = resolver.GetFormatterWithVerify<FriendlyActionType>().Deserialize(ref reader, options);
				break;
			case 17:
				heroFriendlyActionData.MaxResponse1 = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 18:
				heroFriendlyActionData.MaxAction2 = resolver.GetFormatterWithVerify<FriendlyActionType>().Deserialize(ref reader, options);
				break;
			case 19:
				heroFriendlyActionData.MaxResponse2 = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 20:
				heroFriendlyActionData.MaxAction3 = resolver.GetFormatterWithVerify<FriendlyActionType>().Deserialize(ref reader, options);
				break;
			case 21:
				heroFriendlyActionData.MaxResponse3 = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return heroFriendlyActionData;
	}
}
