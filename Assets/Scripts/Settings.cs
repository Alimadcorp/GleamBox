using AYellowpaper.SerializedCollections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

public class Settings : MonoBehaviour
{
    public Image volumeImg;
    public Sprite fullImg, noImg;
    public Slider volumeSlider;
    public Toggle consntToPolicy;
    public Toggle consntToSharing;
    public Toggle consntToSharingI;
    public TextMeshProUGUI credits;
    public TextMeshProUGUI fpsText;
    public GameObject blackbelt;
    public GameObject consent, panel;
    public GameObject shop, quit;
    public Button quitButton;
    public Button consentButton, exitButton;
    public Color unlocked, purchased, equipped;
    public Button[] trailButtons;
    public TextMeshProUGUI[] trailTexts;
    public TextMeshProUGUI currentAmt, reviewShopTxt;
    public Button reviewSubmitButton;
    public GameObject review;
    public TMP_InputField reviewInput;
    public enum ItemState { Locked, Unlocked, Purchased, Equipped };
    [SerializedDictionary("ID", "Price")]
    public SerializedDictionary<string, int> prices;
    [SerializedDictionary("ID", "State")]
    public SerializedDictionary<string, ItemState> states;

    public TextMeshProUGUI[] upgradeTexts;
    public TextMeshProUGUI[] upgradePriceTexts;
    public Button[] upgradeButtons;
    public int[] shownUpgradePrices;
    public int[] initialUpgradePrices;
    public float[] upgradeIncerement;
    public float[] priceIncrement;
    public int[] maxUpdates;
    public int[] updateLevels;
    public bool consentEnabled = false;
    void Start()
    {
        shownUpgradePrices = initialUpgradePrices.ToArray();
        consntToSharingI.isOn = PlayerPrefs.GetInt("Consent") == 1;
        if (PlayerPrefs.GetInt("Consent", -1) == -1 && consentEnabled)
        {
            consent.SetActive(true);
            panel.SetActive(true);
        }
        if (Global.graphEnabled)
        {
            Tayx.Graphy.GraphyManager.Instance.Enable();
        }
        else
        {
            Tayx.Graphy.GraphyManager.Instance.Disable();
        }
        credits.text = credits.text.Replace("{Version}", Application.version).Replace("{Username}", PlayerPrefs.GetString("myUsername")).Replace("{Score}", PlayerPrefs.GetInt("highScore").ToString()).Replace("{UnityVersion}", Application.unityVersion).Replace("{Name}", Application.productName).Replace("{Company}", Application.companyName).Replace("{SaveFolder}", Application.dataPath).Replace("{FrameRate}", Application.targetFrameRate.ToString()).Replace("{Language}", Application.systemLanguage.ToString()).Replace("{Genuine}", Application.genuine ? "Genuine Build" : "Insecure or Altered Build").Replace("{GUID}", Application.buildGUID).Replace("{Platform}", Application.platform.ToString()).Replace("{NetBlobs}", PlayerPrefs.GetInt("NetBlobs").ToString());
        Adjust();
        LoadUpdates();
    }
    private void Update()
    {
        fpsText.text = "Frame Rate: " + (int)(1f / Time.deltaTime);
    }
    private void SaveUpdates()
    {
        string toSave = "";
        for (int i = 0; i < updateLevels.Length; i++)
        {
            toSave += updateLevels[i].ToString() + ",";
        }
        PlayerPrefs.SetString("Upgrades", toSave);
        PlayerPrefs.Save();
    }
    private void Upgrade(int id, int level)
    {
        GameManager game = GameManager.Instance;
        switch (id)
        {
            case 0:
                game.multiplier = 1f + upgradeIncerement[id] * level;
                break;
            case 1:
                game.powerupDuration = 1f + upgradeIncerement[id] * level;
                break;
            case 2:
                game.blobWorth = 1 + (int)upgradeIncerement[id] * level;
                break;
            case 3:
                game.comboBreaker = 3 + (int)upgradeIncerement[id] * level;
                break;
        }
    }
    private float Upgrade(int id, int level, bool yessa)
    {
        float returnValue = 1f;
        switch (id)
        {
            case 0:
                returnValue = 1f + upgradeIncerement[id] * level;
                break;
            case 1:
                returnValue = 1f + upgradeIncerement[id] * level;
                break;
            case 2:
                returnValue = 1 + upgradeIncerement[id] * level;
                break;
            case 3:
                returnValue = 3 + (int)upgradeIncerement[id] * level;
                break;
        }
        return returnValue;
    }
    private void LoadUpdates()
    {
        string saved = PlayerPrefs.GetString("Upgrades");
        if (saved == "") return;
        string[] loaded = saved.Split(",");
        for (int i = 0; i < updateLevels.Length; i++)
        {
            updateLevels[i] = Int32.Parse(loaded[i]);
            shownUpgradePrices[i] = initialUpgradePrices[i] + (int)(updateLevels[i] * priceIncrement[i]);
            Upgrade(i, updateLevels[i]);
        }
    }
    public void SaveStates()
    {
        List<string> parts = new();
        foreach (var kv in states)
        {
            string key = UnityWebRequest.EscapeURL(kv.Key);
            string val = UnityWebRequest.EscapeURL(JsonUtility.ToJson(kv.Value));
            parts.Add(key + "=" + val);
        }
        PlayerPrefs.SetString("States", string.Join("&", parts));
        PlayerPrefs.Save();
    }

