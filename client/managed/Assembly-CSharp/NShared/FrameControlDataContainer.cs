using System.Collections;
using System.Text;
using NShared.NJit;

namespace NShared;

public class FrameControlDataContainer : JitDataContainerDefault<string, FrameControlData>
{
	public const string TABLENAME = "FrameControlTable";

	private const string PREFAB_NAME_POSTFIX = "(Clone)";

	public override TablePathDefine PathDefine => new TablePathDefine("FrameControlTable");

	protected override void OnLoadingValue_from_JsonData(IDictionary jsonDic, FrameControlData value)
	{
		if (value.ContentType != EContentType.Scene)
		{
			StringBuilder stringBuilder = new StringBuilder(value.Name);
			stringBuilder.Append("(Clone)");
			value.Name = stringBuilder.ToString();
		}
	}
}
