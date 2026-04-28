using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

[System.Serializable]
public class SavedInventoryItem
{
    public string itemID;
    public int x;
    public int y;
    public bool isInPlayer;
    public bool isInHub;
}


public class InventoryManager : MonoBehaviour
{

    public static InventoryManager instance;

    [Header("--SerializedData--")]
    [SerializeField] private List<GameObject> itemPrefabs;
    [SerializeField] private List<SavedInventoryItem> savedItems = new List<SavedInventoryItem>();

    [Header("--SetInventoryData---")]
    [SerializeField] int playerRows;
    [SerializeField] int playerCols;
    [SerializeField] float cellSize;
    [SerializeField] Vector3 playerGridOrigin;
    [SerializeField] Vector3 hubGridOrigin;
    public InventoryGrid playerGridReal;
    public InventoryGrid hubGridReal;

    [Header("--UICardPrefabData")]
    [SerializeField] private GameObject inventoryItemUIPrefab;
    [SerializeField] private Transform playerItemLayer;
    [SerializeField] private Transform hubItemLayer;
    [SerializeField] private Transform carItemLayer;

    [SerializeField] public float playerGridSlotWidth;
    [SerializeField] public float playerGridSlotHeight;

    [Header("--UIObjectReferences--")]
    [SerializeField] public GameObject UIMainRoot;
    [SerializeField] public GameObject playerInventoryUIObject;
    [SerializeField] public GameObject hubInventoryUIObject;
    [SerializeField] public GameObject hubPlayerInvUIObject;
    [SerializeField] public GameObject carInventoryUIObject;
    [SerializeField] public GameObject carPlayerInvUIObject;
    [SerializeField] public GameObject primaryWeaponIcon;
    [SerializeField] public GameObject secondaryWeaponIcon;
    [SerializeField] public GameObject playerGridReference;
    [SerializeField] public GameObject hubGridReference;
    [SerializeField] public Sprite emptyWeaponIcon;


    [Header("--InventoryScriptReferences")]
    [SerializeField] public PlayerInventory plrInventoryReference;
    [SerializeField] public HubInventory hubInventoryReference;
    [SerializeField] public CarInventory carInventoryReference;
    [SerializeField] public EquipmentComponent equip;

    [Header("--InventoryAccessObjects--")]
    [SerializeField] GameObject hubInventoryChest;
    [SerializeField] GameObject carObjectChest;



