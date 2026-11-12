using System;
using NGameAsset;
using NGameAsset.NBundleManager;
using NVespa;
using UnityEngine;

namespace NGame2.NUI.NAtlasAssignComponent;

public class AtlasAssignerBase : MonoBehaviour, IAssinger
{
	public const string AtlasBundleFileFormat = "NguiAtlas_UI_Prefab_{0}.unity3d";

	public bool IsControlled;

	public Func<string, bool> IncludeAtlasChecker;

	public int Priorty { get; protected set; }

	private void Awake()
	{
		if (!IsControlled)
		{
			Assign();
		}
	}

	public void SetTarget(Component targetComponent)
	{
		SetTargetComponent(targetComponent);
	}

	public void SetPriorty(Component targetComponent)
	{
		SetPriorityInternal(targetComponent);
	}

	public void SetExcludeAtlasChecker(Func<string, bool> includeAtlasChecker)
	{
		IncludeAtlasChecker = includeAtlasChecker;
	}

	[ContextMenu("Assign")]
	public void Assign()
	{
		AssignAtlas();
	}

	[ContextMenu("Save")]
	public void Save()
	{
		SaveAtlas();
	}

	protected virtual void SetTargetComponent(Component component)
	{
	}

	protected virtual void SetPriorityInternal(Component component)
	{
	}

	protected virtual void AssignAtlas()
	{
	}

	protected virtual void SaveAtlas()
	{
	}

	protected bool IsIncludeAtlas(string atlasName)
	{
		return Vespa_System_Action_Extension_Method.fire(IncludeAtlasChecker, atlasName);
	}

	protected UIAtlas GetAtlas(string atlasName)
	{
		UIAtlas uIAtlas = SprkLoginSettings.TryLoginAtlas(atlasName);
		if ((object)uIAtlas != null)
		{
			return uIAtlas;
		}
		if (String_Extension_Method.IsNullOrEmpty(atlasName))
		{
			return null;
		}
		if (!Define.PatchDownloaded)
		{
			VespaDebug.LogError($"Should never load atlas before patch download (remove or embed it): {atlasName}");
			return null;
		}
		string fileName = $"NguiAtlas_UI_Prefab_{atlasName}.unity3d";
		BundleLoadFromFile bundleLoadFromFile = new BundleLoadFromFile();
		bundleLoadFromFile.Open(fileName);
		Vespa_Common_Class_Extension_Method.Interface((IBundleLoad)bundleLoadFromFile).StartLoad();
		AssetBundle assetBundle = bundleLoadFromFile.AssetBundle;
		if (assetBundle == null)
		{
			return null;
		}
		return assetBundle.mainAsset as UIAtlas;
	}
}
