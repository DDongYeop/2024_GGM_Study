using System;
using UnityEngine;
using TMPro;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using UnityEngine.SceneManagement;

public class Player : Agent
{
    public Sprite[] sprites;
    public float strength = 5f;
    public float gravity = -9.81f;
    public float tilt = 5f;

    private SpriteRenderer spriteRenderer;
    private Vector3 direction;
    private int spriteIndex;

    private Spawner spawner;
    private TextMeshPro scoreText;
    private int score;

    public override void Initialize()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spawner = transform.parent.Find("Spawner").GetComponent<Spawner>();
        scoreText = transform.parent.Find("ScoreText").GetComponent<TextMeshPro>();
        
        InvokeRepeating(nameof(AnimateSprite), 0.15f, 0.15f);
        transform.localPosition = Vector3.zero;
        direction = Vector3.zero;
    }

    public override void OnEpisodeBegin()
    {
        InvokeRepeating(nameof(AnimateSprite), 0.15f, 0.15f);
        transform.localPosition = Vector3.zero;
        direction = Vector3.zero;

        score = 0;
        scoreText.text = "0";
        spawner.ReSetSpawn();
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        var discreteActions = actions.DiscreteActions;

        if (discreteActions[0] == 1 && transform.localPosition.y <= 3.5f)
            Jump();
        
        SetReward(Time.fixedDeltaTime);
    }

    private void FixedUpdate()
    {
        // 플레이어에 중력 적용
        direction.y += gravity * Time.fixedDeltaTime;
        transform.position += direction * Time.fixedDeltaTime;

        // 방향에 따라 스프라이트 기울이기
        Vector3 rotation = transform.eulerAngles;
        rotation.z = direction.y * tilt;
        transform.eulerAngles = rotation;
    }

    //사용자가 에이전트 행동을 직접 조절 
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreateActionsOut = actionsOut.DiscreteActions;
        
        if (Input.GetKey(KeyCode.Space))
            discreateActionsOut[0] = 1;
    }
    
    private void Jump()
    {
        direction = Vector3.up * strength;
    }

    private void AnimateSprite()
    {
        spriteIndex++;

        if (spriteIndex >= sprites.Length)
            spriteIndex = 0;

        if (spriteIndex < sprites.Length && spriteIndex >= 0)
            spriteRenderer.sprite = sprites[spriteIndex];
    }

    private void IncreaseScore()
    {
        score++;
        scoreText.text = score.ToString();
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Obstacle")) 
        {
            SetReward(-10f);
            EndEpisode();
            //SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        } 
        else if (other.gameObject.CompareTag("Scoring")) 
        {
            IncreaseScore();
            SetReward(5f);
        }
    }
}
