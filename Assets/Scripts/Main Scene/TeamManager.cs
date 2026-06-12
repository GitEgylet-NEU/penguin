using NohaSoftware.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

[RequireComponent(typeof(LevelSpawner))]
public class TeamManager : MonoBehaviour
{
    public static TeamManager instance;
    private void Awake()
    {
        instance = this;
        levelSpawner = GetComponent<LevelSpawner>();
    }

    public GameData gameData;

    [Header("Spawn options")]
    [SerializeField] GameObject penguinPrefab;
    public List<Penguin> penguins;
    [Tooltip("Két pingvin közötti távolság (Unity unitban)")] public float penguinDistance;
    [SerializeField] bool randomColors = true;

    [Header("Run")]
    public float initialRunSpeed;
    [Tooltip("Hányszorosára gyorsul a sebesség másodpercenként?")] public float runMultiplier = 0;
    public bool exponential;
    [SerializeField] public float runLength;
    public float runStartZ = float.PositiveInfinity;
    public bool run;
    [Tooltip("Milyen gyorsan fog előreszaladni a pingvin, ha az előtte lévő meghal?")] public float penguinCatchUpSpeed = 8f;

    [Header("Controls")]
    [SerializeField] string horizontalAxisName;
    public float horizontalSensitivity;

    [Header("Camera")]
    [SerializeField] CameraController cameraController;
    [SerializeField] float cameraOffset = 20f;

    [Header("UI")]
    [SerializeField] ProgressBar xpBar;
    [SerializeField] TextMeshProUGUI xpText;
    [SerializeField] LocalizedString newLevelString, newCharacterString, upgradePointsString, beatGameString, levelString;

    [Header("Visuals")]
    [SerializeField] GameObject tomatoPrefab;
    [SerializeField] float tomatoDuration;
    [SerializeField] ParticleSystem ketchupParticlePrefab;

    float setRunMultiplierToThis;
    LevelSpawner levelSpawner;

    private void Start()
    {
        // load or init layout
        if (!SaveManager.instance.LoadObject(Path.Combine(SaveManager.instance.layoutSavePath, BattleManager.instance.layoutName), out BattleLayout layout))
        {
            layout = new BattleLayout(gameData.columns, gameData.rows, BattleManager.Team.Player);
            layout.characterNames[2, 2] = "Penguin";
            if (!SaveManager.instance.SaveObject(Path.Combine(SaveManager.instance.layoutSavePath, BattleManager.instance.layoutName), layout))
            {
                Debug.LogError("can't init default layout");
                return;
            }
        }
        // pingvinek generálása
        int i = 0;
        foreach (var c in layout.GetCharacters().Shuffle().Select(x => gameData.GetPenguinData(x)))
        {
            GameObject obj = Instantiate(penguinPrefab, new Vector3(0, 0, -i * penguinDistance), Quaternion.identity);
            obj.name = c.name;
            //obj.transform.position = new Vector3(0, 0, -i * penguinDistance);
            if (c.slideSprite != null) obj.GetComponentInChildren<SpriteRenderer>().sprite = c.slideSprite;

            if (randomColors)
            {
                Color col = new Color(Random.Range(.4f, .8f), Random.Range(.4f, .8f), Random.Range(.4f, .8f));
                obj.GetComponentInChildren<SpriteRenderer>().color = col;
            }

            Penguin penguin = obj.GetComponent<Penguin>();
            penguin.id = i;
            penguins.Add(penguin);

            i++;
        }

        AudioManager.instance.StartBM();
        cameraController.followTransform = penguins[0].transform;
        cameraController.offset = cameraOffset;

        xpBar.min = SaveManager.instance.progressData.level >= 1 ? gameData.levelXPCosts[SaveManager.instance.progressData.level - 1] + 1 : 0f;
        xpBar.max = SaveManager.instance.progressData.level < gameData.levelXPCosts.Length - 1 ? gameData.levelXPCosts[SaveManager.instance.progressData.level + 1] : gameData.levelXPCosts[SaveManager.instance.progressData.level] + 1;
        xpBar.disappearOnZero = false;

        runLength = gameData.levelRunLength[SaveManager.instance.progressData.level];

        CheckLevels();

        setRunMultiplierToThis = runMultiplier;
        runMultiplier = 0f;

        run = true;
    }

