using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class buttonFunctions : MonoBehaviour
{
    public TextMeshProUGUI BountyAmount1;
    public TextMeshProUGUI BountyAmount2;
    public TextMeshProUGUI SkillPointAmount;
    public TextMeshProUGUI healthXPTxt;
    public TextMeshProUGUI sprintXPTxt;
    public TextMeshProUGUI XPExtTxt;
    public TextMeshProUGUI XPDeadTxt;
    public TextMeshProUGUI hasExtracted;
    public TextMeshProUGUI XpEarned;
    public TextMeshProUGUI currentXPMultiplier;
    public TextMeshProUGUI newXPTotal;
    public TextMeshProUGUI moneyEarned;
    public TextMeshProUGUI TotalCash;
    public TextMeshProUGUI DeathXPEarned;
    public TextMeshProUGUI deathXPTotal;

    int amountXPModExtUpgraded;
    int amountXPModDeadUpgraded;
    int amountSprintUpgraded;
    int amountHPUpgraded;
    int HPResetAmount;
    int SprintResetAmount;
    int possible1;
    int possible2;

    public bool isSprintLocked;
    public bool isHPLocked;


    int GenerateRandomCashAmount()
    {
        int ranAmount = Random.Range(100, 250);
        return ranAmount;
    }

    private void Start()
    {
        possible1 = GenerateRandomCashAmount();
        possible2 = GenerateRandomCashAmount();
    }

    void Update()
    {
        BountyAmount1.text = "Bounty Amount: $" + possible1.ToString();
        BountyAmount2.text = "Bounty Amount: $" +possible2.ToString();
        SkillPointAmount.text = "Skill Points Available:\t\t" + gameManager.instance.shopSystem.skillPointAmount.ToString();
        healthXPTxt.text = "Upgrade Base Health Amount " + ((amountHPUpgraded) * (1 + HPResetAmount)).ToString() + " / 25";
        sprintXPTxt.text = "Upgrade Sprint Speed " + ((amountSprintUpgraded) * (1 + SprintResetAmount)).ToString() + " / 25";
        XPExtTxt.text = "Upgrade Bonus XP earned on Extraction " + amountXPModExtUpgraded.ToString() + " / 5";
        XPDeadTxt.text = "Upgrade amount of XP saved upon death " + amountXPModDeadUpgraded.ToString() + " / 3";
        hasExtracted.text = "Player Extracted:\t\t\t" + gameManager.instance.shopSystem.hasExtracted.ToString();
        XpEarned.text = "XP Earned on Mission:\t\t\t" + gameManager.instance.shopSystem.XPPotential.ToString();
        if (gameManager.instance.shopSystem.hasExtracted && gameManager.instance.shopSystem.alphaKilled)
        {
            currentXPMultiplier.text = "XP Multiplier:\t\t\t" + gameManager.instance.shopSystem.XPModifierExtract.ToString();
            newXPTotal.text = "New Total XP:\t\t\t" + ((gameManager.instance.shopSystem.XPPotential * gameManager.instance.shopSystem.XPModifierExtract)
            + gameManager.instance.shopSystem.XPHas).ToString();
        }
        else
        {
            currentXPMultiplier.text = "Bonus XP Multiplier:\t\t1";
            newXPTotal.text = "New Total XP:\t\t" + (gameManager.instance.shopSystem.XPPotential + gameManager.instance.shopSystem.XPHas).ToString();
        }
        moneyEarned.text = "Money Earned from Contract:\t\t" + gameManager.instance.shopSystem.possibleMoney.ToString();
        TotalCash.text = "Total Money:\t\t\t" + (gameManager.instance.shopSystem.possibleMoney + gameManager.instance.shopSystem.currentMoney).ToString();
        DeathXPEarned.text = "XP Earned:\t\t\t" + gameManager.instance.shopSystem.XPPotential.ToString();
        deathXPTotal.text = "Total XP:\t\t\t" + (gameManager.instance.shopSystem.XPPotential + gameManager.instance.shopSystem.XPHas).ToString();
    }

    public void XPShopXPModExtraction(int cost)
    {
        if(amountXPModExtUpgraded <= 5 && gameManager.instance.shopSystem.skillPointAmount >= cost)
        {
            amountXPModExtUpgraded++;
            gameManager.instance.shopSystem.skillPointAmount -= cost;
            gameManager.instance.shopSystem.UpdatingExtractionXPModifier();
        }
    }

    public void XPShopXPModDeath(int cost)
    {
        if(amountXPModDeadUpgraded <= 3 && gameManager.instance.shopSystem.skillPointAmount >= cost)
        {
            amountXPModDeadUpgraded++;
            gameManager.instance.shopSystem.skillPointAmount -= cost;
            gameManager.instance.shopSystem.UpdatingDeathXPModifier();
        }
    }

    public void XPShopSprint(int cost)
    {
        if(amountSprintUpgraded <= 5 && gameManager.instance.shopSystem.skillPointAmount >= cost)
        {
            amountSprintUpgraded++;
            gameManager.instance.shopSystem.skillPointAmount -= cost;
            gameManager.instance.shopSystem.SprintUpgrade();
        }
        if(amountSprintUpgraded == 5 && gameManager.instance.shopSystem.currentLevel >= ((SprintResetAmount + 1) * amountSprintUpgraded)
            && SprintResetAmount <= 5)
        {
            SprintResetAmount++;
            amountSprintUpgraded = 0;
        }
        else
        {
            isSprintLocked = true;
        }
    }

    public void XPShopHP(int cost)
    {
        if(amountHPUpgraded <= 5 && gameManager.instance.shopSystem.skillPointAmount >= cost)
        {
            amountHPUpgraded++;
            gameManager.instance.shopSystem.skillPointAmount -= cost;
            gameManager.instance.shopSystem.HealthUpgrade();
        }
        if(amountHPUpgraded == 5 && gameManager.instance.shopSystem.currentLevel >= ((HPResetAmount + 1) * amountHPUpgraded)
            && HPResetAmount <= 5)
        {
            HPResetAmount++;
            amountHPUpgraded = 0;
        }
        else
        {
            isHPLocked = true;
        }
    }

    public void BuyShop(int cost)
    {
        if(gameManager.instance.TestBuy(cost))
        {
            gameManager.instance.Buying(cost);
        }
    }

    public void resume()
    {
        gameManager.instance.StateUnpause();
    }

    /*
    public void restart()
    {

        if (SceneManager.GetActiveScene().name == "HUB Area")
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);

            gameManager.instance.RefreshReferences();
            gameManager.instance.StateUnpause();
        }
        else if (SceneManager.GetActiveScene().name != "Hub Area")
        {
            SceneManager.LoadScene(1);

            gameManager.instance.RefreshReferences();
            gameManager.instance.StateUnpause();
        }

    }
    */

    public void quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }


    

    public void OpenPlayerInventory()
    {
        gameManager.instance.OpenMenu(MenuType.PlayerInventory);
    }

    public void OpenWorldInventory()
    {
        gameManager.instance.OpenMenu(MenuType.HubInventory);
    }

    public void OpenPauseMenu()
    {
        gameManager.instance.OpenMenu(MenuType.Pause);
    }

    public void OpenWinMenu()
    {
        gameManager.instance.OpenMenu(MenuType.Win);
    }

    public void OpenLoseMenu()
    {
        gameManager.instance.OpenMenu(MenuType.Lose);
    }

    public void CloseCurrentMenu()
    {
        gameManager.instance.ExitMenuState();
    }

    
    public void OpenBountyBoard()
    {
        gameManager.instance.OpenBountyBoard();
    }

    public void CloseBountyBoard1()
    {
        gameManager.instance.CloseBountyBoard();
        gameManager.instance.shopSystem.SetPossibleMoney(possible1);
    }

    public void CloseBountyBoard2()
    {
        gameManager.instance.CloseBountyBoard();
        gameManager.instance.shopSystem.SetPossibleMoney(possible2);
    }

    public void CloseXPCountingUIExtraction()
    {
        gameManager.instance.CloseAlphaKilledWinScreen();
    }

    public void OpenXPUI()
    {
        gameManager.instance.OpenXPUI();
    }

    public void CloseXPUI()
    {
        gameManager.instance.CloseXPUI();
    }

    public void ExitHUB()
    {
        gameManager.instance.ExitHUB();
    }

    public void EnterFarm()
    {

    }

    public void EnterCabin()
    {

    }

    public void CloseDeathScreen()
    {
        gameManager.instance.CloseDeathScreen();
    }

}
