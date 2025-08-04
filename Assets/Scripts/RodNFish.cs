using System.Collections;
using UnityEngine;

public class RodNFish : MonoBehaviour
{
    public static float fishY;
    public float fishVelY;
    public float timer = 2f;
    public GameObject fish;
    public float growSpeed = 0.1f;
    public float fishGrowSpeed = 1f;
    public float fishSize = 1f;
    public float fishSizeM = 1f;
    public float fishSizeX = 0f;
    public float fishSpd = 2f;
    public float fishSrpd = 3f;
    public float moveSpeed = 1f;
    public float growLimit = 2.5f;
    public bool spawned = false;
    public static RodNFish instance;
    private void Awake()
    {
        instance = this;
    }

    public void Spawn(bool Exit)
    {
        StartCoroutine(Exit ? unspawn() : spawn());
    }

    private void Update()
    {
        if (!Player.Instance.initialStop && spawned)
        {
            timer -= Time.deltaTime;
            fish.transform.localPosition += new Vector3(0, fishVelY * Time.deltaTime * fishSpd, 0);
            fish.transform.localPosition = new Vector3(fish.transform.localPosition.x, Mathf.Clamp(fish.transform.localPosition.y, -fishSrpd, fishSrpd), fish.transform.localPosition.z);
            if (timer < 0)
            {
                timer = Random.Range(3f, 10f);
                fishVelY = Random.Range(-1f, 1f);
            }
            fishY = fish.transform.localPosition.y;
        }
        fish.transform.localScale = Vector3.one * Mathf.Lerp(fish.transform.localScale.x, fishSize * fishSizeM * fishSizeX, 0.5f);
    }
    private IEnumerator spawn()
    {
        while (transform.localScale.y < growLimit)
        {
            transform.localScale += new Vector3(0, Time.deltaTime * growSpeed, 0);
            yield return null;
        }
        fishSizeM = 1f;
        spawned = true;

    }
    private IEnumerator unspawn()
    {
        spawned = false;
        while (transform.localScale.y > 0)
        {
            transform.localScale -= new Vector3(0, Time.deltaTime * growSpeed, 0);
            yield return null;
        }
        transform.localScale = new Vector3(transform.localScale.x, 0, transform.localScale.z);
        fishSizeM = 0f;
    }
}
