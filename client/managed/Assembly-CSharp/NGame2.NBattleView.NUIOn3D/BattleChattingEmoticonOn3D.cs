using NShared;
using NVespa;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NBattleView.NUIOn3D;

public class BattleChattingEmoticonOn3D : BaseBattleChattingEmoticon
{
	public interface IValue
	{
		bool IsDie { get; }

		Vector2 EmoticonOffset { get; }
	}

	public new const string PrefabPath = "Resources/Prefab2/UIOn3D/BattleChattingEmoticonOn3D";

	public static PrefabAuto<BattleChattingEmoticonOn3D> PrefabAuto = new PrefabAuto<BattleChattingEmoticonOn3D>("Resources/Prefab2/UIOn3D/BattleChattingEmoticonOn3D");

	private IValue _value;

	public float EventAlpha = 1f;

	public void Open(IValue value, TeamType teamType)
	{
		_value = value;
		SetPosition();
		SetFrameDirectionByTeamType(teamType);
	}

	public void SetPosition()
	{
		if (_value != null)
		{
			base.transform.localPosition = _value.EmoticonOffset;
		}
	}

	public void UpdateOn3D()
	{
		if (_value != null && base.CheckUpdateElasedTime())
		{
			float a = ((!_value.IsDie) ? Mathf.MoveTowards(_panel.alpha, 1f, UnitySystemTimer.deltaTime * 3f) : Mathf.MoveTowards(_panel.alpha, 0f, UnitySystemTimer.deltaTime * 3f));
			_panel.alpha = Mathf.Min(a, EventAlpha);
			base.transform.rotation = Game3DRoot.instance.GameCamera.MainCamera.transform.rotation;
		}
	}

	public override void ChainUpdate()
	{
		base.ChainUpdate();
		UpdateOn3D();
	}

	public override bool CheckUpdateElasedTime()
	{
		if (!base.CheckUpdateElasedTime())
		{
			return false;
		}
		if (_elapsedTime < _restoreTime && _panel.alpha == 1f)
		{
			_elapsedTime += Time.deltaTime;
			return false;
		}
		if (_elapsedTime < _restoreTime)
		{
			return false;
		}
		return true;
	}

	private void SetFrameDirectionByTeamType(TeamType teamType)
	{
		FrameDirectionType frameTypeByTeamType = GetFrameTypeByTeamType(teamType);
		SetFrameDirection(frameTypeByTeamType);
	}

	private FrameDirectionType GetFrameTypeByTeamType(TeamType teamType)
	{
		return teamType switch
		{
			TeamType.Left => FrameDirectionType.BottomLeft, 
			TeamType.Right => FrameDirectionType.BottomRight, 
			_ => FrameDirectionType.BottomMiddle, 
		};
	}
}
