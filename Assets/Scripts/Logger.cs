using UnityEngine;
using UnityEngine.Networking;

public class Logger : MonoBehaviour
{
    public static int Enabled = 0;
    private void Start()
    {
        Enabled = PlayerPrefs.GetInt("LogEnabled", 0);
    }
    public static void Log(string text) { }
    public static void LogImp(string text) { }
    public static void LogImpToChannel(string text, string channel) { }

    public static Logger instance;
    void Awake() => instance = this;
}
