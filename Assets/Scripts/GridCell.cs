
using UnityEngine;
using UnityEngine.UI;       
using UnityEngine.Events;   
using UnityEngine.EventSystems; 


public class GridCell : MonoBehaviour, IPointerClickHandler 
{
    [Header("Assign Components in Prefab")]
    [Tooltip("Assign the child Image component that displays the symbol/back")]
    public SpriteRenderer cellImage;
    [Tooltip("Assign the Button component IF you used a Button prefab (optional)")]
    public Button cellButton; 


    [HideInInspector] public int cellID = -1; 
    [HideInInspector] public Sprite currentSymbol = null; 

    [System.Serializable]
    public class CellClickedEvent : UnityEvent<int> { }
    public CellClickedEvent OnCellClicked = new CellClickedEvent();

    // --- Initialization ---
    void Awake() 
    {
        if (cellImage == null)
        {
            cellImage = GetComponent<SpriteRenderer>();
            if (cellImage == null)
                Debug.LogError($"GridCell ({gameObject.name}): Cell SpriteRenderer component not found or assigned!", this);
        }
        if (cellButton == null)
        {
       
            cellButton = GetComponent<Button>();
        }

        if (cellButton != null)
        {
            cellButton.onClick.AddListener(HandleClick);
        }
       
    }

    void HandleClick()
    {
        if (cellButton != null && !cellButton.interactable) return;

        OnCellClicked.Invoke(cellID);
    }
    public void OnPointerClick(PointerEventData eventData)
    {
 
        if (cellButton != null) return;
        OnCellClicked.Invoke(cellID);

    }



    public void SetSprite(Sprite sprite)
    {
        if (cellImage != null)
        {
            cellImage.sprite = sprite;
        }
        else
        {
            Debug.LogError($"GridCell ({gameObject.name}): Cannot set sprite, SpriteRenderer component missing!", this);
        }
    }


    public void SetInteractable(bool interactable)
    {

        if (cellButton != null)
        {
            cellButton.interactable = interactable;
        }
       
    }
}