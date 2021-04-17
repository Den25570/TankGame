using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;

public class GameManager : MonoBehaviour
{

    public int m_NumRoundsToWin = 5;
    public float StartDelay = 3f;
    public float EndDelay = 3f;

    public NetworkManager NetworkManager;
    public CameraControl CameraControl;
    public Text MessageText;
    public GameObject MessageMenu;
    public GameObject MainMenu;
    public GameObject TankPrefab;

    public TankManager[] Tanks;
    [HideInInspector] public TankManager PlayerControlledTank;
    public int PlayersRequired = 2;

    [HideInInspector] public string message;
    [HideInInspector] public bool StartGame = false;
    [HideInInspector] public bool RoundEnded = false;

    public enum MatchStatus
    {
        NotStarted = 1,
        Paused = 2,
        OnGoing = 3,
    }
    [HideInInspector] public MatchStatus GameStatus;

    private int RoundNumber;              
    private WaitForSeconds StartWait;     
    private WaitForSeconds EndWait;       
    private TankManager RoundWinner;
    private TankManager GameWinner;

    private InputBuffer inputBuffer;

    private void Start()
    {
        StartGame = false;
        RoundEnded = false;
        GameStatus = MatchStatus.NotStarted;
        StartWait = new WaitForSeconds(StartDelay);
        EndWait = new WaitForSeconds(EndDelay);     
    }

    private void Update()
    {
        if (StartGame)
        {
            StartMatch();
            StartGame = false;
        }     
    }

    public void StartMatch()
    {
        if (!NetworkManager.isHost) 
        {
            inputBuffer = new InputBuffer();
        }
        
        MessageMenu.SetActive(true);
        MainMenu.SetActive(false);

        SpawnAllTanks();
        SetCameraTargets();

        Debug.Log("Setup complete.");

        GameStatus = MatchStatus.OnGoing;

        StartCoroutine(GameLoop());
    }

    public void Endmatch()
    {
        StartGame = false;
        RoundEnded = false;
        GameStatus = MatchStatus.NotStarted;

        MessageMenu.SetActive(false);
        MainMenu.SetActive(true);

        for (int i = 0; i < Tanks.Length; i++)
        {
            Tanks[i].Instance = null;
        }
        PlayerControlledTank = null;
    }

    private void SpawnAllTanks()
    {
        for (int i = 0; i < Tanks.Length; i++)
        {
            Tanks[i].Instance =
                Instantiate(TankPrefab, Tanks[i].SpawnPoint.position, Tanks[i].SpawnPoint.rotation) as GameObject;
            Tanks[i].Index = i + 1;
            Tanks[i].isPlayerControlled = false;
            Tanks[i].GameManager = this;
            Tanks[i].Setup();

            if (NetworkManager.Client.MyIndex == i)
            {
                PlayerControlledTank = Tanks[i];
                PlayerControlledTank.isPlayerControlled = true;
            }
                
        }
    }

    private void SetCameraTargets()
    {
        Transform[] targets = new Transform[Tanks.Length];

        for (int i = 0; i < targets.Length; i++)
        {
            targets[i] = Tanks[i].Instance.transform;
        }

        CameraControl.m_Targets = targets;
    }

    private IEnumerator GameLoop()
    {
        yield return StartCoroutine(RoundStarting());
        yield return StartCoroutine(RoundPlaying());
        yield return StartCoroutine(RoundEnding());

        if (GameWinner != null)
        {
            SceneManager.LoadScene(0);
            NetworkManager.CloseAllConnections();
        }
        else
        {
            StartCoroutine(GameLoop());
        }
    }

    private IEnumerator RoundStarting()
    {
        ResetAllTanks();
        DisableTankControl();

        CameraControl.SetStartPositionAndSize();

        RoundNumber++;
        MessageText.text = "ROUND " + RoundNumber.ToString();

        RoundEnded = false;

        yield return StartWait;
    }

