using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Player : MonoBehaviour, IDropHandler
{
    public Image playerImage = null;
    public Image buffImage = null;
    public Image healthNumberImage = null;
    public Image glowImage = null;

    public int maxHealth = 5;
    public int health = 5; //current health
    public int energy = 1;

    public bool isPlayer;
    public bool isWildlife;

    public GameObject[] timeBalls = new GameObject[5];

    private Animator animator = null;


    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        UpdateHealth();
        UpdateTimeBalls();
    }

    internal void PlayHitAnim()
    {
        if (animator != null)
            animator.SetTrigger("Hit");
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (!GameController.instance.isPlayable)
            return;
        //Debug.Log("Card droppped onto Player/Environment");

        GameObject obj = eventData.pointerDrag;
        if (obj != null)
        {
            Card card = obj.GetComponent<Card>();
            if (card != null)
            {
                GameController.instance.UseCard(card, this, GameController.instance.playersHand);
            }
        }
    }

    internal void UpdateHealth()
    {
        if (health >= 0 && health < GameController.instance.healthNumbers.Length)
        {
            healthNumberImage.sprite = GameController.instance.healthNumbers[health];

        }

        else
        {
            Debug.LogWarning("Health is not a valid number, " + health.ToString());
        }
    }

    internal void SetMirror(bool on)
    {
        buffImage.gameObject.SetActive(on);
    }

    internal bool HasMirror()
    {
        return buffImage.gameObject.activeInHierarchy;
    }

    internal void UpdateTimeBalls()
    {
        for (int m=0; m<5; m++)
        {
            if (energy > m)
                timeBalls[m].SetActive(true);
            else
                timeBalls[m].SetActive(false);
        }
    }
}
