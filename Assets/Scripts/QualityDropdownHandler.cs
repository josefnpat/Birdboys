using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QualityDropdownHandler : MonoBehaviour
{
    void Start()
    {
        Dropdown dd = GetComponent<Dropdown>();
        string[] names = QualitySettings.names;
        List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
        foreach (string name in names)
        {
            options.Add(new Dropdown.OptionData(name));
        }
        dd.ClearOptions();
        dd.AddOptions(options);
        dd.SetValueWithoutNotify(QualitySettings.GetQualityLevel());
        dd.onValueChanged.AddListener(ChangeQuality);
    }

    private void ChangeQuality(int value)
    {
        QualitySettings.SetQualityLevel(value);
    }
}
