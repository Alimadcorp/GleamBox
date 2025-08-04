using AYellowpaper.SerializedCollections;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
public class GameManager : MonoBehaviour
{
    public enum GameMode { Flappy, Clockwise, AntiClockwise, Fish };
    public GameMode gameMode;
    public static GameManager Instance;
    private float sinceGameModeChange = 0f;
    private float sinceWallSpawn = 0f;
    public float time = 0f;
    [Range(0f, 10f)]
    public static int Score = 0;
    public float Speed = 0.25f;
    public float TargetSpeed = 0.25f;
    public float powerupDuration = 1f;
    public int switchThreshold = 1000;
    public float rateOfIncrease;
    public int Blobs = 0;
    public int NetBlobs = 0;

    [Header("Parameters")]
    public float wallSpeed, opening, speedFactor = 1.0f, multiplier = 1.0f, multiplierUpSpeed = 1f;

    [Header("Object References")]
    public GameObject bigDaddy, ScoreFade;
    public Transform scoreFadeSource;
    public GameObject askUsername;
    public FlappyWallSpawner flappyWallSpawner;
    public TextMeshProUGUI multiText;
    public AudioClip moreScore;
    public AudioSource music;
    public GameObject switchText;
    public GameObject comboAdd;
    public GameObject comboSpawner;
    public TextMeshProUGUI hintText;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI blobText;
    public TextMeshProUGUI modeText;
    public TextMeshProUGUI comboText;
    public GameObject blob;
    public Light2D bgLight;
    public float comboMultiplier = 1;
    public int boxesPerSpawn = 2;
    public int isLucky = 0;
    public int blobAmt = 0;
    public bool hintVisible = true;
    public bool titleVisible = true;
    public float SpeedMiniFishFactor;
    public bool Paused = true;
    private int scoreSinceChange = 0;
    public int powerMultiplier = 1;
    public int speedMultiplier = 1;
    public int blobWorth = 1;
    public int comboBreaker = 3;
    public float xs, ys;
    public bool over = false;
    public float alpha = 1;
    public Image[] imgs;
    public Color[] GameModeColors;
    public Color[] comboColors;
    public CanvasGroup scores;

    [SerializedDictionary("ID", "Gradient")]
    public SerializedDictionary<string, Gradient> Trails;

    private void Awake()
    {
        Blobs = PlayerPrefs.GetInt("Blobs");
        NetBlobs = PlayerPrefs.GetInt("NetBlobs");
        if (Instance != null && Instance != this)
        {

            Destroy(gameObject);
            return;
        }
        Instance = this;
        Time.timeScale = 1f;
        Score = 0;
        Global.newSession = false;
    }
    private void Start()
    {
        SetGamemode(gameMode);
        blobText.text = Blobs.ToString();
        Logger.LogImp("Session start (" + Application.platform.ToString() + "): " + PlayerPrefs.GetString("myUsername"));
        SetTrail(PlayerPrefs.GetString("myTrail", "default"));
    }
    public void SetTrail(string id)
    {
        Gradient gradient = Trails[id];
        Player.Instance.trail.colorGradient = gradient;
        PlayerPrefs.SetString("myTrail", id);
        PlayerPrefs.Save();
    }
    public void AddPowerup(string name)
    {
        if (name == "2x")
        {
            Powerups.instance.AddPowerup(name, 10f * powerupDuration);
            powerMultiplier *= 2;
        }
        else if (name == "4x")
        {
            Powerups.instance.AddPowerup(name, 8f * powerupDuration);
            powerMultiplier *= 4;
        }
        else if (name == "slow")
        {
            Powerups.instance.AddPowerup("Slow Mo", 5f * powerupDuration);
            speedMultiplier++;
        }
        else if (name == "luck")
        {
            Powerups.instance.AddPowerup("Luck", 5f * powerupDuration);
            isLucky++;
        }
        AddScore(50, "Collected Powerup");
    }
    public void RemovePowerup(string name)
    {
        if (name == "2x")
        {
            powerMultiplier /= 2;
        }
        else if (name == "4x")
        {
            powerMultiplier /= 4;
        }
        else if (name == "Slow Mo")
        {
            speedMultiplier--;
        }
        else if (name == "Luck")
        {
            isLucky--;
        }
    }
    void FixedUpdate()
    {
        UpdateHintVisibility();
        if (!Paused)
        {
            if (!over)
            {
                Time.timeScale = Mathf.Sqrt((speedMultiplier > 1 ? 0.6f : 1f));
                TargetSpeed += Time.fixedDeltaTime * rateOfIncrease;
                Speed = Mathf.Lerp(Speed, TargetSpeed * (speedMultiplier > 1 ? 0.6f : 1f), 0.2f);
                music.pitch = Mathf.Lerp(music.pitch, (speedMultiplier > 1 ? 0.6f : 1f), 0.2f);
            }
            UpdateMultiplier();
            UpdateTimers();
            HandleWallSpawning();
        }
    }

