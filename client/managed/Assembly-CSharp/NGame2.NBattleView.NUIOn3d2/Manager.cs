using System.Collections.Generic;
using NGame2.N3D;
using NGame2.NBattleView.NDamageOn3d;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NScene;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NBattleView.NUIOn3d2;

public class Manager : MonoBehaviour
{
	private static Manager _instance;

	private static bool DisableThis;

	private static bool _disableNewDamage;

	internal UIPanel _panel;

	private List<OnFieldDisplayBase> _displayList = new List<OnFieldDisplayBase>();

	public static Manager instance
	{
		get
		{
			CreateInstance();
			return _instance;
		}
	}

	public static bool IsValidInstance => _instance != null;

	public static bool DisableNewDamage => _disableNewDamage;

	private static bool ShowAllDamage => Input.GetKey(KeyCode.LeftControl);

	public int DrawCallCount => _panel.DrawCallCount;

	internal Camera _camera
	{
		get
		{
			if (!Game3DRoot.IsValidInstance)
			{
				return null;
			}
			if (Game3DRoot.instance.GameCamera == null)
			{
				return null;
			}
			return Game3DRoot.instance.GameCamera.MainCamera;
		}
	}

	public static void CreateInstance()
	{
		if (!(_instance != null))
		{
			if (GameRoot.isValidInstance)
			{
				_disableNewDamage = GameRoot.Instance.ConstantDataContainer.GetValueBool("DisableNewDamage");
			}
			LayerInfo layerInfo = Game3DRoot.instance.GameCamera.GetLayerInfo(Game3DRoot.ELayerType.Default);
			GameObject gameObject = new GameObject("UIOn3d2 Manager");
			gameObject.layer = layerInfo.Layer;
			Vespa_Unity_Transform_Extension_Method.ParentResetTM(gameObject.transform, Game3DRoot.instance.ContentsGroup.transform);
			_instance = gameObject.AddComponent<Manager>();
			_instance._panel = NGUITools.AddChild<UIPanel>(gameObject);
			_instance._panel.renderQueue = UIPanel.RenderQueue.StartAt;
			_instance._panel.startingRenderQueue = 4700;
			OnFieldDisplayBase.ResetStatic(_instance._camera);
		}
	}

	private void OnDestroy()
	{
		_instance = null;
		OnFieldDisplayBase.ChainOnDestroy();
	}

	private void Start()
	{
		SceneAutoBehaviour<GuiManager>.instance.Add("UiOn3D.Manager", this, delegate
		{
			DisableThis = GUILayout.Toggle(DisableThis, "Disable This");
		});
	}

	public void ReserveCache()
	{
		Damage.PrefabAuto_Normal.Factory.ReserveComponent(50);
		Damage.PrefabAuto_Critical.Factory.ReserveComponent(20);
		Damage.PrefabAuto_Block.Factory.ReserveComponent(20);
		Damage.PrefabAuto_Heal.Factory.ReserveComponent(20);
		DamageDot.PrefabAuto.Factory.ReserveComponent(20);
		DamageConstant.Miss_PrefabAuto.Factory.ReserveComponent(20);
		DamageResist.PrefabAuto.Factory.ReserveComponent(20);
		LootCoin.PrefabAuto.Factory.ReserveComponent(10);
	}

	public void OnEventCamera(EventCamera.EventValue current)
	{
		float num = ((current.ApplyTeamType == TeamType.All) ? current.UiOn3dAlpha : 1f);
		if (!DisableNewDamage)
		{
			SceneAutoBehaviour<NGame2.NBattleView.NDamageOn3d.Manager>.instance.OnEventCamera(num, current.ApplyTeamType);
		}
		_panel.alpha = num;
	}

