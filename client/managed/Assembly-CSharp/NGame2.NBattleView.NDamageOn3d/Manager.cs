using System.Collections.Generic;
using NGame2.NBattleView.NUIOn3d2;
using NGame2.NGameSetting.NBattleView;
using NGameAsset;
using NShared;
using NVespa;
using NVespa.NGui;
using NVespa.NScene;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NBattleView.NDamageOn3d;

public class Manager : SceneAutoBehaviour<Manager>
{
	public enum EAttachType
	{
		Height,
		Hit,
		Center
	}

	public struct DamageFloatData
	{
		public float NumberScale;

		public float IconScale;

		public float TextScale;

		public Color GradientTop;

		public Color GradientBottom;
	}

	private Dictionary<EDamageFloatType, DamageFloatData> _damageFloatDic = new Dictionary<EDamageFloatType, DamageFloatData>();

	private float _dotIconOffset = 1f;

	private float _eventAlpha = 1f;

	private TeamType _eventApplyTeamType = TeamType.All;

	private RenderContainer _normalContainer = new RenderContainer();

	private RenderContainer _criticalContainer = new RenderContainer();

	private bool optionShowAll => SprkArenaPractice.DamageText() == ERenderDamageText.ShowAll;

	protected override void Awake()
	{
		base.Awake();
		SceneAutoBehaviour<GeometryBuilder>.CreateInstance();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		_normalContainer.OnDestroy();
		_criticalContainer.OnDestroy();
		SceneAutoBehaviour<Manager>._instance = null;
	}

	private void Start()
	{
		SceneAutoBehaviour<GuiManager>.instance.Add("DamageOn3d.Manager", this, delegate
		{
			using (new BlockManager().Horizontal())
			{
				if (GUILayout.Button("- 10%", GUILayout.Width(50f)))
				{
					RenderInstance.DisplayScale /= 1.1f;
				}
				GUILayout.Label(RenderInstance.DisplayScale.ToString("F4"));
				if (GUILayout.Button("+ 10%", GUILayout.Width(50f)))
				{
					RenderInstance.DisplayScale *= 1.1f;
				}
			}
			GUILayout.Label("Normal Render " + _normalContainer.Count);
			GUILayout.Label("Critical Render " + _criticalContainer.Count);
			GUILayout.Label("Old DrawCall " + NGame2.NBattleView.NUIOn3d2.Manager.instance.DrawCallCount);
			if (GUILayout.Button("Copy DamageFloat"))
			{
				InitDamageFloat();
			}
		});
		if (GameSetting.data != null && GameSetting.data.BattleView != null)
		{
			InitDamageFloat();
		}
		else
		{
			VespaDebug.LogError("Load damage floating info failed. Check damage floating data in VespaScript>Game>Game Setting Editor");
		}
	}

	private void InitDamageFloat()
	{
		foreach (KeyValuePair<EDamageFloatType, NGame2.NGameSetting.NBattleView.DamageFloatData.DamageFloatInfo> item in (IEnumerable<KeyValuePair<EDamageFloatType, NGame2.NGameSetting.NBattleView.DamageFloatData.DamageFloatInfo>>)(SystemOptionData.ShowFontNew ? GameSetting.data.BattleView.DamageFloatDataNew.Dic : GameSetting.data.BattleView.DamageFloatData.Dic))
		{
			DamageFloatData value = new DamageFloatData
			{
				NumberScale = item.Value.NumberScale,
				IconScale = item.Value.IconScale,
				TextScale = item.Value.TextScale,
				GradientTop = item.Value.ColorTop,
				GradientBottom = item.Value.ColorBottom
			};
			if (!_damageFloatDic.ContainsKey(item.Key))
			{
				_damageFloatDic.Add(item.Key, value);
			}
			else
			{
				_damageFloatDic[item.Key] = value;
			}
		}
	}

	public void OnEventCamera(float eventAlpha, TeamType eventApplyTeamType)
	{
		_eventAlpha = eventAlpha;
		_eventApplyTeamType = eventApplyTeamType;
	}

	public void ChainUpdate()
	{
		SceneAutoBehaviour<RenderInstancePool>.instance.UpdateCommonPropertyBlock(_eventAlpha);
		int renderQueue = 4700;
		renderQueue = _normalContainer.UpdateAndRender(renderQueue);
		renderQueue = _criticalContainer.UpdateAndRender(renderQueue);
	}

