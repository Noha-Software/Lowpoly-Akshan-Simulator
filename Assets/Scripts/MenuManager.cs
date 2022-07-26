using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
	public List<GameObject> screens = new List<GameObject>();
	[SerializeField] GameObject currentScreen;

	private void Start()
	{
		ChangeScreen(0);
	}

	public void ChangeScreen(int index)
	{
		for (int i = 0; i < screens.Count; i++)
		{
			if (i == index)
			{
				currentScreen = screens[i];
				currentScreen.SetActive(true);
			}
			else
			{
				screens[i].SetActive(false);
			}
		}
	}

	public void EnterGame()
	{
		SceneManager.LoadScene("Main", LoadSceneMode.Single);
	}

    public void ExitGame()
	{
		Application.Quit();
	}
}
