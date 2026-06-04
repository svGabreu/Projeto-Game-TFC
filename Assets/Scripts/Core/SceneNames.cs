// SceneNames.cs
// Assets/Scripts/Core/SceneNames.cs
// Constantes de nomes de cena e IDs de SpawnPoint.
// Use sempre essas constantes em vez de strings mágicas.

public static class SceneNames
{
    // ── Cenas do jogo ──────────────────────────────────────────────────────────
    public const string MENU               = "Menu";
    public const string EGITO              = "Egito";
    public const string CASA_SOCIAL        = "Casa_social";
    public const string CASA_RIO_DA_VIDA   = "Casa_RioDaVida";
    public const string CASA_HIEROGLIFOS   = "Casa_Hieroglifos";
    public const string DENTRO_DA_PIRAMIDE = "DentroDaPiramide";
    public const string LABORATORIO_LOGICA = "LaboratiorioLogica";
}

public static class SpawnIDs
{
    // ── SpawnPoints dentro de cada Casa (spawn ao entrar) ─────────────────────
    public const string ENTRADA_CASA_SOCIAL      = "entrada_casa_social";
    public const string ENTRADA_CASA_RIO         = "entrada_casa_rio";
    public const string ENTRADA_CASA_HIEROGLIFOS = "entrada_casa_hieroglifos";
    public const string ENTRADA_PIRAMIDE         = "entrada_piramide";
    public const string ENTRADA_LABORATORIO      = "entrada_laboratorio";

    // ── SpawnPoints no Egito (spawn ao sair de cada Casa) ────────────────────
    public const string EGITO_PORTA_SOCIAL      = "egito_porta_social";
    public const string EGITO_PORTA_RIO         = "egito_porta_rio";
    public const string EGITO_PORTA_HIEROGLIFOS = "egito_porta_hieroglifos";
    public const string EGITO_PORTA_PIRAMIDE    = "egito_porta_piramide";
    public const string EGITO_PORTA_LABORATORIO = "egito_porta_laboratorio";
}
