using System.Collections.Generic;
using NGame2.NRender;
using NGame2.NUI;
using NVespa;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NBattleView.NUIOn3D;

public class WorldMapManager : MonoBehaviour
{
	public enum EType
	{
		Gold,
		ExpPortion,
		Ruby
	}

	public class Item
	{
		public OnFieldDisplayBase Ui;

		public float ElapsedTime;
	}

	private static WorldMapManager _instance;

	internal UIPanel _panel;

	private List<Item> _itemList = new List<Item>();

	private Camera _camera;

	public static WorldMapManager instance
	{
		get
		{
			CreateInstance();
			return _instance;
		}
	}

	public static bool IsValidInstance => _instance != null;

	public static void CreateInstance()
	{
		if (!(_instance != null))
		{
			LayerInfo layerInfo = GameUIRoot.instance.GetLayerInfo(GameUIRoot.ELayerType.Bottom3D);
			GameObject gameObject = new GameObject("UIOn3D WorldMapManager");
			gameObject.layer = layerInfo.Layer;
			Vespa_Unity_Transform_Extension_Method.ParentResetTM(gameObject.transform, GameUIRoot.instance.ContentsGroup.transform);
			_instance = gameObject.AddComponent<WorldMapManager>();
			_instance._panel = NGUITools.AddChild<UIPanel>(gameObject);
			_instance._panel.renderQueue = UIPanel.RenderQueue.StartAt;
			_instance._panel.startingRenderQueue = 4700;
			_instance._camera = layerInfo.Camera;
		}
	}

	private void Awake()
	{
	}

	private void OnDestroy()
	{
		_instance = null;
	}

	public void ChainUpdate()
	{
		UpdateNormal(_itemList);
	}

	public void UpdateNormal(List<Item> i_listItem)
	{
		Vector3 vector = GameUIRoot.instance.GetLayerInfo(GameUIRoot.ELayerType.Bottom3D).Camera.transform.forward + Vector3.up * GameSetting.data.DamageDisplay.WorldMapMoveUpSpeed;
		float popupTime = GameSetting.data.DamageDisplay.PopupTime;
		float waitTime = GameSetting.data.DamageDisplay.WaitTime;
		float shrinkTime = GameSetting.data.DamageDisplay.ShrinkTime;
		float moveUpTime = GameSetting.data.DamageDisplay.MoveUpTime;
		float disappearTime = GameSetting.data.DamageDisplay.DisappearTime;
		float num = GlobalTimer.deltaTime;
		float num2 = 1.5f;
		float num3 = 1f;
		for (int i = 0; i < i_listItem.Count; i++)
		{
			Item item = i_listItem[i];
			item.ElapsedTime += num;
			float num4 = popupTime;
			float num5 = shrinkTime;
			float num6 = ((i != 0) ? 0f : waitTime);
			float elapsedTime = item.ElapsedTime;
			if (elapsedTime < num4)
			{
				float t = elapsedTime / num4;
				item.Ui.Root.transform.localScale = Vector3.one * Mathf.Lerp(0.01f, num2, t);
				item.Ui.Translate(vector * num);
				if (i == 0)
				{
					num = GlobalTimer.deltaTime * shrinkTime / popupTime;
				}
				continue;
			}
			elapsedTime -= num4;
			if (elapsedTime < num6)
			{
				item.Ui.Root.transform.localScale = Vector3.one * num2;
				continue;
			}
			elapsedTime -= num6;
			if (elapsedTime < num5)
			{
				float t2 = elapsedTime / num5;
				item.Ui.Root.transform.localScale = Vector3.one * Mathf.Lerp(num2, num3, t2);
				item.Ui.Translate(vector * num);
				continue;
			}
			elapsedTime -= num5;
			if (elapsedTime < moveUpTime)
			{
				item.Ui.Root.transform.localScale = Vector3.one * num3;
				item.Ui.Translate(vector * num);
				continue;
			}
			elapsedTime -= moveUpTime;
			if (elapsedTime < disappearTime)
			{
				float num7 = elapsedTime / disappearTime;
				item.Ui.Root.alpha = Mathf.Clamp01(1f - num7);
				item.Ui.Translate(vector * num);
			}
			else
			{
				i_listItem.RemoveAt(i);
				i--;
				ReleaseItem(item);
			}
		}
	}

	private void ReleaseItem(Item i_item)
	{
		FactoryBase.ReleaseComponent(i_item.Ui);
	}

	public void AddGold(int i_amount, RenderFacade renderFacade)
	{
		if (GameSetting.data.DamageDisplay.Disabled)
		{
			return;
		}
		Damage damage;
		using (new RecommendPanel(_panel))
		{
			damage = Damage.WorldMapCoinPrefabAuto.Factory.Create(_panel.transform);
			if (damage == null)
			{
				return;
			}
		}
		UIHelper.SetLabelText(damage.Label, "+" + Vespa_Int_Extension_Method.ToCommaString(i_amount));
		Vector3 offset = CalcDisplayOffset(EType.Gold, renderFacade);
		damage.InitPosition(_camera, renderFacade.transform, offset, followCreature: true);
		Item item = new Item();
		item.Ui = damage;
		item.Ui.Root.alpha = 1f;
		item.Ui.Root.transform.localScale = Vector3.one * 0.01f;
		item.ElapsedTime = 0f;
		_itemList.Add(item);
	}

	public void AddRuby(int i_amount, RenderFacade renderFacade)
	{
		if (GameSetting.data.DamageDisplay.Disabled)
		{
			return;
		}
		Damage damage;
		using (new RecommendPanel(_panel))
		{
			damage = Damage.WorldMapRubyPrefabAuto.Factory.Create(_panel.transform);
			if (damage == null)
			{
				return;
			}
		}
		UIHelper.SetLabelText(damage.Label, "+" + Vespa_Int_Extension_Method.ToCommaString(i_amount));
		Vector3 offset = CalcDisplayOffset(EType.Ruby, renderFacade);
		damage.InitPosition(_camera, renderFacade.transform, offset, followCreature: true);
		Item item = new Item();
		item.Ui = damage;
		item.Ui.Root.alpha = 1f;
		item.Ui.Root.transform.localScale = Vector3.one * 0.01f;
		item.ElapsedTime = 0f;
		_itemList.Add(item);
	}

	private Vector3 CalcDisplayOffset(EType i_type, RenderFacade renderFacade)
	{
		Vector3 lossyScale = renderFacade.transform.lossyScale;
		float num = Mathf.Max(Mathf.Max(lossyScale.x, lossyScale.y), lossyScale.z);
		return Vector3.up * renderFacade.ShapeBounds.size.y * 0.75f * num;
	}

	private Vector3 CalcDisplayBaseScale(EType i_type)
	{
		return Vector3.one;
	}

	private string GetDisplayString(EType i_type, int i_amount)
	{
		return i_type switch
		{
			EType.Gold => i_amount.ToString(), 
			EType.Ruby => string.Empty, 
			EType.ExpPortion => string.Empty, 
			_ => i_amount.ToString(), 
		};
	}

	private Color GetDisplayColor(EType i_type, bool i_critical, RenderFacade renderFacade)
	{
		return Color.white;
	}
}
