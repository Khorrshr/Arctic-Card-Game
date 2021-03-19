using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    static public GameController instance = null;

    public Deck playerDeck = new Deck();
    public Deck enemyDeck = new Deck();

    public Hand playersHand = new Hand();
    public Hand enemysHand = new Hand();

    public Player player = null;
    public Player environmentalPlayer = null;

    public List<CardData> cards = new List<CardData>();

    public Sprite[] healthNumbers = new Sprite[10];
    public Sprite[] effectNumbers = new Sprite[10];

    public GameObject cardPrefab = null;
    public Canvas canvas = null;

    public bool isPlayable = false;

    public GameObject effectFromLeftPrefab = null;
    public GameObject effectFromRightPrefab = null;

    public Sprite iceBallImage = null;
    public Sprite moraleDownImage = null;
    public Sprite multiIceBallImage = null;
    public Sprite multiMoraleDownImage = null;
    public Sprite allBallImage = null;

    public bool playersTurn = true;
    public Text turnText = null;
    public Text distanceText = null;

    public int playerDistance = 0;
    public int playerEncounterSuccesses = 0;

    public Image enviroSkipTurn = null;

    public Sprite wildLife = null;
    public Sprite nonWildLife = null;


    private void Awake()
    {
        instance = this;

        SetUpEnviro(); //random starting encounter

        playerDeck.Create();
        enemyDeck.Create();

        StartCoroutine(DealHands());
    }

    public void Quit()
    {
        SceneManager.LoadScene(0);
    }

    
    public void SkipTurn()
    {
        //Debug.Log("Skip Turn");
        if (playersTurn && isPlayable)
            NextPlayersTurn();
    }

    internal IEnumerator DealHands()
    {
        yield return new WaitForSeconds(1);
        for (int t = 0; t<3; t++)
        {
            
            playerDeck.DealCard(playersHand);
            enemyDeck.DealCard(enemysHand);
            yield return new WaitForSeconds(1);
        }
        isPlayable = true;
    }

    internal bool UseCard(Card card, Player usingOnPlayer, Hand fromHand)
    {
        //is card valid
        if (!CardValid(card, usingOnPlayer, fromHand))
            return false;

        //cast card
        isPlayable = false;
        CastCard(card, usingOnPlayer, fromHand);
        player.glowImage.gameObject.SetActive(false);
        environmentalPlayer.glowImage.gameObject.SetActive(false);

        //remove card
        fromHand.RemoveCard(card);

        //deal new card
        return false;
    }

    internal bool CardValid(Card cardBeingPlayed, Player usingOnPlayer, Hand fromHand)
    {
        bool valid = false;

        if (cardBeingPlayed == null)
            return false;

        if (fromHand.isPlayers)
        {
            if (cardBeingPlayed.cardData.cost <= player.energy)
            {
                if (usingOnPlayer.isPlayer && cardBeingPlayed.cardData.isDefenseCard)
                    valid = true;
                if (!usingOnPlayer.isPlayer && !cardBeingPlayed.cardData.isDefenseCard)
                    valid = true;
            }
        }
        else //played from enemys hand
        {
            if (cardBeingPlayed.cardData.cost <= environmentalPlayer.energy)
            {
                if (!usingOnPlayer.isPlayer && cardBeingPlayed.cardData.isDefenseCard)
                    valid = true;
                if (usingOnPlayer.isPlayer && !cardBeingPlayed.cardData.isDefenseCard)
                    valid = true;
            }

        }

        return valid;
    }

    internal void CastCard(Card card, Player usingOnPlayer, Hand fromHand)
    {
        if (card.cardData.isMirrorCard)
        {
            usingOnPlayer.SetMirror(true);
            NextPlayersTurn();
            isPlayable = true;
        }
        else
        {
            if (card.cardData.isDefenseCard) //health cards
            {
                usingOnPlayer.health += card.cardData.effect;
                if (usingOnPlayer.health > usingOnPlayer.maxHealth)
                    usingOnPlayer.health = usingOnPlayer.maxHealth;

                UpdateHealthes();

                StartCoroutine(CastHealEffect(usingOnPlayer));
            }
            else //is Attack card
            {
                CastAttackEffect(card, usingOnPlayer);
            }
            // add score
            if (fromHand.isPlayers)
                playerDistance += card.cardData.effect;

            UpdateDistance();

        }
        //update player energy
        if (fromHand.isPlayers)
        {
            GameController.instance.player.energy -= card.cardData.cost;
            GameController.instance.player.UpdateTimeBalls();
        }
        else
        {
            GameController.instance.environmentalPlayer.energy -= card.cardData.cost;
            GameController.instance.environmentalPlayer.UpdateTimeBalls();
        }
    }

    private IEnumerator CastHealEffect(Player usingOnPlayer)
    {
        yield return new WaitForSeconds(0.5f);
        NextPlayersTurn();
        isPlayable = true;
    }

    internal void CastAttackEffect(Card card, Player usingOnPlayer)
    {
        GameObject effectGO = null;
        if (usingOnPlayer.isPlayer)
            effectGO = Instantiate(effectFromRightPrefab, canvas.gameObject.transform);
        else
            effectGO = Instantiate(effectFromLeftPrefab, canvas.gameObject.transform);

        Effect effect = effectGO.GetComponent<Effect>();
        if (effect)
        {
            effect.targetPlayer = usingOnPlayer;
            effect.sourceCard = card;

            switch (card.cardData.damageType)
            {
                case CardData.DamageType.Morale:
                    if (card.cardData.isMulti)
                        effect.effectImage.sprite = multiMoraleDownImage;
                    else //not multi
                        effect.effectImage.sprite = moraleDownImage;
                break;

                case CardData.DamageType.Cold:
                    if (card.cardData.isMulti)
                        effect.effectImage.sprite = multiIceBallImage;
                    else //not multi
                        effect.effectImage.sprite = iceBallImage;
                break;

                case CardData.DamageType.Both:
                    effect.effectImage.sprite = allBallImage;
                break;

            }
        }

    }

    internal void UpdateHealthes()
    {
        player.UpdateHealth();
        environmentalPlayer.UpdateHealth();

        if (player.health <= 0)
        {
            //Gameover
            StartCoroutine(GameOver());
        }

        if (environmentalPlayer.health <= 0)
        {
            playerEncounterSuccesses++;
            playerDistance += 100;
            UpdateDistance();
            //new encounter
            StartCoroutine(NewEncounter());
        }
    }

    private void SetUpEnviro()
    {
        environmentalPlayer.energy = 0;
        environmentalPlayer.health = 5;
        environmentalPlayer.UpdateHealth();
        environmentalPlayer.isWildlife = true;
        if (UnityEngine.Random.Range(0, 2) == 1) //random events type
            environmentalPlayer.isWildlife = false;
        if (environmentalPlayer.isWildlife)
            environmentalPlayer.playerImage.sprite = wildLife;
        else
            environmentalPlayer.playerImage.sprite = nonWildLife;
    }

    private IEnumerator NewEncounter()
    {
        environmentalPlayer.gameObject.SetActive(false);
        //clear enviro hand
        enemysHand.ClearHand();
        yield return new WaitForSeconds(0.75f);
        //set up new encounter
        SetUpEnviro();
        environmentalPlayer.gameObject.SetActive(true);
        StartCoroutine(DealHands());
    }

    private IEnumerator GameOver()
    {
        yield return new WaitForSeconds(1);
        UnityEngine.SceneManagement.SceneManager.LoadScene(2);
    }

    internal void NextPlayersTurn()
    {
        playersTurn = !playersTurn;
        bool challengeIsCompleted = false;

        if (playersTurn)
        {
            if (player.energy < 5)
                player.energy++;
        }
        else //enviro turn
        {
            if (environmentalPlayer.health > 0)
            {
                if (environmentalPlayer.energy < 5)
                    environmentalPlayer.energy++;
            }
            else
                challengeIsCompleted = true;
        }

        if (challengeIsCompleted)
        {
            playersTurn = !playersTurn;
            if (player.energy < 5)
                player.energy++;
        }
        else
        {
            SetTurnText();
            if (!playersTurn)
                EnviroTurn();
        }

        player.UpdateTimeBalls();
        environmentalPlayer.UpdateTimeBalls();
    }

    internal void SetTurnText()
    {
        if (playersTurn)
        {
            turnText.text = "Your turn";
        }
        else
        {
            turnText.text = "Environmental turn";
        }
    }

    private void EnviroTurn()
    {
        Card card = AIChooseCard();
        StartCoroutine(EnviroCastCard(card));
    }

    private Card AIChooseCard()
    {
        List<Card> available = new List<Card>();
        for (int i=0; i<3; i++)
        {
            if (CardValid(enemysHand.cards[i], environmentalPlayer, enemysHand))
                available.Add(enemysHand.cards[i]);
            else if (CardValid(enemysHand.cards[i], player, enemysHand))
                available.Add(enemysHand.cards[i]);
        }

        if (available.Count == 0) //no playable cards for AI
        {
            NextPlayersTurn();
            return null;
        }
        int choice = UnityEngine.Random.Range(0, available.Count);
        return available[choice];
    }

    private IEnumerator EnviroCastCard(Card card)
    {
        yield return new WaitForSeconds(0.5f);

        if (card)
        {
            //turn over player
            TurnCard(card);
            yield return new WaitForSeconds(2);

            //use card
            if (card.cardData.isDefenseCard)
                UseCard(card, environmentalPlayer, enemysHand);
            else //attack card
                UseCard(card, player, enemysHand);
            yield return new WaitForSeconds(1);

            //deal card
            enemyDeck.DealCard(enemysHand);
            yield return new WaitForSeconds(1);


        }
        else //no card to choose, so skip
        {
            //display enviro skipturn 
            enviroSkipTurn.gameObject.SetActive(true);
            yield return new WaitForSeconds(1);
            //hide enviro skipturn
            enviroSkipTurn.gameObject.SetActive(false);            
        }
    }

    internal void TurnCard(Card card)
    {
        Animator animator = card.GetComponentInChildren<Animator>();
        if (animator)
        {
            animator.SetTrigger("Flip");
        }
        else
            Debug.LogError("No Animator found");
    }

    private void UpdateDistance()
    {
        distanceText.text = "Score: " + playerEncounterSuccesses.ToString() +
                            "  Distance: " + playerDistance.ToString();
    }
}
