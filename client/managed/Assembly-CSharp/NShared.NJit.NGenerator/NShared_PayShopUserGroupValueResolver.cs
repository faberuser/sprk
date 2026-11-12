using System;
using NShared.NJit.NCommon;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_PayShopUserGroupValueResolver : IDataResolverT<PayShopUserGroup>, IDataResolver
{
	private Type HandleType => typeof(PayShopUserGroup);

	Type IDataResolver.GetResolveType()
	{
		return HandleType;
	}

	byte[] IDataResolver.SerializeObject(object obj, JitSerializer jitSerializer)
	{
		if (obj == null || obj.GetType() != typeof(PayShopUserGroup))
		{
			return SimpleBufferWriter.ErrorWrite(0);
		}
		return SerializeValue((PayShopUserGroup)obj, jitSerializer);
	}

	public byte[] SerializeValue(PayShopUserGroup value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public byte[] SerializeReference(PayShopUserGroup value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public PayShopUserGroup Evaluate(ByteSpan span, JitPacker packer)
	{
		ByteSpan nextSpan;
		return (PayShopUserGroup)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void EvaluateLater(PayShopUserGroup target, ByteSpan span, JitPacker packer)
	{
	}

	public PayShopUserGroup EvaluateReference(ByteSpan span, JitPacker packer, out ByteSpan nextSpan)
	{
		return (PayShopUserGroup)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void SkipEvaluateReference(ByteSpan span, out ByteSpan nextSpan)
	{
		span.ReadByMsgPack_Int32(0, out nextSpan);
	}
}
