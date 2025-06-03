using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshPro))]
public class SyncText : NetworkBehaviour
{
    public readonly SyncVar<string> Text = new SyncVar<string>(new SyncTypeSettings(WritePermission.ClientUnsynchronized, ReadPermission.ExcludeOwner));
    private TextMeshPro _textMeshPro;

    private void Awake()
    {
        _textMeshPro = GetComponent<TextMeshPro>();
        Text.OnChange += OnTextChanged;
    }
    
    private void OnTextChanged(string prev, string next, bool asServer)
    {
        if (_textMeshPro != null)
        {
            _textMeshPro.text = next;
        }
    }
    
    [ServerRpc(RunLocally = true)]
    private void SetText(string newText) => Text.Value = newText;
}
