using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Settings;
using UnityEngine.Rendering.Universal;
using System;

public class GameManager : MonoBehaviour
{
    [SerializeField] List<CarData> carData;
    [SerializeField] GameSettings defaultSettings;
    public static GameManager gameManager { get; private set; }
    public static UIManager uiManager { get; private set; }
    public GameData gameData { get; private set; }
    public GameSettings gameSettings { get; private set; }
    public GameSettings DefaultSettings { get { return defaultSettings;} }
    public List<CarData> CarData { get { return carData; } }
    int[,] screenResolution;
    AsyncOperation load;
    Scene scene;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        screenResolution = new int[,]
        {
            { Screen.currentResolution.width, Screen.currentResolution.height },
            { 960, 540 },
            { 1280, 720 },
            { 1920, 1080 },
            { 2560, 1440 } 
        };
        LoadData();
        LoadSettings();
        gameManager = this;
        uiManager = GetComponentInChildren<UIManager>();
        uiManager.StartManager();
        LoadScene(1);
    }

    void OnEnable() => BroadcastMessages<bool>.AddListener(Messages.PAUSE, Pause);
    void OnDisable() => BroadcastMessages<bool>.RemoveListener(Messages.PAUSE, Pause);

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && scene.buildIndex > 1 && Time.timeScale == 1)
            BroadcastMessages<bool>.SendMessage(Messages.PAUSE, true);
    }

    void LoadData()
    {
        GameData _gameData;
        _gameData.carData = carData[0];
        gameData = _gameData;
    }
    void LoadSettings()
    {
        GameSettings _gameSettings = new GameSettings();
        if (PlayerPrefs.HasKey("Braking"))
        {
            _gameSettings.resolution = PlayerPrefs.GetInt("Resolution");
            _gameSettings.quality = PlayerPrefs.GetInt("Quality");
            _gameSettings.lightsTumbler = (KeyCode)PlayerPrefs.GetInt("Lights tumbler");
            _gameSettings.braking = (KeyCode)PlayerPrefs.GetInt("Braking");
            gameSettings = _gameSettings;
        }
        else
        {
            gameSettings = defaultSettings;
            SaveSettings();
        }
        Screen.SetResolution(
            screenResolution[gameSettings.resolution, 0],
            screenResolution[gameSettings.resolution, 1], true
        );
        QualitySettings.SetQualityLevel(gameSettings.quality);
    }
    void SaveSettings()
    {
        PlayerPrefs.SetInt("Resolution", gameSettings.resolution);
        PlayerPrefs.SetInt("Quality", gameSettings.quality);
        PlayerPrefs.SetInt("Lights tumbler", (int)gameSettings.lightsTumbler);
        PlayerPrefs.SetInt("Braking", (int)gameSettings.braking);
        PlayerPrefs.Save();
    }
    public void ResetSettings()
    {
        PlayerPrefs.DeleteAll();
        gameSettings = defaultSettings;
        SaveSettings();
    }

    void Pause(bool isPause)
    {
        Time.timeScale = isPause ? 0f : 1f;
        if (scene.buildIndex > 1)
        {
            Cursor.visible = isPause;
            Cursor.lockState = isPause ? CursorLockMode.None : CursorLockMode.Locked;
        }
    }

    public void SetData(GameData _gameData) => gameData = _gameData;
    public void SetSettings(GameSettings _gameSettings)
    {
        gameSettings = _gameSettings;
        Screen.SetResolution(
            screenResolution[gameSettings.resolution, 0],
            screenResolution[gameSettings.resolution, 1], true
        );
        QualitySettings.SetQualityLevel(gameSettings.quality);
        SaveSettings();
    }

    public void LoadScene(int scene)
    {
        load = SceneManager.LoadSceneAsync(scene);
        load.completed += LoadCompleted;
        StartCoroutine(ProgressLoad());
    }
    void LoadCompleted(AsyncOperation load)
    {
        scene = SceneManager.GetActiveScene();
        BroadcastMessages<bool>.SendMessage(Messages.PAUSE, false);
        uiManager.CloseLoadWindow(scene.buildIndex);
        load.completed -= LoadCompleted;
    }
    IEnumerator ProgressLoad()
    {
        while (!load.isDone)
        {
            float progress = load.progress * 100;
            uiManager.LoadScene((int)progress, "Loading scene: ");
            yield return null;
        }
    }

    public void ExitGame()
    {
        SaveSettings();
        Application.Quit();
    }
}
