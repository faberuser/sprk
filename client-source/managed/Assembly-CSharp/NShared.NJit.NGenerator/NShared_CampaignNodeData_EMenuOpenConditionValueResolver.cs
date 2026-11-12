using System;
using NShared.NJit.NCommon;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_CampaignNodeData_EMenuOpenConditionValueResolver : IDataResolverT<CampaignNodeData.EMenuOpenCondition>, IDataResolver
{
	private Type HandleType => typeof(CampaignNodeData.EMenuOpenCondition);

	Type IDataResolver.GetResolveType()
	{
		return HandleType;
	}

	byte[] IDataResolver.SerializeObject(object obj, JitSerializer jitSerializer)
	{
		if (obj == null || obj.GetType() != typeof(CampaignNodeData.EMenuOpenCondition))
		{
			return SimpleBufferWriter.ErrorWrite(0);
		}
		return SerializeValue((CampaignNodeData.EMenuOpenCondition)obj, jitSerializer);
	}

	public byte[] SerializeValue(CampaignNodeData.EMenuOpenCondition value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public byte[] SerializeReference(CampaignNodeData.EMenuOpenCondition value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public CampaignNodeData.EMenuOpenCondition Evaluate(ByteSpan span, JitPacker packer)
	{
		ByteSpan nextSpan;
		return (CampaignNodeData.EMenuOpenCondition)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void EvaluateLater(CampaignNodeData.EMenuOpenCondition target, ByteSpan span, JitPacker packer)
	{
	}

	public CampaignNodeData.EMenuOpenCondition EvaluateReference(ByteSpan span, JitPacker packer, out ByteSpan nextSpan)
	{
		return (CampaignNodeData.EMenuOpenCondition)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void SkipEvaluateReference(ByteSpan span, out ByteSpan nextSpan)
	{
		span.ReadByMsgPack_Int32(0, out nextSpan);
	}
}
