using UnityEngine;

public class XPSystem : MonoBehaviour
{
    public float XPHas;
    public float XPPotential;
    float XPLevelAmount = 25;
    public float XPModifierExtract= 1.5f;
    float XPModifierDied = .1f;

    public bool alphaKilled;
    public bool hasExtracted;

    public int skillPointAmount;
    public int currentLevel;
    public int currentMoney;
    public int possibleMoney;

    public void UpdatingDeathXPModifier()
    {

        XPModifierDied += .1f;

    }

    public void UpdatingExtractionXPModifier()
    {

        XPModifierExtract += .1f;

    }

    public void SprintUpgrade()
    {
        gameManager.instance.playerScript.sprintSpeed += .5f;
    }

    public void HealthUpgrade()
    {
        gameManager.instance.healthScript.maxHealth += 10;
    }
    public void CountingXP()
    {
        if (hasExtracted && alphaKilled)
        {
            XPHas += XPPotential * XPModifierExtract;
            XPPotential = 0;
            currentMoney += possibleMoney;
            possibleMoney = 0;
        }
        else if (hasExtracted || alphaKilled)
        {
            XPHas += XPPotential;
            XPPotential = 0;
            currentMoney += possibleMoney;
            possibleMoney = 0;
        }
        else
        {
            XPHas += XPPotential * XPModifierDied;
            XPPotential = 0;
        }
        if (XPHas >= XPLevelAmount)
        {
            do
            {
                XPHas -= XPLevelAmount;
                LevelIncrease();
                XPLevelAmount *= 1.5f;
            } while (XPHas >= XPLevelAmount); 
        }
    }

    void LevelIncrease()
    {
        currentLevel++;
        skillPointAmount++;
        //All Base upgrades goes here.
        gameManager.instance.healthScript.maxHealth += 5;
        gameManager.instance.playerScript.moveSpeed += .5f;
    }

    public void SetPossibleMoney(int amount)
    {
        possibleMoney = amount;
    }
}
