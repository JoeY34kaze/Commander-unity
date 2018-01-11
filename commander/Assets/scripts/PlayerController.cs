﻿using System;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour {
    public float MoveSpeed = 7.5f;
	public float JumpForce = 15f;
	public float Gravity = 6f;

	private Vector3 _movement = Vector3.zero;

	private CharacterController _cc;
	private Camera _oc;
	private Transform _po;

	public int Score = 0;
	public bool HasKey = false;

	private Animator _anim;

	// Use this for initialization
	void Start ()
	{
        _cc = GetComponent<CharacterController> ();
		_po = transform.Find("PlayerObject");
		_oc = transform.Find("OverviewCamera").GetComponent<Camera>();
		_anim = _po.GetComponent<Animator>();
	}

	// Update is called once per frame
	void Update ()
	{
		MovePlayer();
		// Kill player if it falls off
		if(_cc.transform.position.y < -20) PlayerKill();
		
		// If key for map is held, show overview camera
		if (Input.GetButton("Map"))
		{
			_oc.enabled = true;
		}
		else
		{
			_oc.enabled = false;
		}
		
	}
	
	private Vector3 handleKeyInput()
	{
		return Vector3.Normalize(new Vector3(-Input.GetAxis("Vertical"), 0, Input.GetAxis("Horizontal")));
	}

	private void MovePlayer()
	{
		// Move on the given movement axis
		Vector3 moveDirection = handleKeyInput();
		moveDirection *= MoveSpeed;
		_movement.x = moveDirection.x;
		_movement.z = moveDirection.z;
		
		_anim.SetFloat("Speed", moveDirection.magnitude);
		
		// Restrict x movement between -5 and +5
		if (_cc.transform.position.x > +5 && _movement.x > 0) _movement.x = 0;
		if (_cc.transform.position.x < -5 && _movement.x < 0) _movement.x = 0;
		
		// Apply gravity if we are in the air
		if (!_cc.isGrounded)
		{
			_movement.y -= Gravity / 10;
		}
		else
		{
			_movement.y = -5;
		}

		// Apply jump force on jump click
		if (_cc.isGrounded)
		{
			_anim.SetTrigger("Grounded");
			_anim.ResetTrigger("Jump");
			if (Input.GetButton("Jump"))
			{
				_anim.SetTrigger("Jump");

				_movement.y = JumpForce;
			}
		}
		else
		{
			_anim.ResetTrigger("Grounded");
		}

		// Apply all calculated movements above
		_cc.Move(_movement * Time.deltaTime);

		// Apply rotation if we are moving by at least the threshold below
		const float thr = 0.05f;
		if(Math.Abs(moveDirection.z) > thr || Math.Abs(moveDirection.x) > thr)
			_po.transform.rotation = Quaternion.LookRotation(moveDirection);
		
	}

	public void PlayerKill()
	{
		_cc.transform.position = Vector3.zero;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.GetComponent<Switch>())
		{
			Switch s = other.GetComponent<Switch>();
			s.TurnSwitch();
		} else if (other.GetComponent<Pickup>())
		{
			other.gameObject.SetActive(false);
			int giveScore = other.GetComponent<Pickup>().ScoreAmount;
			Score += giveScore;
			GameObject.Find("Score").GetComponent<Score>().setScore(Score);
			GameObject.Find("OnPickup").GetComponent<Text>().text = "+" + giveScore;
			GameObject.Find("OnPickup").GetComponent<Animator>().Play("Pickup");
		} else if (other.GetComponent<Key>())
		{
			HasKey = true;
			other.gameObject.SetActive(false);
			GameObject.Find("HasKeyUI").GetComponent<Animator>().Play("HasKey");
			GameObject.Find("OnPickup").GetComponent<Text>().text = "You have the Key";
			GameObject.Find("OnPickup").GetComponent<Animator>().Play("Pickup");
		} else if (other.GetComponent<Spikes>())
		{
			PlayerKill();
		} else if (other.GetComponent<Door>())
		{
			
		}
	}
}
