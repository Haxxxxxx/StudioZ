using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LoadData : MonoBehaviour
{
    enum DataType
    {
        PlayerName,
        PlayerCoins,
        PlayerLevel
    }
    enum InputFieldType
    {
        TMP_InputField,
        TextMeshProUGUI,
        TextMeshPro
    }

    [SerializeField]
    private InputFieldType inputFieldType = InputFieldType.TMP_InputField;

    [SerializeField]
    private DataType dataType = DataType.PlayerName;
    
    private void Start()
    {
        string dataText = "Bob";
        
        switch (dataType)
        {
            case DataType.PlayerName:
                dataText = SaveManager.Instance.playerData.name;
                break;
            case DataType.PlayerCoins:
                dataText = SaveManager.Instance.playerData.coins.ToString();
                break;
            case DataType.PlayerLevel:
                dataText = SaveManager.Instance.playerData.level.ToString();
                break;
        }
        
        switch (inputFieldType)
        {
            case InputFieldType.TextMeshPro:
                GetComponent<TextMeshPro>().text = dataText;
                break;
            case InputFieldType.TextMeshProUGUI:
                GetComponent<TextMeshProUGUI>().text = dataText;
                break;
            case InputFieldType.TMP_InputField:
                GetComponent<TMP_InputField>().SetTextWithoutNotify(dataText);
                break;
        }
    }
}
