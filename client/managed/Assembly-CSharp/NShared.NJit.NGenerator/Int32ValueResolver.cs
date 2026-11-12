using System;
using NShared.NJit.NCommon;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class Int32ValueResolver : IDataResolverT<int>, IDataResolver
{
	private Type HandleType => typeof(int);

	Type IDataResolver.GetResolveType()
	{
		return HandleType;
	}

	byte[] IDataResolver.SerializeObject(object obj, JitSerializer jitSerializer)
	{
		if (obj == null || obj.GetType() != typeof(int))
		{
			return SimpleBufferWriter.ErrorWrite(0);
		}
		return SerializeValue((int)obj, jitSerializer);
	}

	public byte[] SerializeValue(int value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack(value);
		return simpleBufferWriter.ToArray();
	}

	public byte[] SerializeReference(int value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack(value);
		return simpleBufferWriter.ToArray();
	}

	public int Evaluate(ByteSpan span, JitPacker packer)
	{
		ByteSpan nextSpan;
		return span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void EvaluateLater(int target, ByteSpan span, JitPacker packer)
	{
	}

	public int EvaluateReference(ByteSpan span, JitPacker packer, out ByteSpan nextSpan)
	{
		return span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void SkipEvaluateReference(ByteSpan span, out ByteSpan nextSpan)
	{
		span.ReadByMsgPack_Int32(0, out nextSpan);
	}
}
