﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;      //Tells Random to use the Unity Engine random number generator.

public class EnemyAI : MonoBehaviour
{
    public int enemyHP = 10;
    public AudioSource hurtMeSound;
    public int runAwayHP = 3;
    private bool caughtPlayer = false;
    private bool attacking = false;
    private Animator animator;
    private Transform player;
    private NavMeshAgent2D enemy;
    private bool BeenHit = false;
    public float checkDistance = 1.5f; // The distance between enemy and player, when real distance is smaller, enemy will start to walk.
    public static float lastHitTime;
    public static float timeSinceLastHit;
    private bool[] formerStatus; //1-move; 2-kick;

    void Start()
    {
        lastHitTime = 1f;
        animator = GetComponent<Animator>();
        enemy = GetComponent<NavMeshAgent2D>();
        animator.SetBool("PlayerKicking", false);
        player = GameObject.FindGameObjectWithTag("Player").transform;
        formerStatus = new bool[2];
        //enemy.destination = player.position;
    }

    public void EnemyRestoreFromHit()
    {
        animator.SetBool("PlayerMoving", formerStatus[0]);
        animator.SetBool("PlayerKicking", formerStatus[1]);
        animator.SetBool("hitme", false);
        BeenHit = false;
    }

    public void EnemyBeenHit(int damage)
    {
        BeenHit = true;
        animator.speed = 1f;
        hurtMeSound.Play();
        enemyHP -= damage;
        SetEnemyToBeenHit();
        SetBeenHitAnimationDirection();
        formerStatus[0] = animator.GetBool("PlayerMoving");
        formerStatus[1] = animator.GetBool("PlayerKicking");

        //BeenHit = false;
        //stop any current action
    }

    Vector2 GetPlayerDirection()
    {

        float horizontal = player.position.x - transform.position.x;
        float vertical = player.position.y - transform.position.y;

        Vector2 pos = new Vector2(0, 0);
        float offset = 0.4f; //use to make the enemy not that sensetive to direction

        if (horizontal > offset)
        {
            pos.x = 1;
        }
        else if (horizontal < offset * -1)
        {
            pos.x = -1;
        }
        else if (horizontal >= offset * -1 && horizontal <= offset)
        {
            pos.x = 0;
        }

        if (vertical > offset)
        {
            pos.y = 1;
        }
        else if (vertical < offset * -1)
        {
            pos.y = -1;
        }
        else if (vertical >= offset * -1 && vertical <= offset)
        {
            pos.y = 0;
        }

        return pos;
    }

    Vector2 GetDestination(Vector2 playerRawAxis)
    {
        /* 
            This method is used to detect the player's facing, 
            then return the destination that the enemy should move to.
            The destination == center point of the player's collider2D component 
            but slightly move outwards
        */

        Vector2 finalPosition = player.transform.position;

        // player facing up
        if (playerRawAxis.x == 0 && playerRawAxis.y == 1)
        {
            finalPosition.y += 0.5f; // + -> outwards
        }

        // player facing down
        if (playerRawAxis.x == 0 && playerRawAxis.y == -1)
        {
            finalPosition.y -= 0.4f; // - -> outwards
        }

        // player facing right
        if (playerRawAxis.x == 1 && playerRawAxis.y == 0)
        {
            finalPosition.x += 0.4f; // - -> outwards
        }

        // player facing left
        if (playerRawAxis.x == -1 && playerRawAxis.y == 0)
        {
            finalPosition.x -= 0.4f; // - -> outwards
        }

        // =================================

        // player facing up right
        if (playerRawAxis.x > 0 && playerRawAxis.x <= 1 &&
            playerRawAxis.y > 0 && playerRawAxis.y <= 1)
        {
            finalPosition.x += 0.27f; // + -> outwards            
            finalPosition.y += 0.4f; // + -> outwards
        }

        // player facing up left
        if (playerRawAxis.x < 0 && playerRawAxis.x >= -1 &&
            playerRawAxis.y > 0 && playerRawAxis.y <= 1)
        {
            finalPosition.x += 0.35f; // - -> outwards            
            finalPosition.y -= 0.4f; // + -> outwards
        }

        // player facing down right
        if (playerRawAxis.x > 0 && playerRawAxis.x <= 1 &&
            playerRawAxis.y < 0 && playerRawAxis.y >= -1)
        {
            finalPosition.x += 0.32f; // + -> outwards
            finalPosition.y -= 0.3f;  // + -> outwards
        }

        // player facing down left
        if (playerRawAxis.x < 0 && playerRawAxis.x >= -1 &&
            playerRawAxis.y < 0 && playerRawAxis.y >= -1)
        {
            finalPosition.x -= 0.29f; // - -> outwards
            finalPosition.y -= 0.29f; // - -> outwards
        }

        return finalPosition;
    }


    void SetEnemyToMove()
    {
        animator.SetBool("PlayerMoving", true);
        animator.SetBool("PlayerKicking", false);
        animator.SetBool("hitme", false);        
    }

