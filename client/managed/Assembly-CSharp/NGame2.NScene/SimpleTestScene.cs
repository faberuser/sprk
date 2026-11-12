using System;
using System.Text;
using NVespa;
using NVespa.NGlobal;
using NVespa.NGui;
using NVespa.NPrefab2;
using NVespa.NScene;
using NVespa.NSingleton;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;

namespace NGame2.NScene;

public class SimpleTestScene : MonoBehaviour, IInspectorGuiDrawer
{
	public struct MemoryUsage
	{
		public Type Type;

		public string Name;

		public int Memory;
	}

	private static readonly Type[] CollectTypes = new Type[3]
	{
		typeof(Texture),
		typeof(GameObject),
		typeof(Material)
	};

	private int _totalMemoryUsage;

	private VespaList<MemoryUsage> _memoryUsageList = new VespaList<MemoryUsage>();

	private int _countOfGarbageCollect;

	private int _countOfResourceUnload;

	private int _countOfDrawCallReleaseAll;

	private int _countOfDrawCallReleaseInactive;

	private float _nextCollectTime;

	private int _viewIndex;

	private int _collectType;

	private void Start()
	{
		SceneAutoBehaviour<NVespa.NScene.GuiManager>.instance.Add("Simple Test", this, DrawGui);
		SceneAutoBehaviour<NVespa.NScene.GuiManager>.instance.Add("Prefab", base.transform, DrawGui_Prefab);
	}

	private void DrawGui_Prefab()
	{
		if (GUILayout.Button("Next"))
		{
			_viewIndex++;
		}
		if (_viewIndex != 0)
		{
			if (_viewIndex == 1)
			{
				DrawGui_Prefab2();
			}
			else if (_viewIndex == 2)
			{
				DrawGui_Singleton();
			}
			else
			{
				_viewIndex = 0;
			}
		}
	}

	private void DrawGui_Prefab2()
	{
		using (new ScrollBlock(this, "Prefab2"))
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("--- Prefab2 ---");
			SingletonAuto<Container>.instance.Dump(stringBuilder);
			SingletonAuto<ContainerGo>.instance.Dump(stringBuilder);
			GUILayout.Label(stringBuilder.ToString());
		}
	}

	private void DrawGui_Singleton()
	{
	}

	private void DrawGui()
	{
		if (_nextCollectTime < Time.realtimeSinceStartup)
		{
			_collectType %= CollectTypes.Length;
			CollectMemoryUsage(CollectTypes[_collectType]);
			_nextCollectTime = Time.realtimeSinceStartup + 1f;
		}
		using (new BlockManager().Horizontal())
		{
			if (GUILayout.Button("GC.Collect: " + _countOfGarbageCollect))
			{
				GC.Collect();
				_countOfGarbageCollect++;
			}
			if (GUILayout.Button("UnloadUnusedAssets: " + _countOfResourceUnload))
			{
				Resources.UnloadUnusedAssets();
				_countOfResourceUnload++;
			}
		}
		using (new BlockManager().Horizontal())
		{
			if (GUILayout.Button("UIDrawCall ReleaseAll: " + _countOfDrawCallReleaseAll))
			{
				UIDrawCall.ReleaseAll();
				_countOfDrawCallReleaseAll++;
			}
			if (GUILayout.Button("UIDrawCall ReleaseInactive: " + _countOfDrawCallReleaseInactive))
			{
				UIDrawCall.ReleaseInactive();
				_countOfDrawCallReleaseInactive++;
			}
		}
		using (new BlockManager().Horizontal())
		{
			if (GUILayout.Button("Load Scene"))
			{
				SceneManager.LoadScene("SimpleTest");
			}
			if (GUILayout.Button("Load Lobby"))
			{
				GlobalAutoBehaviour<LoadingManager>.instance.MoveScene(LoadingManager.ESceneType.Logo);
			}
			if (GUILayout.Button("Collect " + CollectTypes[_collectType].FullName))
			{
				_collectType = (_collectType + 1) % CollectTypes.Length;
			}
		}
		float width = GlobalAutoBehaviour<NVespa.NGlobal.GuiManager>.instance.DisplayWidth - 200f;
		float height = GlobalAutoBehaviour<NVespa.NGlobal.GuiManager>.instance.DisplayHeight - 200f;
		GUILayout.Label(string.Format("Next Collection Time: {0}", _nextCollectTime.ToString("F2")));
		using (new BlockManager().Vertical(GUILayout.Width(width), GUILayout.Height(height)).Scroll(this, "MemoryScroll"))
		{
			for (int i = 0; i < _memoryUsageList.Count; i++)
			{
				MemoryUsage memoryUsage = _memoryUsageList[i];
				using (new BlockManager().Horizontal())
				{
					string arg = memoryUsage.Name;
					string arg2 = memoryUsage.Type.Name;
					int memory = memoryUsage.Memory;
					GUILayout.Label($"{arg}({arg2}): {Vespa_Int_Extension_Method.ToCommaString(memory)}", GUILayout.Width(500f));
				}
			}
		}
	}

	private void CollectMemoryUsage(Type type)
	{
		_memoryUsageList.Clear();
		_totalMemoryUsage = 0;
		UnityEngine.Object[] array = Resources.FindObjectsOfTypeAll(type);
		for (int i = 0; i < array.Length; i++)
		{
			MemoryUsage item = new MemoryUsage
			{
				Type = array[i].GetType(),
				Name = array[i].name,
				Memory = Profiler.GetRuntimeMemorySize(array[i])
			};
			_memoryUsageList.Add(item);
			_totalMemoryUsage += item.Memory;
		}
		_memoryUsageList.Sort((MemoryUsage lhs, MemoryUsage rhs) => rhs.Memory.CompareTo(lhs.Memory));
	}
}
