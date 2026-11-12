using System;
using NShared.NJit.NCommon;
using NShared.NJit.NResolver;
using NShared.NLocalization;

namespace NShared.NJit.NGenerator;

public class NShared_NLocalization_LanguageTypeValueResolver : IDataResolverT<LanguageType>, IDataResolver
{
	private Type HandleType => typeof(LanguageType);

	Type IDataResolver.GetResolveType()
	{
		return HandleType;
	}

	byte[] IDataResolver.SerializeObject(object obj, JitSerializer jitSerializer)
	{
		if (obj == null || obj.GetType() != typeof(LanguageType))
		{
			return SimpleBufferWriter.ErrorWrite(0);
		}
		return SerializeValue((LanguageType)obj, jitSerializer);
	}

	public byte[] SerializeValue(LanguageType value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public byte[] SerializeReference(LanguageType value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public LanguageType Evaluate(ByteSpan span, JitPacker packer)
	{
		ByteSpan nextSpan;
		return (LanguageType)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void EvaluateLater(LanguageType target, ByteSpan span, JitPacker packer)
	{
	}

	public LanguageType EvaluateReference(ByteSpan span, JitPacker packer, out ByteSpan nextSpan)
	{
		return (LanguageType)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void SkipEvaluateReference(ByteSpan span, out ByteSpan nextSpan)
	{
		span.ReadByMsgPack_Int32(0, out nextSpan);
	}
}
