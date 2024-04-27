using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

//Background music: Jeremy Blake - Powerup! (https://www.youtube.com/watch?v=l7SwiFWOQqM)

public class AudioManager : MonoBehaviour
{
	public static AudioManager instance;
	private static AudioSource BM;
	public AudioMixer audioMixer;
	List<AudioSource> sources = new();

	public AudioMixerGroups Mixer => mixerGroups;
	[SerializeField] AudioMixerGroups mixerGroups;

	[Header("Settings")]
	public bool autoplayBM = true;

	[System.Serializable]
	public sealed class AudioMixerGroups
	{
		public AudioMixerGroup music, UI, SFX, ambience;
	}

	private void Awake()
	{
		instance = this;
	}

	private void Start()
	{
		if (autoplayBM) StartBM();
	}

	public void StartBM()
	{
		if (BM == null)
		{
			BM = gameObject.AddComponent<AudioSource>();
			BM.clip = LoadClip("Jeremy Blake - Powerup!");
			BM.outputAudioMixerGroup = mixerGroups.music;
			BM.loop = true;
			BM.Play();
		}
	}

	public void StopBM()
	{
		if (BM != null)
		{
			BM.Stop();
			Destroy(BM);
		}
	}

	[System.Obsolete("An Audio Mixer Group should be specified for every SFX")]
	public void PlaySound(string name)
	{
		PlaySound(name, null);
	}
	public void PlaySound(string name, AudioMixerGroup mixerGroup)
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

		SFX.outputAudioMixerGroup = mixerGroup;
		sources.Add(SFX);
		SFX.Play();

		StartCoroutine(RemoveSFX());
		IEnumerator RemoveSFX()
		{
			yield return new WaitForSeconds(SFX.clip.length);
			sources.Remove(SFX);
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
}
