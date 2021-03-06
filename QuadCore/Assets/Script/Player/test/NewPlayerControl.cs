﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class NewPlayerControl : MonoBehaviour
{
	public string p_Name;
	public Animator anim;
	public GameObject attackCollider;
	public GameObject defaultCollider;
	public int max_Jump;

	//플레이어 능력치
	public float speed;	//기본 : 2
	public float power;	//기본 : 4
	public float jump;  //기본 : 6

	//플레이어 상태 여부
	public bool isPlaying;
	private int angle;
	private bool isStun;
	private bool isJumpping;
	private bool isFalling;
	private bool isCharging;
	private bool isCharged;

	//카운터
	private float timer_Stun;    //isStun을 대체해주길 바람
	private float timer_Charging;
	private int count_Jump;
	private bool isTriggerOn;
	void Start()
	{
		angle = 0;

		isJumpping = false;
		isFalling = false;
		isPlaying = true;
		isCharging = false;
		isCharged = false;

		timer_Stun = 0f;
		timer_Charging = 0f;
		count_Jump = max_Jump;
		isTriggerOn = false;

        attackCollider.GetComponent<PolygonCollider2D>().enabled = false;
		attackCollider.GetComponent<NewPlayerAttack>().enabled = false;
	}

	void Update()
	{
		if (!(timer_Stun > 0))
			ControllerInput();

		if (isCharging && !isCharged)
		{
			timer_Charging += Time.deltaTime;
			if (timer_Charging > 2)
			{
				isCharged = true;
				anim.SetBool("isCharged", true);
			}
		}
		if (timer_Stun > 0)
		{
			timer_Stun -= Time.deltaTime;
			anim.SetFloat("timer_Stun", timer_Stun);
		}
	}

	void FixedUpdate()
	{
		if(!(timer_Stun > 0))
			Move();

		//떨어지는 중으로 상태 변경
		if (GetComponent<Rigidbody2D>().velocity.y < -1)
		{
			isJumpping = false;
			isFalling = true;
			anim.SetBool("isJumpping", false);
			anim.SetBool("isFalling", true);
			defaultCollider.GetComponent<BoxCollider2D>().enabled = true;
		}
	}

	private void ControllerInput()
	{
		if (Input.GetButtonDown(p_Name + "_A Button"))
		{
			power = 4;	//일반공격 파워
			Attack();
			isCharging = true;
			anim.SetBool("isCharging", true);
		}
		if (Input.GetButtonUp(p_Name + "_A Button"))
		{
			if (isCharged)
			{
				power = 10;	//차징공격 파워
				Attack();
				isCharged = false;
				anim.SetBool("isCharged", false);
			}
			timer_Charging = 0f;
			isCharging = false;
			anim.SetBool("isCharging", false);
		}

		if (Input.GetButtonDown(p_Name + "_B Button") || ( Input.GetAxis(p_Name + "_Triggers") > 0.9f && !isTriggerOn ))
		{
			isTriggerOn = true;
			if(count_Jump > 0)
				Jump();
		}

		//트리거를 놓았는지 확인
		if(isTriggerOn)
			if (Input.GetAxis(p_Name + "_Triggers") < 0.5f)
				isTriggerOn = false;
	}

	private void Move()
	{
		float tempValue = Input.GetAxis(p_Name + "_LeftThumbstickButton");
        int direction = 0;

		if (tempValue > 0.7)	//오른
		{
			direction = 1;
			angle = 0;
			anim.SetBool("isWalking", true);
		}
		else if (tempValue < -0.7)	//왼
		{
			direction = -1;
			angle = 180;
			anim.SetBool("isWalking", true);
		}
		else
		{
			direction = 0;
			anim.SetBool("isWalking", false);
		}

		GetComponent<Rigidbody2D>().velocity = new Vector2(direction * speed, GetComponent<Rigidbody2D>().velocity.y);
		transform.eulerAngles = new Vector3(0, angle, 0);
	}

	private void Jump()
	{
		GetComponent<Rigidbody2D>().velocity = new Vector2(GetComponent<Rigidbody2D>().velocity.x, jump);
		count_Jump--;
		isJumpping = true;
		isFalling = false;
		anim.SetBool("isJumpping", true);
		anim.SetBool("isFalling", false);
		defaultCollider.GetComponent<BoxCollider2D>().enabled = false;
	}

	public void Damaged(float power, int direction)
	{
		timer_Stun = power * 0.25f;
		anim.SetFloat("timer_Stun", timer_Stun);

		GetComponent<Rigidbody2D>().velocity = new Vector2(direction * power, 1 * power);

		Debug.Log(direction * power);

		isCharging = false;
		isCharged = false;
		anim.SetBool("isCharging", false);
		anim.SetBool("isCharged", false);
		timer_Charging = 0;
	}

	public void Die()
	{
		isPlaying = false;
		transform.gameObject.SetActive(false);
		Debug.Log(this.name);
	}
	private void Attack()
	{
		attackCollider.GetComponent<NewPlayerAttack>().enabled = true;
		anim.SetTrigger("trig_Attack");
	}

	private void OnCollisionEnter2D(Collision2D col)
	{
		if (col.gameObject.tag == "Ground" && (isJumpping || isFalling))
		{
			count_Jump = max_Jump; 
			isJumpping = false;
			isFalling = false;
			anim.SetBool("isJumpping", false);
			anim.SetBool("isFalling", false);
			defaultCollider.GetComponent<BoxCollider2D>().enabled = true;
		}
	}

	public bool IsPlaying {get { return isPlaying; } /*set { isPlaying = value; }*/}
	public int Angle {get{return angle;} set{angle = value;}}
	public float Power {get {return power;} set {power = value;}}
	public float StunTimer
	{
		get { return timer_Stun; }
		set { timer_Stun = value; anim.SetFloat("timer_Stun", timer_Stun); }
	}
}