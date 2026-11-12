using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class DbLogDataContainer : JitDataContainerDefault<DbLogDataKey, DbLogData>
{
	public const string TABLENAME = "Server/DbLogTable";

	private JitRef<HashSet<string>> _categoryList = new JitRef<HashSet<string>>();

	private JitDictionary<DbLogDataKey, string> _nameDictionary = new JitDictionary<DbLogDataKey, string>();

	private JitDictionary<DbLogDataKey, string> _typeDictionary = new JitDictionary<DbLogDataKey, string>();

	public override TablePathDefine PathDefine => new TablePathDefine("Server/DbLogTable");

	public DbLogDataContainer()
	{
		base.Packer.RegisterAndBind(_categoryList);
		base.Packer.RegisterAndBind(_nameDictionary);
		base.Packer.RegisterAndBind(_typeDictionary);
	}

	protected override void OnEndLoadFromJsonData()
	{
		foreach (DbLogData item in base.BaseList.JsonLoad_GetOrNew())
		{
			_categoryList.JsonLoad_GetOrNew().Add(ToKeyFormatting(item.Category));
			DbLogDataKey key = new DbLogDataKey(ToKeyFormatting(item.Category), item.Value);
			_nameDictionary.JsonLoad_Get()[key] = item.Name;
			_typeDictionary.JsonLoad_Get()[key] = item.Type;
		}
	}

	public bool HasCategory(string category)
	{
		return _categoryList.Eval().Contains(category);
	}

	public string GetNameByValue(string category, int value)
	{
		return _nameDictionary.GetOrDefault(new DbLogDataKey(category, value));
	}

	public string GetTypeByValue(string category, int value)
	{
		return _typeDictionary.GetOrDefault(new DbLogDataKey(category, value));
	}

	public static string ToKeyFormatting(string key)
	{
		return key.ToLower();
	}
}
