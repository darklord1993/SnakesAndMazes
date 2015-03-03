using UnityEngine;
using System.Collections;

public class Snakehead : MonoBehaviour {


	public enum Direction {right, up, left, down};

	public SnakeHeadBounder leftS, rightS, upS, downS;

	public Direction currentDirection, lastDirection;

	public int delayTimer, delayMax;

	public Vector3 forward, prevPosition, prevForward, right, left, up, down;

	public bool canMoveLeft, canMoveDown, canMoveRight, canMoveUp;

	public int collisionCode;

	// Use this for initialization
	void Start () 
	{
		renderer.material.color = Color.blue;

		currentDirection = Direction.right;
		lastDirection = Direction.right;

		delayMax = 50;

		delayTimer = 0;

		forward = new Vector3 (2, 0, 0);

		collisionCode = 0;

		left = new Vector3(-2,0,0);
		right = new Vector3(2,0,0);
		up = new Vector3(0,0,2);
		down = new Vector3(0,0,-2);

	}
	
	// Update is called once per frame
	void Update () 
	{

		canMoveDown = downS.canMove;
		canMoveUp = upS.canMove;
		canMoveLeft = leftS.canMove;
		canMoveRight = rightS.canMove;

		if (delayTimer == 1) {
			CheckDirections ();
			delayTimer++;
		}
		else if (delayTimer >= delayMax) 
		{
			delayTimer=0;
			transform.position+=forward;



		} 
		else
			delayTimer++;

		prevPosition = transform.position;
		prevForward = forward;

	}

	void checkPaths()
	{
		for (collisionCode = 0; collisionCode > 4; collisionCode++) 
		{
			transform.position += forward;
			transform.position -= forward;
		}
	}

	void OnCollisionEnter(Collision collision) 
	{

		if (collision.gameObject.tag == "GameController") 
		{

			transform.position -= forward;
			changeDirection (collisionCode);
			//renderer.material.color = Color.blue;
		}
		if (collision.gameObject.tag == "Player") {
			collision.gameObject.transform.position=new Vector3(38.9f,132.2f,4.1f);
			collision.gameObject.rigidbody.useGravity=false;
			Debug.Log("Player Eat'ed-ed");
		}

	}

	void OnTriggerEnter(Collider col)
	{
		if (col.gameObject.tag == "Finish") {
			transform.position+=new Vector3(0,100,0);
			Debug.Log("Snake got too hungry");
		}
	}

