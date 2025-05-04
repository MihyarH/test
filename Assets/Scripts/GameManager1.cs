
using UnityEngine;
using System.Collections;
using System.Collections.Generic; 
using UnityEngine.UI;            
using TMPro;
using Gtec.UnityInterface;                    

public class GameManager : MonoBehaviour
{
    // --- Game Settings 
    [Header("Game Settings")]
    [Tooltip("Number of columns in the grid")]
    public int gridColumns = 3;
    [Tooltip("Number of rows in the grid")]
    public int gridRows = 3;
    [Tooltip("Seconds the player has to memorize the symbols")]
    public float memorizeTime = 3.0f;
    [Tooltip("Seconds to display Correct/Wrong feedback")]
    public float feedbackTime = 1.5f;
    [Tooltip("Points awarded for a correct answer")]
    public int pointsPerCorrect = 10;

    // --- Prefabs & Scene References 
    [Header("Assign in Inspector")]
    [Tooltip("The GridCell prefab (UI Button or UI Image with GridCell script)")]
    public GameObject gridCellPrefab;
    [Tooltip("The UI Panel GameObject that has the Grid Layout Group component")]
    public Transform gridPanel;
    [Tooltip("List of Sprites to use as symbols in the grid")]
    public List<Sprite> symbolSprites;
    [Tooltip("Sprite used for the hidden state of the cells (e.g., a question mark)")]
    public Sprite hiddenSprite;
    [Tooltip("The UI Image element used to display the cue symbol")]
    public Image cueImage;
    [Tooltip("The TextMeshProUGUI element used for status messages")]
    public TextMeshProUGUI statusText;
    [Tooltip("The TextMeshProUGUI element used to display the score")] 
    public TextMeshProUGUI scoreText;

    public Sprite darkSprite;
    public Sprite flashSprite;

    // --- Game State ---
    private enum GameState { Initializing, Memorize, Recall, Feedback, GameOver }
    private GameState currentState;

    // --- Internal Logic Variables ---
    private List<GridCell> gridCells = new List<GridCell>(); // Holds references to all instantiated cell scripts
    private Dictionary<int, Sprite> cellSymbolMap = new Dictionary<int, Sprite>(); // Stores the correct symbol for each cell ID
    private Sprite currentCueSymbol = null; // The symbol the player needs to find in the current round
    private int targetCellID = -1; // The ID of the cell where the cue symbol was originally placed
    private Coroutine runningRoundCoroutine = null; // To manage stopping/starting rounds cleanly
    private int score = 0; // <<< ADDED Score variable

    private ERPFlashController2D bCIManager;

    // --- Unity Lifecycle Methods ---
    void Start()
    {
        // Validate settings first
        if (!ValidateSettings())
        {
            // Stop the game if setup is invalid
            Debug.LogError("GAME MANAGER SETUP INVALID - Halting execution. Check Inspector assignments.", this);
            this.enabled = false; // Disable this script
            return;
        }

        bCIManager = GameObject.FindAnyObjectByType<ERPFlashController2D>();

        // Start the first round
        StartNewGame();
    }

