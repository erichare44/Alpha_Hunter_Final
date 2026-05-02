using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using TMPro;
using System.Runtime.CompilerServices;
public enum MenuType
{
    Pause,
    Win,
    Lose,
    PlayerInventory,
    HubInventory,
    CarInventory,
    BountyBoard,
    XPUI,
    MainMenu
}

public class gameManager : MonoBehaviour
{
    public static gameManager instance;
    public Transform player;
    public GameObject User;
    public ThirdPersonMotor playerScript;
    public XPSystem shopSystem;
    public GameObject BountyBoardUI;
    public GameObject XPUI;
    public GameObject SpawnPos;
    public PlayerHealth healthScript;
    public GameObject BarHealth;
    public HealthBar healthBar;
    public GameObject AlphaWinScreen;
    public GameObject DeathScreen;

    [Header("Contract Setup")]
    public List<monsterGroup> monsterPool;
    public List<string> poiPool = new List<string> { "Cabin", "FarmHouse" };
    

    [Header("Current Active Contract")]
    public monsterGroup selectedMonster;
    public string selectedPOI;


    [Header("UI Elements and Menus")]
    [SerializeField] public GameObject menuActive;
    [SerializeField] GameObject menuPause;
    [SerializeField] GameObject interactPrompt;
    public GameObject UIMoneyObject;
    TMP_Text moneyCounter;
    public GameObject UIObjective;
    TMP_Text currentObjective;

    [Header("Moon State")]
    private bool isBloodMoon;


    // private values needed for menus
    float timeScaleOriginal;
    public bool isPaused => menuActive != null;
    private MenuType activeMenuType;
    public bool canBuy;
    bool needsWin = false;
    bool needsDeath = false;
    public int talkedNPCCount;

    [SerializeField] GameObject hubInventoryChest;
    [SerializeField] GameObject carObject;


    private void Awake()
    {
     
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            timeScaleOriginal = Time.timeScale;

            //roll for blood moon
            isBloodMoon = Random.value <= 0.25f;

            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else 
        {
            Destroy(gameObject);
            return;
        }          
       
    }
    private void Start()
    {
        RefreshReferences();   
        SpawnMoonSystem();
        //uncomment code below to test single contract in POI
       //if(selectedMonster == null || selectedPOI == null) GenerateContract();
        
    }
    

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {

        RefreshReferences();
        SpawnMoonSystem();

        
        if (playerScript != null && SpawnPos != null)
        {
            playerScript.SpawnPlayer();
        }
    }
    

    public void RefreshReferences()
    {
        if (menuPause != null) menuPause.SetActive(false);
        if (XPUI != null) XPUI.SetActive(false);
        if (BountyBoardUI != null) BountyBoardUI.SetActive(false);
        if (AlphaWinScreen != null) AlphaWinScreen.SetActive(false);
        if (DeathScreen != null) DeathScreen.SetActive(false);
        //player setup
        User = GameObject.FindWithTag("Player");
            if (User != null)
        {
            playerScript = User.GetComponent<ThirdPersonMotor>();
            shopSystem = User.GetComponent<XPSystem>();
            healthScript = User.GetComponent<PlayerHealth>();

            if (InventoryManager.instance != null)
            {
                InventoryManager.instance.InitializeManager();                
            }
        }
        //find scene specific objects
        hubInventoryChest = GameObject.FindWithTag("HubChest");
        SpawnPos = GameObject.FindWithTag("PlayerSpawnPoint");
        //menuPause = GameObject.Find("PauseMenuRoot");
        

        //health bar logic
        BarHealth = GameObject.FindWithTag("HealthBar");
        if(BarHealth != null)
        {
            healthBar = BarHealth.GetComponent<HealthBar>();
            if(healthBar != null && healthScript != null)
            {
                healthBar.UpdateHealthBar(healthScript.currentHealth, healthScript.maxHealth);
            }
        }

        //UI Elements
        UIMoneyObject = GameObject.FindWithTag("MoneyCounter");
        if(UIMoneyObject != null)
        {
            moneyCounter = UIMoneyObject.GetComponentInChildren<TMP_Text>();
        }

        UIObjective = GameObject.FindWithTag("Objective");
        if(UIObjective != null)
        {
            currentObjective = UIObjective.GetComponentInChildren<TMP_Text>();
        }
        needsDeath = false;
        needsWin = false;
        //scene UI stuff
        //XPUI = GameObject.FindWithTag("XPUI");
        //BountyBoardUI = GameObject.FindWithTag("Bounty Board");
        //AlphaWinScreen = GameObject.FindWithTag("XPCounter");
        //DeathScreen = GameObject.FindWithTag("LoseMenu");

        //safe deactivation
        

        
    }
    private void SpawnMoonSystem()
    {
        //choose prefab used
        string prefab = isBloodMoon ? "Blood_Moon" : "Regular_Moon";

        //load prefab
        GameObject moonPrefab = Resources.Load<GameObject>(prefab);

        if(moonPrefab != null )
        {
            Instantiate(moonPrefab);
            //Debug.LogError($"<color=green> Moon Spawned");
        }
        else
        {
            //Debug.LogError($"<color=red> Moon Spawn Failed");
        }

    }
    