	private bool TryGetAttachPosition(int creatureUid, EAttachType attachType, out Vector3 oPosition)
	{
		Creature creature = BattleViewGenericSingleton<CreatureManager>.instance.GetCreature(creatureUid);
		if (creature == null)
		{
			oPosition = Vector3.zero;
			return false;
		}
		if (_eventApplyTeamType != TeamType.All && _eventApplyTeamType != creature.TeamType)
		{
			oPosition = Vector3.zero;
			return false;
		}
		Transform transform = null;
		Vector3 vector = Vector3.zero;
		switch (attachType)
		{
		case EAttachType.Height:
			transform = creature.RenderFacade.HeightTm;
			break;
		case EAttachType.Hit:
			transform = creature.RenderFacade.AssetCreature.GetPivot(EPivotType.Hit);
			break;
		case EAttachType.Center:
			transform = creature.RenderFacade.transform;
			vector = creature.RenderFacade.ShapeBounds.center;
			break;
		default:
			oPosition = Vector3.zero;
			return false;
		}
		if (transform == null)
		{
			oPosition = Vector3.zero;
			return false;
		}
		oPosition = transform.position + vector;
		return true;
	}

	public void OnHeal(int creatureUid, long heal)
	{
		if (!optionShowAll || !TryGetAttachPosition(creatureUid, EAttachType.Height, out var oPosition))
		{
			return;
		}
		DamageFloatData damageFloatData = _damageFloatDic[EDamageFloatType.Heal];
		GeometryBuilder.GeometryData geometryData = SceneAutoBehaviour<GeometryBuilder>.instance.CreateNumber(heal, damageFloatData.GradientTop, damageFloatData.GradientBottom, damageFloatData.NumberScale);
		if (geometryData != null)
		{
			geometryData.Position = oPosition;
			_normalContainer.Add(RenderInstance.EShaderType.Heal, geometryData, 1.5f);
			GeometryBuilder.GeometryData geometryData2 = SceneAutoBehaviour<GeometryBuilder>.instance.CreateImage(GeometryBuilder.ECachedSprite.Char_Plus, damageFloatData.GradientTop, damageFloatData.GradientBottom, damageFloatData.TextScale);
			if (geometryData2 != null)
			{
				Vector2 layoutOffset = new Vector2
				{
					x = (0f - (geometryData.LayoutWidth + geometryData2.LayoutWidth)) * 0.5f
				};
				geometryData2.Position = oPosition;
				geometryData2.LayoutOffset = layoutOffset;
				_normalContainer.Add(RenderInstance.EShaderType.Heal, geometryData2, 1.5f);
			}
		}
	}

	public void OnDamageNormal(int creatureUid, long damage)
	{
		if (optionShowAll && TryGetAttachPosition(creatureUid, EAttachType.Center, out var oPosition))
		{
			DamageFloatData damageFloatData = _damageFloatDic[EDamageFloatType.NormalDamage];
			GeometryBuilder.GeometryData geometryData = SceneAutoBehaviour<GeometryBuilder>.instance.CreateNumber(damage, damageFloatData.GradientTop, damageFloatData.GradientBottom, damageFloatData.NumberScale);
			if (geometryData != null)
			{
				geometryData.Position = oPosition;
				_normalContainer.Add(RenderInstance.EShaderType.Default, geometryData, 0.5f);
			}
		}
	}

	public void OnDamageSkill(int creatureUid, long damage)
	{
		if (optionShowAll && TryGetAttachPosition(creatureUid, EAttachType.Center, out var oPosition))
		{
			DamageFloatData damageFloatData = _damageFloatDic[EDamageFloatType.SkillDamage];
			GeometryBuilder.GeometryData geometryData = SceneAutoBehaviour<GeometryBuilder>.instance.CreateNumber(damage, damageFloatData.GradientTop, damageFloatData.GradientBottom, damageFloatData.NumberScale);
			if (geometryData != null)
			{
				geometryData.Position = oPosition;
				_criticalContainer.Add(RenderInstance.EShaderType.Critical, geometryData, 1f);
			}
		}
	}

	public void OnDamageCritical(int creatureUid, long damage)
	{
		if (!TryGetAttachPosition(creatureUid, EAttachType.Center, out var oPosition))
		{
			return;
		}
		DamageFloatData damageFloatData = _damageFloatDic[EDamageFloatType.CriticalDamage];
		GeometryBuilder.GeometryData geometryData = SceneAutoBehaviour<GeometryBuilder>.instance.CreateImage(GeometryBuilder.ECachedSprite.Text_Critical, damageFloatData.GradientTop, damageFloatData.GradientBottom, damageFloatData.TextScale);
		if (geometryData == null)
		{
			return;
		}
		geometryData.Position = oPosition;
		if (optionShowAll)
		{
			GeometryBuilder.GeometryData geometryData2 = SceneAutoBehaviour<GeometryBuilder>.instance.CreateNumber(damage, damageFloatData.GradientTop, damageFloatData.GradientBottom, damageFloatData.NumberScale);
			if (geometryData2 != null)
			{
				geometryData.LayoutOffset.y = (geometryData2.LayoutHeight + geometryData.LayoutHeight) * 0.6f;
				geometryData2.Position = oPosition;
				_criticalContainer.Add(RenderInstance.EShaderType.Critical, geometryData2, 1f);
			}
		}
		_criticalContainer.Add(RenderInstance.EShaderType.Critical, geometryData, 1f);
	}

