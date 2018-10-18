using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KMBombInfoHelper;

using Random = UnityEngine.Random;

public class PartyTime : MonoBehaviour {
	public GameObject Screen;
	public KMSelectable Dice;
	public KMSelectable[] Spaces;
	public KMBombModule BombModule;
	public KMBombInfo BombInfo;
	public KMAudio BombAudio;

	public Texture2D[] diceNum = new Texture2D[7];
	public Texture2D[] screenNum = new Texture2D[7];
	public Texture2D[] spaceTexA = new Texture2D[0];
	public Texture2D[] spaceTexB = new Texture2D[0];

	int batCountD = 0;
	int batCountAA = 0;
	int indCount = 0;
	bool reverseMove = false;

	int[] nowSpcTex = new int[20];
	int[] isSpcT = new int[20];

	int canRoll = 5;
	bool ranDoNum = true;
	int prevNumber = 0;
	int nowSpace = 0;
	int moveTimer = 0;
	bool pressDice = false;

	bool moduleSolved = false;

	static int moduleIdCounter = 1;
	int moduleId;

	void Start () {
		moduleId = moduleIdCounter++;

		batCountD = BombInfo.GetBatteryCount (KMBombInfoHelper.Battery.D);
		batCountAA = BombInfo.GetBatteryCount (KMBombInfoHelper.Battery.AA);
		indCount = BombInfo.GetIndicators ().Count ();

		Debug.LogFormat (@"[Party Time #{0}] D Battery spaces advance {1} spaces.", moduleId, Mathf.Clamp (batCountD, 0, 6));
		Debug.LogFormat (@"[Party Time #{0}] AA Battery spaces advance {1} spaces.", moduleId, Mathf.Clamp (batCountAA, 0, 6));
		Debug.LogFormat (@"[Party Time #{0}] Indicator spaces regress {1} spaces.", moduleId, Mathf.Clamp (indCount, 0, 6));

		Spaces [0].transform.GetChild (0).gameObject.GetComponent <Renderer> ().material.SetTexture ("_MainTex", spaceTexB [0]);
		nowSpcTex [0] = 0;
		Spaces [19].transform.GetChild (0).gameObject.GetComponent <Renderer> ().material.SetTexture ("_MainTex", spaceTexA [7]);
		nowSpcTex [19] = 7;

		int currSpcBatD = 0;
		int currSpcBatAA = 0;
		int currSpcInd = 0;
		int currSpcWat = 0;
		int currSpcFire = 0;

		for (int i = 1; i < Spaces.Length - 1; i++) {
			int chooseTex = -1;

			while (chooseTex == -1) {
				chooseTex = Random.Range (1, 7);

				switch (chooseTex) {
				case 2:
					currSpcBatD++;

					if (currSpcBatD >= 4 || batCountD == 0) {
						chooseTex = -1;
						currSpcBatD = 4;
					}
					break;

				case 3:
					currSpcBatAA++;

					if (currSpcBatAA >= 4 || batCountAA == 0) {
						chooseTex = -1;
						currSpcBatAA = 4;
					}
					break;

				case 4:
					currSpcInd++;

					if (currSpcInd >= 4 || indCount == 0) {
						chooseTex = -1;
						currSpcInd = 4;
					}
					break;

				case 5:
					currSpcWat++;

					if (currSpcWat >= 5) {
						chooseTex = -1;
						currSpcWat = 5;
					}
					break;

				case 6:
					currSpcFire++;

					if (currSpcFire >= 5) {
						chooseTex = -1;
						currSpcFire = 5;
					}
					break;
				}
			}

			Spaces [i].transform.GetChild (0).gameObject.GetComponent <Renderer> ().material.SetTexture ("_MainTex", spaceTexA [chooseTex]);

			nowSpcTex [i] = chooseTex;
		}

		Debug.LogFormat (@"[Party Time #{0}] There are {1} Water spaces.", moduleId, currSpcWat - (currSpcWat == 5 ? 1 : 0));
		Debug.LogFormat (@"[Party Time #{0}] There are {1} Fire spaces.", moduleId, currSpcFire - (currSpcFire == 5 ? 1 : 0));

		object[] logLetters = new object[21];

		logLetters [0] = moduleId;

		for (int i = 0; i < Spaces.Length; i++) {
			isSpcT [i] = -1;
			int leftSpc = i - 1;
			int rightSpc = i + 1;
			int calcSpc = (((i % 5) * 2) + 1);
			int upSpc = i - calcSpc;
			int downSpc = i + 10 - calcSpc;

			if (nowSpcTex [i] == 5) {
				if ((currSpcFire >= 3 && currSpcWat < 3) || (nowSpcTex [leftSpc] == 2 || nowSpcTex [leftSpc] == 3) || (nowSpcTex [rightSpc] == 2 || nowSpcTex [rightSpc] == 3) || (nowSpcTex [Mathf.Clamp (upSpc, 0, 19)] == 2 || nowSpcTex [Mathf.Clamp (upSpc, 0, 19)] == 3) || (nowSpcTex [Mathf.Clamp (downSpc, 0, 19)] == 2 || nowSpcTex [Mathf.Clamp (downSpc, 0, 19)] == 3)) {
					isSpcT [i] = 0;
				} else {
					isSpcT [i] = 1;
				}
			} else {
				if (nowSpcTex [i] == 6) {
					if ((currSpcWat >= 3 && currSpcFire < 3) || nowSpcTex [leftSpc] == 5 || nowSpcTex [rightSpc] == 5 || nowSpcTex [Mathf.Clamp (upSpc, 0, 19)] == 5 || nowSpcTex [Mathf.Clamp (downSpc, 0, 19)] == 5) {
						isSpcT [i] = 1;
					} else {
						isSpcT [i] = 0;
					}
				}
			}

			if (nowSpcTex [i] == 5 || nowSpcTex [i] == 6) {
				Debug.LogFormat (@"[Party Time #{0}] Space #{1} is {2}.", moduleId, i, isSpcT [i] == 1 ? "correct" : "incorrect");
			}

			/*string[] spcLetter = new string[7] { "S", "B", "A", "I", "W", "F", "G", "/X", "/N", "/Y" };
			 
			logLetters [i + 1] = spcLetters [nowSpcTex [i]] + spcLetters [isSpcT [i] + 8];*/
		}

		//Debug.LogFormat (@"[Party Time #{0}]\n ({1})-({2})-({3})-({4})-({5})\n({10})-({9})-({8})-({7})-({6})\n({11})-({12})-({13})-({14})-({15})\n({20})-({19})-({18})-({17})-({16})", logLetters);

		Dice.OnInteract += delegate() {
			onDicePress ();
			return false;
		};

		for (int i = 0; i < Spaces.Length; i++) {
			int j = i;

			Spaces [i].OnInteract += delegate () {
				onSpacePress (j);
				return false;
			};
		}
	}

