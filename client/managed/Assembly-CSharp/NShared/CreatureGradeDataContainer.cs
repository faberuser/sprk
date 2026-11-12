using System.Linq;
using MessagePack;
using NShared.NJit;

namespace NShared;

public class CreatureGradeDataContainer : JitDataContainerDefault<int, CreatureGradeData>
{
	[MessagePackObject(false)]
	public class ContainerInfo
	{
		[Key(0)]
		public int MinGrade { get; set; }

		[Key(1)]
		public int MaxGrade { get; set; }
	}

	public const string TABLENAME = "CreatureGradeTable";

	private JitRef<ContainerInfo> _containerInfo = new JitRef<ContainerInfo>();

	public override TablePathDefine PathDefine => new TablePathDefine("CreatureGradeTable");

	public int MinGrade => _containerInfo.Eval().MinGrade;

	public int MaxGrade => _containerInfo.Eval().MaxGrade;

	public CreatureGradeDataContainer()
	{
		base.Packer.RegisterAndBind(_containerInfo);
	}

	protected override void OnEndLoadFromJsonData()
	{
		_containerInfo.JsonLoad_GetOrNew().MinGrade = (from v in base.BaseList.JsonLoad_GetOrNew()
			select v.Grade).DefaultIfEmpty(0).Min();
		_containerInfo.JsonLoad_GetOrNew().MaxGrade = (from v in base.BaseList.JsonLoad_GetOrNew()
			select v.Grade).DefaultIfEmpty(0).Max();
	}
}