    void Update()
    {
        // Example: Allow restarting the game with a key press (optional)
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("Restarting game...");
            StartNewGame();
        }
    }

    // --- Setup and Validation ---
    bool ValidateSettings()
    {
        bool isValid = true;
        if (gridCellPrefab == null) { Debug.LogError("GameManager Error: Grid Cell Prefab not assigned!", this); isValid = false; }
        else if (gridCellPrefab.GetComponent<GridCell>() == null) { Debug.LogError($"GameManager Error: Grid Cell Prefab is missing the GridCell script!", this); isValid = false; }
        if (gridPanel == null) { Debug.LogError("GameManager Error: Grid Panel transform not assigned!", this); isValid = false; }
        else if (gridPanel.GetComponent<GridLayoutGroup>() == null) { Debug.LogError($"GameManager Error: Assigned Grid Panel is missing the Grid Layout Group component!", this); isValid = false; }
        if (symbolSprites == null || symbolSprites.Count == 0) { Debug.LogError("GameManager Error: Symbol Sprites list is empty or not assigned!", this); isValid = false; }
        if (hiddenSprite == null) { Debug.LogError("GameManager Error: Hidden Sprite not assigned!", this); isValid = false; }
        if (cueImage == null) { Debug.LogError("GameManager Error: Cue Image not assigned!", this); isValid = false; }
        if (statusText == null) { Debug.LogError("GameManager Error: Status Text not assigned!", this); isValid = false; }
        if (scoreText == null) { Debug.LogError("GameManager Error: Score Text not assigned!", this); isValid = false; } // <<< ADDED Check

        int requiredSymbols = gridColumns * gridRows;
        if (symbolSprites != null && symbolSprites.Count < requiredSymbols)
        {
            Debug.LogError($"GameManager Error: Not enough unique symbols ({symbolSprites.Count}) for the grid size ({requiredSymbols})!", this);
            isValid = false;
        }
        // Check for null sprites in the list
        if (symbolSprites != null)
        {
            for (int i = 0; i < symbolSprites.Count; i++)
            {
                if (symbolSprites[i] == null)
                {
                    Debug.LogError($"GameManager Error: Symbol Sprites list contains a null entry at index {i}!", this);
                    isValid = false;
                    break;
                }
            }
        }
        return isValid;
    }

    // --- Game Flow Control ---

    void StartNewGame()
    {
        score = 0; 
        UpdateScoreDisplay();

        // Stop any previous round coroutine to prevent overlaps
        if (runningRoundCoroutine != null)
        {
            StopCoroutine(runningRoundCoroutine);
            runningRoundCoroutine = null;
        }
        // Start the main game loop as a coroutine
        runningRoundCoroutine = StartCoroutine(RoundSequence());
    }

    // Main coroutine managing the phases of a single round
    IEnumerator RoundSequence()
    {
        currentState = GameState.Initializing;
        statusText.text = "Initializing...";
        cueImage.gameObject.SetActive(false);
        ClearGrid();

        // Phase 1: Setup Grid
        GenerateGrid();
        AssignSymbols();
        yield return null; // Wait a frame for UI to update if needed

        // Phase 2: Memorize
        currentState = GameState.Memorize;
        statusText.text = "Memorize!";
        // Ensure symbols are visible and cells are not interactable
        foreach (GridCell cell in gridCells)
        {
            cell.SetSprite(cell.currentSymbol);
            cell.SetInteractable(false);
        }
        yield return new WaitForSeconds(memorizeTime);

        // Phase 3: Hide and Prepare Recall
        FlipAllCellsToHidden();
        PrepareRecallPhase();
        yield return null;

        // Phase 4: Recall (Wait for player input via OnCellSelected)
        currentState = GameState.Recall;
        statusText.text = "Select the cell where this symbol was:";
        // Make cells interactable
        foreach (GridCell cell in gridCells)
        {
            cell.SetInteractable(true);
        }
        // The coroutine now waits until OnCellSelected is called by a cell's click event

    }


    void ClearGrid()
    {
        // Destroy previous cells efficiently
        foreach (Transform child in gridPanel)
        {
            // Unsubscribe from the event before destroying to prevent memory leaks
            GridCell cell = child.GetComponent<GridCell>();
            if (cell != null)
            {
                // Make sure listener is removed if it was added
                cell.OnCellClicked.RemoveListener(OnCellSelected);
            }
            Destroy(child.gameObject);
        }
        gridCells.Clear();
        cellSymbolMap.Clear(); 
    }

    void GenerateGrid()
    {
        int cellCount = gridColumns * gridRows;
        gridCells = new List<GridCell>(cellCount);

        float cellSize = 50f;
        float spacing = 200f;
        Vector2 startPos = new Vector2(-((gridColumns - 1) * spacing) / 2f, ((gridRows - 1) * spacing) / 2f);

        for (int row = 0; row < gridRows; row++)
        {
            for (int col = 0; col < gridColumns; col++)
            {
                int i = row * gridColumns + col;

                GameObject cellGO = Instantiate(gridCellPrefab, gridPanel);
                cellGO.name = "GridCell_" + i;

                // Make cell larger
                cellGO.transform.localScale = new Vector3(cellSize, cellSize, 1f);

                // Set position in grid
                Vector2 position = new Vector2(startPos.x + col * spacing, startPos.y - row * spacing);
                cellGO.transform.localPosition = position;

                // Create and assign BCI flash object
                ERPFlashObject2D bci_object = new ERPFlashObject2D
                {
                    ClassId = i + 1,
                    GameObject = cellGO,
                    DarkSprite = darkSprite,
                    FlashSprite = flashSprite,
                    Rotate = false
                };

                bCIManager.ApplicationObjects.Add(bci_object);

                GridCell cell = cellGO.GetComponent<GridCell>();
                if (cell != null)
                {
                    cell.cellID = i;
                    cell.OnCellClicked.AddListener(OnCellSelected);
                    gridCells.Add(cell);
                }
                else
                {
                    Debug.LogError($"GameManager: GridCell component not found on instantiated prefab instance {i}!", cellGO);
                    Destroy(cellGO);
                }
            }
        }
    }



    void AssignSymbols()
    {
        // Create a temporary list of available symbols to pick from
        List<Sprite> availableSymbols = new List<Sprite>(symbolSprites);

        // Shuffle the list randomly (Fisher-Yates shuffle)
        for (int i = 0; i < availableSymbols.Count - 1; i++)
        {
            // Pick random index from i to end
            int rndIndex = Random.Range(i, availableSymbols.Count);
            // Swap elements
            Sprite temp = availableSymbols[i];
            availableSymbols[i] = availableSymbols[rndIndex];
            availableSymbols[rndIndex] = temp;
        }

        // Assign the first N shuffled symbols to the cells
        cellSymbolMap.Clear(); 
        for (int i = 0; i < gridCells.Count; i++)
        {
  
            if (i < availableSymbols.Count)
            {
                gridCells[i].currentSymbol = availableSymbols[i]; // Store the symbol internally in the cell script
                cellSymbolMap.Add(gridCells[i].cellID, availableSymbols[i]); // Store the correct mapping (Cell ID -> Symbol)
            }
            else
            {
                // Should not happen if validation passes
                Debug.LogError($"GameManager AssignSymbols: Ran out of symbols to assign at index {i}. Grid size might be larger than symbol list.");
                // Assign hidden sprite or handle error appropriately
                gridCells[i].SetSprite(hiddenSprite);
                gridCells[i].currentSymbol = hiddenSprite;
            }
        }
    }

    void FlipAllCellsToHidden()
    {
        foreach (GridCell cell in gridCells)
        {
            cell.SetSprite(hiddenSprite);
        }
    }

    void PrepareRecallPhase()
    {

        if (cellSymbolMap.Count == 0)
        {
            Debug.LogError("GameManager: Cannot start Recall phase, no symbols were mapped!");
            statusText.text = "Error: Setup failed!";
            return; // Stop the round sequence here
        }

        // Pick a random cell ID that was assigned a symbol
        List<int> usedCellIDs = new List<int>(cellSymbolMap.Keys);
        targetCellID = usedCellIDs[Random.Range(0, usedCellIDs.Count)];

        // Get the corresponding symbol
        currentCueSymbol = cellSymbolMap[targetCellID];

        // Display the cue symbol
        cueImage.sprite = currentCueSymbol;
        cueImage.gameObject.SetActive(true); 
    }

    // --- Input Handling ---

    void OnCellSelected(int selectedCellID)
    {
        // Only process clicks during the Recall phase
        if (currentState != GameState.Recall)
        {
            // Debug.Log($"Click on cell {selectedCellID} ignored, not in Recall state.");
            return;
        }

        Debug.Log($"Player clicked Cell ID: {selectedCellID}. Target was: {targetCellID}");

        // --- Selection Processed ---
        currentState = GameState.Feedback;

        // Disable further interaction on all cells
        foreach (GridCell cell in gridCells)
        {
            cell.SetInteractable(false);
        }

        // Check if the selection is correct
        bool isCorrect = (selectedCellID == targetCellID);

        // Reveal the selected cell's actual symbol
        GridCell selectedCell = gridCells.Find(cell => cell.cellID == selectedCellID);
        if (selectedCell != null)
        {
            selectedCell.SetSprite(selectedCell.currentSymbol);
        }

        // If wrong, also reveal the correct cell's symbol
        if (!isCorrect)
        {
            GridCell correctCell = gridCells.Find(cell => cell.cellID == targetCellID);
            if (correctCell != null)
            {
                correctCell.SetSprite(correctCell.currentSymbol);
         
            }
        }

        // Start feedback phase (visual/text feedback and delay)
        StartCoroutine(FeedbackPhase(isCorrect));
    }

    // --- Feedback and Round Transition ---
    IEnumerator FeedbackPhase(bool correct)
    {
        // State is already set to Feedback in OnCellSelected
        cueImage.gameObject.SetActive(false);

        if (correct)
        {
            statusText.text = "Correct!";
            score += pointsPerCorrect; 
            UpdateScoreDisplay(); 
            Debug.Log($"Selection Correct! Score: {score}");
 
        }
        else
        {
            statusText.text = "Wrong!";
            Debug.Log("Selection Incorrect!");
            // Optional: Add visual feedback
        }

        // Wait for the specified feedback time
        yield return new WaitForSeconds(feedbackTime);

        // --- End of Round ---
    

        // For now, just start a new round
        Debug.Log("Starting next round...");
        StartNewGame(); // This will restart the RoundSequence coroutine
    }

    // --- UI Update ---

    // Updates the score text UI element
    void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score;
        }
        else
        {
            // This check prevents errors if the scoreText wasn't assigned,
      
            Debug.LogWarning("GameManager: Score Text UI element not assigned in Inspector.");
        }
    }
}