using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheLostTent;
using TMPro;
using System;
using System.Linq;

public class LevelManager : MonoBehaviour
{
    public GameObject[] enemyPrefabs;
    public GameObject spawnAnimationPrefab;
    public Camera cameraMain;
    public string levelTextPrefix = "Level -> ";
    public TextMeshProUGUI textObj;
    public TextMeshProUGUI levelObj;
    public int spawnAmount = 5;
    public float spawnRadius;
    public float restartWaitDuration = 1f;
    public float baseSpeed = 1;
    public float speedIncrement = .2f;
    public float spawnDelay = 0.1f;
    public float spawnAnimDuration = 1f;
    private int enemiesAlive = 0;
    private Vector2 spawnCenter;
    private int levelNum = -1;
    private bool isSpawning;
    private Pooler pooler;
    private Transform playerTransform;
    private List<string> enemyTags;
    private List<GameObject> characterObjs;
    [SerializeField]
    private Vector2 startPosition = new Vector2(13, 6);

    private void Awake()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        pooler = GameObject.FindGameObjectWithTag("Pooler").GetComponent<Pooler>();
    }

    private void Start()
    {
        enemyTags = new List<string>(enemyPrefabs.Select(x => x.name));
        characterObjs = new List<GameObject>();
        levelNum = -1;
        isSpawning = false;
        textObj.text = enemiesAlive.ToString();
        playerTransform.GetComponentInChildren<Heart>().deathEvent += () =>
        {
            ResetLevel();
        };
        Invoke("SpawnNow", spawnDelay);
    }

    void SpawnNow()
    {
        StartCoroutine(SpawnEnemies(playerTransform.position));
    }

    public void SpawnAt(Vector2 position)
    {
        if (!isSpawning)
        {
            StartCoroutine(SpawnEnemies(position));
        }
        else
        {
            // Debug.LogWarning("Was already spawning");
        }
    }

    IEnumerator SpawnEnemies(Vector2 spawnCenter)
    {
        isSpawning = true;
        levelNum++;
        // increase spawn amount linearly
        int spawnAmount = this.spawnAmount + levelNum % 2;

        levelObj.text = levelTextPrefix + (levelNum + 1);
        Vector2[] spawnPostions = new Vector2[spawnAmount];
        for (int i = 0; i < spawnAmount; i++)
        {
            DoSpawnAnimation(i);
        }
        yield return new WaitForSeconds(spawnAnimDuration);
        for (int i = 0; i < spawnAmount; i++)
        {
            SpawnEnemy(i);
        }
        // mark the end of spawn 
        yield return null;
        // Debug.Log("Enemies Alive - " + enemiesAlive);
        isSpawning = false;


        void DoSpawnAnimation(int index)
        {
            var spawnpos = spawnPostions[index] = spawnCenter + UnityEngine.Random.insideUnitCircle * spawnRadius;
            var obj = pooler.RetrieveFromPool(Constants.PoolTags.SpawnAnim, spawnpos, Vector3Int.zero, transform);
            pooler.ReturnToPool(Constants.PoolTags.SpawnAnim, obj, spawnAnimDuration);
        }

        void SpawnEnemy(int index)
        {
            // find spawn position
            var spawnpos = spawnPostions[index];
            var enemyTag = GetEnemyTag();
            var enemyObj = pooler.RetrieveFromPool(enemyTag, spawnpos, Vector3Int.zero);

            // set target as player
            enemyObj.GetComponent<Enemy>().target = playerTransform;

            // set world camera 
            enemyObj.GetComponentInChildren<Canvas>().worldCamera = cameraMain;

            // increase enemy count as we spawned one
            enemiesAlive++;
            textObj.text = enemiesAlive.ToString();
            characterObjs.Add(enemyObj);

            // on death decrease enemy count
            enemyObj.GetComponent<Heart>().deathEvent += () =>
            {
                enemiesAlive--;
                textObj.text = enemiesAlive.ToString();
                characterObjs.Remove(enemyObj);
                Debug.Log("Enemies Alive - " + enemiesAlive);
                // if enemiesAlive goes to zero
                // then spawn item rewards
                
            };
            // set enemy movement speed
            enemyObj.GetComponent<CharacterMotor>().movementSpeed = baseSpeed + levelNum * speedIncrement;
        }
    }

    public void ResetLevel()
    {
        // start from first level
        levelNum = -1;
        enemiesAlive = 0;
        StartCoroutine(ResetLevelCoroutine());
    }

    private IEnumerator ResetLevelCoroutine()
    {
        // disable all characters
        playerTransform.localScale = Vector3.zero;

        foreach (GameObject character in characterObjs)
        {
            character.GetComponent<Enemy>().KillMe();
        }
        characterObjs = new List<GameObject>();

        yield return new WaitForSecondsRealtime(restartWaitDuration);

        // reset player
        playerTransform.localScale = Vector3.one;
        var witch = playerTransform.GetComponent<Witch>();
        witch.ResetAt(startPosition);

        // spawn characters
        SpawnAt(startPosition);
    }

    private string GetEnemyTag()
    {
        switch (levelNum)
        {
            case 0:
                return enemyTags[0];
            case 1:
                return enemyTags[1];
            case 2:
                return enemyTags[2];
            default:
                return enemyTags[UnityEngine.Random.Range(0, enemyTags.Count)];
        }
    }
}
