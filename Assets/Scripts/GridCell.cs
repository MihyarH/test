// GridCell.cs (for Click Input)
using UnityEngine;
using UnityEngine.UI;       // Required for UI elements like Button and Image
using UnityEngine.Events;   // Required for UnityEvent
using UnityEngine.EventSystems; // Required for IPointerClickHandler

// Attach this script to your GridCell PREFAB.
// The prefab should be a UI Button or a UI Image.
public class GridCell : MonoBehaviour, IPointerClickHandler // Implement click handler interface
{
    // --- Assign in Prefab Inspector ---
    [Header("Assign Components in Prefab")]
    [Tooltip("Assign the child Image component that displays the symbol/back")]
    public Image cellImage;
    [Tooltip("Assign the Button component IF you used a Button prefab (optional)")]
    public Button cellButton; // Assign this if your prefab is a Button

    // --- Set by GameManager ---
    // These are marked HideInInspector because the GameManager sets them directly via code.
    [HideInInspector] public int cellID = -1; // Unique ID (e.g., 0, 1, 2...) set by GameManager
    [HideInInspector] public Sprite currentSymbol = null; // The actual symbol this cell holds (set by GameManager)

    // --- Event ---
    // This event is triggered when the cell is clicked. GameManager subscribes to this.
    // It sends the cellID of the clicked cell.
    [System.Serializable]
    public class CellClickedEvent : UnityEvent<int> { }
    public CellClickedEvent OnCellClicked = new CellClickedEvent();

    // --- Initialization ---
    void Awake() // Use Awake for component checks as it runs before Start
    {
        // --- Automatically find components if not assigned (optional but good practice) ---
        if (cellImage == null)
        {
            cellImage = GetComponent<Image>();
            if (cellImage == null)
                Debug.LogError($"GridCell ({gameObject.name}): Cell Image component not found or assigned!", this);
        }
        if (cellButton == null)
        {
            // Only try to find Button if you intend to use its features.
            // If the prefab is just an Image, cellButton will remain null.
            cellButton = GetComponent<Button>();
        }

        // --- Setup Click Listener if using a Button ---
        if (cellButton != null)
        {
            // If a Button component exists, add a listener to its built-in onClick event.
            // This listener calls our HandleClick method.
            cellButton.onClick.AddListener(HandleClick);
        }
        // If no Button component, clicks will be handled by OnPointerClick via the IPointerClickHandler interface.
    }

    // --- Click Handling ---

    // Method called by the Button's onClick listener (if a Button component exists)
    void HandleClick()
    {
        // Optional: Check if the button is interactable before invoking.
        // The Button component usually handles this check itself, but explicit check is fine.
        if (cellButton != null && !cellButton.interactable) return;

        // Invoke the event, notifying any listeners (like GameManager) that this cell was clicked.
        OnCellClicked.Invoke(cellID);
        // Debug.Log("Button Click Handled for Cell ID: " + cellID);
    }

    // Method called by the EventSystem because this script implements IPointerClickHandler.
    // This works even if the GameObject is just a UI Image (as long as it has a Raycast Target).
    public void OnPointerClick(PointerEventData eventData)
    {
        // If we are using a Button component, let its HandleClick method (called via onClick listener)
        // manage the event invocation. Avoid invoking twice.
        if (cellButton != null) return;

        // If there's no Button component, invoke the event here.
        OnCellClicked.Invoke(cellID);
        // Debug.Log("IPointerClick Handled for Cell ID: " + cellID);
    }


    // --- Public Methods (Called by GameManager) ---

    // Sets the sprite displayed by the cell's Image component.
    public void SetSprite(Sprite sprite)
    {
        if (cellImage != null)
        {
            cellImage.sprite = sprite;
        }
        else
        {
            Debug.LogError($"GridCell ({gameObject.name}): Cannot set sprite, Image component missing!", this);
        }
    }

    // Enables or disables interaction for the cell.
    public void SetInteractable(bool interactable)
    {
        // If using a Button, directly set its interactable state.
        if (cellButton != null)
        {
            cellButton.interactable = interactable;
        }
        // If using just an Image, you might need additional logic here if you want visual
        // feedback for the non-interactable state (e.g., change color/alpha).
        // The click itself is implicitly ignored by GameManager if not in the Recall state,
        // but visual feedback is good UX. Example:
        // if (cellImage != null) {
        //     cellImage.color = interactable ? Color.white : new Color(0.8f, 0.8f, 0.8f, 0.7f); // Dim if not interactable
        // }
    }
}