    public void LoadStates()
    {
        string data = PlayerPrefs.GetString("States");
        if (string.IsNullOrEmpty(data)) return;

        states.Clear();
        var pairs = data.Split('&');
        foreach (var pair in pairs)
        {
            var kv = pair.Split('=');
            if (kv.Length != 2) continue;

            string key = UnityWebRequest.UnEscapeURL(kv[0]);
            string val = UnityWebRequest.UnEscapeURL(kv[1]);
            states[key] = JsonUtility.FromJson<ItemState>(val);
        }
    }

    public void CancelExit()
    {
        Application.Quit();
    }
    public void onPolicyToggle(bool did)
    {
        consentButton.interactable = did;
    }
    public void Consent()
    {
        Logger.Enabled = consntToSharing.isOn ? 1 : 0;
        PlayerPrefs.SetInt("Consent", consntToSharing.isOn ? 1 : 0);
        PlayerPrefs.Save();
    }
    public void ConsentAgain(bool what)
    {
        Logger.Enabled = what ? 1 : 0;
        PlayerPrefs.SetInt("Consent", what ? 1 : 0);
        PlayerPrefs.Save();
    }
    public void OpenCredits()
    {
        blackbelt.SetActive(UnityEngine.Random.Range(0, 10) == 5 ? true : false);
    }
    public void ToggleFPS()
    {
        Global.graphEnabled = !Global.graphEnabled;
        if (Global.graphEnabled)
        {
            Tayx.Graphy.GraphyManager.Instance.Enable();
        }
        else
        {
            Tayx.Graphy.GraphyManager.Instance.Disable();
        }
    }
    void Adjust()
    {
        AudioListener.volume = PlayerPrefs.GetFloat("prefs_volume", 1f);
        volumeSlider.value = AudioListener.volume;
        if (volumeSlider.value < 0.0001f)
        {
            volumeImg.sprite = noImg;
        }
    }
    public void SetVolume(float volume)
    {
        volumeImg.sprite = volumeSlider.value < 0.0001f ? noImg : fullImg;
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat("prefs_volume", volume);
        PlayerPrefs.Save();
    }
    public void ToggleShop()
    {
        shop.SetActive(!shop.activeInHierarchy);
    }
    public void Quit()
    {
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            Logger.Log("Attempt to exit a web build: " + PlayerPrefs.GetString("myUsername"));
            StartCoroutine(ExitCoroutine());
        }
        else
        {
            Application.Quit();
        }
    }
    private IEnumerator ExitCoroutine()
    {
        float t = 0;
        Color initial = quitButton.GetComponent<Image>().color;
        while (t < 1)
        {
            t += Time.unscaledDeltaTime;
            quitButton.GetComponent<Image>().color = Color.Lerp(initial, new Color(1, 0.2f, 0.2f), t);
            yield return null;
        }
        quit.SetActive(true);
    }
    public void Logging(bool enable)
    {
        Logger.Enabled = enable ? 1 : 0;
        PlayerPrefs.SetInt("LogEnabled", enable ? 1 : 0);
        PlayerPrefs.SetInt("Consent", 1);
        PlayerPrefs.Save();
    }
    public void Redirect(string url)
    {
        Application.OpenURL(url);
    }

    public void UpdateShopUI()
    {
        LoadStates();
        LoadUpdates();
        if (PlayerPrefs.GetInt("review") == 1)
        {
            reviewShopTxt.text = "Leave a review";
        }
        GameManager.Instance.blobText.text = GameManager.Instance.Blobs.ToString();
        currentAmt.text = $"{GameManager.Instance.Blobs}";

        for (int i = 0; i < updateLevels.Length; i++)
        {
            upgradeTexts[i].text = $"{Upgrade(i, updateLevels[i], true):F2}";
            upgradeButtons[i].interactable = shownUpgradePrices[i] <= GameManager.Instance.Blobs;
            upgradePriceTexts[i].text = shownUpgradePrices[i].ToString();
            if (updateLevels[i] >= maxUpdates[i])
            {
                upgradeButtons[i].interactable = false;
                upgradePriceTexts[i].text = "Max";
            }
        }

        for (int i = 0; i < trailButtons.Length; i++)
        {
            if (states.ElementAt(i).Value == ItemState.Unlocked)
            {
                trailButtons[i].interactable = GameManager.Instance.Blobs >= prices.ElementAt(i).Value;
                trailButtons[i].GetComponent<Image>().color = unlocked;
                trailTexts[i].text = prices.ElementAt(i).Value.ToString();
            }
            else if (states.ElementAt(i).Value == ItemState.Purchased)
            {
                trailButtons[i].interactable = true;
                trailTexts[i].text = "Equip";
                trailButtons[i].GetComponent<Image>().color = purchased;
            }
            else if (states.ElementAt(i).Value == ItemState.Equipped)
            {
                trailButtons[i].interactable = true;
                trailButtons[i].GetComponent<Image>().color = equipped;
                trailTexts[i].text = "Equipped";
            }
        }
    }
    public void updateItem(int id)
    {
        if (shownUpgradePrices[id] <= GameManager.Instance.Blobs)
        {
            GameManager.Instance.Blobs -= shownUpgradePrices[id];
            updateLevels[id]++;
            Logger.Log($"{PlayerPrefs.GetString("myUsername")} (Purchased Upgrade): {id} to level {updateLevels[id]} for price {shownUpgradePrices[id]}");
            shownUpgradePrices[id] += (int)priceIncrement[id];
            Upgrade(id, updateLevels[id]);
        }
        PlayerPrefs.SetInt("Blobs", GameManager.Instance.Blobs);
        PlayerPrefs.Save();
        SaveUpdates();
        UpdateShopUI();
    }
    public void buyItem(string itemId)
    {
        if (states[itemId] == ItemState.Unlocked)
        {
            if (prices[itemId] <= GameManager.Instance.Blobs)
            {
                GameManager.Instance.Blobs -= prices[itemId];
                for (int i = 0; i < states.Count; i++)
                {
                    if (states.ElementAt(i).Value == ItemState.Equipped)
                    {
                        states[states.ElementAt(i).Key] = ItemState.Purchased;
                        break;
                    }
                }
                states[itemId] = ItemState.Equipped;
                Logger.Log($"{PlayerPrefs.GetString("myUsername")} (Purchased Item): {itemId}");
                PlayerPrefs.SetInt("Blobs", GameManager.Instance.Blobs);
                PlayerPrefs.Save();
                GameManager.Instance.SetTrail(itemId);
            }
        }
        else if (states[itemId] == ItemState.Purchased)
        {
            for (int i = 0; i < states.Count; i++)
            {
                if (states.ElementAt(i).Value == ItemState.Equipped)
                {
                    states[states.ElementAt(i).Key] = ItemState.Purchased;
                    break;
                }
            }
            states[itemId] = ItemState.Equipped;
            GameManager.Instance.SetTrail(itemId);
        }
        SaveStates();
        UpdateShopUI();
    }
    public void SubmitReview()
    {
        Logger.LogImpToChannel("Review (" + PlayerPrefs.GetString("myUsername", "Anonymous()") + "): " + reviewInput.text, "gleamboxreview");
        reviewInput.text = "";
        if (PlayerPrefs.GetInt("review") == 0)
        {
            GameManager.Instance.NetBlobs += 500;
            GameManager.Instance.Blobs += 500;
            PlayerPrefs.SetInt("Blobs", GameManager.Instance.Blobs);
            PlayerPrefs.SetInt("NetBlobs", GameManager.Instance.NetBlobs);
        }
        PlayerPrefs.SetInt("review", 1);
        PlayerPrefs.Save();
        UpdateShopUI();
    }
    public void validateValue(string value)
    {
        reviewSubmitButton.interactable = value.Length > 30;
    }
}