    void CheckLevels()
    {
        if (SaveManager.instance.progressData.lastCheckedLevel != SaveManager.instance.progressData.level)
        {
            List<string> popupText = new();

            int levelDiff = SaveManager.instance.progressData.level - (SaveManager.instance.progressData.lastCheckedLevel + 1);
            if (levelDiff > 0)
            {
                levelString.Arguments = new object[] { SaveManager.instance.progressData.level - levelDiff + 1 };
                string oldLevel = levelString.GetLocalizedString();
                levelString.Arguments = new object[] { SaveManager.instance.progressData.level + 1 };
                string newLevel = levelString.GetLocalizedString();
                popupText.Add($"{oldLevel} -> {newLevel}");
            }

            int upPoints = 0;
            //leveled up
            for (int i = SaveManager.instance.progressData.lastCheckedLevel + 1; i < SaveManager.instance.progressData.level; i++)
            {
                upPoints += gameData.levelUpgradePointRewards[i + 1];
                if (gameData.levelUpgradeCharacterRewards.Length <= i) continue;
                PenguinData penguin = gameData.levelUpgradeCharacterRewards[i];
                if (penguin == null) continue;
                newCharacterString.Arguments = new object[] { penguin.LocalizedName };
                popupText.Add(newCharacterString.GetLocalizedString());
                SaveManager.instance.progressData.characterLevels.GetElement(penguin.name).Value = 0;
            }
            if (upPoints > 0)
            {
                upgradePointsString.Arguments = new object[] { upPoints };
                popupText.Add(upgradePointsString.GetLocalizedString());
            }
            SaveManager.instance.progressData.lastCheckedLevel = SaveManager.instance.progressData.level - 1;
            SaveManager.instance.SaveProgress();

            if (popupText.Any())
            {
                if (SaveManager.instance.progressData.level == 8) popupText.Add(beatGameString.GetLocalizedString());
                UIController.instance.SetPopup(newLevelString.GetLocalizedString(), string.Join("\n\n", popupText));
                AudioManager.instance.PlaySound("new level reached", AudioManager.instance.Mixer.UI);
            }
        }
    }

    //  private void FixedUpdate()
    //  {
    //      if (run)
    //{
    //          // mindegyik pingvin mozgatása előre
    //          foreach (Penguin penguin in penguins)
    //          {
    //              if (penguin == null) continue;
    //              penguin.transform.position += new Vector3(0, 0, penguin.speed * Time.fixedDeltaTime);
    //          }
    //      }
    //  }

    private void Update()
    {
        if (xpBar.isActiveAndEnabled)
        {
            xpBar.SetValue(SaveManager.instance.progressData.xp);

            if (SaveManager.instance.progressData.level < gameData.levelXPCosts.Length - 1)
                xpText.text = $"{SaveManager.instance.progressData.xp} / {gameData.levelXPCosts[SaveManager.instance.progressData.level + 1]}";
            else
                xpText.text = string.Empty;
        }

        if (run)
        {
            // all penguins are dead
            if (!penguins.Any())
            {
                Debug.Log("All penguins have died.");
                run = false;
                levelSpawner.enabled = false;
                AudioManager.instance.StopBM();
                AudioManager.instance.PlaySound("game over", AudioManager.instance.Mixer.SFX);
                //UIController.instance.ToggleDocument(UIController.instance.gameOverDoc,true);
                UIController.instance.EndGame(false, -1f);
                enabled = false;
                return;
            }

            // run leállítása, ha az első pingvin a végére ér
            if (penguins.FirstOrDefault().transform.position.z >= runStartZ + runLength + 5f)
            {
                Debug.Log("battle");
                run = false;
                ControlHandler.instance.canStrafe = false;

                UIController.instance.runProgressBar.gameObject.SetActive(false);

                BattleManager.instance.startZ = penguins.FirstOrDefault().transform.position.z - BattleManager.instance.length / 2f;
                BattleManager.instance.Initialize();
                foreach (var penguin in penguins)
                {
                    penguin.gameObject.SetActive(false);
                }
            }

            if (UIController.instance.runProgressBar.gameObject.activeInHierarchy) UIController.instance.runProgressBar.SetValue(penguins.First().transform.position.z - runStartZ);
        }
    }

    public void StartGame()
    {
        runMultiplier = setRunMultiplierToThis;
        xpBar.gameObject.SetActive(false);
        runStartZ = penguins.First().transform.position.z;

        UIController.instance.runProgressBar.max = runLength;
        UIController.instance.runProgressBar.SetValue(0f);
        UIController.instance.runProgressBar.gameObject.SetActive(true);
    }

