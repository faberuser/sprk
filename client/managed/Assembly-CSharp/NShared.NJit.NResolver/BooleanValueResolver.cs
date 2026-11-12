using System;
using NShared.NJit.NCommon;

namespace NShared.NJit.NResolver;

public class BooleanValueResolver : IDataResolverT<bool>, IDataResolver
{
	private Type HandleType => typeof(bool);

	Type IDataResolver.GetResolveType()
	{
		return HandleType;
	}

	byte[] IDataResolver.SerializeObject(object obj, JitSerializer jitSerializer)
	{
		if (obj == null || obj.GetType() != typeof(bool))
		{
			return SimpleBufferWriter.ErrorWrite(0);
		}
		return SerializeValue((bool)obj, jitSerializer);
	}

	public byte[] SerializeValue(bool value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteByMsgPack(value);
		return simpleBufferWriter.ToArray();
	}

	public byte[] SerializeReference(bool value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteByMsgPack(value);
		return simpleBufferWriter.ToArray();
	}

	public bool Evaluate(ByteSpan span, JitPacker packer)
	{
		ByteSpan nextSpan;
		return span.ReadByMsgPackT<bool>(0, out nextSpan);
	}

	public void EvaluateLater(bool target, ByteSpan span, JitPacker packer)
	{
	}

	public bool EvaluateReference(ByteSpan span, JitPacker packer, out ByteSpan nextSpan)
	{
		return span.ReadByMsgPackT<bool>(0, out nextSpan);
	}

	public void SkipEvaluateReference(ByteSpan span, out ByteSpan nextSpan)
	{
		span.ReadByMsgPackT<bool>(0, out nextSpan);
	}
}
