using UnityEngine;
using System.Collections;
using System;

using System.Collections.Generic;

using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = UnityEngine.Random;


public enum Target
{
    SCORE,
    COLLECT,
    INGREDIENT,
    BLOCKS
}

public enum LIMIT
{
    MOVES,
    TIME
}

public enum Ingredients
{
    None = 0,
    Ingredient1,
    Ingredient2
}

public enum CollectItems
{
    None = 0,
    Item1,
    Item2,
    Item3,
    Item4,
    Item5,
    Item6
}

public enum RewardedAdsType
{
    GetLifes,
    GetGems,
    GetGoOn
}

public class InitScript : MonoBehaviour
{
    public static InitScript Instance;
    public static int openLevel;


    public static float RestLifeTimer;
    public static string DateOfExit;
    public static DateTime today;
    public static DateTime DateOfRestLife;
    public static string timeForReps;
    private static int Lifes;

    bool loginForSharing;

    public RewardedAdsType currentReward;

    public static int lifes
    {
        get
        {
            return InitScript.Lifes;
        }
        set
        {
            InitScript.Lifes = value;
        }
    }

    public int CapOfLife = 5;
    public float TotalTimeForRestLifeHours = 0;
    public float TotalTimeForRestLifeMin = 15;
    public float TotalTimeForRestLifeSec = 60;
    public int FirstGems = 20;
    public static int Gems;
    public static int waitedPurchaseGems;
    public List<AdEvents> adsEvents = new List<AdEvents>();

    public static bool sound = false;
    public static bool music = false;
    public int ShowChartboostAdsEveryLevel;
    public int ShowAdmobAdsEveryLevel;
    public int dailyRewardedFrequency;//2.2.2
    public RewardedAdsTime dailyRewardedFrequencyTime;//2.2.3
    public int[] dailyRewardedShown;
    public DateTime[] dailyRewardedShownDate;
    private bool leftControl;
    public string admobUIDAndroid;
    public string admobUIDIOS;
    public string admobRewardedUIDAndroid;
    public string admobRewardedUIDIOS;
    public bool LoginEnable;

    public int ShowRateEvery;
    public string RateURL;
    private GameObject rate;
    public int rewardedGems = 5;
    public bool losingLifeEveryGame;
    public static Sprite profilePic;
    public GameObject facebookButton;
    private string lastResponse = string.Empty;

    protected string LastResponse
    {
        get
        {
            return this.lastResponse;
        }

        set
        {
            this.lastResponse = value;
        }
    }

    private string status = "Ready";
    public string RateURLIOS; //2.1.5

    protected string Status
    {
        get
        {
            return this.status;
        }

        set
        {
            this.status = value;
        }
    }

