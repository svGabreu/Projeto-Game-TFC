using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    [SerializeField] private GameObject MainMenu;
    [SerializeField] private GameObject OptionsMenu;

    [Header("Configurações de Áudio do Botão Jogar")]
    [SerializeField] private AudioSource audioSource; // Arraste o AudioSource do menu aqui
    [SerializeField] private AudioClip playButtonSound; // Coloque o áudio da sirene aqui

    public void Play()
    {
        // Se houver um áudio configurado, toca e espera terminar antes de mudar de cena
        if (audioSource != null && playButtonSound != null)
        {
            StartCoroutine(PlaySoundAndLoadScene());
        }
        else
        {
            // Caso falte o áudio por engano, carrega direto para não travar o jogo
            SceneManager.LoadScene(1);
        }
    }

    private IEnumerator PlaySoundAndLoadScene()
    {
        // Toca o som da sirene apenas uma vez
        audioSource.PlayOneShot(playButtonSound);

        // Aguarda a duração exata do clipe de áudio terminar
        yield return new WaitForSeconds(playButtonSound.length);

        // Carrega a cena do jogo após o som acabar
        SceneManager.LoadScene(1);
    }

    public void Options()
    {
        MainMenu.SetActive(false);
        OptionsMenu.SetActive(true);
    }

    public void OptionsClose()
    {
        OptionsMenu.SetActive(false);
        MainMenu.SetActive(true);
    }

    public void Quit()
    {
        Application.Quit();
    }
}