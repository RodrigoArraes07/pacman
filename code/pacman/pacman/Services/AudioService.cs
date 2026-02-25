using System.Diagnostics;

namespace pacman.Services;

public class AudioService : IAudioService
{
    private readonly ILogger<AudioService> _logger;
    private readonly string _soundsDir;

    // ── Windows: WMP COM (um objeto por som) ───────────────────────────────────
    private dynamic? _wmpGameStart;
    private dynamic? _wmpEatDotA;
    private dynamic? _wmpEatDotB;
    private dynamic? _wmpLoseLife;
    private dynamic? _wmpGameOver;
    private dynamic? _wmpFrightened;
    private dynamic? _wmpWin;
    private dynamic? _wmpUiClick;
    private bool _wmpAvailable;
    public bool IsMuted { get; set; }

    // ── Linux: processo persistente do loop de susto ───────────────────────────
    private Process? _frightenedProcess;
    private bool _eatDotToggle;

    public AudioService(ILogger<AudioService> logger)
    {
        _logger    = logger;
        _soundsDir = Path.Combine(AppContext.BaseDirectory, "Assets", "Sounds");

        _logger.LogInformation("AudioService init. SoundsDir={Dir} Exists={Exists}",
            _soundsDir, Directory.Exists(_soundsDir));

        if (OperatingSystem.IsWindows())
            InitWindowsAudio();
    }

    // ── Init WMP COM ───────────────────────────────────────────────────────────
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    private void InitWindowsAudio()
    {
        var type = Type.GetTypeFromProgID("WMPlayer.OCX.7");
        if (type == null)
        {
            _logger.LogWarning("WMP COM (WMPlayer.OCX.7) não encontrado.");
            return;
        }

        try
        {
            _wmpGameStart  = CreateWmp(type, "game_start.mp3");
            _wmpEatDotA    = CreateWmp(type, "eat_dot.mp3");
            _wmpEatDotB    = CreateWmp(type, "eat_dot.mp3");
            _wmpLoseLife   = CreateWmp(type, "lose_life.mp3");
            _wmpGameOver   = CreateWmp(type, "game_over.mp3");
            _wmpFrightened = CreateWmp(type, "frightened.mp3", loop: true);
            _wmpWin        = CreateWmp(type, "win.mp3");
            _wmpUiClick    = CreateWmp(type, "ui_click.mp3");
            _wmpAvailable  = true;
            _logger.LogInformation("WMP COM inicializado com sucesso.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao inicializar WMP COM.");
        }
    }

    private dynamic CreateWmp(Type type, string filename, bool loop = false)
    {
        var path = Path.Combine(_soundsDir, filename);
        dynamic wmp = Activator.CreateInstance(type)!;
        wmp.settings.autoStart = false;
        wmp.settings.volume    = 100;
        if (loop) wmp.settings.setMode("loop", true);
        wmp.URL = path;
        _logger.LogInformation("WMP loaded: {File}", filename);
        return wmp;
    }

    private static void PlayWmp(dynamic wmp)
    {
        try
        {
            wmp.controls.stop();
            wmp.controls.play();
        }
        catch { /* ignora erros de playback */ }
    }

    // ── Linux: spawna ffplay (fallback: mpg123) ────────────────────────────────
    private void SpawnLinux(string filename)
    {
        var path = Path.Combine(_soundsDir, filename);
        if (!TrySpawn("ffplay", $"-nodisp -autoexit -v quiet \"{path}\""))
            TrySpawn("mpg123", $"-q \"{path}\"");
    }

    private static bool TrySpawn(string exe, string args)
    {
        try
        {
            Process.Start(new ProcessStartInfo(exe, args)
            {
                CreateNoWindow  = true,
                UseShellExecute = false
            });
            return true;
        }
        catch { return false; }
    }

    private Process? SpawnLinuxLoop(string filename)
    {
        var path = Path.Combine(_soundsDir, filename);
        try
        {
            return Process.Start(new ProcessStartInfo("ffplay",
                $"-loop 0 -nodisp -v quiet \"{path}\"")
            { CreateNoWindow = true, UseShellExecute = false })
            ?? Process.Start(new ProcessStartInfo("mpg123", $"-Z -q \"{path}\"")
            { CreateNoWindow = true, UseShellExecute = false });
        }
        catch { return null; }
    }

    // ── Interface ──────────────────────────────────────────────────────────────
    public void PlayGameStart()
    {
        if (IsMuted) return;
        if (OperatingSystem.IsWindows() && _wmpAvailable) PlayWmp(_wmpGameStart!);
        else SpawnLinux("game_start.mp3");
    }

    public void PlayEatDot()
    {
        if (IsMuted) return;
        _eatDotToggle = !_eatDotToggle;
        if (OperatingSystem.IsWindows() && _wmpAvailable)
            PlayWmp(_eatDotToggle ? _wmpEatDotA! : _wmpEatDotB!);
        else
            SpawnLinux("eat_dot.mp3");
    }

    public void PlayLoseLife()
    {
        if (IsMuted) return;
        if (OperatingSystem.IsWindows() && _wmpAvailable) PlayWmp(_wmpLoseLife!);
        else SpawnLinux("lose_life.mp3");
    }

    public void PlayGameOver()
    {
        if (IsMuted) return;
        if (OperatingSystem.IsWindows() && _wmpAvailable) PlayWmp(_wmpGameOver!);
        else SpawnLinux("game_over.mp3");
    }

    public void PlayWin()
    {
        if (IsMuted) return;
        if (OperatingSystem.IsWindows() && _wmpAvailable) PlayWmp(_wmpWin!);
        else SpawnLinux("win.mp3");
    }

    public void PlayUiClick()
    {
        if (IsMuted) return;
        if (OperatingSystem.IsWindows() && _wmpAvailable) PlayWmp(_wmpUiClick!);
        else SpawnLinux("ui_click.mp3");
    }

    public void PlayFrightened()
    {
        if (IsMuted) return;
        if (OperatingSystem.IsWindows() && _wmpAvailable)
        {
            try
            {
                _wmpFrightened!.controls.stop();
                _wmpFrightened!.controls.play();
            }
            catch { }
        }
        else
        {
            StopFrightened();
            _frightenedProcess = SpawnLinuxLoop("frightened.mp3");
        }
    }

    public void StopFrightened()
    {
        if (OperatingSystem.IsWindows() && _wmpAvailable)
        {
            try { _wmpFrightened!.controls.stop(); } catch { }
        }
        else
        {
            try { _frightenedProcess?.Kill(); } catch { }
            _frightenedProcess = null;
        }
    }
}
