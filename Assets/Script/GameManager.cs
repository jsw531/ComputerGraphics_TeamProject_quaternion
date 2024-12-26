using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public PlayerInventory playerInventory;
    public Transform startPoint;
    public float clearDistance = 2f;
    public UIManager uIManager;

    void Update()
    {
        if (playerInventory.HasCollectedAllFloppyDisks())
        {
            float distanceToStart = Vector3.Distance(transform.position, startPoint.position);
            if (distanceToStart <= clearDistance)
            {
                GameClear();
            }
        }
    }

    void GameClear()
    {
        uIManager.Clear();
    }
}
