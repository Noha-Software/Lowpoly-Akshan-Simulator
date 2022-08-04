using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Kevlaris.UI
{
	[RequireComponent(typeof(Image))]
	[RequireComponent(typeof(RectTransform))]
	internal class Progress : MonoBehaviour
	{
		RectTransform progress;
		Image progressImage;
		float value;
		float maxValue;
		float maxWidth;
		Gradient colors;
		float lerpTime = .5f;
		bool init = false;

		internal void Initialise(Color startColor, Color endColor, float maxValue, float lerpTime = .5f)
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
		internal void Initialise(Gradient colors, float maxValue, float lerpTime = .5f)
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

		internal void SetValue(float value)
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

		internal float GetValue()
		{
			return value;
		}
	}

	public class ProgressBar
	{
		Progress progress;
		bool init = false;

		public ProgressBar(Transform parent, Color startColor, Color endColor, float maxValue, float lerpTime = .5f)
		{
			GameObject progressBarObj = new GameObject("ProgressBar"); // create background gameobject for progressbar
			progressBarObj.transform.parent = parent;
			Image progressBarImg = progressBarObj.AddComponent<Image>();
			progressBarImg.color = new Color(180, 180, 180, 85); // #B4B4B4 with 85% transparency
			GameObject progressObj = new GameObject("Progress"); // create actual progress gameobject
			progressObj.transform.parent = progressObj.transform;
			progress = progressObj.AddComponent<Progress>();
			progress.Initialise(startColor, endColor, maxValue, lerpTime);
			init = true;
		}
		public ProgressBar(Transform parent, Gradient colors, float maxValue, float lerpTime = .5f)
		{
			GameObject progressBarObj = new GameObject("ProgressBar"); // create background gameobject for progressbar
			progressBarObj.transform.parent = parent;
			Image progressBarImg = progressBarObj.AddComponent<Image>();
			progressBarImg.color = new Color(180, 180, 180, 85); // #B4B4B4 with 85% transparency
			GameObject progressObj = new GameObject("Progress"); // create actual progress gameobject
			progressObj.transform.parent = progressObj.transform;
			progress = progressObj.AddComponent<Progress>();
			progress.Initialise(colors, maxValue, lerpTime);
			init = true;
		}

		public void SetValue(float value)
		{
			if (!init) return;
			progress.SetValue(value);
		}
		public float GetValue()
		{
			if (!init) return -1;
			return progress.GetValue();
		}
	}
}