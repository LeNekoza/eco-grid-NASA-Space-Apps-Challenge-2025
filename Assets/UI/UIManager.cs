using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Interaction Prompt UI")]
    public GameObject interactionPromptPanel;
    public TextMeshProUGUI promptText;
    public Button useButton;
    public Button cancelButton;

    // This makes it easy for other scripts to access this UIManager
    public static UIManager instance;

    private void Awake()
    {
        // Singleton pattern: ensures there's only one UIManager
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (interactionPromptPanel != null)
        {
            interactionPromptPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Shows the interaction prompt with a specific message and action.
    /// </summary>
    /// <param name="message">The text to display.</param>
    /// <param name="useAction">The action to perform when 'Use' is clicked.</param>
    public void ShowInteractionPrompt(string message, UnityAction useAction)
    {
        ShowInteractionPrompt(message, useAction, HideInteractionPrompt);
    }

    public void ShowInteractionPrompt(string message, UnityAction useAction, UnityAction cancelAction)
    {
        if (promptText != null)
        {
            promptText.text = message;
        }

        // Clear any previous listeners to avoid bugs
        if (useButton != null)
        {
            useButton.onClick.RemoveAllListeners();
        }
        if (cancelButton != null)
        {
            cancelButton.onClick.RemoveAllListeners();
        }

        // Add the new actions
        if (useButton != null)
        {
            useButton.onClick.AddListener(() => {
                useAction?.Invoke();
                HideInteractionPrompt();
            });
        }
        if (cancelButton != null)
        {
            cancelButton.onClick.AddListener(() => {
                cancelAction?.Invoke();
                HideInteractionPrompt();
            });
        }

        if (interactionPromptPanel != null)
        {
            interactionPromptPanel.SetActive(true);
        }
    }

    public void HideInteractionPrompt()
    {
        interactionPromptPanel.SetActive(false);
    }
}