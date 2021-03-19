using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Effect : MonoBehaviour
{
    public Player targetPlayer = null;
    public Card sourceCard = null;
    public Image effectImage = null;

    public void EndTrigger()
    {
        bool bounce = false;

        if (targetPlayer.HasMirror())
        {
            bounce = true;
            targetPlayer.SetMirror(false);

            if (targetPlayer.isPlayer)
            {
                GameController.instance.CastAttackEffect(sourceCard, GameController.instance.environmentalPlayer);
            }
            else
            {
                GameController.instance.CastAttackEffect(sourceCard, GameController.instance.player);
            }

        }
        else //business as usual. no mirror
        {
            int effect = sourceCard.cardData.effect;
            if (!targetPlayer.isPlayer) // enemy/environment
            {
                if (sourceCard.cardData.damageType == CardData.DamageType.Morale && targetPlayer.isWildlife)
                    effect = effect / 2;
                if (sourceCard.cardData.damageType == CardData.DamageType.Cold && !targetPlayer.isWildlife)
                    effect = effect / 2;
            }

            targetPlayer.health -= effect;
            targetPlayer.PlayHitAnim();

            GameController.instance.UpdateHealthes();
            //check for death

            if (!bounce)
                GameController.instance.NextPlayersTurn();

            GameController.instance.isPlayable = true;
        }

        Destroy(gameObject);

    }
}
