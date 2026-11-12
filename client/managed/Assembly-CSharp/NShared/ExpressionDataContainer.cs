using NShared.NJit;
using NVespa;

namespace NShared;

public class ExpressionDataContainer : JitDataContainerDefault<int, ExpressionData>
{
	public const string TABLENAME = "ExpressionTable";

	private JitDictionary<string, ExpressionData> _data_by_NameLower = new JitDictionary<string, ExpressionData>();

	public override TablePathDefine PathDefine => new TablePathDefine("ExpressionTable");

	public ExpressionDataContainer()
	{
		base.Packer.RegisterAndBind(_data_by_NameLower);
	}

	protected override void OnEndLoadFromJsonData()
	{
		foreach (ExpressionData item in base.BaseList.JsonLoad_GetOrNew())
		{
			if (!String_Extension_Method.IsNullOrEmpty(item.Name))
			{
				string key = item.Name.ToLower();
				if (_data_by_NameLower.JsonLoad_Get().ContainsKey(key))
				{
					VespaDebug.LogError("Duplicated expression name : " + item.Name);
				}
				else
				{
					_data_by_NameLower.JsonLoad_AddIfNotExist(key, item);
				}
			}
		}
	}

	protected override void OnEndLoad_Final()
	{
		foreach (ExpressionData allDatum in GetAllData())
		{
			allDatum.SetExpressionData();
		}
	}

	public ExpressionData GetData(string name)
	{
		return _data_by_NameLower.GetOrDefault(name.ToLower());
	}
}
