using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    [SerializeField] private GameObject MainMenu;
    [SerializeField] private GameObject OptionsMenu;

    public void Play()
    {
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
