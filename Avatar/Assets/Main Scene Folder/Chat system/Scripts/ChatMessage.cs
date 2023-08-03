using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ChatMessage : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI messageText;

    public void SetText(string str)
    { messageText.text = str; }

    public void SetMessage(ulong playerID, string message)
    {
        messageText.text = $"<color=grey>Player {playerID}</color>: {message}";
    }
}