    private void Update()
    {
        PlayerMenuControls();
        if (needsWin) { OpenWinExtraction(); }
        if (needsDeath) { OpenDeathScreen(); }
        if (moneyCounter != null && shopSystem != null)
        { moneyCounter.text = "Current Money: " + shopSystem.currentMoney.ToString(); }
        if (selectedPOI == "")
        {
            currentObjective.text = "Select a Bounty";
        }
        if (selectedPOI != "" && talkedNPCCount <= 5)
        {
            currentObjective.text = "Investigate the town";
        }
        else if (selectedPOI != "" && talkedNPCCount >= 5)
        {
            currentObjective.text = "Alpha Spotted at " + selectedPOI;
        }
        Scene temp = SceneManager.GetActiveScene();
        if (temp.name == "Cabin" || temp.name == "FarmHouse")
        {
            currentObjective.text = "Kill the Alpha and Extract";
        }
    }

    private void PlayerMenuControls()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            HandlePauseInput();
        }

        if (Input.GetButtonDown("OpenInventory"))
        {
            InventoryManager.instance.HandleInventoryInput();
        }
    }

    private void HandlePauseInput()
    {
        if (menuActive == null)
        {
            OpenMenu(MenuType.Pause);
        }
        else if (menuActive == menuPause)
        {
            StateUnpause();
        }
    }


    public void StatePause()
    {
        
        Time.timeScale = 0;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }


    public void StateUnpause()
    {
       
        Time.timeScale = timeScaleOriginal;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (menuActive != null)
        {
            menuActive.SetActive(false);
            menuActive = null;
        }
    }

    void GenerateContract()
    {
        //Master roll
        selectedMonster = monsterPool[Random.Range(0, monsterPool.Count)];
        selectedPOI = poiPool[Random.Range(0, poiPool.Count)];
    }

    public void ExitMenuState()
    {
        StateUnpause();
    }

    public void OpenMenu(MenuType type)
    {
        switch (type)
        {
            case MenuType.PlayerInventory:
                gameManager.instance.StatePause();
                gameManager.instance.menuActive = InventoryManager.instance.playerInventoryUIObject;
                InventoryManager.instance.PopulatePlayerInvOriginUI();
                gameManager.instance.menuActive.SetActive(true);
                break;
            case MenuType.Pause:
                gameManager.instance.StatePause();
                gameManager.instance.menuActive = menuPause;
                gameManager.instance.menuActive.SetActive(true);
                break;
            case MenuType.HubInventory:
                gameManager.instance.StatePause();
                gameManager.instance.menuActive = InventoryManager.instance.hubInventoryUIObject;
                InventoryManager.instance.PopulateHubPlayerInventoryUI();
                gameManager.instance.menuActive.SetActive(true);
                break;
        }
    }

    public void OpenHubChest()
    { 
        InventoryManager.instance.hubInventoryReference.isOpen = true;
        if (menuActive == null)
        { 
            OpenMenu(MenuType.HubInventory);
        }
        else if (menuActive == InventoryManager.instance.hubInventoryUIObject)
        {
            StateUnpause();
        }
    }

    public void OpenBountyBoard()
    {
        StatePause();
        menuActive = BountyBoardUI;
        BountyBoardUI.SetActive(true);
        activeMenuType = MenuType.BountyBoard;
    }

    public void CloseBountyBoard()
    {
        GenerateContract();
        BountyBoardUI.SetActive(false);
        ExitMenuState();
    }

    public void OpenXPUI()
    {
        StatePause();
        menuActive = XPUI;
        XPUI.SetActive(true);
    }

    public void CloseXPUI()
    {
        XPUI.SetActive(false);
        ExitMenuState();
    }
    public void loadLevel(string lvl)
    {
        //StateUnpause();

        SceneManager.LoadScene(lvl);
    }
    public void ExitHUB()
    {
        loadLevel("Main Town");
        
    }
    
    public void Death()
    {
        if (healthScript != null && healthBar != null)
        {
            healthScript.currentHealth = healthScript.maxHealth;
            healthBar.UpdateHealthBar(healthScript.maxHealth, healthScript.currentHealth);
        }

        SceneManager.LoadScene("HUB Area");
        needsDeath = true;
    }

    public void Extraction()
    {
        //clear contract data
        selectedMonster = null;
        selectedPOI = "";
        if(healthScript != null && healthBar != null)
        {
            healthScript.currentHealth = healthScript.maxHealth;
            healthBar.UpdateHealthBar(healthScript.maxHealth, healthScript.currentHealth);
        }

        SceneManager.LoadScene("HUB Area");
        shopSystem.hasExtracted = true;
        needsWin = true;
    }
    public bool TestBuy(int cost)
    {
        if (shopSystem.currentMoney >= cost)
        {
            return true;
        }
        else { return false; }
    }

    void OpenWinExtraction()
    {
        needsWin = false;
        StatePause();
        activeMenuType = MenuType.Win;
        menuActive = AlphaWinScreen;
        if(AlphaWinScreen != null) AlphaWinScreen.SetActive(true);
    }

    public void CloseAlphaKilledWinScreen()
    {
        needsWin = false;
        shopSystem.CountingXP();
        AlphaWinScreen.SetActive(false);
        StateUnpause();
    }

    public void Buying(int cost)
    {
        shopSystem.currentMoney -= cost;
    }



    public GameObject GetMenuActive()
    {
        return menuActive;
    }

    public void OpenDeathScreen()
    {
        needsDeath = false;

        StatePause();
        activeMenuType = MenuType.Lose;
        menuActive = DeathScreen;
        if(DeathScreen != null) DeathScreen.SetActive(true);
    }

    public void CloseDeathScreen()
    {
        needsDeath = false;
        shopSystem.CountingXP();
        StateUnpause();
    }

    public void BackToHUB()
    {
        loadLevel("HUB Area");
    }
}