    private IEnumerator RoundPlaying()
    {
        EnableTankControl();

        MessageText.text = "";

        while(!OneTankLeft()/* || (!RoundEnded && !isHost)*/)
        {
            yield return null;
        } 
    }

    private IEnumerator RoundEnding()
    {
        DisableTankControl();

       // if (isHost)
        {
            RoundWinner = GetRoundWinner();
            if (RoundWinner != null)
            {
                RoundWinner.Wins++;
            }
            GameWinner = GetGameWinner();
            message = EndMessage();

            RoundEnded = true;
        }

        MessageText.text = message;       

        yield return EndWait;
    }

    private bool OneTankLeft()
    {
        int numTanksLeft = 0;

        for (int i = 0; i < Tanks.Length; i++)
        {
            if (Tanks[i].Instance.activeSelf)
                numTanksLeft++;
        }

        return numTanksLeft <= 1;
    }

    private TankManager GetRoundWinner()
    {
        for (int i = 0; i < Tanks.Length; i++)
        {
            if (Tanks[i].Instance.activeSelf)
                return Tanks[i];
        }

        return null;
    }

    private TankManager GetGameWinner()
    {
        for (int i = 0; i < Tanks.Length; i++)
        {
            if (Tanks[i].Wins == m_NumRoundsToWin)
                return Tanks[i];
        }

        return null;
    }

    private string EndMessage()
    {
        string message = "DRAW!";

        if (RoundWinner != null)
            message = RoundWinner.ColoredPlayerText + " WINS THE ROUND!";

        message += "\n\n\n\n";

        for (int i = 0; i < Tanks.Length; i++)
        {
            message += Tanks[i].ColoredPlayerText + ": " + Tanks[i].Wins + " WINS\n";
        }

        if (GameWinner != null)
            message = GameWinner.ColoredPlayerText + " WINS THE GAME!";

        return message;
    }

    private void ResetAllTanks()
    {
        for (int i = 0; i < Tanks.Length; i++)
        {
            Tanks[i].Reset();
        }
    }

    private void EnableTankControl()
    {
        for (int i = 0; i < Tanks.Length; i++)
        {
            Tanks[i].EnableControl();
        }
    }

    private void DisableTankControl()
    {
        for (int i = 0; i < Tanks.Length; i++)
        {
            Tanks[i].DisableControl();
        }
    }

    //ServerOnly
    public WorldSnapshot GenerateWorldSnapshot()
    {
        var WorldSnapshot = new WorldSnapshot();

        WorldSnapshot.RoundNumber = RoundNumber;
        foreach (var TankManagerInstance in Tanks)
        {
            TankData Tank = new TankData(TankManagerInstance);
            WorldSnapshot.TankData.Add(Tank);
        }
      
        return WorldSnapshot;
    }

    //ClientOnly
    public void ApplyWorldSnapshot(WorldSnapshot snapshot)
    {
       // RoundNumber = snapshot.RoundNumber;

        inputBuffer.InterpolateSnapshot(NetworkManager.Client.MyIndex, ref snapshot);
        foreach(var tank in snapshot.TankData)
        {
            var clientTankInstance = Tanks.Where(x => x.Index == tank.index);
            if (clientTankInstance != null && clientTankInstance.Count() > 0)
                clientTankInstance.First().UpdateTankData(tank);
            else
                Debug.LogError("Player with index " + tank.index + " not found!");
        }
    }

    //ServerOnly
    public void ApplyInputs(Input inputs)
    {
        var serverTankInstance = Tanks.Where(x => x.Index == inputs.Index).First();

        serverTankInstance.UpdateInputs(inputs);
    }

    public void ApplyShot(int index, float chargeValue)
    {
        var serverTankInstance = Tanks.Where(x => x.Index == index).First();
        serverTankInstance.Fire(chargeValue);
    }

    public void WriteInInputBuffer(TimeInput input)
    {
        inputBuffer.Write(input);
    }
}