using System;
using NGame2.NUI;
using NGame2.NUI.NComponent2.NBattle.NSub;
using NGame2.NUtil;
using NNguiExtension;
using NShared;
using NVespa;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NBattleView.NUIOn3D;

public class HUD : LinkFactoryComponent, IInspectorGuiDrawer
{
	public enum EHudType
	{
		Me,
		Ally,
		Enemy,
		Party
	}

	public interface IValue
	{
		bool IsDie { get; }

		float HpRatio { get; }

		float HudOffset { get; }

		long Hp { get; }

		long MaxHp { get; }

		int Mp { get; }

		long MaxMp { get; }

		CastingComponent.ICastingValue Casting { get; }
	}

	public const string PrefabPath = "Resources/Prefab2/UIOn3D/HUD";

	public static PrefabAuto<HUD> PrefabAuto = new PrefabAuto<HUD>("Resources/Prefab2/UIOn3D/HUD");

	public static LocalStorageBool ToggleHpNumber = new LocalStorageBool(i_default: false, typeof(HUD).FullName + "ToggleHpNumber");

	public const float PositionUpdateVelocity = 0.1f;

	[NonSerialized]
	public float EventAlpha = 1f;

	[SerializeField]
	private UIPanel _panel;

	[SerializeField]
	private UIWidget _root;

	[SerializeField]
	private UIWidget _rootNickname;

	[SerializeField]
	private UISpriteSelector _spriteHp;

	[SerializeField]
	private UIWidget _widgetBossFrame;

	[SerializeField]
	private UISpriteSelector _tierSelector;

	[SerializeField]
	private UISimpleGrid _buffGrid;

	[SerializeField]
	private UISimpleGrid _shieldGrid;

	[SerializeField]
	private UISimpleGrid _castingGrid;

	[SerializeField]
	private UILabel _labelNickname;

	[SerializeField]
	private UILabel _labelMp;

	private int _creatureUid;

	private IValue _value;

	private bool _hideHUD;

	private EHudType _hudType;

	private float _sqrPositionUpdateVel;

	private BattleStateUiController _stateUiController = new BattleStateUiController();

	private ShieldComponent _shieldComponent;

	private CastingComponent _castingComponent;

	public static HUD CreateInstance(Transform parent)
	{
		if (IsCreateSkip())
		{
			return null;
		}
		return PrefabAuto.Factory.Create(parent);
	}

	public static void ReleaseInstance(HUD instance)
	{
		if (!(instance == null))
		{
			FactoryBase.ReleaseComponent(instance);
		}
	}

	public void Open(EHudType hudType, SpawnCreature spawnCreature, IValue value, string nickName = null)
	{
		UIHelper.EnableGameObject(_root, String_Extension_Method.IsNullOrEmpty(nickName));
		UIHelper.EnableGameObject(_rootNickname, !String_Extension_Method.IsNullOrEmpty(nickName));
		if (!String_Extension_Method.IsNullOrEmpty(nickName))
		{
			UIHelper.SetLabelText(_labelNickname, nickName);
		}
		_hudType = hudType;
		_creatureUid = spawnCreature.CreatureUid;
		_value = value;
		Close();
		_stateUiController.Open(_creatureUid, _buffGrid);
		_shieldComponent = UIHelper.AddGridItem(_shieldGrid, ShieldComponent.PrefabAuto);
		_shieldComponent.Open(_creatureUid);
		_castingComponent = UIHelper.AddGridItem(_castingGrid, CastingComponent.PrefabAuto);
		_castingComponent.Open(_creatureUid);
		if (_spriteHp != null)
		{
			_spriteHp.SelectEnum(_hudType);
		}
		if (_tierSelector != null)
		{
			MonsterGradeType grade = spawnCreature.CreateInfo.Grade;
			bool enable = _tierSelector.SelectEnum(grade);
			UIHelper.EnableGameObject(_tierSelector, enable);
			UIHelper.EnableGameObject(_widgetBossFrame, enable);
		}
		base.transform.localPosition = Vector3.up * _value.HudOffset;
		_sqrPositionUpdateVel = 0.010000001f;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		_stateUiController.Reset();
	}

	public void Close()
	{
		EventAlpha = 1f;
		_stateUiController.Reset();
		if (_shieldComponent != null)
		{
			_shieldComponent.Reset();
			FactoryBase.ReleaseComponent(_shieldComponent);
			_shieldComponent = null;
		}
		if (_castingComponent != null)
		{
			FactoryBase.ReleaseComponent(_castingComponent);
			_castingComponent = null;
		}
	}

	private UISprite GetHpSprite()
	{
		if (_spriteHp != null)
		{
			return _spriteHp.GetTargetSprite();
		}
		return null;
	}

	public void ChainUpdate()
	{
		if (!(_panel == null) && !(_shieldComponent == null) && !(_castingComponent == null))
		{
			UpdateAlpha();
			UpdateHp();
			UpdateMp();
			_stateUiController.ChainUpdate();
			_shieldComponent.ChainUpdate();
			_castingComponent.ChainUpdate(_value.Casting);
			RepositionComponent();
			UpdatePosition();
			base.transform.rotation = Game3DRoot.instance.GameCamera.MainCamera.transform.rotation;
		}
	}

	private void UpdatePosition()
	{
		Vector3 vector = Vector3.up * _value.HudOffset;
		Vector3 localPosition = base.transform.localPosition;
		Vector3 vector2 = vector - localPosition;
		if (vector2.sqrMagnitude < _sqrPositionUpdateVel)
		{
			base.transform.localPosition = vector;
		}
		else
		{
			base.transform.localPosition = localPosition + vector2 * 0.1f;
		}
	}

	private void UpdateAlpha()
	{
		float a = ((!_value.IsDie) ? Mathf.MoveTowards(_panel.alpha, 1f, UnitySystemTimer.deltaTime * 3f) : Mathf.MoveTowards(_panel.alpha, 0f, UnitySystemTimer.deltaTime * 3f));
		_panel.alpha = Mathf.Min(a, EventAlpha);
	}

	private void UpdateHp()
	{
		UIHelper.SetSpriteFillAmount(GetHpSprite(), _value.HpRatio);
		if (ToggleHpNumber.Value)
		{
			UIHelper.EnableGameObject(_rootNickname, enable: true);
			string text = $"Hp {_value.Hp} / {_value.MaxHp}";
			UIHelper.SetLabelText(_labelNickname, text);
		}
	}

	private void UpdateMp()
	{
		if (ToggleHpNumber.Value)
		{
			string text = $"Mp {_value.Mp} / {_value.MaxMp}";
			UIHelper.SetLabelText(_labelMp, text);
		}
	}

	private void RepositionComponent()
	{
		float num = _shieldGrid.transform.localPosition.y;
		if (_shieldComponent.IsEnabled)
		{
			num -= (float)_shieldComponent.RootWidget.height;
		}
		_castingGrid.transform.localPosition = Vespa_Unity_Vector3_Extension_Method.ReplaceY(_castingGrid.transform.localPosition, num);
	}

	private static bool IsCreateSkip()
	{
		return false;
	}
}