	void Update () {
		if (ranDoNum) {
			int rngNumber = prevNumber;

			do {
				rngNumber = Random.Range (1, 7);
			} while (rngNumber == prevNumber);

			prevNumber = rngNumber;

			Dice.transform.GetChild (0).gameObject.GetComponent <Renderer> ().material.SetTexture ("_MainTex", diceNum [rngNumber]);
		} else {
			if (moveTimer == 20) {
				if (prevNumber == 0) {
					moveTimer = 0;

					switch (nowSpcTex [nowSpace]) {
					case 0:
					case 1:
					case 5:
					case 6:
					case 8:
					case 9:
					case 10:
					case 11:
						stopMoving ();
						break;

					case 2:
						if (!reverseMove) {
							prevNumber += Mathf.Clamp (batCountD, 1, 6);
						} else {
							stopMoving ();
						}
						break;

					case 3:
						if (!reverseMove) {
							prevNumber += Mathf.Clamp (batCountAA, 1, 6);
						} else {
							stopMoving ();
						}
						break;
						
					case 4:
						reverseMove = true;
						prevNumber += Mathf.Clamp (indCount, 1, 6);
						break;
					}

					Dice.transform.GetChild (0).gameObject.GetComponent <Renderer> ().material.SetTexture ("_MainTex", diceNum [prevNumber]);
				} else {
					if (nowSpace < 19) {
						Spaces [nowSpace].transform.GetChild (0).gameObject.GetComponent <Renderer> ().material.SetTexture ("_MainTex", spaceTexA [nowSpcTex [nowSpace]]);
						nowSpace += reverseMove == false ? 1 : -1;							
						Spaces [nowSpace].transform.GetChild (0).gameObject.GetComponent <Renderer> ().material.SetTexture ("_MainTex", spaceTexB [nowSpcTex [nowSpace]]);

						if (nowSpace != 0) {
							prevNumber--;
						} else {
							prevNumber = 0;
							reverseMove = false;
						}

						Dice.transform.GetChild (0).gameObject.GetComponent <Renderer> ().material.SetTexture ("_MainTex", diceNum [prevNumber]);

						if (nowSpcTex [nowSpace] != 5 && nowSpcTex [nowSpace] != 6) {
							moveTimer = 0;
						} else {
							moveTimer = 21;
							pressDice = true;
						}
					}
				}
			}

			moveTimer++;

			if (moveTimer > 21) {
				moveTimer = 21;
			}
		}

		if (nowSpace == 19) {
			if (!moduleSolved) {
				BombModule.HandlePass ();
				Debug.LogFormat (@"[Party Time #{0}] Module solved!", moduleId);
				moduleSolved = true;
			}
		}
	}

