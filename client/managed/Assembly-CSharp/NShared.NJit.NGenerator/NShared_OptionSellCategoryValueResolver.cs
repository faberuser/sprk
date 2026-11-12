using System;
using NShared.NJit.NCommon;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_OptionSellCategoryValueResolver : IDataResolverT<OptionSellCategory>, IDataResolver
{
	private Type HandleType => typeof(OptionSellCategory);

	Type IDataResolver.GetResolveType()
	{
		return HandleType;
	}

	byte[] IDataResolver.SerializeObject(object obj, JitSerializer jitSerializer)
	{
		if (obj == null || obj.GetType() != typeof(OptionSellCategory))
		{
			return SimpleBufferWriter.ErrorWrite(0);
		}
		return SerializeValue((OptionSellCategory)obj, jitSerializer);
	}

	public byte[] SerializeValue(OptionSellCategory value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public byte[] SerializeReference(OptionSellCategory value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public OptionSellCategory Evaluate(ByteSpan span, JitPacker packer)
	{
		ByteSpan nextSpan;
		return (OptionSellCategory)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void EvaluateLater(OptionSellCategory target, ByteSpan span, JitPacker packer)
	{
	}

	public OptionSellCategory EvaluateReference(ByteSpan span, JitPacker packer, out ByteSpan nextSpan)
	{
		return (OptionSellCategory)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void SkipEvaluateReference(ByteSpan span, out ByteSpan nextSpan)
	{
		span.ReadByMsgPack_Int32(0, out nextSpan);
	}
}
