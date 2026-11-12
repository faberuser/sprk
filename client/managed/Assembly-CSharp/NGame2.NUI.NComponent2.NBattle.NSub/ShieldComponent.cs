using System.Collections.Generic;
using NGame2.NBattleView;
using NGame2.NBattleView.NUIOn3D;
using NGame2.NMessage;
using NGame2.NMessage.NFastHandler;
using NShared;
using NVespa;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NBattle.NSub;

public class ShieldComponent : LinkFactoryComponent, IInspectorGuiDrawer
{
	private sealed class ShieldStateInfo
	{
		public long ShieldHp;

		public long ShieldMaxHp;
	}

	public const string PrefabPath = "Prefab2/Component/Battle/Sub/ShieldComponent";

	public static PrefabAuto<ShieldComponent> PrefabAuto = new PrefabAuto<ShieldComponent>("Prefab2/Component/Battle/Sub/ShieldComponent");

	public static Color ShieldColor_Physical = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	public static Color ShieldColor_Magical = new Color32(0, 150, byte.MaxValue, byte.MaxValue);

	public static Color ShieldColor_Perfect = new Color32(byte.MaxValue, 200, 0, byte.MaxValue);

	public UIWidget RootWidget;

	public UISprite GaugeSprite;

	public UIWidget ShieldTextRoot;

	public UILabel ShieldValueLabel;

	private int _creatureUid;

	private Holder _receiverHolder = new Holder();

	private Dictionary<long, ShieldStateInfo> _dicCurrentShieldState = new Dictionary<long, ShieldStateInfo>(10);

	public bool IsEnabled => UIHelper.IsEnabledGameObjet(RootWidget);

	public void Open(int creatureUid)
	{
		_creatureUid = creatureUid;
		UIHelper.EnableGameObject(RootWidget, enable: false);
		UIHelper.EnableGameObject(ShieldTextRoot, enable: false);
		CreatureVerifier verifier = new CreatureVerifier(_creatureUid);
		_receiverHolder.AddByCreatureUid<CallbackMessage<ShieldBegin>>(verifier, OnMessageFast, _creatureUid);
		_receiverHolder.AddByCreatureUid<CallbackMessage<ShieldEnd>>(verifier, OnMessageFast, _creatureUid);
		_receiverHolder.AddByCreatureUid<CallbackMessage<ShieldModified>>(verifier, OnMessageFast, _creatureUid);
	}

	public void Reset()
	{
		_creatureUid = 0;
		_dicCurrentShieldState.Clear();
	}

	public float Reposition()
	{
		if (RootWidget == null)
		{
			return 0f;
		}
		float num = RootWidget.cachedTransform.localPosition.y;
		if (UIHelper.IsEnabledGameObjet(RootWidget))
		{
			num -= (float)RootWidget.height;
		}
		return num;
	}

	public void ChainUpdate()
	{
	}

	private void OnMessageFast(CallbackMessage<ShieldBegin> message)
	{
		if (RootWidget == null || !message.IsValidMessage() || message.Callback.CreatureUid != _creatureUid)
		{
			return;
		}
		bool flag = (message.Callback.DamageFlag & DamageFlag.Physical) != 0;
		bool flag2 = (message.Callback.DamageFlag & DamageFlag.Magical) != 0;
		Color color;
		if (flag && flag2)
		{
			color = ShieldColor_Perfect;
		}
		else if (flag)
		{
			color = ShieldColor_Physical;
		}
		else
		{
			if (!flag2)
			{
				UIHelper.EnableGameObject(RootWidget, enable: false);
				return;
			}
			color = ShieldColor_Magical;
		}
		UIHelper.SetWidgetColor(GaugeSprite, color);
		AddShield(message.Callback.OperationUid, message.Callback.ShieldMaxHp);
	}

	private void OnMessageFast(CallbackMessage<ShieldEnd> message)
	{
		if (!(RootWidget == null) && message.IsValidMessage() && message.Callback.CreatureUid == _creatureUid)
		{
			RemoveShield(message.Callback.OperationUid);
		}
	}

	private void OnMessageFast(CallbackMessage<ShieldModified> message)
	{
		if (!(RootWidget == null) && message.IsValidMessage() && message.Callback.CreatureUid == _creatureUid)
		{
			ModifiedShield(message.Callback.OperationUid, message.Callback.ShieldHp);
		}
	}

	private void UpdateShield()
	{
		bool flag = __Extension_Method_ICollection__.COUNT(_dicCurrentShieldState) > 0;
		UIHelper.EnableGameObject(RootWidget, flag);
		if (flag)
		{
			long num = 0L;
			long num2 = 0L;
			Dictionary<long, ShieldStateInfo>.Enumerator enumerator = _dicCurrentShieldState.GetEnumerator();
			while (enumerator.MoveNext())
			{
				ShieldStateInfo value = enumerator.Current.Value;
				num += value.ShieldHp;
				num2 += value.ShieldMaxHp;
			}
			num2 = ((num2 > 0) ? num2 : 1);
			float fillAmount = (float)num / (float)num2;
			UIHelper.SetSpriteFillAmount(GaugeSprite, fillAmount);
			if (HUD.ToggleHpNumber.Value)
			{
				UIHelper.EnableGameObject(ShieldTextRoot, enable: true);
				string text = $"Hp {num} / {num2}";
				UIHelper.SetLabelText(ShieldValueLabel, text);
			}
		}
	}

	private void AddShield(long operationId, long maxHp)
	{
		if (!_dicCurrentShieldState.ContainsKey(operationId))
		{
			ShieldStateInfo value = new ShieldStateInfo
			{
				ShieldHp = maxHp,
				ShieldMaxHp = maxHp
			};
			_dicCurrentShieldState.Add(operationId, value);
			UpdateShield();
		}
	}

	private void ModifiedShield(long operationId, long hp)
	{
		if (_dicCurrentShieldState.ContainsKey(operationId))
		{
			_dicCurrentShieldState[operationId].ShieldHp = hp;
			UpdateShield();
		}
	}

	private void RemoveShield(long operationId)
	{
		if (_dicCurrentShieldState.ContainsKey(operationId))
		{
			_dicCurrentShieldState.Remove(operationId);
			UpdateShield();
		}
	}
}
