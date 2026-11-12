using NGame2.NUI.NComponent2.NGuildAttendance;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NWindow.NGuildAttendance;

public class GuildAttendancePopup : BaseWindow
{
	public interface IListener
	{
		void CreateGuildAttendanceComponent(ref GuildAttendanceComponent component, Transform rootTransform);
	}

	public const string PrefabPath = "Prefab2/Window/Guild/GuildAttendancePopup";

	public Transform GuildAttendanceComponentRootTransform;

	private IListener _iListener;

	private GuildAttendanceComponent _guildAttendanceComp;

	public void Open(IListener iListener)
	{
		Open(delegate
		{
			Init(iListener);
		}, delegate
		{
		});
	}

	public override void Close()
	{
		base.Close();
		FactoryBase.SafeReleaseComponent(ref _guildAttendanceComp);
	}

	public override void OnSystemBackButton()
	{
		base.OnSystemBackButton();
		Close();
	}

	private void Init(IListener iListener)
	{
		_iListener = iListener;
		CreateGuildAttendanceComponent();
	}

	private void CreateGuildAttendanceComponent()
	{
		if (_iListener != null)
		{
			_iListener.CreateGuildAttendanceComponent(ref _guildAttendanceComp, GuildAttendanceComponentRootTransform);
		}
	}
}
