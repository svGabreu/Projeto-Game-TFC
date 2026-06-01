// PlayerSetupTool.cs
// Assets/Editor/PlayerSetupTool.cs
// Menu: Tools/Player Setup/Setup Selected as Player
//
// Selecione o novo modelo de personagem na Hierarquia e rode o menu.
// O script configura automaticamente todos os componentes necessários:
//   Rigidbody, CapsuleCollider, Animator, Player.cs, PlayerInput,
//   PlayerInteraction.cs, AudioSources (Walk / Sprint / Jump),
//   além de apontar a CameraControl da Main Camera para o novo personagem.

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public static class PlayerSetupTool
{
    // ─────────────────────────────────────────────────────────────────────────
    [MenuItem("Tools/Player Setup/Setup Selected as Player")]
    public static void SetupPlayer()
    {
        var go = Selection.activeGameObject;
        if (go == null)
        {
            EditorUtility.DisplayDialog("Player Setup", "Selecione o GameObject do novo personagem na Hierarquia primeiro!", "OK");
            return;
        }

        if (!EditorUtility.DisplayDialog("Player Setup",
            $"Configurar '{go.name}' como personagem jogável?\n\nIsso adicionará:\n" +
            "• Rigidbody  • CapsuleCollider  • Animator\n" +
            "• Player.cs  • PlayerInput  • PlayerInteraction.cs\n" +
            "• 3 AudioSources (Walk/Sprint/Jump)\n" +
            "• CameraControl na Main Camera\n\nContinuar?",
            "Sim", "Cancelar"))
            return;

        Undo.SetCurrentGroupName("Setup Player Character");
        int group = Undo.GetCurrentGroup();
        int changes = 0;

        // ── Tag ────────────────────────────────────────────────────────────────
        if (go.tag != "Player")
        {
            Undo.RecordObject(go, "Set Tag");
            go.tag = "Player";
            changes++;
            Debug.Log("[PlayerSetup] Tag definida como 'Player'.");
        }

        // ── Rigidbody ─────────────────────────────────────────────────────────
        var rb = go.GetComponent<Rigidbody>();
        if (rb == null) rb = Undo.AddComponent<Rigidbody>(go);
        Undo.RecordObject(rb, "Setup Rigidbody");
        rb.mass = 1f;
        rb.linearDamping = 0f;
        rb.angularDamping = 0.05f;
        rb.useGravity = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.constraints = RigidbodyConstraints.FreezeRotationX
                       | RigidbodyConstraints.FreezeRotationY
                       | RigidbodyConstraints.FreezeRotationZ;
        EditorUtility.SetDirty(rb);
        changes++;
        Debug.Log("[PlayerSetup] Rigidbody configurado.");

        // ── CapsuleCollider ───────────────────────────────────────────────────
        var col = go.GetComponent<CapsuleCollider>();
        if (col == null) col = Undo.AddComponent<CapsuleCollider>(go);
        Undo.RecordObject(col, "Setup Capsule");
        col.height = 1.8f;
        col.radius = 0.35f;
        col.center = new Vector3(0f, 0.9f, 0f);
        col.direction = 1; // Y-axis
        EditorUtility.SetDirty(col);
        changes++;
        Debug.Log("[PlayerSetup] CapsuleCollider configurado.");

        // ── Animator ──────────────────────────────────────────────────────────
        var anim = go.GetComponent<Animator>();
        if (anim == null) anim = Undo.AddComponent<Animator>(go);
        Undo.RecordObject(anim, "Setup Animator");

        // Busca o PlayerAnimator.controller
        var controllerGUIDs = AssetDatabase.FindAssets("PlayerAnimator t:AnimatorController");
        if (controllerGUIDs.Length > 0)
        {
            var ctrl = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
                AssetDatabase.GUIDToAssetPath(controllerGUIDs[0]));
            if (ctrl != null)
            {
                anim.runtimeAnimatorController = ctrl;
                Debug.Log($"[PlayerSetup] Animator → {ctrl.name}");
            }
        }
        else
        {
            Debug.LogWarning("[PlayerSetup] PlayerAnimator.controller não encontrado — atribua manualmente.");
        }
        anim.applyRootMotion = false;
        EditorUtility.SetDirty(anim);
        changes++;

        // ── Player.cs ─────────────────────────────────────────────────────────
        var player = go.GetComponent<Player>();
        if (player == null) player = Undo.AddComponent<Player>(go);
        Undo.RecordObject(player, "Setup Player");
        // Valores padrão iguais ao script original
        player.walkSpeed     = 5f;
        player.sprintSpeed   = 9f;
        player.rotationSpeed = 15f;
        player.jumpForce     = 12f;
        player.fallMultiplier    = 2.5f;
        player.lowJumpMultiplier = 2f;
        player.coyoteTime     = 0.15f;
        player.jumpBufferTime = 0.2f;
        EditorUtility.SetDirty(player);
        changes++;
        Debug.Log("[PlayerSetup] Player.cs configurado com valores padrão.");

        // ── PlayerInput ───────────────────────────────────────────────────────
        var pi = go.GetComponent<UnityEngine.InputSystem.PlayerInput>();
        if (pi == null) pi = Undo.AddComponent<UnityEngine.InputSystem.PlayerInput>(go);
        Undo.RecordObject(pi, "Setup PlayerInput");

        // Busca PlayerControl.inputactions
        var actionAssetGUIDs = AssetDatabase.FindAssets("PlayerControl t:InputActionAsset");
        if (actionAssetGUIDs.Length > 0)
        {
            var asset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(
                AssetDatabase.GUIDToAssetPath(actionAssetGUIDs[0]));
            if (asset != null)
            {
                pi.actions = asset;
                pi.defaultActionMap = "Player";
                pi.notificationBehavior = PlayerNotifications.SendMessages;
                Debug.Log($"[PlayerSetup] PlayerInput → {asset.name} (Send Messages)");
            }
        }
        else
        {
            Debug.LogWarning("[PlayerSetup] PlayerControl.inputactions não encontrado — atribua manualmente.");
        }
        EditorUtility.SetDirty(pi);
        changes++;

        // ── AudioSources ──────────────────────────────────────────────────────
        var walkAudio   = GetOrCreateAudioSource(go, "WalkAudioSource");
        var sprintAudio = GetOrCreateAudioSource(go, "SprintAudioSource");
        var jumpAudio   = GetOrCreateAudioSource(go, "JumpAudioSource");
        changes += 3;

        // Conecta no Player.cs via SerializedObject (campos privados com SerializeField)
        var so = new SerializedObject(player);
        so.Update();
        var walkSrc   = so.FindProperty("walkstepAudioSource");
        var sprintSrc = so.FindProperty("footstepAudioSource");
        var jumpSrc   = so.FindProperty("jumpstepAudioSource");
        if (walkSrc   != null) walkSrc.objectReferenceValue   = walkAudio;
        if (sprintSrc != null) sprintSrc.objectReferenceValue = sprintAudio;
        if (jumpSrc   != null) jumpSrc.objectReferenceValue   = jumpAudio;
        so.ApplyModifiedProperties();
        Debug.Log("[PlayerSetup] AudioSources criados e conectados.");

        // ── PlayerInteraction.cs ──────────────────────────────────────────────
        var interaction = go.GetComponent<PlayerInteraction>();
        if (interaction == null) interaction = Undo.AddComponent<PlayerInteraction>(go);
        Undo.RecordObject(interaction, "Setup PlayerInteraction");
        interaction.interactionRange = 3f;
        // Layer "Interactable" — tenta encontrar pelo nome
        int interactLayer = LayerMask.NameToLayer("Interactable");
        if (interactLayer >= 0)
        {
            interaction.interactableLayer = 1 << interactLayer;
            Debug.Log($"[PlayerSetup] PlayerInteraction → Layer 'Interactable' configurado.");
        }
        else
        {
            Debug.LogWarning("[PlayerSetup] Layer 'Interactable' não encontrado — configure manualmente em PlayerInteraction.");
        }
        EditorUtility.SetDirty(interaction);
        changes++;

        // ── Tenta conectar InteractionPromptText ──────────────────────────────
        ConnectInteractionPrompt(interaction);

        // ── CameraControl na Main Camera ──────────────────────────────────────
        if (Camera.main != null)
        {
            var camCtrl = Camera.main.GetComponent<CameraControl>();
            if (camCtrl == null) camCtrl = Undo.AddComponent<CameraControl>(Camera.main.gameObject);
            Undo.RecordObject(camCtrl, "Setup CameraControl");
            camCtrl.playerTransform  = go.transform;
            camCtrl.offset           = new Vector3(0f, 2f, -4f);
            camCtrl.mouseSensitivity = 10f;
            EditorUtility.SetDirty(camCtrl);
            changes++;
            Debug.Log($"[PlayerSetup] CameraControl → aponta para '{go.name}'.");
        }
        else
        {
            Debug.LogWarning("[PlayerSetup] Main Camera não encontrada — configure CameraControl manualmente.");
        }

        // ── Finaliza ──────────────────────────────────────────────────────────
        Undo.CollapseUndoOperations(group);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log($"[PlayerSetup] Concluído! {changes} configurações aplicadas em '{go.name}'.");
        EditorUtility.DisplayDialog("Player Setup",
            $"'{go.name}' configurado com sucesso!\n\n" +
            $"{changes} componentes/configurações aplicadas.\n\n" +
            "Verifique:\n" +
            "• groundLayer no Player.cs (deve ser 'Ground')\n" +
            "• interactableLayer no PlayerInteraction.cs\n" +
            "• AudioClips nos 3 AudioSources\n" +
            "• Animator Controller (se não encontrado automaticamente)\n" +
            "• Salve a cena (Ctrl+S)",
            "OK");
    }

    // ─────────────────────────────────────────────────────────────────────────
    [MenuItem("Tools/Player Setup/Fix Camera → Selected Player")]
    public static void FixCamera()
    {
        var go = Selection.activeGameObject;
        if (go == null) { Debug.LogWarning("[PlayerSetup] Selecione o personagem primeiro."); return; }

        if (Camera.main != null)
        {
            var camCtrl = Camera.main.GetComponent<CameraControl>();
            if (camCtrl == null) camCtrl = Undo.AddComponent<CameraControl>(Camera.main.gameObject);
            Undo.RecordObject(camCtrl, "Fix CameraControl");
            camCtrl.playerTransform = go.transform;
            EditorUtility.SetDirty(camCtrl);
            Debug.Log($"[PlayerSetup] Camera apontada para '{go.name}'.");
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    /// <summary>Corrige a tag da Main Camera e reconecta CameraControl.playerTransform.</summary>
    [MenuItem("Tools/Player Setup/Fix Main Camera Tag + Reconnect")]
    public static void FixMainCameraTag()
    {
        // 1. Encontra o objeto "Main Camera" na cena e garante tag correta
        Camera[] cams = Resources.FindObjectsOfTypeAll<Camera>();
        Camera mainCam = null;
        foreach (var cam in cams)
        {
            if (!cam.gameObject.scene.IsValid()) continue;
            if (cam.gameObject.name == "Main Camera" || cam.gameObject.name == "MainCamera")
            {
                mainCam = cam;
                break;
            }
        }

        if (mainCam == null)
        {
            EditorUtility.DisplayDialog("Fix Camera", "Nenhuma câmera chamada 'Main Camera' encontrada na cena!", "OK");
            return;
        }

        // Garante tag "MainCamera"
        if (mainCam.tag != "MainCamera")
        {
            Undo.RecordObject(mainCam.gameObject, "Fix Camera Tag");
            mainCam.tag = "MainCamera";
            EditorUtility.SetDirty(mainCam.gameObject);
            Debug.Log($"[PlayerSetup] Tag da câmera '{mainCam.name}' definida como 'MainCamera'.");
        }
        else
        {
            Debug.Log($"[PlayerSetup] Câmera '{mainCam.name}' já tinha tag 'MainCamera'. OK.");
        }

        // 2. Reconecta CameraControl.playerTransform ao Personagem
        var go = Selection.activeGameObject;
        if (go != null && go.GetComponent<Player>() != null)
        {
            var camCtrl = mainCam.GetComponent<CameraControl>();
            if (camCtrl == null) camCtrl = Undo.AddComponent<CameraControl>(mainCam.gameObject);
            Undo.RecordObject(camCtrl, "Reconnect Camera");
            camCtrl.playerTransform = go.transform;
            EditorUtility.SetDirty(camCtrl);
            Debug.Log($"[PlayerSetup] CameraControl.playerTransform → '{go.name}'.");
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        EditorUtility.DisplayDialog("Fix Camera",
            $"Câmera '{mainCam.name}' tag = 'MainCamera' ✓\n" +
            (go != null ? $"CameraControl → '{go.name}' ✓" : "Selecione o Personagem e rode novamente para reconectar CameraControl."),
            "OK");
    }

    // ─────────────────────────────────────────────────────────────────────────
    [MenuItem("Tools/Player Setup/Show Player Component Checklist")]
    public static void ShowChecklist()
    {
        var go = Selection.activeGameObject;
        string target = go != null ? go.name : "(nenhum selecionado)";

        bool hasRb      = go != null && go.GetComponent<Rigidbody>() != null;
        bool hasCap     = go != null && go.GetComponent<CapsuleCollider>() != null;
        bool hasAnim    = go != null && go.GetComponent<Animator>() != null;
        bool hasPlayer  = go != null && go.GetComponent<Player>() != null;
        bool hasPI      = go != null && go.GetComponent<UnityEngine.InputSystem.PlayerInput>() != null;
        bool hasInteract= go != null && go.GetComponent<PlayerInteraction>() != null;
        bool hasCam     = Camera.main != null && Camera.main.GetComponent<CameraControl>() != null;

        string S(bool v) => v ? "✓" : "✗";

        EditorUtility.DisplayDialog("Player Component Checklist",
            $"GameObject: {target}\n\n" +
            $"{S(hasRb)}      Rigidbody\n" +
            $"{S(hasCap)}      CapsuleCollider\n" +
            $"{S(hasAnim)}      Animator\n" +
            $"{S(hasPlayer)}      Player.cs\n" +
            $"{S(hasPI)}      PlayerInput\n" +
            $"{S(hasInteract)}      PlayerInteraction.cs\n" +
            $"{S(hasCam)}      CameraControl (Main Camera)\n\n" +
            "Itens manuais obrigatórios:\n" +
            "• Player.cs → groundLayer (Layer 'Ground')\n" +
            "• PlayerInteraction → interactableLayer\n" +
            "• Animator → Controller\n" +
            "• 3 AudioSources → AudioClips",
            "OK");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────────────────
    private static AudioSource GetOrCreateAudioSource(GameObject parent, string childName)
    {
        // Procura filho existente
        Transform existing = null;
        foreach (Transform t in parent.transform)
            if (t.name == childName) { existing = t; break; }

        if (existing != null)
        {
            var as2 = existing.GetComponent<AudioSource>();
            if (as2 != null) return as2;
        }

        // Cria filho
        var go = new GameObject(childName);
        Undo.RegisterCreatedObjectUndo(go, "Create " + childName);
        go.transform.SetParent(parent.transform, false);
        var src = go.AddComponent<AudioSource>();
        src.playOnAwake = false;
        src.spatialBlend = 0f; // 2D
        EditorUtility.SetDirty(go);
        return src;
    }

    private static void ConnectInteractionPrompt(PlayerInteraction interaction)
    {
        // Tenta encontrar o InteractionPrompt na cena pelo nome
        var allTmps = Resources.FindObjectsOfTypeAll<TMPro.TextMeshProUGUI>();
        foreach (var tmp in allTmps)
        {
            if (tmp.gameObject.scene.IsValid() &&
                (tmp.gameObject.name == "InteractionPrompt" || tmp.gameObject.name.Contains("InteractionPrompt")))
            {
                var so = new SerializedObject(interaction);
                so.Update();
                var prop = so.FindProperty("interactionPromptText");
                if (prop != null)
                {
                    prop.objectReferenceValue = tmp;
                    so.ApplyModifiedProperties();
                    Debug.Log($"[PlayerSetup] InteractionPrompt conectado: {tmp.gameObject.name}");
                }
                return;
            }
        }
        Debug.LogWarning("[PlayerSetup] InteractionPrompt não encontrado na cena — conecte manualmente em PlayerInteraction.cs");
    }
}
#endif
