using Avatar = NGame2.NUI.NWorldMap.NMapLayout.Avatar;
using NGame2.NRender;
using NGame2.NUI.NWorldMap;
using NGame2.NUI.NWorldMap.NMapLayout;
using NGameAsset;
using UnityEngine;

public sealed class SprkLobbyAccessoryState : MonoBehaviour
{
	public readonly SprkLobbyAccessoryLease Lease = new SprkLobbyAccessoryLease();

	public readonly SprkLobbyAppearance Appearance = new SprkLobbyAppearance();

	public Avatar Target;

	public ICreatureLoad ObservedLoad;

	public ICreature ObservedCreature;

	public float Deadline;

	public float NextTry;

	private void OnDisable()
	{
		Lease.Cancel();
		ObservedLoad = null;
		ObservedCreature = null;
		NextTry = 0f;
	}

	private void Update()
	{
		if (Target == null)
		{
			return;
		}
		object value = SprkLobbyAvatar.CurrentOwner();
		if (!Lease.Matches(value))
		{
			Lease.Bind(null);
			Target = null;
		}
		else
		{
			if (Time.realtimeSinceStartup < NextTry)
			{
				return;
			}
			NextTry = Time.realtimeSinceStartup + 0.1f;
			int num = Appearance.Observe(SprkLobbyAvatar.CurrentHero());
			if ((num & 1) != 0)
			{
				SprkLobbyAvatar.Reload(GetComponent<MapLayout>(), Target);
				return;
			}
			if (num != 0)
			{
				Lease.Queue(value);
				Deadline = Time.realtimeSinceStartup + 10f;
			}
			RenderFacade renderFacade = Target.RenderFacade;
			ICreatureLoad creatureLoad = ((renderFacade == null) ? null : renderFacade.AssetCreatureAsync);
			ICreature creature = creatureLoad?.Value;
			if (creatureLoad != ObservedLoad || creature != ObservedCreature)
			{
				ObservedLoad = creatureLoad;
				ObservedCreature = creature;
				Lease.Queue(value);
				Deadline = Time.realtimeSinceStartup + 10f;
			}
			if (Lease.Pending)
			{
				if (SprkLobbyAvatar.Apply(Target))
				{
					Lease.Take(value, ready: true);
				}
				else if (Time.realtimeSinceStartup >= Deadline)
				{
					Lease.Take(value, ready: true);
					Debug.LogWarning("SPRK lobby accessories: avatar not ready; kept current hero.");
				}
			}
		}
	}
}
