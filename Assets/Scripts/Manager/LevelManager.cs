using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheLostTent;
using TMPro;
using System;

public class LevelManager : MonoBehaviour
{
    public GameObject[] enemyPrefabs;
    public GameObject spawnAnimationPrefab;
    public Camera cameraMain;
    public string levelTextPrefix = "Level -> ";
    public TextMeshProUGUI textObj;
    public TextMeshProUGUI levelObj;
    public int spawnAmount;
    public float spawnRadius;
    public float baseSpeed = 1;
    public float speedIncrement = .2f;
    public float spawnAnimDuration = 1f;
    private float enemiesAlive = 0;
    private Vector2 spawnCenter;
    private int levelNum = -1;
    private bool isSpawning;
    private Pooler pooler;
    private Transform playerTransform;
    private string[] enemyTags;
    private void Awake()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        pooler = GameObject.FindGameObjectWithTag("Pooler").GetComponent<Pooler>();
    }
    private void Start()
    {
        enemyTags = new string[] {
            Constants.PoolTags.Skeleton,
            Constants.PoolTags.Archer,
            Constants.PoolTags.Mage
        };
        levelNum = -1;
        isSpawning = false;
        textObj.text = enemiesAlive.ToString();
    }

    private void Update()
    {
        if (enemiesAlive <= 0 && !isSpawning)
        {
            StartCoroutine(SpawnEnemies());
        }
    }

    IEnumerator SpawnEnemies()
    {
        isSpawning = true;
        levelNum++;
        spawnCenter = playerTransform.position;
        // increase spawn amount linearly
        int spawnAmount = this.spawnAmount + (levelNum + 2) % 3 + (int)2 * levelNum / 3;

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
            var obj = pooler.RetrieveFromPool(enemyTag, spawnpos, Vector3Int.zero);

            // set target as player
            obj.GetComponent<Enemy>().target = playerTransform;

            // set world camera 
            obj.GetComponentInChildren<Canvas>().worldCamera = cameraMain;

            // increase enemy count as we spawned one
            enemiesAlive++;
            textObj.text = enemiesAlive.ToString();

            // on death decrease enemy count
            obj.GetComponent<Heart>().deathEvent += () =>
            {
                enemiesAlive--;
                textObj.text = enemiesAlive.ToString();
                Debug.Log("Enemies Alive - " + enemiesAlive);
            };
            // set enemy movement speed
            obj.GetComponent<CharacterMotor>().movementSpeed = baseSpeed + levelNum * speedIncrement;
        }
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
                return enemyTags[UnityEngine.Random.Range(0, enemyTags.Length)];
        }
    }
}
