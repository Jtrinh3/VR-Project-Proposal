﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunUFOScript : EnemyScript
{
    //static variables are shared between game objects
    protected bool notFalling = true;
    protected bool attacking = false;
    protected float dist;
    protected Transform turret;
    protected Coroutine velocityReset;
    protected int counter = 0;
    public static GameObject bullet = null;

    public override void objectIsGrabbed()
    {
        isGrabbed = true;
        wasGrabbed = true;
        attacking = false;
        if (damageRoutine != null)
            StopCoroutine(damageRoutine);
        //StartCoroutine(breakEnemy());
    }

    public override void objectIsReleased()
    {
        isGrabbed = false;
        StartCoroutine(resetVelocity());
    }

    protected IEnumerator resetVelocity()
    {
        checkVelocity();
        yield return new WaitForSeconds(1f);
        GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
        moveTowardTarget();
    }



    protected override void moveTowardTarget()
    {
        if (notFalling)
        {
            // Move our position a step closer to the target.
            float step = curr_speed * Time.deltaTime; // calculate distance to move
            transform.position = Vector3.MoveTowards(transform.position, Target.position, step);

            // Check if the position of the cube and sphere are approximately equal.
            if (Vector3.Distance(transform.position, Target.position) <= 22f || destroyed)
            {
                if (attacking == false)
                {
                    damageRoutine = StartCoroutine(attack());
                    attacking = true;
                }
                if(Vector3.Distance(transform.position, Target.position) < 20f)
                    curr_speed = 0.0f;
            }
            else if(Vector3.Distance(transform.position, Target.position) > 20f)
            {
                curr_speed = speed;
                attacking = false;
                if(damageRoutine != null)
                    StopCoroutine(damageRoutine);
                damageRoutine = null;

            }

        }

        Physics.IgnoreLayerCollision(9, 9, false);
        Physics.IgnoreLayerCollision(9, 0, false);

    }

    protected IEnumerator attack()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            GameObject temp = Object.Instantiate(bullet, new Vector3(turret.position.x, turret.position.y, turret.position.z-1.4f), Quaternion.identity, turret);
            temp.SetActive(true);
        }
    }



    public override IEnumerator breakEnemy()
    {
        yield return new WaitForSeconds(.2f);
        if (!destroyed)
            destroyEnemy();
    }

    protected override void checkVelocity()
    {
        Vector3 velocity = gameObject.GetComponent<Rigidbody>().velocity;
        float deathVelocity = 20;
        if ((velocity.x > deathVelocity || velocity.y > deathVelocity || velocity.z > deathVelocity) && true)
        //if(avgVelocity >= 4f && wasGrabbed)
        { 
            StartCoroutine(breakEnemy());
        }

       // float avgVelocity = (velocity.x + velocity.y + velocity.z) / 3;

    }

    public void allowFall()
    {
        if (!GameObject.Find("Round Tracker").GetComponent<RoundScript>().space)
        {
            gameObject.GetComponent<Rigidbody>().useGravity = true;
            notFalling = false;
        }
    }

    //if colliding with castle begin damage routine
    protected override void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Castle")
        {
            if (damageRoutine == null)
            {
                GameObject c = GameObject.Find("Castle");
                damageRoutine = c.GetComponent<CastleScript>().StartDamage(1); //change inner number for additional damage
                isCollided = true;
            }
        }
        if (collision.gameObject.tag == "Floor")
        {

            Physics.IgnoreCollision(collision.collider, gameObject.GetComponentInChildren<Collider>());

        }
    }


    // Start is called before the first frame update
    protected override void Start()
    {
        speed = 4.0f;
        curr_speed = speed;

        body = transform.GetChild(0).GetChild(0).transform;
        turret = transform.GetChild(0).GetChild(0).GetChild(0).transform;
        Target = GameObject.Find("Tracking Cube").transform;
        bullet = GameObject.Find("Round Tracker").GetComponent<RoundScript>().alienBullet;

        dist = Vector3.Distance(body.position, Target.position);
    }

    protected override void Update()
    {
        base.Update();
        

        //destroys object if they fall through the ground or get too far from the castle
        if (dist > 150)
        {
            destroyEnemy();
        }

        float curr_dist = Vector3.Distance(body.position, Target.position);
        if (dist < curr_dist && !wasGrabbed && notFalling)
        {
            StartCoroutine(resetVelocity());
        }

        //bady facement to target
        if (isGrabbed == false)
        {
            body.LookAt(Target.position - transform.position);
            body.Rotate(new Vector3(-190, 0, 180));
        }

        if(attacking == false && damageRoutine != null)
        {
            StopCoroutine(damageRoutine);
            damageRoutine = null;
        }


    }

}
