using NGame2.NNodeTree.NUtil;
using NVespa;

namespace NGame2.NNodeTree;

public class PackContainer : SingletonAuto<PackContainer>
{
	public IncreaseDic<string, PackLoad> Dic = new IncreaseDic<string, PackLoad>();

	public void OnSceneChanged()
	{
		Dic.Clear();
	}

	public PackLoad GetPack(string pathName)
	{
		PackHelper.FixPathName(ref pathName);
		PackLoad vALUE = Dic.GETVALUE(pathName, null);
		if (vALUE != null)
		{
			return vALUE;
		}
		vALUE = CreateNodeTreePack(pathName);
		if (vALUE == null)
		{
			return null;
		}
		Dic.Add(pathName, vALUE);
		return vALUE;
	}

	public PackLoad GetExistPack(string pathName)
	{
		PackHelper.FixPathName(ref pathName);
		PackLoad vALUE = Dic.GETVALUE(pathName, null);
		if (vALUE != null)
		{
			return vALUE;
		}
		return null;
	}

	private PackLoad CreateNodeTreePack(string pathName)
	{
		PackLoad packLoad = new PackLoad();
		packLoad.Open(pathName);
		return packLoad;
	}
}
