using Mirror.Birdboys;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Mirror.Birdboys
{

    public class GameManagerBirdboys : MonoBehaviour
    {

        public GameObject scorePanel;
        public Text scoreText;
        public GameObject inGameMenuPanel;

        void Update()
        {

            if (Input.GetButton("Cancel"))
            {
                Application.Quit();
            }

            if (Input.GetButton("ShowScorePanel"))
            {
                scorePanel.SetActive(true);
                string score = "~Birdboys~\n\n";
                GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
                foreach (GameObject playerGameObject in players)
                {
                    PlayerBird player = playerGameObject.GetComponent<PlayerBird>();

                    if (player.isLocalPlayer)
                    {
                        score += "<color=green>";
                    }
                    else
                    {
                        score += "<color=white>";
                    }

                    score += player.playerName + " [";
                    score += player.kills + "/";
                    score += player.deaths + "/";
                    score += Mathf.Floor(player.assists / 3) + "]";

                    score += "</color>\n";
                }
                scoreText.text = score;
            }
            else
            {
                scorePanel.SetActive(false);
            }

        }

    }
}