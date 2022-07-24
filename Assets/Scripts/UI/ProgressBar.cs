using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Kevlaris.UI
{
	public class ProgressBar : MonoBehaviour
	{
		RectTransform progress;
		Image progressImage;
		float value;
		float maxValue;
		float maxWidth;
		Gradient colors;
		float lerpTime = .5f;
		bool init = false;
		public void Initialise(Color startColor, Color endColor, float maxValue, float lerpTime = .5f)
		{
			GradientColorKey startColorKey = new GradientColorKey(startColor, 1);
			GradientColorKey endColorKey = new GradientColorKey(endColor, 0);
			colors = new Gradient
			{
				colorKeys = new GradientColorKey[] { startColorKey, endColorKey }
			};
			this.maxValue = maxValue;
			value = maxValue;
			this.lerpTime = lerpTime;
			progress = transform.GetChild(0).GetComponent<RectTransform>();
			progressImage = progress.GetComponent<Image>();
			maxWidth = progress.sizeDelta.x;
			gameObject.SetActive(true);
			init = true;
		}
		public void Initialise(Gradient colors, float maxValue, float lerpTime = .5f)
		{
			this.colors = colors;
			this.maxValue = maxValue;
			value = maxValue;
			this.lerpTime = lerpTime;
			progress = transform.GetChild(0).GetComponent<RectTransform>();
			progressImage = progress.GetComponent<Image>();
			maxWidth = progress.sizeDelta.x;
			gameObject.SetActive(true);
			init = true;
		}

		public void SetValue(float value)
		{
			if (!init) return;
			if (value < 0)
				this.value = 0;
			else
				this.value = value;
			StartCoroutine(LerpToValue(this.value));
		}

		IEnumerator LerpToValue(float target)
		{
			float time = 0;
			while (time < lerpTime)
			{
				progress.sizeDelta = new Vector2(Mathf.Lerp(progress.sizeDelta.x, target / maxValue * maxWidth, time / lerpTime), progress.sizeDelta.y);
				progressImage.color = Color.Lerp(progressImage.color, colors.Evaluate(target / maxValue), time / lerpTime);
				time += Time.deltaTime;
				yield return null;
			}
			progress.sizeDelta = new Vector2(target / maxValue * maxWidth, progress.sizeDelta.y);
			progressImage.color = colors.Evaluate(value / maxValue);
		}

		public float GetValue()
		{
			return value;
		}
	}
}