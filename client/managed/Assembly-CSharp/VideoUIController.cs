using UnityEngine;
using UnityEngine.Video;

public class VideoUIController : MonoBehaviour
{
	public VideoPlayer videoPlayer;

	private void Start()
	{
		videoPlayer.Play();
	}
}