	void stopMoving () {
		if (nowSpace != 19) {
			if (canRoll == 0) {
				Spaces [nowSpace].transform.GetChild (0).gameObject.GetComponent <Renderer> ().material.SetTexture ("_MainTex", spaceTexA [nowSpcTex [nowSpace]]);
				nowSpace = 0;
				Spaces [nowSpace].transform.GetChild (0).gameObject.GetComponent <Renderer> ().material.SetTexture ("_MainTex", spaceTexB [nowSpcTex [nowSpace]]);

				canRoll = 5;
				Screen.transform.GetChild (0).gameObject.GetComponent <Renderer> ().material.SetTexture ("_MainTex", screenNum [canRoll]);
			}

			ranDoNum = true;
			reverseMove = false;
		}
	}

	void onDicePress () {
		BombAudio.PlayGameSoundAtTransform (KMSoundOverride.SoundEffect.ButtonPress, transform);
		GetComponent <KMSelectable> ().AddInteractionPunch ();

		if (moduleSolved) {
			return;
		};

		if (ranDoNum) {
			moveTimer = 0;

			if (canRoll > 0) {
				canRoll--;

				Screen.transform.GetChild (0).gameObject.GetComponent <Renderer> ().material.SetTexture ("_MainTex", screenNum [canRoll]);
			}

			ranDoNum = false;
		} else {
			if (moveTimer == 21 && pressDice) {
				if (isSpcT [nowSpace] == 0) {
					if (nowSpcTex [nowSpace] == 5) {
						Spaces [nowSpace].transform.GetChild (0).gameObject.GetComponent <Renderer> ().material.SetTexture ("_MainTex", spaceTexB [8]);
						nowSpcTex [nowSpace] = 8;
					} else {
						if (nowSpcTex [nowSpace] == 6) {
							Spaces [nowSpace].transform.GetChild (0).gameObject.GetComponent <Renderer> ().material.SetTexture ("_MainTex", spaceTexB [10]);
							nowSpcTex [nowSpace] = 10;
						}
					}

					BombAudio.PlaySoundAtTransform ("SoundCorrect", this.transform);
					Debug.LogFormat (@"[Party Time #{0}] You pressed the dice when space #{1} was incorrect, which is correct.", moduleId, nowSpace);
				} else {
					if (nowSpcTex [nowSpace] == 5) {
						Spaces [nowSpace].transform.GetChild (0).gameObject.GetComponent <Renderer> ().material.SetTexture ("_MainTex", spaceTexB [9]);
						nowSpcTex [nowSpace] = 9;
					} else {
						if (nowSpcTex [nowSpace] == 6) {
							Spaces [nowSpace].transform.GetChild (0).gameObject.GetComponent <Renderer> ().material.SetTexture ("_MainTex", spaceTexB [11]);
							nowSpcTex [nowSpace] = 11;
						}
					}

					BombModule.HandleStrike ();
					Debug.LogFormat (@"[Party Time #{0}] You pressed the dice when space #{1} was correct, which is incorrect.", moduleId, nowSpace);
				}

				moveTimer = 0;
				pressDice = false;
			}
		}
	}

	void onSpacePress (int spacePressed) {
		if (nowSpace == spacePressed && (nowSpcTex [nowSpace] == 5 || nowSpcTex [nowSpace] == 6)) {
			if (isSpcT [nowSpace] != -1) {
				BombAudio.PlayGameSoundAtTransform (KMSoundOverride.SoundEffect.ButtonPress, transform);
				GetComponent <KMSelectable> ().AddInteractionPunch ();

				if (isSpcT [nowSpace] == 1) {
					if (nowSpcTex [nowSpace] == 5) {
						Spaces [nowSpace].transform.GetChild (0).gameObject.GetComponent <Renderer> ().material.SetTexture ("_MainTex", spaceTexB [8]);
						nowSpcTex [nowSpace] = 8;
					} else {
						if (nowSpcTex [nowSpace] == 6) {
							Spaces [nowSpace].transform.GetChild (0).gameObject.GetComponent <Renderer> ().material.SetTexture ("_MainTex", spaceTexB [10]);
							nowSpcTex [nowSpace] = 10;
						}
					}

					BombAudio.PlaySoundAtTransform ("SoundCorrect", this.transform);
					Debug.LogFormat (@"[Party Time #{0}] You pressed space #{1} when it was correct, which is correct.", moduleId, spacePressed);
				} else {
					if (nowSpcTex [nowSpace] == 5) {
						Spaces [nowSpace].transform.GetChild (0).gameObject.GetComponent <Renderer> ().material.SetTexture ("_MainTex", spaceTexB [9]);
						nowSpcTex [nowSpace] = 9;
					} else {
						if (nowSpcTex [nowSpace] == 6) {
							Spaces [nowSpace].transform.GetChild (0).gameObject.GetComponent <Renderer> ().material.SetTexture ("_MainTex", spaceTexB [11]);
							nowSpcTex [nowSpace] = 11;
						}
					}

					BombModule.HandleStrike ();
					Debug.LogFormat (@"[Party Time #{0}] You pressed space #{1} when it was incorrect, which is incorrect.", moduleId, spacePressed);
				}

				moveTimer = 0;
			}
		}
	}
}