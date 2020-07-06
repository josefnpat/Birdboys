using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MouseSensitivityHandler : MonoBehaviour
{

    public GameObject gameManager;
    private GameSettings gs;
    void Start()
    {
        gs = gameManager.GetComponent<GameSettings>();

        Slider ms = GetComponent<Slider>();
        ms.value = gs.mouseSensitivity;
        ms.onValueChanged.AddListener(ChangeSensitivity);
    }

    private void ChangeSensitivity(float value)
    {
        gs.mouseSensitivity = value;
    }
}
