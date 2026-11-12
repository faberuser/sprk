using System.Collections.Generic;
using System.Linq;
using MessagePack;
using NShared.NJit;

namespace NShared;

public class GatewayToTrialDataContainer : JitDataContainerDefault<int, GatewayToTrialData>
{
	[MessagePackObject(false)]
	public class ContainerInfo
	{
		[Key(0)]
		public int MinStep { get; set; }

		[Key(1)]
		public int MaxStep { get; set; }
	}

	public const string TABLENAME = "GatewayToTrialTable";

	private JitDictionary<int, GatewayToTrialData> _datas_by_Step = new JitDictionary<int, GatewayToTrialData>();

	private JitRef<ContainerInfo> _containerInfo = new JitRef<ContainerInfo>();

	public override TablePathDefine PathDefine => new TablePathDefine("GatewayToTrialTable");

	public int MinStep => _containerInfo.Eval().MinStep;

	public int MaxStep => _containerInfo.Eval().MaxStep;

	public GatewayToTrialDataContainer()
	{
		base.Packer.RegisterAndBind(_datas_by_Step);
		base.Packer.RegisterAndBind(_containerInfo);
	}

	protected override void OnEndLoadFromJsonData()
	{
		List<GatewayToTrialData> list = base.BaseList.JsonLoad_GetOrNew();
		Dictionary<int, GatewayToTrialData> dictionary = _datas_by_Step.JsonLoad_Get();
		ContainerInfo containerInfo = _containerInfo.JsonLoad_GetOrNew();
		foreach (GatewayToTrialData item in list)
		{
			dictionary[item.Step] = item;
		}
		containerInfo.MinStep = (from v in GetAllData()
			select v.Step).DefaultIfEmpty(int.MaxValue).Min();
		containerInfo.MaxStep = (from v in GetAllData()
			select v.Step).DefaultIfEmpty(int.MinValue).Max();
	}

	public GatewayToTrialData GetDataByStep(int step)
	{
		return _datas_by_Step.GetOrDefault(step);
	}
}