	public void ChainUpdate()
	{
		if (!DisableNewDamage)
		{
			SceneAutoBehaviour<NGame2.NBattleView.NDamageOn3d.Manager>.instance.ChainUpdate();
		}
		OnFieldDisplayBase.MainCamera = _instance._camera;
		for (int i = 0; i < _displayList.Count; i++)
		{
			OnFieldDisplayBase onFieldDisplayBase = _displayList[i];
			onFieldDisplayBase.ChainUpdate();
			if (onFieldDisplayBase.IsEnd)
			{
				FactoryBase.ReleaseComponent(onFieldDisplayBase);
				_displayList.RemoveAt(i);
				i--;
			}
		}
	}

	private T CreateDisplayInstance<T>(PrefabAuto<T> prefabAuto) where T : OnFieldDisplayBase
	{
		if (DisableThis)
		{
			return null;
		}
		T val = prefabAuto.Factory.Create(_panel.transform, setActive: false);
		if (val.gameObject.layer != _panel.gameObject.layer)
		{
			Vespa_Unity_GameObject_Extension_Method.ChangeLayerRecursive(val.gameObject, _panel.gameObject.layer);
		}
		return val;
	}

	private void CreateDamageDisplay(PrefabAuto<Damage> damagePrefab, int creatureUid, long damage)
	{
		Damage damage2 = CreateDisplayInstance(damagePrefab);
		if (!(damage2 == null))
		{
			if (!damage2.Open(creatureUid, damage))
			{
				FactoryBase.ReleaseComponent(damage2);
			}
			else
			{
				_displayList.Add(damage2);
			}
		}
	}

	public void OnHeal(int creatureUid, long heal)
	{
		if (!DisableNewDamage)
		{
			SceneAutoBehaviour<NGame2.NBattleView.NDamageOn3d.Manager>.instance.OnHeal(creatureUid, heal);
			if (!ShowAllDamage)
			{
				return;
			}
		}
		using (new RecommendPanel(_panel))
		{
			CreateDamageDisplay(Damage.PrefabAuto_Heal, creatureUid, heal);
		}
	}

	public void OnDamageNormal(int creatureUid, long damage)
	{
		if (!DisableNewDamage)
		{
			SceneAutoBehaviour<NGame2.NBattleView.NDamageOn3d.Manager>.instance.OnDamageNormal(creatureUid, damage);
			if (!ShowAllDamage)
			{
				return;
			}
		}
		using (new RecommendPanel(_panel))
		{
			CreateDamageDisplay(Damage.PrefabAuto_Normal, creatureUid, damage);
		}
	}

	public void OnDamageSkill(int creatureUid, long damage)
	{
		if (!DisableNewDamage)
		{
			SceneAutoBehaviour<NGame2.NBattleView.NDamageOn3d.Manager>.instance.OnDamageSkill(creatureUid, damage);
			if (!ShowAllDamage)
			{
				return;
			}
		}
		using (new RecommendPanel(_panel))
		{
			CreateDamageDisplay(Damage.PrefabAuto_Skill, creatureUid, damage);
		}
	}

	public void OnDamageCritical(int creatureUid, long damage)
	{
		if (!DisableNewDamage)
		{
			SceneAutoBehaviour<NGame2.NBattleView.NDamageOn3d.Manager>.instance.OnDamageCritical(creatureUid, damage);
			if (!ShowAllDamage)
			{
				return;
			}
		}
		using (new RecommendPanel(_panel))
		{
			CreateDamageDisplay(Damage.PrefabAuto_Critical, creatureUid, damage);
		}
	}

	public void OnBlock(int creatureUid, long damage)
	{
		if (!DisableNewDamage)
		{
			SceneAutoBehaviour<NGame2.NBattleView.NDamageOn3d.Manager>.instance.OnBlock(creatureUid, damage);
			if (!ShowAllDamage)
			{
				return;
			}
		}
		using (new RecommendPanel(_panel))
		{
			CreateDamageDisplay(Damage.PrefabAuto_Block, creatureUid, damage);
		}
	}

