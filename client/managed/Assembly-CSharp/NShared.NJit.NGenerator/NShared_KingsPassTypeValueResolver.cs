using System;
using NShared.NJit.NCommon;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_KingsPassTypeValueResolver : IDataResolverT<KingsPassType>, IDataResolver
{
	private Type HandleType => typeof(KingsPassType);

	Type IDataResolver.GetResolveType()
	{
		return HandleType;
	}

	byte[] IDataResolver.SerializeObject(object obj, JitSerializer jitSerializer)
	{
		if (obj == null || obj.GetType() != typeof(KingsPassType))
		{
			return SimpleBufferWriter.ErrorWrite(0);
		}
		return SerializeValue((KingsPassType)obj, jitSerializer);
	}

	public byte[] SerializeValue(KingsPassType value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public byte[] SerializeReference(KingsPassType value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public KingsPassType Evaluate(ByteSpan span, JitPacker packer)
	{
		ByteSpan nextSpan;
		return (KingsPassType)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void EvaluateLater(KingsPassType target, ByteSpan span, JitPacker packer)
	{
	}

	public KingsPassType EvaluateReference(ByteSpan span, JitPacker packer, out ByteSpan nextSpan)
	{
		return (KingsPassType)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void SkipEvaluateReference(ByteSpan span, out ByteSpan nextSpan)
	{
		span.ReadByMsgPack_Int32(0, out nextSpan);
	}
}