	void CheckDirections()
	{
		int randDir = (int)Random.Range (0, 4);

		if (currentDirection == Direction.right) {
			if (canMoveRight) {
				if (canMoveUp && canMoveDown) {
					if (randDir == 0) {
						currentDirection = Direction.up;
						forward = up;
					} else if (randDir == 1) {
						currentDirection = Direction.down;
						forward = down;
					}
				} else if (canMoveUp && !canMoveDown) {
					if (randDir == 0 || randDir == 1) {
						currentDirection = Direction.up;
						forward = up;
					}
				} else if (!canMoveUp && canMoveDown) {
					if (randDir == 0 || randDir == 1) {
						currentDirection = Direction.down;
						forward = down;
					}
				}
			} else {
				if (canMoveUp && canMoveDown) {
					if (randDir == 0||randDir == 1) {
						currentDirection = Direction.up;
						forward = up;
					} else if (randDir == 2||randDir == 3) {
						currentDirection = Direction.down;
						forward = down;
					}
				} else if (canMoveUp && !canMoveDown) {
						currentDirection = Direction.up;
						forward = up;
				} else if (!canMoveUp && canMoveDown) {
						currentDirection = Direction.down;
						forward = down;
				}
			}
		}
		else if (currentDirection == Direction.left) {
			if (canMoveLeft) {
				if (canMoveUp && canMoveDown) {
					if (randDir == 0) {
						currentDirection = Direction.up;
						forward = up;
					} else if (randDir == 1) {
						currentDirection = Direction.down;
						forward = down;
					}
				} else if (canMoveUp && !canMoveDown) {
					if (randDir == 0 || randDir == 1) {
						currentDirection = Direction.up;
						forward = up;
					}
				} else if (!canMoveUp && canMoveDown) {
					if (randDir == 0 || randDir == 1) {
						currentDirection = Direction.down;
						forward = down;
					}
				}
			} else {
				if (canMoveUp && canMoveDown) {
					if (randDir == 0||randDir == 1) {
						currentDirection = Direction.up;
						forward = up;
					} else if (randDir == 2||randDir == 3) {
						currentDirection = Direction.down;
						forward = down;
					}
				} else if (canMoveUp && !canMoveDown) {
					currentDirection = Direction.up;
					forward = up;
				} else if (!canMoveUp && canMoveDown) {
					currentDirection = Direction.down;
					forward = down;
				}
			}
		}
		else if (currentDirection == Direction.up) {
			if (canMoveUp) {
				if (canMoveLeft && canMoveRight) {
					if (randDir == 0) {
						currentDirection = Direction.left;
						forward = left;
					} else if (randDir == 1) {
						currentDirection = Direction.right;
						forward = right;
					}
				} else if (canMoveLeft && !canMoveRight) {
					if (randDir == 0 || randDir == 1) {
						currentDirection = Direction.left;
						forward = left;
					}
				} else if (!canMoveLeft && canMoveRight) {
					if (randDir == 0 || randDir == 1) {
						currentDirection = Direction.right;
						forward = right;
					}
				}
			} else {
				if (canMoveLeft && canMoveRight) {
					if (randDir == 0||randDir == 1) {
						currentDirection = Direction.left;
						forward = left;
					} else if (randDir == 2||randDir == 3) {
						currentDirection = Direction.right;
						forward = right;
					}
				} else if (canMoveLeft && !canMoveRight) {
					currentDirection = Direction.left;
					forward = left;
				} else if (!canMoveLeft && canMoveRight) {
					currentDirection = Direction.right;
					forward = right;
				}
			}
		}
		else if (currentDirection == Direction.down) {
			if (canMoveDown) {
				if (canMoveLeft && canMoveRight) {
					if (randDir == 0) {
						currentDirection = Direction.left;
						forward = left;
					} else if (randDir == 1) {
						currentDirection = Direction.right;
						forward = right;
					}
				} else if (canMoveLeft && !canMoveRight) {
					if (randDir == 0 || randDir == 1) {
						currentDirection = Direction.left;
						forward = left;
					}
				} else if (!canMoveLeft && canMoveRight) {
					if (randDir == 0 || randDir == 1) {
						currentDirection = Direction.right;
						forward = right;
					}
				}
			} else {
				if (canMoveLeft && canMoveRight) {
					if (randDir == 0||randDir == 1) {
						currentDirection = Direction.left;
						forward = left;
					} else if (randDir == 2||randDir == 3) {
						currentDirection = Direction.right;
						forward = right;
					}
				} else if (canMoveLeft && !canMoveRight) {
					currentDirection = Direction.left;
					forward = left;
				} else if (!canMoveLeft && canMoveRight) {
					currentDirection = Direction.right;
					forward = right;
				}
			}
		}
	}
	
	void changeDirection(int code)
	{
		if (currentDirection == Direction.right) 
		{
			forward = new Vector3(0,0,2);
			currentDirection = Direction.up;
		}
		else if (currentDirection == Direction.up) {
			forward = new Vector3(-2,0,0);
			currentDirection = Direction.left;
		}
		else if (currentDirection == Direction.left) {
			forward = new Vector3(0,0,-2);
			currentDirection = Direction.down;
		}
		else if (currentDirection == Direction.down) {
			forward = new Vector3(2,0,0);
			currentDirection = Direction.right;
		}
	}
}
