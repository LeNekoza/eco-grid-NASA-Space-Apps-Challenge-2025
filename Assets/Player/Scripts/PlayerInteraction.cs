using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;


public class PlayerInteraction : MonoBehaviour
{
    // A reference to the currently available interactable object
    private Interactable currentInteractable;
    private PlantingScript plantingScript;

    private void Awake()
    {
        plantingScript = GetComponent<PlantingScript>();
    }

    void Update()
    {
        // Check if the player presses the "E" key and if there is an interactable object nearby
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame && currentInteractable != null)
        {
            // Build the appropriate Use action
            UnityAction useAction;
            if (currentInteractable.name == "Water 1" && plantingScript != null)
            {
                useAction = plantingScript.UpgradePlantedTileLevel1;
            }
            else
            {
                useAction = currentInteractable.onInteract.Invoke;
            }

            UIManager.instance.ShowInteractionPrompt(currentInteractable.promptMessage, useAction);
            Debug.Log("Interacted with " + currentInteractable.name);
        }
    }

    // Called by Unity when the player's collider enters a trigger collider
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the object we collided with has an Interactable component
        if (other.GetComponent<Interactable>() != null)
        {
            currentInteractable = other.GetComponent<Interactable>();
        }
    }

    // Called by Unity when the player's collider exits a trigger collider
    private void OnTriggerExit2D(Collider2D other)
    {
        // If we are exiting the trigger of our current interactable, clear it
        if (other.GetComponent<Interactable>() == currentInteractable)
        {
            currentInteractable = null;
        }
    }
}