	public void OnDamageDot(int creatureUid, long damage, string stateType)
	{
		if (!DisableNewDamage)
		{
			SceneAutoBehaviour<NGame2.NBattleView.NDamageOn3d.Manager>.instance.OnDamageDot(creatureUid, damage, stateType);
			if (!ShowAllDamage)
			{
				return;
			}
		}
		using (new RecommendPanel(_panel))
		{
			if (String_Extension_Method.IsNullOrEmpty(stateType))
			{
				stateType = string.Empty;
			}
			string stateType2 = stateType;
			StateData data = GameRoot.Instance.StateDataContainer.GetData(stateType);
			if (data != null && !String_Extension_Method.IsNullOrEmpty(data.Group))
			{
				stateType2 = data.Group;
			}
			DamageDot damageDot = CreateDisplayInstance(DamageDot.PrefabAuto);
			if (!(damageDot == null))
			{
				if (!damageDot.Open(creatureUid, damage, stateType2))
				{
					FactoryBase.ReleaseComponent(damageDot);
				}
				else
				{
					_displayList.Add(damageDot);
				}
			}
		}
	}

	public void OnMiss(int creatureUid)
	{
		if (!DisableNewDamage)
		{
			SceneAutoBehaviour<NGame2.NBattleView.NDamageOn3d.Manager>.instance.OnMiss(creatureUid);
			if (!ShowAllDamage)
			{
				return;
			}
		}
		using (new RecommendPanel(_panel))
		{
			DamageConstant damageConstant = CreateDisplayInstance(DamageConstant.Miss_PrefabAuto);
			if (!(damageConstant == null))
			{
				if (!damageConstant.Open(creatureUid))
				{
					FactoryBase.ReleaseComponent(damageConstant);
				}
				else
				{
					_displayList.Add(damageConstant);
				}
			}
		}
	}

	public void OnImmune(int creatureUid, bool isMagical)
	{
		if (!DisableNewDamage)
		{
			SceneAutoBehaviour<NGame2.NBattleView.NDamageOn3d.Manager>.instance.OnImmune(creatureUid, isMagical);
			if (!ShowAllDamage)
			{
				return;
			}
		}
		using (new RecommendPanel(_panel))
		{
			DamageConstant damageConstant = CreateDisplayInstance(isMagical ? DamageConstant.ImmuneMagical_PrefabAuto : DamageConstant.ImmunePhysical_PrefabAuto);
			if (!(damageConstant == null))
			{
				if (!damageConstant.Open(creatureUid))
				{
					FactoryBase.ReleaseComponent(damageConstant);
				}
				else
				{
					_displayList.Add(damageConstant);
				}
			}
		}
	}

	public void OnResist(int creatureUid, string stateType)
	{
		if (!DisableNewDamage)
		{
			SceneAutoBehaviour<NGame2.NBattleView.NDamageOn3d.Manager>.instance.OnResist(creatureUid, stateType);
			if (!ShowAllDamage)
			{
				return;
			}
		}
		using (new RecommendPanel(_panel))
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
			DamageResist damageResist = CreateDisplayInstance(DamageResist.PrefabAuto);
			if (!(damageResist == null))
			{
				if (!damageResist.Open(creatureUid, text))
				{
					FactoryBase.ReleaseComponent(damageResist);
				}
				else
				{
					_displayList.Add(damageResist);
				}
			}
		}
	}

	public void OnLootCoin(int creatureUid, int amount)
	{
		using (new RecommendPanel(_panel))
		{
			LootCoin lootCoin = CreateDisplayInstance(LootCoin.PrefabAuto);
			if (!(lootCoin == null))
			{
				if (!lootCoin.Open(creatureUid, amount))
				{
					FactoryBase.ReleaseComponent(lootCoin);
				}
				else
				{
					_displayList.Add(lootCoin);
				}
			}
		}
	}

	public void OnLootItem(int creatureUid, ItemData item)
	{
		using (new RecommendPanel(_panel))
		{
			LootItem lootItem = CreateDisplayInstance(LootItem.PrefabAuto);
			if (!(lootItem == null))
			{
				if (!lootItem.Open(creatureUid, item))
				{
					FactoryBase.ReleaseComponent(lootItem);
				}
				else
				{
					_displayList.Add(lootItem);
				}
			}
		}
	}
}
