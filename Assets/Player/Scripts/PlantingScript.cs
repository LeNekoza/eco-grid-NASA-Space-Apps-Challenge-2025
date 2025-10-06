using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;
using System.Collections;
using TMPro;

public class PlantingScript : MonoBehaviour
{
    [Header("Tilemaps")]
    [SerializeField] private Tilemap farmLandTilemap; // Grid child named "FarmLand"
    [SerializeField] private Tilemap seedlingTilemap; // Grid child named "Seedling"

    [Header("SeedlingPalette Tiles")]
    [SerializeField] private TileBase seedTile1; // Assign from SeedlingPalette
    [SerializeField] private TileBase seedTile2; // Assign from SeedlingPalette
    [SerializeField] private TileBase seedTile3; // Assign from SeedlingPalette

    [Header("Upgraded Seedling Tiles (Water 1)")]
    [SerializeField] private TileBase upgradedSeedTile1; // Assign upgraded tile for seed 1
    [SerializeField] private TileBase upgradedSeedTile2; // Assign upgraded tile for seed 2
    [SerializeField] private TileBase upgradedSeedTile3; // Assign upgraded tile for seed 3

    [SerializeField] private TextMeshProUGUI  cropCount1; // Assign from Crop Count UI
    [SerializeField] private TextMeshProUGUI cropCount2; // Assign from Crop Count UI
    [SerializeField] private TextMeshProUGUI cropCount3; // Assign from Crop Count UI

    // Using the new Input System Keyboard for hotkeys 1/2/3

    private int inventory1;
    private int inventory2;
    private int inventory3;

    

    private void Awake()
    {
        if (farmLandTilemap == null)
        {
            var farmLand = GameObject.Find("FarmLand");
            if (farmLand != null)
            {
                farmLandTilemap = farmLand.GetComponent<Tilemap>();
                if (farmLandTilemap != null)
                {
                    Debug.Log("Planting: Found FarmLand Tilemap via GameObject.Find");
                }
            }
        }

        if (farmLandTilemap == null)
        {
            Debug.LogWarning("Planting: Missing reference to FarmLand Tilemap.");
        }

        if (seedlingTilemap == null)
        {
            var seedling = GameObject.Find("Seedling");
            if (seedling != null)
            {
                seedlingTilemap = seedling.GetComponent<Tilemap>();
                if (seedlingTilemap != null)
                {
                    Debug.Log("Planting: Found Seedling Tilemap via GameObject.Find");
                }
            }
        }

        if (seedlingTilemap == null)
        {
            Debug.LogWarning("Planting: Missing reference to Seedling Tilemap.");
        }

        // Ensure seedling tiles do not block player movement
        TryDisableTilemapColliders(seedlingTilemap);

        InitializeInventoryFromUI();
    }

    private void Start()
    {
        StartCoroutine(ApplyWeatherInventoryNextFrame());
    }

    private IEnumerator ApplyWeatherInventoryNextFrame()
    {
        // Wait one frame to allow TimerScript to initialize WeatherGameConfig with a random selection
        yield return null;

        if (!WeatherGameConfig.HasSelection)
        {
            yield break;
        }

        int target = Mathf.Max(0, WeatherGameConfig.TargetPlantCount);
        // Distribute target across three inventories as evenly as possible
        int q = target / 3;
        int r = target % 3;

        inventory1 = q + (r > 0 ? 1 : 0);
        inventory2 = q + (r > 1 ? 1 : 0);
        inventory3 = q;

        UpdateTMP(cropCount1, inventory1);
        UpdateTMP(cropCount2, inventory2);
        UpdateTMP(cropCount3, inventory3);

        Debug.Log($"Planting: Inventory set from weather target {target} -> [1:{inventory1}] [2:{inventory2}] [3:{inventory3}]");
    }

    private void Update()
    {
        if (farmLandTilemap == null || seedlingTilemap == null)
        {
            return;
        }

        var keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }

        if (keyboard.digit1Key.wasPressedThisFrame)
        {
            TryPlant(seedTile1, 1);
        }
        else if (keyboard.digit2Key.wasPressedThisFrame)
        {
            TryPlant(seedTile2, 2);
        }
        else if (keyboard.digit3Key.wasPressedThisFrame)
        {
            TryPlant(seedTile3, 3);
        }
    }

    private static void TryDisableTilemapColliders(Tilemap tilemap)
    {
        if (tilemap == null)
        {
            return;
        }
        var tilemapCollider = tilemap.GetComponent<TilemapCollider2D>();
        if (tilemapCollider != null)
        {
            tilemapCollider.enabled = false;
        }
        var compositeCollider = tilemap.GetComponent<CompositeCollider2D>();
        if (compositeCollider != null)
        {
            compositeCollider.enabled = false;
        }
    }

    private void TryPlant(TileBase seedTile, int slot)
    {
        if (seedTile == null)
        {
            Debug.LogWarning("Planting: Seed tile not assigned for this hotkey.");
            return;
        }

        if (!HasInventory(slot))
        {
            Debug.Log($"Planting: No inventory left for slot {slot}; planting blocked.");
            return;
        }

        Vector3 worldPosition = transform.position;
        Vector3Int cell = farmLandTilemap.WorldToCell(worldPosition);

        Debug.Log($"Planting: Attempt at world {worldPosition}, cell {cell}.");

        // Only plant if standing on farmland and the seedling tilemap cell is empty
        if (farmLandTilemap.GetTile(cell) == null)
        {
            Debug.Log("Planting: Not over FarmLand tile; cannot plant here.");
            return;
        }
        if (seedlingTilemap.GetTile(cell) != null)
        {
            Debug.Log("Planting: Cell already occupied on Seedling tilemap.");
            return;
        }

        seedlingTilemap.SetTile(cell, seedTile);
        Debug.Log($"Planting: Planted '{seedTile.name}' at cell {cell}.");

        DecrementInventoryAndUpdateUI(slot);
    }

    // Upgrades the planted seed under the player to its corresponding upgraded tile (Water 1)
    public void UpgradePlantedTileLevel1()
    {
        if (seedlingTilemap == null)
        {
            Debug.LogWarning("Planting: Cannot upgrade; missing Seedling tilemap.");
            return;
        }

        int upgradedCount = 0;
        var bounds = seedlingTilemap.cellBounds;
        foreach (var pos in bounds.allPositionsWithin)
        {
            TileBase current = seedlingTilemap.GetTile(pos);
            if (current == null)
            {
                continue;
            }

            Sprite currentSprite = seedlingTilemap.GetSprite(pos);

            if (MatchesSeed(current, currentSprite, seedTile1) && upgradedSeedTile1 != null)
            {
                seedlingTilemap.SetTile(pos, upgradedSeedTile1);
                upgradedCount++;
                continue;
            }
            if (MatchesSeed(current, currentSprite, seedTile2) && upgradedSeedTile2 != null)
            {
                seedlingTilemap.SetTile(pos, upgradedSeedTile2);
                upgradedCount++;
                continue;
            }
            if (MatchesSeed(current, currentSprite, seedTile3) && upgradedSeedTile3 != null)
            {
                seedlingTilemap.SetTile(pos, upgradedSeedTile3);
                upgradedCount++;
                continue;
            }
        }

        Debug.Log(upgradedCount > 0
            ? $"Planting: Upgraded {upgradedCount} planted tiles to Water 1."
            : "Planting: No matching planted tiles found to upgrade or upgraded tiles not assigned.");
    }

    private static bool MatchesSeed(TileBase currentTile, Sprite currentSprite, TileBase seedTile)
    {
        if (currentTile == seedTile)
        {
            return true;
        }
        if (currentSprite == null || seedTile == null)
        {
            return false;
        }
        var seedAsTile = seedTile as Tile;
        if (seedAsTile != null && seedAsTile.sprite == currentSprite)
        {
            return true;
        }
        return false;
    }

    // Returns true if the tile at the given world position is an upgraded seedling and gets collected (removed)
    public bool TryCollectUpgradedUnderPosition(Vector3 worldPosition)
    {
        if (seedlingTilemap == null)
        {
            return false;
        }

        Vector3Int cell = seedlingTilemap.WorldToCell(worldPosition);
        TileBase current = seedlingTilemap.GetTile(cell);
        if (current == null)
        {
            return false;
        }

        bool isUpgraded =
            (upgradedSeedTile1 != null && current == upgradedSeedTile1) ||
            (upgradedSeedTile2 != null && current == upgradedSeedTile2) ||
            (upgradedSeedTile3 != null && current == upgradedSeedTile3);

        if (!isUpgraded)
        {
            return false;
        }

        seedlingTilemap.SetTile(cell, null);
        Debug.Log($"Planting: Collected upgraded plant at cell {cell}.");
        return true;
    }

    private void InitializeInventoryFromUI()
    {
        inventory1 = ParseCountFromTMP(cropCount1);
        inventory2 = ParseCountFromTMP(cropCount2);
        inventory3 = ParseCountFromTMP(cropCount3);
        UpdateTMP(cropCount1, inventory1);
        UpdateTMP(cropCount2, inventory2);
        UpdateTMP(cropCount3, inventory3);
        Debug.Log($"Planting: Inventory initialized - [1:{inventory1}] [2:{inventory2}] [3:{inventory3}]");
    }

    private static int ParseCountFromTMP(TextMeshProUGUI label)
    {
        if (label == null)
        {
            return 0;
        }
        var text = label.text;
        if (int.TryParse(text, out var value))
        {
            return Mathf.Max(0, value);
        }
        // Fallback: extract digits only
        int acc = 0;
        bool foundDigit = false;
        for (int i = 0; i < text.Length; i++)
        {
            if (char.IsDigit(text[i]))
            {
                foundDigit = true;
                int digit = text[i] - '0';
                acc = acc * 10 + digit;
            }
            else if (foundDigit)
            {
                break;
            }
        }
        return Mathf.Max(0, acc);
    }

    private bool HasInventory(int slot)
    {
        switch (slot)
        {
            case 1: return inventory1 > 0;
            case 2: return inventory2 > 0;
            case 3: return inventory3 > 0;
            default: return false;
        }
    }

    private void DecrementInventoryAndUpdateUI(int slot)
    {
        switch (slot)
        {
            case 1:
                if (inventory1 > 0) inventory1--;
                UpdateTMP(cropCount1, inventory1);
                break;
            case 2:
                if (inventory2 > 0) inventory2--;
                UpdateTMP(cropCount2, inventory2);
                break;
            case 3:
                if (inventory3 > 0) inventory3--;
                UpdateTMP(cropCount3, inventory3);
                break;
        }
        Debug.Log($"Planting: Inventory after plant - [1:{inventory1}] [2:{inventory2}] [3:{inventory3}]");
    }

    private static void UpdateTMP(TextMeshProUGUI label, int value)
    {
        if (label != null)
        {
            label.text = value.ToString();
        }
    }
}