    // Use this for initialization
    void Awake()
    {
        Application.targetFrameRate = 60;
        Instance = this;
        RestLifeTimer = PlayerPrefs.GetFloat("RestLifeTimer");
        DateOfExit = PlayerPrefs.GetString("DateOfExit", "");
        print(DateOfExit);
        Gems = PlayerPrefs.GetInt("Gems");
        lifes = PlayerPrefs.GetInt("Lifes");
        {//2.2.2 rewarded limit
            dailyRewardedShown = new int[Enum.GetValues(typeof(RewardedAdsType)).Length];
            dailyRewardedShownDate = new DateTime[Enum.GetValues(typeof(RewardedAdsType)).Length];
            for (int i = 0; i < dailyRewardedShown.Length; i++)
            {
                dailyRewardedShown[i] = PlayerPrefs.GetInt(((RewardedAdsType)i).ToString());
                dailyRewardedShownDate[i] = DateTimeManager.GetLastDateTime(((RewardedAdsType)i).ToString());
            }
        }
        if (PlayerPrefs.GetInt("Lauched") == 0)
        {    //First lauching
            lifes = CapOfLife;
            PlayerPrefs.SetInt("Lifes", lifes);
            Gems = FirstGems;
            PlayerPrefs.SetInt("Gems", Gems);
            PlayerPrefs.SetInt("Music", 1);
            PlayerPrefs.SetInt("Sound", 1);

            PlayerPrefs.SetInt("Lauched", 1);
            PlayerPrefs.Save();
        }
        rate = Instantiate(Resources.Load("Prefabs/Rate")) as GameObject;
        rate.SetActive(false);
        rate.transform.SetParent(GameObject.Find("CanvasGlobal").transform);
        rate.transform.localPosition = Vector3.zero;
        rate.GetComponent<RectTransform>().anchoredPosition = (Resources.Load("Prefabs/Rate") as GameObject).GetComponent<RectTransform>().anchoredPosition;
        rate.transform.localScale = Vector3.one;

        if (gameObject.GetComponent<AspectCamera>() == null) gameObject.AddComponent<AspectCamera>().map = FindObjectOfType<LevelsMap>().transform.Find("map_background_01").GetComponent<SpriteRenderer>().sprite; //gameObject.AddComponent<AspectCamera>().topPanel = GetComponent<LevelManager>().Level.transform.Find("Canvas/Panel/Panel/Panel").GetComponent<RectTransform>();//2.2.2

        GameObject.Find("Music").GetComponent<AudioSource>().volume = PlayerPrefs.GetInt("Music");
        SoundBase.Instance.GetComponent<AudioSource>().volume = PlayerPrefs.GetInt("Sound");

        Transform canvas = GameObject.Find("CanvasGlobal").transform;
        foreach (Transform item in canvas)
        {
            item.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
            leftControl = true;
        if (Input.GetKeyUp(KeyCode.LeftControl))
            leftControl = false;

        if (Input.GetKeyUp(KeyCode.U))
        {
            for (int i = 1; i < GameObject.Find("Levels").transform.childCount; i++)
            {
                SaveLevelStarsCount(i, 1);
            }

        }
    }

    public void SaveLevelStarsCount(int level, int starsCount)
    {
        Debug.Log(string.Format("Stars count {0} of level {1} saved.", starsCount, level));
        PlayerPrefs.SetInt(GetLevelKey(level), starsCount);

    }

    private string GetLevelKey(int number)
    {
        return string.Format("Level.{0:000}.StarsCount", number);
    }


    public bool RewardedReachedLimit(RewardedAdsType type)//2.2.2
    {
        if (dailyRewardedFrequency == 0) return false;
        dailyRewardedShown[(int)type] = PlayerPrefs.GetInt(type.ToString());
        if (!DateTimeManager.IsPeriodPassed(type.ToString())) return true;
        if (dailyRewardedFrequency > 0 && dailyRewardedShown[(int)type] >= dailyRewardedFrequency) return true;
        dailyRewardedShown[(int)type]++;
        PlayerPrefs.SetInt(type.ToString(), dailyRewardedShown[(int)type]);
        if (dailyRewardedShown[(int)type] >= dailyRewardedFrequency) DateTimeManager.SetDateTimeNow(type.ToString());
        PlayerPrefs.Save();

        return false;
    }

    public void ShowRate()
    {
        rate.SetActive(true);
    }


    void CheckRewardedAds()
    {
        RewardIcon reward = GameObject.Find("CanvasGlobal").transform.Find("Reward").GetComponent<RewardIcon>();
        if (currentReward == RewardedAdsType.GetGems)
        {
            reward.SetIconSprite(0);

            reward.gameObject.SetActive(true);
            AddGems(rewardedGems);
            GameObject.Find("CanvasGlobal").transform.Find("GemsShop").GetComponent<AnimationManager>().CloseMenu();
        }
        else if (currentReward == RewardedAdsType.GetLifes)
        {
            reward.SetIconSprite(1);
            reward.gameObject.SetActive(true);
            RestoreLifes();
            GameObject.Find("CanvasGlobal").transform.Find("LiveShop").GetComponent<AnimationManager>().CloseMenu();
        }
        else if (currentReward == RewardedAdsType.GetGoOn)
        {
            GameObject.Find("CanvasGlobal").transform.Find("PreFailed").GetComponent<AnimationManager>().GoOnFailed();
        }

    }

    public void SetGems(int count)
    {
        Gems = count;
        PlayerPrefs.SetInt("Gems", Gems);
        PlayerPrefs.Save();
    }


    public void AddGems(int count)
    {
        Gems += count;
        PlayerPrefs.SetInt("Gems", Gems);
        PlayerPrefs.Save();
    }

    public void SpendGems(int count)
    {
        SoundBase.Instance.GetComponent<AudioSource>().PlayOneShot(SoundBase.Instance.cash);
        Gems -= count;
        PlayerPrefs.SetInt("Gems", Gems);
        PlayerPrefs.Save();
    }


    public void RestoreLifes()
    {
        lifes = CapOfLife;
        PlayerPrefs.SetInt("Lifes", lifes);
        PlayerPrefs.Save();
    }

    public void AddLife(int count)
    {
        lifes += count;
        if (lifes > CapOfLife)
            lifes = CapOfLife;
        PlayerPrefs.SetInt("Lifes", lifes);
        PlayerPrefs.Save();
    }

    public int GetLife()
    {
        if (lifes > CapOfLife)
        {
            lifes = CapOfLife;
            PlayerPrefs.SetInt("Lifes", lifes);
            PlayerPrefs.Save();
        }
        return lifes;
    }

    public void PurchaseSucceded()
    {
        AddGems(waitedPurchaseGems);
        waitedPurchaseGems = 0;
    }

    public void SpendLife(int count)
    {
        if (lifes > 0)
        {
            lifes -= count;
            PlayerPrefs.SetInt("Lifes", lifes);
            PlayerPrefs.Save();
        }
        //else
        //{
        //    GameObject.Find("Canvas").transform.Find("RestoreLifes").gameObject.SetActive(true);
        //}
    }

    public void BuyBoost(BoostType boostType, int price, int count)
    {
        PlayerPrefs.SetInt("" + boostType, count);
        PlayerPrefs.Save();
    }

    public void SpendBoost(BoostType boostType)
    {
        PlayerPrefs.SetInt("" + boostType, PlayerPrefs.GetInt("" + boostType) - 1);
        PlayerPrefs.Save();
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            if (RestLifeTimer > 0)
            {
                PlayerPrefs.SetFloat("RestLifeTimer", RestLifeTimer);
            }
            PlayerPrefs.SetInt("Lifes", lifes);
            PlayerPrefs.SetString("DateOfExit", DateTime.Now.ToString());
            PlayerPrefs.Save();
        }
    }

