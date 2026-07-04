// SceneTransitionManager.cs
// Assets/Scripts/Core/SceneTransitionManager.cs
// Singleton persistente entre cenas.
// Gerencia transições com fade preto + async loading.
// O Canvas de fade é criado por código — nenhum prefab necessário.
//
// Uso:
//   SceneTransitionManager.Instance.GoToScene("Casa_social", "entrada_casa_social", "egito_porta_social");
//   SceneTransitionManager.Instance.ReturnToPreviousScene();

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    [Header("Configuração")]
    [Tooltip("Duração do fade in/out em segundos.")]
    public float fadeDuration = 0.4f;

    // ── Estado público (lido por SpawnPoint) ──────────────────────────────────
    /// <summary>ID do SpawnPoint a usar na cena que está sendo carregada.</summary>
    public string TargetSpawnID { get; private set; } = "";

    /// <summary>Nome da cena anterior (usado para voltar ao Egito).</summary>
    public string PreviousScene { get; private set; } = "";

    /// <summary>ID do SpawnPoint para quando voltar à cena anterior.</summary>
    public string PreviousSpawnID { get; private set; } = "";

    public bool IsTransitioning { get; private set; } = false;

    // ── Fade UI ───────────────────────────────────────────────────────────────
    private Image fadeImage;

    // ─────────────────────────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            // Verifica se o Manager existente já é o Manager do jogo principal:
            // usa GameStateManager.Instance (que pode estar em GO separado do STM,
            // devido ao SetParent(null) no GSM.Awake) em vez de GetComponent.
            bool existingIsGameManager = GameStateManager.Instance != null;
            if (existingIsGameManager)
            {
                Destroy(gameObject);
                return;
            }

            // Manager existente é o intro (sem GameStateManager).
            // O Manager do Egito deve substituí-lo, completando o fade-in.
            bool oldWasTransitioning   = Instance.IsTransitioning;
            string savedTargetSpawnID  = Instance.TargetSpawnID;   // preserva spawn alvo do Menu
            string savedPreviousScene  = Instance.PreviousScene;
            string savedPreviousSpawn  = Instance.PreviousSpawnID;
            Destroy(Instance.gameObject);
            Instance = this;
            TargetSpawnID   = savedTargetSpawnID;
            PreviousScene   = savedPreviousScene;
            PreviousSpawnID = savedPreviousSpawn;
            DontDestroyOnLoad(gameObject);
            BuildFadeCanvas();
            PersistUIGlobal();
            if (oldWasTransitioning)
            {
                fadeImage.color = new Color(0f, 0f, 0f, 1f);
                StartCoroutine(CompleteHandoff());
            }
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        BuildFadeCanvas();
        PersistUIGlobal();
    }

    private IEnumerator CompleteHandoff()
    {
        yield return null;
        yield return null;
        yield return StartCoroutine(Fade(1f, 0f));
        IsTransitioning = false;
    }

    // Persiste o UIGlobal entre cenas. Os painéis filhos (PainelExame, PainelInventario, etc.)
    // iniciam inativos — seus Awake() não disparam, então não podem chamar DontDestroyOnLoad.
    // Este método cobre esse caso ao ser chamado pelo SceneTransitionManager ativo.
    private void PersistUIGlobal()
    {
        // GameObject.Find() ignora objetos inativos — usa GetRootGameObjects() no lugar
        var roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (var root in roots)
        {
            if (root.name != "UIGlobal") continue;

            // UIGlobal precisa estar ativo para que seus filhos possam funcionar
            if (!root.activeSelf)
            {
                root.SetActive(true);
                Debug.Log("[STM] UIGlobal estava inativo — ativado automaticamente.");
            }

            DontDestroyOnLoad(root);
            Debug.Log("[STM] UIGlobal persistido via DontDestroyOnLoad.");
            return;
        }

        Debug.LogWarning("[STM] UIGlobal não encontrado na cena — verifique a hierarquia do Egito.");
    }

    /// <summary>
    /// Cria por código um Canvas fullscreen com um painel preto para o fade.
    /// Fica como filho deste GO (DontDestroyOnLoad automático).
    /// </summary>
    private void BuildFadeCanvas()
    {
        // Canvas root
        var canvasGO = new GameObject("FadeCanvas");
        canvasGO.transform.SetParent(transform, false);

        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999; // sempre na frente de tudo

        canvasGO.AddComponent<CanvasScaler>();
        // Sem GraphicRaycaster — não precisa de interação

        // Painel preto cobrindo a tela toda
        var panelGO = new GameObject("BlackPanel");
        panelGO.transform.SetParent(canvasGO.transform, false);

        fadeImage         = panelGO.AddComponent<Image>();
        fadeImage.color   = new Color(0f, 0f, 0f, 0f);   // começa transparente
        fadeImage.raycastTarget = false;                   // não bloqueia cliques

        var rt         = panelGO.GetComponent<RectTransform>();
        rt.anchorMin   = Vector2.zero;
        rt.anchorMax   = Vector2.one;
        rt.offsetMin   = Vector2.zero;
        rt.offsetMax   = Vector2.zero;
    }

    // ── API pública ───────────────────────────────────────────────────────────

    /// <summary>
    /// Transita para outra cena com fade preto.
    /// </summary>
    /// <param name="targetScene">Nome exato da cena de destino.</param>
    /// <param name="spawnID">ID do SpawnPoint onde o jogador vai aparecer.</param>
    /// <param name="returnSpawnID">
    ///   ID do SpawnPoint que deve ser usado quando o jogador SAIR dessa cena de volta.
    ///   Normalmente é o ponto no Egito próximo à porta que acabou de entrar.
    /// </param>
    public void GoToScene(string targetScene, string spawnID = "", string returnSpawnID = "")
    {
        if (IsTransitioning) return;
        IsTransitioning = true; // Define imediatamente — evita double-trigger no mesmo frame

        PreviousScene   = SceneManager.GetActiveScene().name;
        PreviousSpawnID = returnSpawnID;
        TargetSpawnID   = spawnID;

        Debug.Log($"[STM] GoToScene → '{targetScene}' | target='{spawnID}' | return='{returnSpawnID}' | PreviousScene='{PreviousScene}'");
        StartCoroutine(TransitionRoutine(targetScene));
    }

    /// <summary>
    /// Volta para a cena anterior (ex: sair de uma Casa e voltar ao Egito).
    /// Usa os dados salvos pelo último GoToScene.
    /// </summary>
    public void ReturnToPreviousScene()
    {
        if (string.IsNullOrEmpty(PreviousScene)) return;
        GoToScene(PreviousScene, PreviousSpawnID);
    }

    // ── Coroutine de transição ────────────────────────────────────────────────
    private IEnumerator TransitionRoutine(string sceneName)
    {
        IsTransitioning = true;

        // Fecha todos os painéis antes de transitar — evita que estado de UI
        // de uma cena vaze para a próxima e bloqueie interações
        CloseAllPanels();

        // Garante Time.timeScale normal (pode estar 0 se painel de UI estava aberto)
        Time.timeScale = 1f;
        // Trava cursor durante a transição
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;

        // 1. Fade OUT (transparente → preto)
        yield return StartCoroutine(Fade(0f, 1f));

        // 2. Carrega a cena de forma assíncrona
        var op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        // Aguarda o carregamento (progress vai de 0 a 0.9 antes de activar)
        while (op.progress < 0.9f)
            yield return null;

        // Ativa a cena (chama Awake/Start dos objetos)
        op.allowSceneActivation = true;

        // Aguarda a cena estar totalmente carregada
        while (!op.isDone)
            yield return null;

        // 3. Um frame de buffer para os objetos da nova cena inicializarem
        yield return null;
        yield return null;

        // 4. Fade IN (preto → transparente)
        yield return StartCoroutine(Fade(1f, 0f));

        IsTransitioning = false;
    }

    // ── Fade ─────────────────────────────────────────────────────────────────
    private IEnumerator Fade(float fromAlpha, float toAlpha)
    {
        if (fadeImage == null) yield break;

        float elapsed = 0f;
        Color c = fadeImage.color;
        c.a = fromAlpha;
        fadeImage.color = c;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime; // unscaled: funciona mesmo com timeScale = 0
            c.a = Mathf.Lerp(fromAlpha, toAlpha, elapsed / fadeDuration);
            fadeImage.color = c;
            yield return null;
        }

        c.a = toAlpha;
        fadeImage.color = c;
    }

    // ── Fecha todos os painéis antes de transitar ─────────────────────────────
    private void CloseAllPanels()
    {
        // Fecha cada painel persistente pelo singleton, se existir e estiver aberto
        if (InventoryUI.Instance   != null && InventoryUI.Instance.IsOpen())   InventoryUI.Instance.ClosePanel();
        if (SocialUI.Instance      != null && SocialUI.Instance.IsOpen())      SocialUI.Instance.ClosePanel();
        if (RioDaVidaUI.Instance   != null && RioDaVidaUI.Instance.IsOpen())   RioDaVidaUI.Instance.ClosePanel();
        if (ItemExamineUI.Instance != null && ItemExamineUI.Instance.IsOpen()) ItemExamineUI.Instance.ClosePanel();

        // MuralUI não é singleton — fecha qualquer instância ativa na cena
        var murais = FindObjectsByType<MuralUI>(FindObjectsSortMode.None);
        foreach (var m in murais)
            if (m.IsOpen()) m.CloseMural();
    }

    // ── Utilitário: limpa o spawn ID após uso (chamado por SpawnPoint) ────────
    public void ConsumeSpawnID() => TargetSpawnID = "";
}
