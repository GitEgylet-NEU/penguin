using NohaSoftware.Utilities;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
	[SerializeField] RectTransform fill;
	[SerializeField] RectTransform background;
	Color original;

	public float min = 0f;
	public float max;
	public Gradient shineGradient;
	public float shineDuration = .2f;
	Coroutine shineCoroutine;
	float _Value;
	public float Value
	{
		get
		{
			return _Value;
		}
		private set
		{
			_Value = Mathf.Clamp(value, min, max);
		}
	}
	public bool disappearOnZero = false;

	private void Start()
	{
		original = fill.GetComponent<Image>().color;
	}

	private void Update()
	{
		if (disappearOnZero && Value == min) gameObject.SetActive(false);

		fill.SetWidth((Value - min) / (max - min) * background.rect.width);
	}

	public void SetValue(float newValue)
	{
		if (shineDuration > 0f && newValue != Value)
		{
			float multiplier = Mathf.Abs(Value - newValue) / (max - min);
			if (shineCoroutine != null) StopCoroutine(shineCoroutine);
			shineCoroutine = StartCoroutine(ShineCoroutine(multiplier));
		}
		Value = newValue;
	}
	public void ChangeValue(float amount)
	{
		Value += amount;
	}

	IEnumerator ShineCoroutine(float intensity)
	{
		Image sr = fill.GetComponent<Image>();
		Color start = sr.color;
		float t = 0f;
		while (t <= shineDuration / 2f)
		{
			sr.color = Color.Lerp(start, shineGradient.Evaluate(intensity), t * 2f);
			t += Time.deltaTime;
			yield return null;
		}
		t = 0f;
		while (t <= shineDuration / 2f)
		{
			sr.color = Color.Lerp(shineGradient.Evaluate(intensity), original, t * 2f);
			t += Time.deltaTime;
			yield return null;
		}
		sr.color = original;
	}
}