    void SetEnemyToKick()
    {
        lastHitTime = Time.time;
        attacking = true;
        animator.SetBool("PlayerMoving", false);
        animator.SetBool("PlayerKicking", true);
        animator.SetBool("hitme", false);        
    }
    void SetEnemyToBeenHit()
    {
        animator.SetBool("PlayerMoving", false);
        animator.SetBool("PlayerKicking", false);
        animator.SetBool("hitme", true);
    }
    void SetBeenHitAnimationDirection()
    {
        Vector2 pos = GetPlayerDirection();
        animator.SetFloat("MoveX", pos.x);
        animator.SetFloat("MoveY", pos.y);
    }
    void SetMoveAnimationDirection()
    {
        Vector2 pos = GetPlayerDirection();
        animator.SetFloat("MoveX", pos.x);
        animator.SetFloat("MoveY", pos.y);
    }

    void SetKickAnimationDirection()
    {
        Vector2 pos = GetPlayerDirection();
        animator.SetFloat("LastMoveX", pos.x);
        animator.SetFloat("LastMoveY", pos.y);
    }

    void StartToAttack()
    {
        if (!BeenHit)
        {
            enemy.Stop();
            animator.speed = 1f;
            SetEnemyToKick();
            SetKickAnimationDirection();
        }
        else
        {
            if (!animator.GetCurrentAnimatorStateInfo(0).IsName("hit"))
            {
                BeenHit = false;
                animator.SetBool("hitme", false);
            }
        }


    }

    private float AwayFromPlayerVertically(float playerY, float moveY)
    {
        float newY = 0f;
        if (playerY > 0)
        {
            newY -= moveY;
        }
        else if (playerY < 0)
        {
            newY += moveY;
        }
        return newY;
    }

    Vector2 GetFurthestPointAfterPlayerToEnemy()
    {
        Vector2 playerPosition = GetPlayerDirection();
        Vector2 newPosition = transform.position;

        float moveX = 1.5f; // delta value to move
        float moveY = 1.5f; // delta value to move

        if (playerPosition.x > 0) //player at the right side of enemy
        {
            newPosition.x -= moveX;
        }
        else if (playerPosition.x < 0) //player at the left side of enemy
        {
            newPosition.x += moveX;
        }
        newPosition.y = AwayFromPlayerVertically(playerPosition.y, moveY);

        return newPosition;
    }

    // Update is called once per frame
    void Update()
    {
        //Get's the time snice the emeny last made an attack
        timeSinceLastHit = Time.time - lastHitTime;

        //After a been hit anamation has finshed, the been hit anamation is set to false
        // if (!animator.GetCurrentAnimatorStateInfo(0).IsName("hit"))
        // {
        //     BeenHit = false;
        //     animator.SetBool("hitme", false);
        // }

        //Makese the kick animation == to false after .3 secounds after it was set to true
        if (attacking && timeSinceLastHit >= .3)
        {
            attacking = false;
            //Debug.Log("hit");
            PlayerHealth.doDamage(2);
        }

        //Debug.Log("remainingDistance: " + enemy.remainingDistance);
        Transform target = GameObject.FindGameObjectWithTag("Player").transform;
        float remainingDistance = Vector2.Distance(transform.position, target.position);

        if (!BeenHit)
        {
            //EnemyRestoreFromHit();
            if (enemyHP <= 0)
            {
                //play a dead animation
                Destroy(gameObject);
            }

            if (enemyHP <= runAwayHP)
            {
                //makes enemy run away
                enemy.Resume();
                SetEnemyToMove();
                enemy.speed = 0.5f;
                animator.speed = 0.3f;
                enemy.destination = GetFurthestPointAfterPlayerToEnemy();
                return;
            }

            if (caughtPlayer)
            {
                if (!animator.GetCurrentAnimatorStateInfo(0).IsName("PlayerKicking") && !attacking)
                {
                    animator.SetBool("PlayerKicking", false);

                }
                if (!attacking && timeSinceLastHit >= 2.5f)
                {
                    StartToAttack();
                }
                return;
            }

            if (remainingDistance > 0)
            {
                SetEnemyToMove();
                SetMoveAnimationDirection();
                enemy.destination = target.position;

                if (remainingDistance > checkDistance)
                {
                    enemy.speed = 2f;
                    animator.speed = 1f;
                }
                else if (remainingDistance <= checkDistance)
                {
                    enemy.speed = 0.5f;
                    animator.speed = 0.2f;
                    //enemy.destination = GetDestination(GetPlayerDirection());
                    //enemy.destination = player.position;
                }
            }
            else if (remainingDistance <= 0)
            { // caught the player
                StartToAttack();
            }


        }
    }

    // void OnCollisionEnter2D(Collision2D coll)
    // {
    // }

    // void OnCollisionExit2D(Collision2D coll)
    // {
    // }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!BeenHit)
        {
            if (other.tag == "Player")
            {
                if (enemyHP > runAwayHP) { caughtPlayer = true; }
                //StartToAttack();
            }
            else if (other.tag == "Enemy")
            {
                Debug.Log("Enemy");
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            caughtPlayer = false;
            if (BeenHit)
            {
                BeenHit = false;
            }
            animator.speed = 1f;
            enemy.Resume();
        }
        else if (other.tag == "Enemy")
        {
            Debug.Log("Enemy");
        }
    }
}