    private void UpdateHintVisibility()
    {
        hintText.color = new Color(1, 1, 1, Mathf.Lerp(hintText.color.a, hintVisible ? 1f : 0f, 0.05f));
        titleText.color = new Color(1, 1, 1, Mathf.Lerp(titleText.color.a, titleVisible ? 1f : 0f, 0.05f));
        if (alpha > 0.00001f)
        {
            alpha = Mathf.Lerp(alpha, titleVisible ? 1f : 0f, 0.05f);
            for (int i = 0; i < imgs.Length; i++)
            {
                imgs[i].color = new Color(imgs[i].color.r, imgs[i].color.g, imgs[i].color.b, alpha);
            }
            scores.alpha = 1 - alpha;
        }
        else
        {
            alpha = 0;
            for (int i = 0; i < imgs.Length; i++)
            {
                imgs[i].gameObject.GetComponent<Button>().interactable = false;
            }
            scores.alpha = 1;
        }
    }

    private void UpdateMultiplier()
    {
        multiplier += multiplierUpSpeed * Time.fixedDeltaTime;
        if (multiplier >= 1.5f)
        {
            multiText.text = $"x{(multiplier * powerMultiplier * comboMultiplier):F1}";
        }
    }

    private void UpdateTimers()
    {
        sinceWallSpawn += Time.fixedDeltaTime * (speedMultiplier > 1 ? 0.6f : 1f);
        time += Time.fixedDeltaTime;
        sinceGameModeChange += Time.fixedDeltaTime;
        comboText.text = $"x{comboMultiplier} Combo";
        blobText.text = Blobs.ToString();
        comboText.GetComponent<ComboText>().SetColor(comboColors[(int)comboMultiplier - 1], true);

        if (time > 10f / multiplier)
        {
            AddScore(100, "Time Bonus");
            time = 0;
        }
    }

    private void HandleWallSpawning()
    {
        if (gameMode == GameMode.Flappy && sinceGameModeChange > 2f && sinceWallSpawn > 1f)
        {
            sinceWallSpawn = Random.Range(-3f, -1f);
            sinceWallSpawn += Mathf.Clamp((multiplier - 1f) / 3, 0f, 1f);
            flappyWallSpawner.Spawn(
                wallSpeed * Mathf.Clamp(multiplier / 4, 1, 2),
                false,
                opening * Random.Range(0.7f, 1.3f) / Mathf.Clamp(multiplier / 4, 1, 2)
            );
        }
        if ((gameMode == GameMode.AntiClockwise || gameMode == GameMode.Clockwise) && blobAmt < boxesPerSpawn)
        {
            Blob newBlob = Instantiate(blob, RandomTransform(), Quaternion.identity, bigDaddy.transform).GetComponent<Blob>();
            int r = (isLucky > 0) ? Random.Range(1, 3) : Random.Range(0, 15);
            if (r == 1)
            {
                int random = (int)Random.Range(0, 4);
                if (random == 3 && (isLucky > 0)) random--;
                newBlob.MakePowerup(Blob.intToId(random));
            }
            blobAmt++;
        }
    }

    private Vector3 RandomTransform()
    {
        return (new Vector3(Random.Range(-xs, xs), Random.Range(-ys, ys), -0.5f) + bigDaddy.transform.position);
    }

