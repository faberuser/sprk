using NShared.NJit;

namespace NShared;

public struct TablePathDefine
{
	public enum ELoadFrom
	{
		TableData,
		LocalizationRepo
	}

	public string Path { get; private set; }

	public ELoadFrom LoadFrom { get; private set; }

	public int Version { get; private set; }

	public string JitTablePath => GetJitTablePath();

	public string JsonTablePath_NoExtension => GetJsonTablePath();

	public string JsonTablePath_WithExtension => GetJsonTablePath() + ".json";

	public TablePathDefine(string path, ELoadFrom loadFrom = ELoadFrom.TableData, int version = 0)
	{
		this = default(TablePathDefine);
		Path = path;
		LoadFrom = loadFrom;
		Version = version;
	}

	public override string ToString()
	{
		return Path;
	}

	public bool IsValid()
	{
		return !string.IsNullOrEmpty(Path);
	}

	private string GetJitTablePath()
	{
		if (LoadFrom == ELoadFrom.TableData)
		{
			return String_Extension_Method.CombinePath(GlobalPathDefine.TableJit, Path) + ".jit";
		}
		if (LoadFrom == ELoadFrom.LocalizationRepo)
		{
			return String_Extension_Method.CombinePath(GlobalPathDefine.Localization_TableJit, Path) + ".jit";
		}
		JitOption.JitLogError($"미구현 옵션: LoadFrom={LoadFrom}, Path={Path}");
		return String_Extension_Method.CombinePath(GlobalPathDefine.TableJit, Path) + ".jit";
	}

	private string GetJsonTablePath()
	{
		if (LoadFrom == ELoadFrom.TableData)
		{
			return String_Extension_Method.CombinePath(GlobalPathDefine.TableData, Path);
		}
		if (LoadFrom == ELoadFrom.LocalizationRepo)
		{
			return String_Extension_Method.CombinePath(GlobalPathDefine.LocalizationRepository_in_Editor, Path);
		}
		JitOption.JitLogError($"미구현 옵션: LoadFrom={LoadFrom}, Path={Path}");
		return String_Extension_Method.CombinePath(GlobalPathDefine.TableData, Path);
	}
}