	public void OnBlock(int creatureUid, long damage)
	{
		if (!TryGetAttachPosition(creatureUid, EAttachType.Center, out var oPosition))
		{
			return;
		}
		DamageFloatData damageFloatData = _damageFloatDic[EDamageFloatType.Block];
		GeometryBuilder.GeometryData geometryData = SceneAutoBehaviour<GeometryBuilder>.instance.CreateImage(GeometryBuilder.ECachedSprite.Text_Block, damageFloatData.GradientTop, damageFloatData.GradientBottom, damageFloatData.TextScale);
		if (geometryData == null)
		{
			return;
		}
		Vector3 vector = default(Vector3);
		if (optionShowAll)
		{
			GeometryBuilder.GeometryData geometryData2 = SceneAutoBehaviour<GeometryBuilder>.instance.CreateNumber(damage, damageFloatData.GradientTop, damageFloatData.GradientBottom, damageFloatData.NumberScale);
			if (geometryData2 != null)
			{
				geometryData2.Position = oPosition;
				_normalContainer.Add(RenderInstance.EShaderType.Block, geometryData2, 1f);
				vector.y = (geometryData2.LayoutHeight + geometryData.LayoutHeight) * 0.6f;
			}
		}
		GeometryBuilder.GeometryData geometryData3 = SceneAutoBehaviour<GeometryBuilder>.instance.CreateImage(GeometryBuilder.ECachedSprite.Icon_Block, Color.white, Color.white, damageFloatData.IconScale);
		if (geometryData3 != null)
		{
			Vector3 vector2 = new Vector3
			{
				y = vector.y,
				x = (0f - geometryData.LayoutWidth) / 2f
			};
			vector.x = geometryData3.LayoutWidth / 2f;
			geometryData3.Position = oPosition;
			geometryData3.LayoutOffset = vector2;
			_normalContainer.Add(RenderInstance.EShaderType.Block, geometryData3, 1f);
		}
		geometryData.Position = oPosition;
		geometryData.LayoutOffset = vector;
		_normalContainer.Add(RenderInstance.EShaderType.Block, geometryData, 1f);
	}

	public void OnDamageDot(int creatureUid, long damage, string origStateType)
	{
		if (!TryGetAttachPosition(creatureUid, EAttachType.Center, out var oPosition))
		{
			return;
		}
		Creature creature = BattleViewGenericSingleton<CreatureManager>.instance.GetCreature(creatureUid);
		if (creature == null)
		{
			return;
		}
		string text = origStateType;
		StateData data = GameRoot.Instance.StateDataContainer.GetData(text);
		if (data != null && !String_Extension_Method.IsNullOrEmpty(data.Group))
		{
			text = data.Group;
		}
		DamageDotData.DotInfo dotInfo = null;
		if (!String_Extension_Method.IsNullOrEmpty(text))
		{
			dotInfo = ((!SystemOptionData.ShowFontNew) ? GameSetting.data.BattleView.DamageDot.Dic.GETVALUE(text, null) : GameSetting.data.BattleView.DamageDotNew.Dic.GETVALUE(text, null));
		}
		DamageFloatData damageFloatData = _damageFloatDic[EDamageFloatType.Dot];
		Color gradianTop;
		Color gradianBottom;
		if (dotInfo == null)
		{
			gradianTop = damageFloatData.GradientTop;
			gradianBottom = damageFloatData.GradientBottom;
		}
		else
		{
			gradianTop = dotInfo.ColorTop;
			gradianBottom = dotInfo.ColorBottom;
		}
		GeometryBuilder.GeometryData geometryData = SceneAutoBehaviour<GeometryBuilder>.instance.CreateNumber(damage, gradianTop, gradianBottom, damageFloatData.NumberScale);
		if (geometryData != null)
		{
			bool flag = creature.TeamType == TeamType.Right;
			oPosition = (geometryData.Position = oPosition + (flag ? Vector3.right : Vector3.left) * _dotIconOffset);
			geometryData.ExtraValue = ((creature.TeamType == TeamType.Left) ? (-1f) : 1f);
			_normalContainer.Add(RenderInstance.EShaderType.Dot, geometryData, 1f);
			GeometryBuilder.GeometryData geometryData2 = SceneAutoBehaviour<GeometryBuilder>.instance.CreateImage("Icon_Dot_" + text, Color.white, Color.white, damageFloatData.IconScale);
			if (geometryData2 != null)
			{
				Vector2 layoutOffset = geometryData.LayoutOffset;
				layoutOffset.x -= geometryData.LayoutWidth * 0.5f;
				geometryData.LayoutOffset.x += geometryData2.LayoutWidth * 0.5f;
				geometryData2.Position = oPosition;
				geometryData2.LayoutOffset = layoutOffset;
				geometryData2.ExtraValue = geometryData.ExtraValue;
				_normalContainer.Add(RenderInstance.EShaderType.Dot, geometryData2, 1f);
			}
		}
	}