    private Vector2 originalAnchorMin;
    private Vector2 originalAnchorMax;
    private Vector2 originalPivot;
    private Vector2 originalAnchoredPosition;
    private Vector3 originalScale;
    private Transform originalParent;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            LoadItemPrefabs();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }


    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }


    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //Debug.Log("Scene loaded: " + scene.name);

        InitializeManager();
        LoadInventoryState();
    }

    public void InitializeManager()
    {
        if (gameManager.instance.User != null)
        {
            plrInventoryReference = gameObject.GetComponent<PlayerInventory>();
            hubInventoryReference = gameObject.GetComponent<HubInventory>();
            carInventoryReference = gameObject.GetComponent<CarInventory>();

            UIMainRoot = GameObject.Find("UI");

            /*

            playerInventoryUIObject = GameObject.Find("InventoryRoot");
            hubInventoryUIObject = GameObject.Find("HubInventoryRoot");
            primaryWeaponIcon = GameObject.Find("PrimaryIcon");
            secondaryWeaponIcon = GameObject.Find("SecondaryIcon");
            playerGridReference = GameObject.Find("PlayerInvGrid");
            hubGridReference = GameObject.Find("HubInventoryGrid");



            playerItemLayer = GameObject.Find("ItemOverlay").transform;
            hubItemLayer = GameObject.Find("HubItemOverlay").transform;
            */

            AssignSceneUIReferences();

            InitializeAllGrids();


            if (playerInventoryUIObject != null && hubInventoryUIObject != null)
            {
                if (playerInventoryUIObject.activeSelf == true)
                {
                    playerInventoryUIObject.SetActive(false);
                }

                if (hubInventoryUIObject.activeSelf == true)
                {
                    hubInventoryUIObject.SetActive(false);
                }

                plrInventoryReference.inventoryGrid = playerGridReference;


                InitializePlayerInventoryRect();
            }

            equip = gameObject.GetComponent<EquipmentComponent>();
            equip.inventory = plrInventoryReference;
            if (equip != null)
            {
                AssignDefaultSprites(equip);
                AssignSpritesFromEquipment(equip);
            }
        }
    }

    private GameObject FindSceneObject(string objectName)
    {
        Transform[] allTransforms = FindObjectsByType<Transform>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        foreach (Transform t in allTransforms)
        {
            if (t.name == objectName && t.gameObject.scene.IsValid())
            {
                return t.gameObject;
            }
        }

        return null;
    }

    private void AssignSceneUIReferences()
    {
        playerInventoryUIObject = FindSceneObject("InventoryRoot");
        hubInventoryUIObject = FindSceneObject("HubInventoryRoot");
        carInventoryUIObject = FindSceneObject("CarInventoryRoot");

        primaryWeaponIcon = FindSceneObject("PrimaryIcon");
        secondaryWeaponIcon = FindSceneObject("SecondaryIcon");

        playerGridReference = FindSceneObject("PlayerInvGrid");
        hubGridReference = FindSceneObject("HubInventoryGrid");

        GameObject itemOverlayObj = FindSceneObject("ItemOverlay");
        if (itemOverlayObj != null)
            playerItemLayer = itemOverlayObj.transform;

        GameObject hubOverlayObj = FindSceneObject("HubItemOverlay");
        if (hubOverlayObj != null)
            hubItemLayer = hubOverlayObj.transform;
    }

    private void InitializePlayerInventoryRect()
    {
        RectTransform rect = playerGridReference.GetComponent<RectTransform>();

        originalParent = playerGridReference.transform.parent;
        originalAnchorMin = rect.anchorMin;
        originalAnchorMax = rect.anchorMax;
        originalPivot = rect.pivot;
        originalAnchoredPosition = rect.anchoredPosition;
        originalScale = playerGridReference.transform.localScale;
    }

    public void CreatePlayerItemUI(GameObject itemObject)
    {
        if (itemObject == null)
        {
            return;
        }

        if (inventoryItemUIPrefab == null)
        {
            return;
        }

        if (playerItemLayer == null)
        {
            return;
        }

        InGameItem wi = itemObject.GetComponent<InGameItem>();
        if (wi == null)
        {
            return;
        }

        GameObject uiItem = Instantiate(inventoryItemUIPrefab, playerItemLayer);
        GridControllerUI itemUI = uiItem.GetComponent<GridControllerUI>();

        if (itemUI == null)
        {
            return;
        }

        itemUI.Setup(wi, playerItemLayer, playerGridSlotWidth, playerGridSlotHeight);
    }


    public void PopulateHubPlayerInventoryUI()
    {
        RectTransform rect = playerGridReference.GetComponent<RectTransform>();

        playerGridReference.transform.SetParent(hubInventoryUIObject.transform, false);
        playerGridReference.transform.localScale = Vector3.one;

        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
    }

    public void PopulatePlayerInvOriginUI()
    {
        RectTransform rect = playerGridReference.GetComponent<RectTransform>();

        playerGridReference.transform.SetParent(originalParent, false);
        playerGridReference.transform.localScale = originalScale;

        rect.anchorMin = originalAnchorMin;
        rect.anchorMax = originalAnchorMax;
        rect.pivot = originalPivot;
        rect.anchoredPosition = originalAnchoredPosition;
    }

    public void InitializeAllGrids()
    {
        playerGridOrigin = playerInventoryUIObject.GetComponent<Image>().transform.position;
        hubGridOrigin = hubInventoryUIObject.GetComponent<Image>().transform.position;

        CreateRealPlayerGrid();
        CreateRealHubGrid();
    }

    public void CreateRealPlayerGrid()
    {
        playerGridReal = new InventoryGrid(playerCols, playerRows, cellSize, playerGridOrigin);
    }

    public void CreateRealHubGrid()
    {
        hubGridReal = new InventoryGrid(playerCols, playerRows, cellSize, hubGridOrigin);
    }

    public void SaveInventoryState()
    {
        savedItems.Clear();

        foreach (GameObject item in plrInventoryReference.playerInventory)
        {
            SaveOneItem(item, true, false);
        }

        foreach (GameObject item in hubInventoryReference.hubInventory)
        {
            SaveOneItem(item, false, true);
        }
    }

    private void SaveOneItem(GameObject item, bool isInPlayer, bool isInHub)
    {
        if (item == null) return;

        InGameItem wi = item.GetComponent<InGameItem>();
        if (wi == null || wi.itemInstance == null || wi.itemInstance.definition == null) return;

        savedItems.Add(new SavedInventoryItem
        {
            itemID = wi.itemInstance.definition.itemID,
            x = wi.itemInstance.x,
            y = wi.itemInstance.y,
            isInPlayer = isInPlayer,
            isInHub = isInHub
        });
    }

    public void HandleInventoryInput()
    {
        if (gameManager.instance.menuActive == null)
        {
            gameManager.instance.OpenMenu(MenuType.PlayerInventory);

        }
        else if (gameManager.instance.menuActive == instance.playerInventoryUIObject || gameManager.instance.menuActive == instance.hubInventoryUIObject)
        {
            gameManager.instance.StateUnpause();
        }
    }

    public void LoadInventoryState()
    {
        if (plrInventoryReference == null || hubInventoryReference == null)
        {
            return;
        }

        if (playerItemLayer == null)
        {
            return;
        }


        plrInventoryReference.playerInventory.Clear();
        hubInventoryReference.hubInventory.Clear();

        CreateRealPlayerGrid();
        CreateRealHubGrid();

        foreach (SavedInventoryItem saved in savedItems)
        {
            GameObject prefab = GetItemPrefabByID(saved.itemID);
            if (prefab == null) continue;

            GameObject item = Instantiate(prefab);
            DontDestroyOnLoad(item);
            item.SetActive(false);

            if (saved.isInPlayer)
            {
                playerGridReal.PlaceWeaponAt(item, saved.x, saved.y);
                plrInventoryReference.playerInventory.Add(item);
                CreatePlayerItemUI(item);
            }
            else if (saved.isInHub)
            {
                hubGridReal.PlaceWeaponAt(item, saved.x, saved.y);
                hubInventoryReference.hubInventory.Add(item);
                // create hub UI later if needed
            }
        }
    }

    private void LoadItemPrefabs()
    {
        if (itemPrefabs == null)
        {
            itemPrefabs = new List<GameObject>();
        }

        GameObject[] loadedPrefabs = Resources.LoadAll<GameObject>("InventoryItems");

        foreach (GameObject prefab in loadedPrefabs)
        {
            if (prefab.GetComponent<InGameItem>() != null)
            {
                itemPrefabs.Add(prefab);
            }
        }

        //Debug.Log("Loaded inventory prefabs: " + itemPrefabs.Count);
    }

    private GameObject GetItemPrefabByID(string itemID)
    {
        foreach (GameObject prefab in itemPrefabs)
        {
            InGameItem wi = prefab.GetComponent<InGameItem>();
            if (wi == null) continue;

            ItemDefinition def = wi.GetItemDefinition();
            if (def != null && def.itemID == itemID)
                return prefab;
        }

        return null;
    }

    public void AssignDefaultSprites(EquipmentComponent equip)
    {
        if (equip.GetPrimaryIcon() == null)
        {
            primaryWeaponIcon.GetComponent<Image>().sprite = emptyWeaponIcon;
        }
        if (equip.GetSecondaryIcon() == null)
        {
            secondaryWeaponIcon.GetComponent<Image>().sprite = emptyWeaponIcon;
        }
    }


    public void AssignSpritesFromEquipment(EquipmentComponent equip)
    {
        if (equip.GetPrimaryIcon() != null && equip.GetSecondaryIcon() != null)
        {
            primaryWeaponIcon.GetComponent<Image>().sprite = equip.GetPrimaryIcon();
            secondaryWeaponIcon.GetComponent<Image>().sprite = equip.GetSecondaryIcon();
        }
    }

    public void ChangePrimarySprite(InGameItem item)
    {
        primaryWeaponIcon.GetComponent<Image>().sprite = item.itemInstance.icon;
    }

    public void ChangeSecondarySprite(InGameItem item)
    {
        secondaryWeaponIcon.GetComponent<Image>().sprite = item.itemInstance.icon;
    }


    public GameObject GetHubInventoryMenu()
    {
        return hubInventoryUIObject;
    }


    public GameObject GetPlayerGrid()
    {
        return playerGridReference;
    }

    public Transform GetPlayerItemLayer()
    {
        return playerItemLayer;
    }

    public float GetPlayerGridSlotWidth()
    {
        return playerGridSlotWidth;
    }

    public float GetPlayerGridSlotHeight()
    {
        return playerGridSlotHeight;
    }

    public GameObject GetHubGrid() => hubGridReference;

    public Transform GetHubItemLayer() => hubItemLayer;
    public Transform GetCarItemLayer() => carItemLayer;
}
