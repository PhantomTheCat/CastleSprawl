using UnityEngine;

/// <summary>
/// Class for tracking player stats, such as the key, rather than movement
/// </summary>
public class PlayerBehavior : MonoBehaviour
{
    //Properties
    private int amountOfKeysCollected = 0;
    private int keysNeededToEscape = 1;


    //Methods
    public void CollectKey()
    {
        amountOfKeysCollected++;
    }

    public void UseKey()
    {
        amountOfKeysCollected--;
    }

    public bool CheckIfEnoughKeys()
    {
        //Seeing if we have enough keys
        //(will return true if we have enough)
        if (amountOfKeysCollected >= keysNeededToEscape)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
