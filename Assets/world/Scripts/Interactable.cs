using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    [Tooltip("The message that will be displayed in the UI prompt.")]
    public string promptMessage;

    [Tooltip("The function(s) to call when the 'Use' button is clicked.")]
    public UnityEvent onInteract;
}
