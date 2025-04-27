using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    [Header("Referências")]
    public PlayerCombat playerCombat; // Referência pro script de combate
    public RawImage barraDeVida; // Agora é RawImage, não Image!

    [Header("Cores da Vida")]
    public Color corVidaAlta = Color.green;
    public Color corVidaMedia = Color.yellow;
    public Color corVidaBaixa = Color.red;

    void Update()
    {
        if (playerCombat == null || barraDeVida == null)
            return;

        AtualizarCorDaVida();
    }

    private void AtualizarCorDaVida()
    {
        float porcentagemVida = (float)playerCombat.playerHealth / 100f;

        if (porcentagemVida > 0.6f)
        {
            barraDeVida.color = corVidaAlta;
        }
        else if (porcentagemVida > 0.3f)
        {
            barraDeVida.color = corVidaMedia;
        }
        else
        {
            barraDeVida.color = corVidaBaixa;
        }
    }
}
