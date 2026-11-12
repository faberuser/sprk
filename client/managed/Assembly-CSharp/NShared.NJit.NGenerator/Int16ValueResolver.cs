using System;
using NShared.NJit.NCommon;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class Int16ValueResolver : IDataResolverT<short>, IDataResolver
{
	private Type HandleType => typeof(short);

	Type IDataResolver.GetResolveType()
	{
		return HandleType;
	}

	byte[] IDataResolver.SerializeObject(object obj, JitSerializer jitSerializer)
	{
		if (obj == null || obj.GetType() != typeof(short))
		{
			return SimpleBufferWriter.ErrorWrite(0);
		}
		return SerializeValue((short)obj, jitSerializer);
	}

	public byte[] SerializeValue(short value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt16ByMsgPack(value);
		return simpleBufferWriter.ToArray();
	}

	public byte[] SerializeReference(short value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt16ByMsgPack(value);
		return simpleBufferWriter.ToArray();
	}

	public short Evaluate(ByteSpan span, JitPacker packer)
	{
		ByteSpan nextSpan;
		return span.ReadByMsgPack_Int16(0, out nextSpan);
	}

	public void EvaluateLater(short target, ByteSpan span, JitPacker packer)
	{
	}

	public short EvaluateReference(ByteSpan span, JitPacker packer, out ByteSpan nextSpan)
	{
		return span.ReadByMsgPack_Int16(0, out nextSpan);
	}

	public void SkipEvaluateReference(ByteSpan span, out ByteSpan nextSpan)
	{
		span.ReadByMsgPack_Int16(0, out nextSpan);
	}
}
