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

        Color corAtual = Color.white; // Cor padrão

        if (porcentagemVida > 0.6f)
        {
            corAtual = corVidaAlta;
        }
        else if (porcentagemVida > 0.3f)
        {
            corAtual = corVidaMedia;
        }
        else
        {
            corAtual = corVidaBaixa;
        }

        // Atualiza a cor do RawImage normalmente
        barraDeVida.color = corAtual;

        // Atualiza também o _Color no material (se tiver Material)
        if (barraDeVida.material != null)
        {
            barraDeVida.material.SetColor("_Color", corAtual);
        }
    }
}
