using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MasterVolumeHandler : MonoBehaviour
{
    void Start()
    {
        Slider mv = GetComponent<Slider>();
        mv.value = AudioListener.volume;
        mv.onValueChanged.AddListener(ChangeVolume);
    }

    private void ChangeVolume(float value)
    {
        AudioListener.volume = value;
    }
}
