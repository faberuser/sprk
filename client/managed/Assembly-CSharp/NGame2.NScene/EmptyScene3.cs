using System;
using System.Collections;
using System.Collections.Generic;
using NGame2.N3D;
using NGame2.NCutScene;
using NGame2.NRender;
using NShared;
using NVespa;
using NVespa.NGui;
using NVespa.NScene;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NScene;

public class EmptyScene3 : BaseScene, IInspectorGuiDrawer
{
	private bool _continue;

	private VespaCoroutine.RuntimeData _testRuntime;

	private static readonly string[] CutSceneArray = new string[3] { "CutScene/SN1_4A", "CutScene/SN1_5A", "CutScene/SN1_6A" };

	private void Awake()
	{
	}

	private void OnDestroy()
	{
	}

	private void Start()
	{
		SceneAutoBehaviour<GuiManager>.instance.Add("EmptyScene3", this, DrawGui);
	}

	public override IPreload PreloadAsset()
	{
		return base.PreloadAsset();
	}

	public override void OnSceneUnloading(LoadingManager.ESceneType nextScene)
	{
	}

	public override void OnSceneLoaded(Action onComplete)
	{
		base.OnSceneLoaded(onComplete);
	}

	private void DrawGui()
	{
		if (GUILayout.Button("Test AssetBundle"))
		{
			_continue = true;
			VespaCoroutine.Start(this, coTestAssetBundle());
		}
		if (GUILayout.Button("Test CutScene"))
		{
			_continue = true;
			VespaCoroutine.Start(this, coTestCutScene());
		}
		if (GUILayout.Button("Start Coroutine"))
		{
			_testRuntime = VespaCoroutine.StartRuntime(coTestCoroutine());
		}
		using (new BlockManager().Enable(_testRuntime != null))
		{
			if (GUILayout.Button("Stop Coroutine"))
			{
				VespaCoroutine.Stop(_testRuntime);
			}
		}
		_continue = GUILayout.Toggle(_continue, "Continue");
	}

	public override void Update()
	{
		base.Update();
	}

	private IEnumerator coTestCoroutine()
	{
		int loop = 0;
		while (true)
		{
			yield return new WaitForSeconds(1f);
			loop++;
		}
	}

	private IEnumerator coTestCutScene()
	{
		while (_continue)
		{
			for (int index = 0; index < __Extension_Method_ICollection__.COUNT(CutSceneArray); index++)
			{
				string scriptPath = CutSceneArray[index];
				bool isComplete = false;
				SceneAutoBehaviour<CutSceneManager>.instance.Open(scriptPath, canSkip: false, delegate
				{
					isComplete = true;
				});
				while (!isComplete)
				{
					yield return null;
				}
				yield return new WaitForSeconds(1f);
			}
			yield return VespaCoroutine.Start(this, LoadingManager.coResetEnvironment());
			yield return new WaitForSeconds(1f);
		}
	}

	private IEnumerator coTestAssetBundle()
	{
		while (_continue)
		{
			List<RenderFacade> renderList = new List<RenderFacade>();
			CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(1);
			if (data != null)
			{
				for (int i = 1; i <= 10; i++)
				{
					RenderFacade renderFacade = new RenderFacadeBuilder(base.transform, TeamType.Left, data.CodeName, 1).Build();
					if (!(renderFacade == null))
					{
						renderList.Add(renderFacade);
					}
				}
			}
			while (true)
			{
				bool flag = true;
				for (int j = 0; j < __Extension_Method_ICollection__.COUNT(renderList); j++)
				{
					if (!renderList[j].AssetCreatureAsync.ISDONE())
					{
						flag = false;
					}
				}
				if (flag)
				{
					break;
				}
				yield return null;
			}
			yield return new WaitForSeconds(1f);
			for (int k = 0; k < __Extension_Method_ICollection__.COUNT(renderList); k++)
			{
				RenderFacadeBuilder.ReleaseRenderFacade(renderList[k]);
			}
			renderList.Clear();
			yield return new WaitForSeconds(1f);
			yield return VespaCoroutine.Start(this, LoadingManager.coResetEnvironment());
			yield return new WaitForSeconds(1f);
		}
	}
}
