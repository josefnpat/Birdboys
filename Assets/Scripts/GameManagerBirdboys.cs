using Mirror.Birdboys;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Mirror.Birdboys
{

    public class GameManagerBirdboys : MonoBehaviour
    {

        public GameObject inGameMenuPanel;
        public GameObject welcomePanel;
        public GameObject scorePanel;
        public Text scoreText;

        void Update()
        {

            if (Input.GetButtonDown("Cancel"))
            {
                ToggleMenu();
            }

            if (Input.GetButton("ShowScorePanel") && !welcomePanel.activeSelf && !inGameMenuPanel.activeSelf)
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

        public void ToggleMenu()
        {
            inGameMenuPanel.SetActive(!inGameMenuPanel.activeSelf);
            Cursor.lockState = inGameMenuPanel.activeSelf ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = inGameMenuPanel.activeSelf;
        }

        public void QuitGame()
        {
            Application.Quit();
        }

    }
}