    /// <summary>A pingvinek mozgatása oldalra egy megadott érték alapján.</summary>
    /// <param name="horizontal">Horizontális érték, pl. <see cref="Input.GetAxisRaw(string)"/>-ből. Negatív esetén balra, pozitív esetén jobbra mozog.</param>
    public void Move(float horizontal)
    {
        if (!run) return;

        float amount = horizontal * horizontalSensitivity * Time.deltaTime;
        Vector2 moveCommand = new Vector2(penguins.First().transform.position.z, penguins.First().transform.position.x + amount);
        //Vector2 moveCommand = new Vector2(penguins.First().transform.position.z, amount);


        int i = 0;
        foreach (Penguin penguin in penguins)
        {
            if (penguin == null) continue;
            penguin.AddMoveCommand(moveCommand, i == 0);
            i++;
        }
    }

    public bool GetPenguinByID(int id, out Penguin penguin)
    {
        penguin = penguins.Where(p => p.id == id).FirstOrDefault();
        return penguin != null;
    }

    /// <summary>Pingvin kitörlése az x. pozíción</summary>
    /// <param name="index">0 = első</param>
    public void RemovePenguinAtPosition(int index)
    {
        if (penguins.Count - 1 < index) throw new System.IndexOutOfRangeException();
        RemovePenguin(penguins[index]);
    }
    /// <summary>Megadott ID-jű pingvin kitörlése</summary>
    public void RemovePenguinWithID(int id)
    {
        if (penguins.Count == 0) return;
        if (GetPenguinByID(id, out Penguin penguin))
        {
            RemovePenguin(penguin);
        }
    }
    public void RemovePenguin(Penguin penguin)
    {
        if (penguin == null) throw new System.ArgumentNullException();
        if (!penguins.Contains(penguin)) throw new System.ArgumentException($"Penguin not in TeamManager. ({penguin.id} - {penguin.name})");
        Debug.Log($"Remove Penguin #{penguin.id} ({penguin.name})");

        try
        {
            StartCoroutine(PlaceTomato(penguin.transform.position));
            StartCoroutine(PlaceKetchup(penguin.transform.position));
            AudioManager.instance.PlaySound("tree hit", AudioManager.instance.Mixer.SFX);
            AudioManager.instance.PlaySound("splat", AudioManager.instance.Mixer.SFX);

            int idx = penguins.IndexOf(penguin);
            penguins.Remove(penguin);
            Destroy(penguin.gameObject);

            if (idx == 0)
            {
                HandleFirstPenguinDead();
            }
            else
            {
                HandlePenguinGaps();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Can't remove Penguin #{penguin.id} ({penguin.name})");
            Debug.LogException(e);
            return;
        }

    }

    private void HandleFirstPenguinDead()
    {
        if (!run || !penguins.Any()) return;
        Penguin first = penguins.First();
        if (first == null) throw new System.NullReferenceException("Null reference for first penguin, but penguins is not empty.");
        cameraController.SetTransform(first.transform, true);
        foreach (Penguin p in penguins)
        {
            if (p.id == first.id) continue;
            p.GetBehind(first);
        }
        HandlePenguinGaps();
    }

    private void HandlePenguinGaps()
    {
        if (!run || !penguins.Any()) return;
        Penguin prev = null;
        foreach (Penguin p in penguins)
        {
            if (prev == null)
            {
                prev = p;
            }
            else
            {
                p.CatchUp(prev);
                prev = p;
            }
        }
    }

    IEnumerator PlaceTomato(Vector3 position)
    {
        GameObject tomato = Instantiate(tomatoPrefab);
        tomato.transform.position = position;
        SpriteRenderer sr = tomato.GetComponentInChildren<SpriteRenderer>();
        float t = 0;
        while (t < tomatoDuration)
        {
            t += Time.deltaTime;
            tomato.transform.localScale = Vector3.Lerp(Vector3.one, 1.2f * Vector3.one, t / tomatoDuration);
            if (t >= tomatoDuration * 0.8f)
            {
                sr.color = Color.Lerp(Color.white, new Color(1, 1, 1, 0), (t - tomatoDuration * 0.8f) / (tomatoDuration * 0.2f));
            }
            yield return null;
        }
        Destroy(tomato);
    }

    IEnumerator PlaceKetchup(Vector3 position)
    {
        ParticleSystem ketchup = Instantiate(ketchupParticlePrefab);
        ketchup.transform.position = position;
        yield return new WaitUntil(() => !ketchup.isPlaying);
        Destroy(ketchup.gameObject);
    }
}
