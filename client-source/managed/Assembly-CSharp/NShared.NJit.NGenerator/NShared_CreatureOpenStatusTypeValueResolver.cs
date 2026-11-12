using System;
using NShared.NJit.NCommon;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_CreatureOpenStatusTypeValueResolver : IDataResolverT<CreatureOpenStatusType>, IDataResolver
{
	private Type HandleType => typeof(CreatureOpenStatusType);

	Type IDataResolver.GetResolveType()
	{
		return HandleType;
	}

	byte[] IDataResolver.SerializeObject(object obj, JitSerializer jitSerializer)
	{
		if (obj == null || obj.GetType() != typeof(CreatureOpenStatusType))
		{
			return SimpleBufferWriter.ErrorWrite(0);
		}
		return SerializeValue((CreatureOpenStatusType)obj, jitSerializer);
	}

	public byte[] SerializeValue(CreatureOpenStatusType value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public byte[] SerializeReference(CreatureOpenStatusType value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public CreatureOpenStatusType Evaluate(ByteSpan span, JitPacker packer)
	{
		ByteSpan nextSpan;
		return (CreatureOpenStatusType)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void EvaluateLater(CreatureOpenStatusType target, ByteSpan span, JitPacker packer)
	{
	}

	public CreatureOpenStatusType EvaluateReference(ByteSpan span, JitPacker packer, out ByteSpan nextSpan)
	{
		return (CreatureOpenStatusType)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void SkipEvaluateReference(ByteSpan span, out ByteSpan nextSpan)
	{
		span.ReadByMsgPack_Int32(0, out nextSpan);
	}
}
