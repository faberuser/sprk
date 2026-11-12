using System;
using NShared.NJit.NCommon;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_EPunishmentDungeonTypeValueResolver : IDataResolverT<EPunishmentDungeonType>, IDataResolver
{
	private Type HandleType => typeof(EPunishmentDungeonType);

	Type IDataResolver.GetResolveType()
	{
		return HandleType;
	}

	byte[] IDataResolver.SerializeObject(object obj, JitSerializer jitSerializer)
	{
		if (obj == null || obj.GetType() != typeof(EPunishmentDungeonType))
		{
			return SimpleBufferWriter.ErrorWrite(0);
		}
		return SerializeValue((EPunishmentDungeonType)obj, jitSerializer);
	}

	public byte[] SerializeValue(EPunishmentDungeonType value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public byte[] SerializeReference(EPunishmentDungeonType value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public EPunishmentDungeonType Evaluate(ByteSpan span, JitPacker packer)
	{
		ByteSpan nextSpan;
		return (EPunishmentDungeonType)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void EvaluateLater(EPunishmentDungeonType target, ByteSpan span, JitPacker packer)
	{
	}

	public EPunishmentDungeonType EvaluateReference(ByteSpan span, JitPacker packer, out ByteSpan nextSpan)
	{
		return (EPunishmentDungeonType)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void SkipEvaluateReference(ByteSpan span, out ByteSpan nextSpan)
	{
		span.ReadByMsgPack_Int32(0, out nextSpan);
	}
}
