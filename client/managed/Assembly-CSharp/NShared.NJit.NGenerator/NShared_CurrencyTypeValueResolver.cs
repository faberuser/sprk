using System;
using NShared.NJit.NCommon;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_CurrencyTypeValueResolver : IDataResolverT<CurrencyType>, IDataResolver
{
	private Type HandleType => typeof(CurrencyType);

	Type IDataResolver.GetResolveType()
	{
		return HandleType;
	}

	byte[] IDataResolver.SerializeObject(object obj, JitSerializer jitSerializer)
	{
		if (obj == null || obj.GetType() != typeof(CurrencyType))
		{
			return SimpleBufferWriter.ErrorWrite(0);
		}
		return SerializeValue((CurrencyType)obj, jitSerializer);
	}

	public byte[] SerializeValue(CurrencyType value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public byte[] SerializeReference(CurrencyType value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public CurrencyType Evaluate(ByteSpan span, JitPacker packer)
	{
		ByteSpan nextSpan;
		return (CurrencyType)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void EvaluateLater(CurrencyType target, ByteSpan span, JitPacker packer)
	{
	}

	public CurrencyType EvaluateReference(ByteSpan span, JitPacker packer, out ByteSpan nextSpan)
	{
		return (CurrencyType)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void SkipEvaluateReference(ByteSpan span, out ByteSpan nextSpan)
	{
		span.ReadByMsgPack_Int32(0, out nextSpan);
	}
}
