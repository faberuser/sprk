using System;
using NShared.NJit.NCommon;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_StatTypeValueResolver : IDataResolverT<StatType>, IDataResolver
{
	private Type HandleType => typeof(StatType);

	Type IDataResolver.GetResolveType()
	{
		return HandleType;
	}

	byte[] IDataResolver.SerializeObject(object obj, JitSerializer jitSerializer)
	{
		if (obj == null || obj.GetType() != typeof(StatType))
		{
			return SimpleBufferWriter.ErrorWrite(0);
		}
		return SerializeValue((StatType)obj, jitSerializer);
	}

	public byte[] SerializeValue(StatType value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public byte[] SerializeReference(StatType value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public StatType Evaluate(ByteSpan span, JitPacker packer)
	{
		ByteSpan nextSpan;
		return (StatType)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void EvaluateLater(StatType target, ByteSpan span, JitPacker packer)
	{
	}

	public StatType EvaluateReference(ByteSpan span, JitPacker packer, out ByteSpan nextSpan)
	{
		return (StatType)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void SkipEvaluateReference(ByteSpan span, out ByteSpan nextSpan)
	{
		span.ReadByMsgPack_Int32(0, out nextSpan);
	}
}
