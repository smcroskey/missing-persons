﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Character2D
{
public class BouncePad : MonoBehaviour {

	private Rigidbody2D playerRBody;

	[Tooltip("Set the Force of the Bounce pad. Is multiplied by 100 in script.")]
	[Range(0,10)]
	public float bounceForce;
	void Start()
	{
		bounceForce *= 100;
	}
	void OnCollisionEnter2D(Collision2D col)
	{
		if(col.gameObject.tag == "Player")
		{
			
			playerRBody = col.gameObject.GetComponent<Rigidbody2D>();
			Vector2 playerPos = col.transform.position;
			Vector2 dir = col.contacts[0].point - playerPos;
			dir = -dir.normalized;
			playerRBody.AddForce(dir*bounceForce);
		}
	}
}
}