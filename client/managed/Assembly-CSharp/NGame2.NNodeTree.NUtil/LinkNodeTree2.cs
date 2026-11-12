using LitJson;
using NGame2.NScene;
using NVespa;

namespace NGame2.NNodeTree.NUtil;

public class LinkNodeTree2 : IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public enum EGetResult
	{
		Ok,
		Loading,
		Invalid
	}

	public string PackPathName = string.Empty;

	public long TreeUid = -1L;

	public string Code = string.Empty;

	public TreeData GetTree(out EGetResult oResult)
	{
		return GetTree(ifExist: false, out oResult);
	}

	public TreeData GetTree(bool ifExist, out EGetResult oResult)
	{
		PackData pack = GetPack(ifExist, out oResult);
		if (pack == null)
		{
			return null;
		}
		for (int i = 0; i < pack.TreeList.Count; i++)
		{
			PackData.UidTree uidTree = pack.TreeList[i];
			if (TreeUid == uidTree.Uid)
			{
				oResult = EGetResult.Ok;
				return uidTree.Tree;
			}
		}
		oResult = EGetResult.Invalid;
		return null;
	}

	private PackData GetPack(bool ifExist, out EGetResult oResult)
	{
		if (String_Extension_Method.IsNullOrEmpty(PackPathName))
		{
			oResult = EGetResult.Invalid;
			return null;
		}
		PackLoad packLoad = ((!ifExist) ? SingletonAuto<PackContainer>.instance.GetPack(PackPathName) : SingletonAuto<PackContainer>.instance.GetExistPack(PackPathName));
		if (!packLoad.ISDONE())
		{
			oResult = EGetResult.Loading;
			return null;
		}
		if (!packLoad.ISVALID())
		{
			oResult = EGetResult.Invalid;
			return null;
		}
		oResult = EGetResult.Ok;
		return packLoad.Value;
	}

	public void ReserveCache(IResourcePreloader preloader)
	{
		preloader.AddNodeTree(PackPathName, TreeUid);
	}

	bool ILoadJson.Load(JsonData json)
	{
		if (!json.IsObject())
		{
			return false;
		}
		PackPathName = json.GetJsonAny("PackPathName", "PackPath").GETSTRING(PackPathName);
		Code = json["Code"].GETSTRING(Code);
		TreeUid = json["TreeUid"].GETLONG(TreeUid);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("PackPathName", PackPathName);
		jsonData.AddFieldString("Code", Code);
		jsonData.AddFieldLong("TreeUid", TreeUid);
		return jsonData;
	}
}
