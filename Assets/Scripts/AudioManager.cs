using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Background music: Jeremy Blake - Powerup! (https://www.youtube.com/watch?v=l7SwiFWOQqM)
//Tree hit: https://freesound.org/people/Josethehedgehog/sounds/390362/ (CC0)

public class AudioManager : MonoBehaviour
{
	float volume = .75f;
    public static AudioManager instance;
	private static AudioSource BM;
    List<AudioSource> sources = new List<AudioSource>();
    List<string> sourcenames = new List<string>();

	private void Awake()
	{
		instance = this;
	}

    private void Start()
    {
		BM = gameObject.AddComponent<AudioSource>();
		BM.clip = LoadClip("Jeremy Blake - Powerup!");
		BM.volume = volume;
		BM.loop = true;
		BM.Play();
	}

    public void PlaySound(string name)
	{
		AudioSource SFX = gameObject.AddComponent<AudioSource>();

		try
		{
			SFX.clip = LoadClip(name);
		}
		catch (System.Exception)
		{
			Debug.LogWarning($"Can't load audio \"{name}\"");
			Destroy(SFX);
			return;
		}

		if (sourcenames.Contains(name))
		{
			return;
		}

		SFX.volume = volume;
		sources.Add(SFX);
		sourcenames.Add(name);
		SFX.Play();

		StartCoroutine(RemoveSFX());
		IEnumerator RemoveSFX()
		{
			yield return new WaitForSeconds(SFX.clip.length);
			sources.Remove(SFX);
			sourcenames.Remove(name);
			Destroy(SFX);
		}
	}

	AudioClip LoadClip(string name)
	{
		AudioClip clip = Resources.Load<AudioClip>($"Audio/{name}");
		if (clip == null)
		{
			throw new System.IO.FileNotFoundException($"Couldn't find an AudioClip at Resources/Audio/{name}");
		}
		return clip;
	}

	public float GetVolume() => volume;
	public void SetVolume(float volume)
	{
		this.volume = volume;
		foreach (var src in sources)
		{
			src.volume = this.volume;
		}
	}
}
