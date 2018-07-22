using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessBoardManager : MonoBehaviour
{
    public GameObject FullPlane;
    public GameObject RestartButton;
    public GameObject TriggerWall;
    public GameObject PauseMenu;
    public static bool Paused { get; private set; }
    public static bool GameStarted;
    public GameObject ChessPlatePrefab;
    public GameObject CheckerPrefab;
    public float PlateRadius { get; private set; }
    private int ChessBoardRadius { get; set; }
    public static ChessBoardManager Instance { get; private set; }
    public static List<GameObject> ActiveCheckers;
    public int TeamsNumber { get; private set; }
    public Color[] TeamsColors { get; private set; }
    private List<Vector3>[] TeamsPositions { get; set; }
    public bool WaitingEndOfTurn { get; private set; }
    public static int EnemyCheckersOut { get; private set; }
    public static int OwnCheckersOut { get; private set; }
    public static float ForceScale { get; private set; }
    private static Vector3 NormalForceOfKnock { get; set; }
    private float SwipeBeginTime;
    private Vector3 SwipeBeginPosition;
    private Rigidbody KnockedChecker;
    public static int TeamTurn { get; private set; }
    private static float ComputerFixedForce;
    public bool[] IsComputerPlayer;
    private static float MinVelocity { get; set; }

    void KnockChecker(Vector3 swipe, float time)
    {
        var swipeAngle = Vector3.SignedAngle(Vector3.up, swipe, -Vector3.forward);
        var camera = Camera.main.transform.forward;
        var cameraDirection = new Vector3(camera.x, 0f, camera.z).normalized;
        var force = Quaternion.Euler(new Vector3(0, swipeAngle, 0)) * (cameraDirection + NormalForceOfKnock) * ForceScale / time;
        Debug.Log("AddingForce");
        this.KnockedChecker.AddForce(force);
    }

    void ComputerKnockChecker()
    {
        var computerChecker = (GameObject)null;
        foreach (var checker in ActiveCheckers)
        {
            if (checker.GetComponent<Checker>().TeamNumber == TeamTurn)
            {
                computerChecker = checker;
                KnockedChecker = computerChecker.GetComponent<Rigidbody>();
                break;
            }
        }

        var enemyChecker = (GameObject)null;
        foreach (var checker in ActiveCheckers)
        {
            if (checker.GetComponent<Checker>().TeamNumber != TeamTurn)
            {
                enemyChecker = checker;
                break;
            }
        }

        var forceDirection = enemyChecker.transform.position - computerChecker.transform.position;
        forceDirection = new Vector3(forceDirection.x, 0, forceDirection.z).normalized + NormalForceOfKnock;
        this.KnockedChecker.AddForce(forceDirection * ComputerFixedForce);
    }

    public static void CheckerOutOfGame(GameObject checker)
    {
        var teamNumber = checker.GetComponent<Checker>().TeamNumber;
        if (teamNumber == TeamTurn)
        {
            OwnCheckersOut++;
        }
        else
        {
            EnemyCheckersOut++;
        }

        Debug.Log(teamNumber.ToString() + " is out");
        ActiveCheckers.Remove(checker);
        Destroy(checker);
    }

    public void PlayWithComputer()
    {
        GameStarted = true;
        TeamTurn = 0;
        IsComputerPlayer = new bool[]
        {
            false,
            true
        };

        RestartGame();
    }

    public void PlayWithPlayer()
    {
        GameStarted = true;
        TeamTurn = 0;
        IsComputerPlayer = new bool[]
        {
            false,
            false
        };

        RestartGame();
    }

    public void RestartGame()
    {
        OwnCheckersOut = 0;
        EnemyCheckersOut = 0;
        if (ActiveCheckers != null)
        {
            foreach (var checker in ActiveCheckers)
            {
                Destroy(checker);
                ActiveCheckers = null;
            }
        }

        GenerateCheckers();
        SwitchPauseTo(false);
    }

    void GenerateChessboard()
    {
        var fullPlane = Instantiate(FullPlane, Vector3.zero, Quaternion.identity);
        var teamNumber = 0;
        var color = TeamsColors[teamNumber];
        var z = -this.ChessBoardRadius;
        var zIncrementation = 1;
        for (var x = -this.ChessBoardRadius; x < this.ChessBoardRadius; x++)
        {
            while (z >= -this.ChessBoardRadius && z < this.ChessBoardRadius)
            {
                var platePosition = new Vector3(x + this.PlateRadius, 0f, z + this.PlateRadius);
                var newPlate = Instantiate(this.ChessPlatePrefab, platePosition, Quaternion.identity);
                var renderer = newPlate.GetComponent<Renderer>();
                renderer.material.color = color;

                z += zIncrementation;
                teamNumber += 1;
                teamNumber %= this.TeamsNumber;
                color = TeamsColors[teamNumber];
            }

            zIncrementation *= -1;
            z += zIncrementation;
        }

        var walls = new GameObject[6];
        var wallRadius = ((this.ChessBoardRadius * 2) + 1) * this.PlateRadius;
        var positions = new Vector3[]
        {
            new Vector3(+wallRadius, 0f, 0f),
            new Vector3(-wallRadius, 0f, 0f),
            new Vector3(0f, 0f, +wallRadius),
            new Vector3(0f, 0f, -wallRadius),
            new Vector3(0f, +wallRadius, 0f),
            new Vector3(0f, -wallRadius, 0f),
        };

        var rotations = new Quaternion[]
        {
            Quaternion.Euler(0f, 0f, +90f),
            Quaternion.Euler(0f, 0f, -90f),
            Quaternion.Euler(+90f, 0f, 0f),
            Quaternion.Euler(-90f, 0f, 0f),
            Quaternion.identity,
            Quaternion.identity,
        };

        for (var i = 0; i < 6; i++)
        {
            walls[i] = Instantiate(this.TriggerWall, positions[i], rotations[i]);
            walls[i].transform.localScale.Set(wallRadius, 0f, wallRadius);
        }
    }

    void SwitchPauseTo(bool pause)
    {
        Paused = pause;
        this.PauseMenu.SetActive(pause);
        if (pause)
        {
            Time.timeScale = 0f;
            RestartButton.SetActive(GameStarted);
        }
        else
        {
            Time.timeScale = 1f;
        }
    }

    void Start()
    {
        Instance = this;

        ForceScale = 100;
        this.SwitchPauseTo(true);
        GameStarted = false;
        this.TeamsNumber = 2;
        TeamTurn = 0;
        this.TeamsColors = new Color[] { Color.white, Color.black };
        IsComputerPlayer = new bool[] { false, true };
        this.ChessBoardRadius = 4;
        this.PlateRadius = 0.5f;
        ComputerFixedForce = 1000f;
        NormalForceOfKnock = new Vector3(0, 0.05f, 0);
        MinVelocity = 0.01f;

        this.GenerateChessboard();
    }

    void GenerateCheckers()
    {
        this.TeamsPositions = new List<Vector3>[this.TeamsNumber];
        var z = new int[] { -4, 3 };
        var checkersHeight = this.CheckerPrefab.transform.localScale.y;
        for (var teamNumber = 0; teamNumber < this.TeamsNumber; teamNumber++)
        {
            this.TeamsPositions[teamNumber] = new List<Vector3>();
        }

        for (var x = -this.ChessBoardRadius; x < this.ChessBoardRadius; x++)
        {
            for (var teamNumber = 0; teamNumber < this.TeamsNumber; teamNumber++)
            {
                this.TeamsPositions[teamNumber].Add(new Vector3(x + this.PlateRadius, checkersHeight, z[teamNumber] + this.PlateRadius));
            }
        }

        ActiveCheckers = new List<GameObject>();

        for (var teamNumber = 0; teamNumber < this.TeamsNumber; teamNumber++)
        {
            var teamPositions = this.TeamsPositions[teamNumber];
            var color = this.TeamsColors[teamNumber];
            foreach (var position in teamPositions)
            {
                var newChecker = Instantiate(this.CheckerPrefab, position, Quaternion.identity);
                newChecker.GetComponent<Checker>().TeamNumber = teamNumber;
                ActiveCheckers.Add(newChecker);
                var renderer = newChecker.GetComponent<Renderer>();
                renderer.materials[0].color = color;
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            this.SwitchPauseTo(!Paused);
        }

        if (WaitingEndOfTurn)
        {
            var allSleep = true;
            foreach (var checker in ActiveCheckers)
            {
                allSleep &= checker.GetComponent<Rigidbody>().velocity.magnitude < MinVelocity;
            }

            if (allSleep)
            {
                WaitingEndOfTurn = false;
                if (ActiveCheckers.Count == 0)
                {
                    RestartGame();
                    return;
                }
                else
                {
                    var checkersLeftOfTeam = new int[TeamsNumber];
                    foreach (var checker in ActiveCheckers)
                    {
                        checkersLeftOfTeam[checker.GetComponent<Checker>().TeamNumber]++;
                    }

                    for (var teamNumber = 0; teamNumber< TeamsNumber; teamNumber++)
                    {
                        if (checkersLeftOfTeam[teamNumber] == ActiveCheckers.Count)
                        {
                            TeamTurn = teamNumber;
                            RestartGame();
                            return;
                        }
                    }
                }

                if (OwnCheckersOut != 0 || EnemyCheckersOut == 0)
                {
                    TeamTurn++;
                    TeamTurn %= TeamsNumber;
                }

                OwnCheckersOut = 0;
                EnemyCheckersOut = 0;
            }
        }
        else
        {
            if (IsComputerPlayer[TeamTurn])
            {
                ComputerKnockChecker();
                WaitingEndOfTurn = true;
            }
        }

        if (Input.GetMouseButtonDown(0) && !Paused && !this.WaitingEndOfTurn)
        {
            Debug.Log("Swipe Start");
            RaycastHit hit;
            var mousePosition = Input.mousePosition;
            var ray = Camera.main.ScreenPointToRay(mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                Debug.Log("Ray Casted");
                var checker = hit.rigidbody;
                Debug.Log(hit.transform.gameObject.ToString());
                if (hit.transform.gameObject.GetComponent<Checker>().TeamNumber == TeamTurn)
                {
                    Debug.Log("Swipe Started");
                    this.KnockedChecker = checker;
                    this.SwipeBeginTime = Time.time;
                    this.SwipeBeginPosition = mousePosition;
                }
                else
                {
                    Debug.Log("Checker of another Team");
                }
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            Debug.Log("Swipe Finished");
            if (this.KnockedChecker != null)
            {
                var swipe = Input.mousePosition - this.SwipeBeginPosition;
                var time = Time.time - this.SwipeBeginTime;
                this.KnockChecker(swipe, time);
                this.WaitingEndOfTurn = true;
            }
            else
            {
                Debug.Log("No Checker For Knock");
            }
        }
    }
}
