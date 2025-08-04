using UnityEngine;
using Dan.Main;
using TMPro;
using Dan.Models;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Linq;
public class LeaderboardManager : MonoBehaviour
{
    public TextMeshProUGUI loading;
    public TextMeshProUGUI error;
    public LeaderEntry[] ldarray;
    public LeaderEntry[] ldarray2;
    public GameObject parent, parent2;
    public TMP_InputField username;
    public Button ubutton;
    public GameObject UIParent, bg;
    private int scorer = 0;
    private bool submiting = false;
    public Button relButton;
    public Button mainButton;
    public Button secButton;
    public TextMeshProUGUI mainText;
    public TextMeshProUGUI secText;
    public void OpenLeaderboard(bool hi)
    {
        if (!hi)
        {
            SubmitEntry(scorer);
        }
        if (!gameObject.activeInHierarchy)
        {
            bg.SetActive(true);
            gameObject.SetActive(true);
            parent.SetActive(false);
            loading.gameObject.SetActive(true);
            error.gameObject.SetActive(false);
        }
        relButton.interactable = false;
        Leaderboards.Main.GetEntries(OnLoaded, OnError);
    }
    private void OnLoaded(Entry[] entries)
    {
        loading.gameObject.SetActive(false);
        parent.SetActive(true);
        relButton.interactable = true;

        for(int i = 0; i < entries.Length; i++)
        {
            entries[i].Blobs = Int32.Parse(entries[i].Extra.Split(":::::")[0]);
        }
        Entry[] sortedByBlobs = entries.OrderByDescending(e => e.Blobs).ToArray();
        for(int i = 0; i < sortedByBlobs.Length; i++)
        {
            sortedByBlobs[i].Rank = i + 1;
        }
        bool foundMine = false, foundMineInTop10 = false; int myEntryI = 0;
        bool foundMineBlob = false, foundMineInTop10Blob = false; int myEntryIBlob = 0;
        for (int i = 0; i < entries.Length; i++)
        {
            if (entries[i].IsMine())
            {
                foundMine = true;
                myEntryI = i;
                if (entries[i].Score != PlayerPrefs.GetInt("highScore"))
                {
                    if (!submiting)
                    {
                        SubmitEntry(PlayerPrefs.GetInt("highScore"));
                    }
                }
            }
        }
        for (int i = 0; i < sortedByBlobs.Length; i++)
        {
            if (sortedByBlobs[i].IsMine())
            {
                foundMineBlob = true;
                myEntryIBlob = i;
                if (sortedByBlobs[i].Blobs != PlayerPrefs.GetInt("NetBlobs"))
                {
                    if (!submiting)
                    {
                        SubmitEntry(PlayerPrefs.GetInt("highScore"));
                    }
                }
            }
        }
        if (foundMine)
        {
            if (PlayerPrefs.GetInt("highScore") < entries[myEntryI].Score || PlayerPrefs.GetInt("NetBlobs") < sortedByBlobs[myEntryIBlob].Blobs)
            {
                PlayerPrefs.SetInt("highScore", entries[myEntryI].Score);
                PlayerPrefs.SetString("myUsername", entries[myEntryI].Username);
                PlayerPrefs.Save();
            }
        }
        for (int i = 0; i < (entries.Length <= 11 ? entries.Length : 11); i++)
        {
            ldarray[i].SetEntry(entries[i].Username, entries[i].Rank, entries[i].Score, entries[i].IsMine());
            if (entries[i].IsMine())
            {
                foundMineInTop10 = true;
            }
        }
        for (int i = 0; i < (sortedByBlobs.Length <= 11 ? sortedByBlobs.Length : 11); i++)
        {
            ldarray2[i].SetEntry(sortedByBlobs[i].Username, i + 1, sortedByBlobs[i].Blobs, sortedByBlobs[i].IsMine());
            if (sortedByBlobs[i].IsMine())
            {
                foundMineInTop10Blob = true;
            }
        }
        int myEntryIndex = 10;
        int myEntryIndexBlob = 10;
        if (!foundMineInTop10 && foundMine)
        {
            ldarray[myEntryIndex].SetEntry(entries[myEntryI].Username, entries[myEntryI].Rank, entries[myEntryI].Score, true);
        }
        if (!foundMineInTop10Blob && foundMineBlob)
        {
            ldarray2[myEntryIndexBlob].SetEntry(sortedByBlobs[myEntryIBlob].Username, sortedByBlobs[myEntryIBlob].Rank, sortedByBlobs[myEntryIBlob].Blobs, true);
        }
        if (!foundMine)
        {
            if (!submiting && PlayerPrefs.GetInt("highScore") != 0 && PlayerPrefs.GetString("myUsername") != "")
            {
                SubmitEntry(PlayerPrefs.GetInt("highScore"));
            }
        }

    }
    private void OnError(string _error)
    {
        loading.gameObject.SetActive(false);
        parent.gameObject.SetActive(false);
        error.gameObject.SetActive(true);
        error.text = _error;
    }
    public void ProcessInput(string input)
    {
        ubutton.interactable = input.Trim() != "";
    }
    public void SetUsername()
    {
        if (PlayerPrefs.GetString("myUsername") == "")
        {
            Logger.Log("Connect: " + username + ", " + PlayerPrefs.GetInt("highScore"));
            PlayerPrefs.SetString("myUsername", username.text);
            PlayerPrefs.Save();
            if (PlayerPrefs.GetInt("highScore") > 0) SubmitEntry(-1);
            UIParent.SetActive(false);
        }
        else
        {
            string lastName = PlayerPrefs.GetString("myUsername");
            if (username.text.ToUpper() == "RESETDATA")
            {
                Logger.Log("RESETDATA: " + username + ", " + lastName + ", " + PlayerPrefs.GetInt("highScore"));
                DeleteEntry();
                PlayerPrefs.DeleteKey("highScore");
                PlayerPrefs.DeleteKey("myUsername");
                PlayerPrefs.Save();
            }
            PlayerPrefs.SetString("myUsername", username.text);
            PlayerPrefs.Save();
            if (PlayerPrefs.GetString("myUsername") != lastName)
            {
                Logger.Log("Change name: " + username.text + " to " + lastName + ", " + PlayerPrefs.GetInt("highScore"));
                if (PlayerPrefs.GetInt("highScore") > 0) SubmitEntry(PlayerPrefs.GetInt("highScore"));
            }
            else
            {
                if (PlayerPrefs.GetInt("highScore") > 0) SubmitEntry(PlayerPrefs.GetInt("highScore"));
            }
            UIParent.SetActive(false);
        }
    }
    public void SubmitEntry(int score)
    {
        if (PlayerPrefs.GetInt("highScore") == 0) return;
        submiting = true;
        if (score == -1) score = scorer;
        scorer = score;
        string usnm = PlayerPrefs.GetString("myUsername");
        if (usnm == null || usnm == "")
        {
            UIParent.SetActive(true);
        }
        else
        {
            if (username.text.ToUpper() == "RESETDATA") { PlayerPrefs.DeleteAll(); PlayerPrefs.Save(); SceneManager.LoadSceneAsync(0); return; }
            if (usnm.ToLower() == "delete entry") { DeleteEntry(); return; }
            if (usnm.ToLower() == "dont upload") return;
            Logger.Log("Submit Entry: " + usnm + ", " + score);
            Leaderboards.Main.UploadNewEntry(usnm, score, PlayerPrefs.GetInt("NetBlobs").ToString() + ":::::" + PlayerPrefs.GetString("history"), Grs);
        }
    }
    private void SES()
    {
        SubmitEntry(scorer);
    }
    public void OpenEdit()
    {
        UIParent.SetActive(true);
        username.text = PlayerPrefs.GetString("myUsername");
    }
    public void Grs(bool done)
    {
        if (!done)
        {
            Invoke("SES", 1f);
        }
        else
        {
            submiting = false;
            OpenLeaderboard(true);
        }
    }

    public void DeleteEntry()
    {
        Logger.Log("Delete Entry: " + PlayerPrefs.GetString("myUsername") + ", " + PlayerPrefs.GetString("highScore"));
        PlayerPrefs.SetString("myUsername", "");
        Leaderboards.Main.DeleteEntry(OnDelete);
    }
    private void OnDelete(bool yes)
    {
        OpenLeaderboard(true);
        SceneManager.LoadSceneAsync(0); return;
    }
    public void OpenLB(bool sec)
    {
        if (sec)
        {
            secButton.interactable = false;
            mainButton.interactable = true;
            parent.SetActive(false);
            parent2.SetActive(true);
        }
        else
        {
            secButton.interactable = true;
            mainButton.interactable = false;
            parent.SetActive(true);
            parent2.SetActive(false);
        }
    }
}