	public void OnMiss(int creatureUid)
	{
		if (TryGetAttachPosition(creatureUid, EAttachType.Center, out var oPosition))
		{
			oPosition += Vector3.down * 0.2f;
			DamageFloatData damageFloatData = _damageFloatDic[EDamageFloatType.Miss];
			GeometryBuilder.GeometryData geometryData = SceneAutoBehaviour<GeometryBuilder>.instance.CreateImage(GeometryBuilder.ECachedSprite.Text_Miss, damageFloatData.GradientTop, damageFloatData.GradientBottom, damageFloatData.TextScale);
			if (geometryData != null)
			{
				geometryData.Position = oPosition;
				geometryData.LayoutOffset = Vector3.down * 0.25f;
				_normalContainer.Add(RenderInstance.EShaderType.Miss, geometryData, 1f);
			}
		}
	}

	public void OnImmune(int creatureUid, bool isMagical)
	{
		if (TryGetAttachPosition(creatureUid, EAttachType.Center, out var oPosition))
		{
			DamageFloatData damageFloatData = _damageFloatDic[EDamageFloatType.Immune];
			GeometryBuilder.ECachedSprite cachedSprite = (isMagical ? GeometryBuilder.ECachedSprite.Text_Immune_Magical : GeometryBuilder.ECachedSprite.Text_Immune_Physical);
			GeometryBuilder.GeometryData geometryData = SceneAutoBehaviour<GeometryBuilder>.instance.CreateImage(cachedSprite, damageFloatData.GradientTop, damageFloatData.GradientBottom, damageFloatData.TextScale);
			if (geometryData != null)
			{
				geometryData.Position = oPosition;
				_normalContainer.Add(RenderInstance.EShaderType.Resist, geometryData, 1f);
			}
		}
	}

	public void OnResist(int creatureUid, string stateType)
	{
		if (!TryGetAttachPosition(creatureUid, EAttachType.Center, out var oPosition))
		{
			return;
		}
		DamageFloatData damageFloatData = _damageFloatDic[EDamageFloatType.Resist];
		GeometryBuilder.GeometryData geometryData = SceneAutoBehaviour<GeometryBuilder>.instance.CreateImage(GeometryBuilder.ECachedSprite.Text_Resist, damageFloatData.GradientTop, damageFloatData.GradientBottom, damageFloatData.IconScale);
		if (geometryData != null)
		{
			string text = stateType;
			StateData data = GameRoot.Instance.StateDataContainer.GetData(stateType);
			if (data != null && !String_Extension_Method.IsNullOrEmpty(data.Group))
			{
				text = data.Group;
			}
			if (String_Extension_Method.IsNullOrEmpty(text))
			{
				text = "Debuff";
			}
			Vector3 zero = Vector3.zero;
			GeometryBuilder.GeometryData geometryData2 = SceneAutoBehaviour<GeometryBuilder>.instance.CreateImage("Text_StateType_" + text, damageFloatData.GradientTop, damageFloatData.GradientBottom, damageFloatData.TextScale);
			if (geometryData2 != null)
			{
				Vector3 zero2 = Vector3.zero;
				zero.y = (0f - geometryData2.LayoutHeight) / 2f;
				zero2.y = geometryData.LayoutHeight / 2f;
				geometryData2.Position = oPosition;
				geometryData2.LayoutOffset = zero2;
				_normalContainer.Add(RenderInstance.EShaderType.Resist, geometryData2, 1f);
			}
			geometryData.Position = oPosition;
			geometryData.LayoutOffset = zero;
			_normalContainer.Add(RenderInstance.EShaderType.Resist, geometryData, 1f);
		}
	}
}
