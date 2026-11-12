using System.Collections.Generic;
using NVespa;
using NVespa.NGui;
using NVespa.NScene;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NRender3D.NCinema;

public class CinemaOwnerContainer : SingletonAuto<CinemaOwnerContainer>
{
	public Dictionary<ECinemaOwnerType, CinemaOwner> _cinemaOwnerDic = new Dictionary<ECinemaOwnerType, CinemaOwner>();

	public Dictionary<ECinemaOwnerType, TestPlayNodeTreeInfo> _testPlayNodeTreeDic = new Dictionary<ECinemaOwnerType, TestPlayNodeTreeInfo>();

	public CinemaOwnerContainer()
	{
		SceneAutoBehaviour<GuiManager>.instance.Add("CinemaOwnerContainer", this, delegate
		{
			using (new BlockManager().Vertical())
			{
				if (__Extension_Method_ICollection__.IsNullOrEmpty(_cinemaOwnerDic))
				{
					return;
				}
				foreach (KeyValuePair<ECinemaOwnerType, CinemaOwner> item in _cinemaOwnerDic)
				{
					CinemaOwner value = item.Value;
					IDataProvider dataProvider = value.DataProvider;
					Cinema cinema = value.Cinema;
					bool flag = dataProvider != null && cinema != null;
					using (new BlockManager().Horizontal())
					{
						GUILayout.Label("OnwerType : " + item.Key);
						GUILayout.Label("Valid :" + flag);
					}
					if (!flag)
					{
						break;
					}
					using (new BlockManager().Horizontal())
					{
						TestPlayNodeTreeInfo testPlayNodeTreeInfo = _testPlayNodeTreeDic[item.Key];
						using (new BlockManager().Vertical(GUILayout.Width(350f)))
						{
							using (new BlockManager().Horizontal())
							{
								GUILayout.Label("Pack", GUILayout.Width(75f));
								testPlayNodeTreeInfo.Pack = GUILayout.TextField(testPlayNodeTreeInfo.Pack, GUILayout.Width(250f));
							}
							using (new BlockManager().Horizontal())
							{
								GUILayout.Label("Name", GUILayout.Width(75f));
								testPlayNodeTreeInfo.Name = GUILayout.TextField(testPlayNodeTreeInfo.Name, GUILayout.Width(250f));
							}
							using (new BlockManager().Horizontal())
							{
								testPlayNodeTreeInfo.IsClosePrevNodeTree = GUILayout.Toggle(testPlayNodeTreeInfo.IsClosePrevNodeTree, "Close Previous NodeTree");
							}
						}
						if (GUILayout.Button("Play", GUILayout.Width(50f)))
						{
							if (value == null || testPlayNodeTreeInfo == null || String_Extension_Method.IsNullOrEmpty(testPlayNodeTreeInfo.Pack) || String_Extension_Method.IsNullOrEmpty(testPlayNodeTreeInfo.Name))
							{
								break;
							}
							value.SetNextNodeTree(testPlayNodeTreeInfo.IsClosePrevNodeTree, testPlayNodeTreeInfo.Pack, testPlayNodeTreeInfo.Name, null, null);
						}
					}
					using (new BlockManager().Horizontal())
					{
						using (new BlockManager().TitleBox("NodeTree").Vertical())
						{
							GUILayout.Label($"Pack : {dataProvider.NodeTreePack}");
							GUILayout.Label($"Name : {dataProvider.NodeTreeName}");
							GUILayout.Label($"BGM Code : {cinema.BgmCode}");
							GUILayout.Label($"State : {value.CinemaState.ToString()}");
						}
						using (new BlockManager().TitleBox("Texture").Vertical())
						{
							bool num = dataProvider.Texture != null;
							int num2 = (num ? dataProvider.Texture.width : dataProvider.TextureWidth);
							int num3 = (num ? dataProvider.Texture.height : dataProvider.TextureHeight);
							GUILayout.Label($"Width : {num2}");
							GUILayout.Label($"Height : {num3}");
						}
						if (cinema != null && cinema.RenderTarget != null)
						{
							using (new BlockManager().TitleBox("RenderTexture").Vertical())
							{
								RenderTexture renderTarget = cinema.RenderTarget;
								GUILayout.Label($"Width : {renderTarget.width}");
								GUILayout.Label($"Hieght : {renderTarget.height}");
								GUILayout.Label($"Format : {renderTarget.format.ToString()}");
							}
						}
					}
					value.DrawGui();
				}
			}
		});
	}

	private void AddCinemaOwner(CinemaOwner cinemaOwner)
	{
		if (cinemaOwner != null)
		{
			_cinemaOwnerDic[cinemaOwner.OwnerType] = cinemaOwner;
			_testPlayNodeTreeDic[cinemaOwner.OwnerType] = new TestPlayNodeTreeInfo();
		}
	}

	private CinemaOwner Get(ECinemaOwnerType ownerType)
	{
		CinemaOwner value = null;
		if (!_cinemaOwnerDic.TryGetValue(ownerType, out value))
		{
			value = new CinemaOwner(ownerType);
			AddCinemaOwner(value);
		}
		return value;
	}

	public CinemaOwner GetCinemaOwner(ECinemaOwnerType ownerType)
	{
		return Get(ownerType);
	}

	public static void OnSceneChanged()
	{
		if (!SingletonAuto<CinemaOwnerContainer>.isValidInstance)
		{
			return;
		}
		Dictionary<ECinemaOwnerType, CinemaOwner> cinemaOwnerDic = SingletonAuto<CinemaOwnerContainer>.instance._cinemaOwnerDic;
		foreach (KeyValuePair<ECinemaOwnerType, CinemaOwner> item in cinemaOwnerDic)
		{
			item.Value?.Close();
		}
		cinemaOwnerDic.Clear();
		SingletonAuto<CinemaOwnerContainer>.instance._testPlayNodeTreeDic.Clear();
	}
}