    void OnApplicationQuit()
    {   //1.4  added 
        if (RestLifeTimer > 0)
        {
            PlayerPrefs.SetFloat("RestLifeTimer", RestLifeTimer);
        }
        PlayerPrefs.SetInt("Lifes", lifes);
        PlayerPrefs.SetString("DateOfExit", DateTime.Now.ToString());
        PlayerPrefs.Save();
    }

    public void OnLevelClicked(object sender, LevelReachedEventArgs args)
    {
        if (EventSystem.current.IsPointerOverGameObject(-1))
            return;
        if (!GameObject.Find("CanvasGlobal").transform.Find("MenuPlay").gameObject.activeSelf && !GameObject.Find("CanvasGlobal").transform.Find("GemsShop").gameObject.activeSelf && !GameObject.Find("CanvasGlobal").transform.Find("LiveShop").gameObject.activeSelf && !GameObject.Find("CanvasGlobal").transform.Find("Settings").gameObject.activeSelf)  //2.1.6
        {
            PlayerPrefs.SetInt("OpenLevel", args.Number);
            PlayerPrefs.Save();
            LevelManager.THIS.MenuPlayEvent();
            LevelManager.THIS.LoadLevel();
            openLevel = args.Number;
            GameObject.Find("CanvasGlobal").transform.Find("MenuPlay").gameObject.SetActive(true);
        }
    }

    void OnEnable()
    {
        LevelsMap.LevelSelected += OnLevelClicked;
    }

    void OnDisable()
    {
        LevelsMap.LevelSelected -= OnLevelClicked;

        PlayerPrefs.SetFloat("RestLifeTimer", RestLifeTimer);
        PlayerPrefs.SetInt("Lifes", lifes);
        PlayerPrefs.SetString("DateOfExit", DateTime.Now.ToString());
        PlayerPrefs.Save();

    }
}
