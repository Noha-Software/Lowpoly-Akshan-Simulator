using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SpawnPlayers : MonoBehaviour
{
	[SerializeField] GameObject playerPrefab;
	[SerializeField] List<GameObject> objectToSpawn = new List<GameObject>();

	[SerializeField] float minX;
	[SerializeField] float maxX;
	[SerializeField] float minY;
	[SerializeField] float maxY;

	private void Start()
	{
		Vector2 randomPosition = new Vector2(Random.Range(minX, maxX), Random.Range(minY, maxY));
		PhotonNetwork.Instantiate(playerPrefab.name, randomPosition, Quaternion.identity);
		foreach (GameObject obj in objectToSpawn)
		{
			PhotonNetwork.Instantiate(obj.name, Vector2.zero, Quaternion.identity);
		}
	}
}