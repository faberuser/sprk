using System;
using NGame2.NUI;
using NVespa;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NBattleView.NUIOn3D;

public class WorldMapHud : MonoBehaviour
{
	public interface IValue
	{
		int CreatureUid { get; }

		float HudOffset { get; }
	}

	public const string PrefabPath = "Resources/Prefab2/UIOn3D/WorldMapHud";

	public static PrefabAuto<WorldMapHud> PrefabAuto = new PrefabAuto<WorldMapHud>("Resources/Prefab2/UIOn3D/WorldMapHud");

	[NonSerialized]
	public float EventAlpha = 1f;

	[SerializeField]
	private UIPanel _panel;

	[SerializeField]
	private UILabel _nameLabel;

	[SerializeField]
	private UIWidget _HPRoot;

	private IValue _value;

	private bool _hideHUD;

	public static WorldMapHud CreateInstance(Transform parent)
	{
		return PrefabAuto.Factory.Create(parent);
	}

	public static void ReleaseInstance(WorldMapHud instance)
	{
		FactoryBase.ReleaseComponent(instance);
	}

	public void Open(IValue value)
	{
		_value = value;
		Reset();
		base.transform.localPosition = Vector3.up * _value.HudOffset;
	}

	public void SetName(string Name)
	{
		UIHelper.SetLabelText(_nameLabel, Name);
	}

	private void Reset()
	{
		EventAlpha = 1f;
	}

	public void ChainUpdate()
	{
		if (!(_panel == null))
		{
			Camera cameraByLayer = GlobalAutoBehaviour<GlobalCameraManager>.instance.GetCameraByLayer(base.gameObject.layer);
			if (!(cameraByLayer == null))
			{
				base.transform.rotation = cameraByLayer.transform.rotation;
			}
		}
	}

	public void ShowName(bool isShow)
	{
		UIHelper.EnableGameObject(_nameLabel, isShow);
	}

	public void ShowHP(bool isShow)
	{
		UIHelper.EnableGameObject(_HPRoot, isShow);
	}
}