    public void SetGamemode(GameMode _gameMode)
    {
        Player.Instance.coolDown = 1f;
        Player.Instance.direction = 0;
        Player.Instance.rb.gravityScale = 0;
        Player.Instance.rb.linearVelocity = Vector2.zero;
        Player.Instance.initialStop = true;
        Player.Instance.GetComponent<SpriteRenderer>().color = GameModeColors[(int)_gameMode];
        Player.Instance.GetComponentInChildren<Light2D>().color = GameModeColors[(int)_gameMode];
        bgLight.color = GameModeColors[(int)_gameMode];
        opening = Mathf.Clamp(opening - 2, 9, 20);

        if (gameMode == GameMode.Clockwise || gameMode == GameMode.AntiClockwise)
        {
            blobAmt = 0;
        }
        if (gameMode == GameMode.Fish)
        {
            Player.Instance.FishMode(true);
            RodNFish.instance.Spawn(true);
        }
        gameMode = _gameMode;
        sinceGameModeChange = 0f;

        TextMeshPro tmp;
        switch (gameMode)
        {

            case GameMode.Clockwise:
                BigDaddy.moveDirection = Vector3.zero;
                Paused = true;
                tmp = Instantiate(switchText, bigDaddy.transform.position + Vector3.down * 3, Quaternion.identity, bigDaddy.transform).GetComponent<TextMeshPro>();
                tmp.text = "Snake\nMode";
                modeText.text = "Snake Mode";
                hintVisible = true;
                Player.Instance.reflectionPercentage = 0f;
                hintText.text = "Tap to switch direction";
                EndBlobs();
                Invoke("EndBlobs", 1f);
                break;
            case GameMode.AntiClockwise:
                BigDaddy.moveDirection = Vector3.zero;
                Paused = true;
                tmp = Instantiate(switchText, bigDaddy.transform.position + Vector3.down * 3, Quaternion.identity, bigDaddy.transform).GetComponent<TextMeshPro>();
                tmp.text = "Snake\nMode\n(Anti Clockwise)";
                modeText.text = "Snake Mode (Anti Clockwise)";
                hintVisible = true;
                Player.Instance.reflectionPercentage = 0f;
                hintText.text = "Tap to switch directions";
                EndBlobs();
                Invoke("EndBlobs", 1f);
                break;
            case GameMode.Flappy:
                BigDaddy.moveDirection = Vector3.right * 13;
                Paused = true;
                Player.Instance.initialStop = true;
                Player.Instance.ResetPosition();
                tmp = Instantiate(switchText, bigDaddy.transform.position + Vector3.up * 3, Quaternion.identity, bigDaddy.transform).GetComponent<TextMeshPro>();
                tmp.text = "Flappy\nMode";
                modeText.text = "Flappy Mode";
                hintText.text = "Tap to jump";
                hintVisible = true;
                EndBlobs();
                Invoke("EndBlobs", 1f);
                break;
            case GameMode.Fish:
                BigDaddy.moveDirection = Vector3.right * 30;
                Paused = true;
                Player.Instance.initialStop = true;
                Player.Instance.FishMode(false);
                RodNFish.instance.Spawn(false);
                tmp = Instantiate(switchText, bigDaddy.transform.position + Vector3.up * 3, Quaternion.identity, bigDaddy.transform).GetComponent<TextMeshPro>();
                tmp.text = "Fish\nMode";
                modeText.text = "Fish Mode";
                hintText.text = "Hold to lift, keep the bar aligned to the fish";
                hintVisible = true;
                EndBlobs();
                Invoke("EndBlobs", 1f);
                break;
        }
    }
    private void EndBlobs()
    {

        Blob[] blobs = bigDaddy.GetComponentsInChildren<Blob>();
        foreach (var blob in blobs)
        {
            if (blob != null) blob.Collect();
        }
    }

    public void NextGameMode()
    {
        scoreSinceChange = 0;
        if (gameMode == GameMode.Flappy)
        {
            DestroyAllPipes();
        }
        sinceGameModeChange = 0f;
        scoreSinceChange = 0;
        boxesPerSpawn++;
        if (comboMultiplier > 1)
        {
            Player.Instance.sinceLastCollect = 0;
        }
        int current = (int)gameMode;
        if (gameMode == GameMode.Fish)
        {
            RodNFish.instance.fishSizeX = 0f;
        }
        int next = 3;
        do { next = Random.Range(0, 4); }
        while (next == current);
        GameMode nextMode = (GameMode)next;
        if (nextMode == GameMode.Fish)
        {
            RodNFish.instance.fishSizeX = 1f;
        }
        SetGamemode(nextMode);
    }
    public void NextGameMode(bool bruh)
    {
        scoreSinceChange = 0;
        if (gameMode == GameMode.Flappy)
        {
            DestroyAllPipes();
        }
        sinceGameModeChange = 0f;
        scoreSinceChange = 0;
        boxesPerSpawn++;
        if (comboMultiplier > 1)
        {
            Player.Instance.sinceLastCollect = 0;
        }
        GameMode nextMode = GameMode.Clockwise;
        SetGamemode(nextMode);
    }

    public void AddScore(int amt, string source)
    {
        if (over) return;
        if (source == "Blob") blobAmt--;
        titleVisible = false;
        blobText.text = Blobs.ToString();
        int scoreToAdd = (int)(amt * multiplier * powerMultiplier * comboMultiplier);
        Score += scoreToAdd;
        scoreSinceChange += (int)(amt * multiplier);

        if (amt > 5)
        {
            GameObject scr = Instantiate(ScoreFade, scoreFadeSource.position, scoreFadeSource.rotation, bigDaddy.transform);
            scr.GetComponent<TextMeshPro>().text = $"+{scoreToAdd} {source}";
        }

        if (amt >= 100 && moreScore != null)
        {
            AudioSource.PlayClipAtPoint(moreScore, transform.position, 0.2f);
        }

        if (scoreSinceChange > switchThreshold * multiplier)
        {
            NextGameMode();
        }
    }
    void DestroyAllPipes()
    {
        foreach (FlappyWall wall in bigDaddy.GetComponentsInChildren<FlappyWall>())
        {
            wall.Blow();
        }
    }
    public void StartGame()
    {
        Paused = false;
        Player.Instance.rb.gravityScale = gameMode == GameMode.Flappy ? Player.Instance.initialG : 0;
    }
    public void AddCombo(float strength)
    {
        if (strength < 0.1f) { comboMultiplier = 1; return; }
        comboMultiplier += strength;
        GameObject newComboText = Instantiate(comboAdd, comboSpawner.transform.position, Quaternion.identity, bigDaddy.transform);
        newComboText.GetComponent<TextMeshPro>().text = $"x{comboMultiplier} Combo";
        newComboText.GetComponent<ComboText>().SetColor(comboColors[Mathf.Clamp((int)comboMultiplier - 1, 0, comboColors.Length - 1)], false);
    }
}