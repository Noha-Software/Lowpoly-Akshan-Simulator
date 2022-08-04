using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class MenuManager : MonoBehaviourPunCallbacks
{
	[Header("Panels")]
	public List<GameObject> panels = new List<GameObject>();
	[SerializeField] GameObject currentPanel;

	[Header("Input")]
	[SerializeField] TMP_InputField createLobbyInput;
	[SerializeField] TMP_InputField joinLobbyInput;
	[SerializeField] TMP_InputField usernameInput;
	[SerializeField] GameObject usernamePrompt;

	[Header("Player")]
	string username;

	private void Start()
	{
		ChangeScreen(0);
		usernamePrompt.SetActive(true);
	}

	public override void OnJoinedRoom()
	{
		PhotonNetwork.LoadLevel("Main");
	}

	public void ChangeScreen(int index)
	{
		for (int i = 0; i < panels.Count; i++)
		{
			if (i == index)
			{
				currentPanel = panels[i];
				currentPanel.SetActive(true);
			}
			else
			{
				panels[i].SetActive(false);
			}
		}
	}

	public void ExitGame()
	{
		Application.Quit();
	}

	public void LogoClick()
	{
		Application.OpenURL("https://github.com/Noha-Software");
	}

	public void OnUsernameInputChanged()
	{
		if (usernameInput.text.Length >= 3)
		{
			usernamePrompt.transform.GetChild(0).gameObject.SetActive(true);
		}
		else
		{
			usernamePrompt.transform.GetChild(0).gameObject.SetActive(false);
		}
	}

	public void SetUsername()
	{
		username = usernameInput.text;
		PhotonNetwork.NickName = username;
		usernamePrompt.SetActive(false);
	}

	public void CreateRoom()
	{
		PhotonNetwork.CreateRoom(createLobbyInput.text);
	}
	public void JoinRoom()
	{
		PhotonNetwork.JoinRoom(joinLobbyInput.text);
	}
}
