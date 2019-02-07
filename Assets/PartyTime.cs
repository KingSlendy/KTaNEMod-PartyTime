using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using KMBombInfoHelper;
using UnityEngine;

using Random = UnityEngine.Random;

public class PartyTime : MonoBehaviour {
	public KMAudio BombAudio;
	public KMBombInfo BombInfo;
	public KMBombModule BombModule;
	public KMSelectable[] Spaces;
	public KMSelectable Dice;
	public GameObject Screen;
	public Texture2D[] diceNum = new Texture2D[7];
	public Texture2D[] screenNum = new Texture2D[7];
	public Texture2D[] spaceTexA = new Texture2D[0];
	public Texture2D[] spaceTexB = new Texture2D[0];

	int batCountD, batCountAA, indCount;
	bool landedBat, reverseMove;

	readonly int[] nowSpcTex = new int[20];
	readonly int[] isSpcT = new int[20];

	int canRoll = 5;
	bool ranDoNum = true;
	int prevNumber, nowSpace, moveTimer;
	bool pressDice;

	List<int> tpDie = new List<int>();
	List<int> tpSpaces = new List<int>();
	readonly Color[] tpColors = { new Color32(227, 201, 23, 255), new Color32(255, 0, 255, 255), Color.red };
	bool tpRoll;

	bool moduleSolved;

	static int moduleIdCounter = 1;
	int moduleId;

	void Start() {
		moduleId = moduleIdCounter++;

		batCountD = BombInfo.GetBatteryCount(KMBombInfoHelper.Battery.D);
		batCountAA = BombInfo.GetBatteryCount(KMBombInfoHelper.Battery.AA) + BombInfo.GetBatteryCount(KMBombInfoHelper.Battery.AAx3) + BombInfo.GetBatteryCount(KMBombInfoHelper.Battery.AAx4);
		indCount = BombInfo.GetIndicators().Count();

		Debug.LogFormat(@"[Party Time #{0}] D Battery spaces advance {1} spaces.", moduleId, Mathf.Clamp(batCountD, 0, 6));
		Debug.LogFormat(@"[Party Time #{0}] AA Battery spaces advance {1} spaces.", moduleId, Mathf.Clamp(batCountAA, 0, 6));
		Debug.LogFormat(@"[Party Time #{0}] Indicator spaces regress {1} spaces.", moduleId, Mathf.Clamp(indCount, 0, 6));

		Spaces[0].transform.GetChild(0).GetComponent<Renderer>().material.SetTexture("_MainTex", spaceTexB[0]);
		nowSpcTex[0] = 0;
		Spaces[19].transform.GetChild(0).GetComponent<Renderer>().material.SetTexture("_MainTex", spaceTexA[7]);
		nowSpcTex[19] = 7;

		var currSpcBatD = 0;
		var currSpcBatAA = 0;
		var currSpcInd = 0;
		var currSpcWat = 0;
		var currSpcFire = 0;

		for (int i = 1; i < Spaces.Length - 1; i++) {
			int chooseTex = -1;

			while (chooseTex == -1) {
				chooseTex = Random.Range(1, 7);

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

						if (currSpcInd >= 3 || indCount == 0) {
							chooseTex = -1;
							currSpcInd = 3;
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

			Spaces[i].transform.GetChild(0).GetComponent<Renderer>().material.SetTexture("_MainTex", spaceTexA[chooseTex]);

			nowSpcTex[i] = chooseTex;
		}

		Debug.LogFormat(@"[Party Time #{0}] There are {1} Water spaces.", moduleId, currSpcWat - (currSpcWat == 5 ? 1 : 0));
		Debug.LogFormat(@"[Party Time #{0}] There are {1} Fire spaces.", moduleId, currSpcFire - (currSpcFire == 5 ? 1 : 0));

		object[] logLetters = new object[21];
		logLetters[0] = moduleId;
		var spaceLetters = new[] { "S", " ", "D", "A", "I", "W", "F", "G" };

		for (int i = 0; i < Spaces.Length; i++) {
			isSpcT[i] = -1;
			var leftSpc = i - 1;
			var rightSpc = i + 1;
			var calcSpc = (((i % 5) * 2) + 1);
			var upSpc = i - calcSpc;
			var downSpc = i + 10 - calcSpc;

			if (nowSpcTex[i] == 5) {
				if ((currSpcFire > 3 && currSpcWat < 3) || (nowSpcTex[leftSpc] == 2 || nowSpcTex[leftSpc] == 3) || (nowSpcTex[rightSpc] == 2 || nowSpcTex[rightSpc] == 3) || (nowSpcTex[Mathf.Clamp(upSpc, 0, 19)] == 2 || nowSpcTex[Mathf.Clamp(upSpc, 0, 19)] == 3) || (nowSpcTex[Mathf.Clamp(downSpc, 0, 19)] == 2 || nowSpcTex[Mathf.Clamp(downSpc, 0, 19)] == 3)) {
					isSpcT[i] = 0;
				} else {
					isSpcT[i] = 1;
				}
			} else {
				if (nowSpcTex[i] == 6) {
					if ((currSpcWat > 3 && currSpcFire < 3) || nowSpcTex[leftSpc] == 5 || nowSpcTex[rightSpc] == 5 || nowSpcTex[Mathf.Clamp(upSpc, 0, 19)] == 5 || nowSpcTex[Mathf.Clamp(downSpc, 0, 19)] == 5) {
						isSpcT[i] = 1;
					} else {
						isSpcT[i] = 0;
					}
				}
			}

			logLetters[i + 1] = spaceLetters[nowSpcTex[i]];

			if (nowSpcTex[i] == 5 || nowSpcTex[i] == 6) {
				Debug.LogFormat(@"[Party Time #{0}] Space #{1} is {2}.", moduleId, i, isSpcT[i] == 1 ? "correct" : "incorrect");
			}
		}

		Debug.LogFormat(@"[Party Time #{0}] Current board:", moduleId);
		var nowLog = logLetters.Skip(1).Take(5).ToArray();
		Debug.LogFormat(@"[Party Time #{0}] ({1})-({2})-({3})-({4})-({5})", moduleId, nowLog[0], nowLog[1], nowLog[2], nowLog[3], nowLog[4]);
		nowLog = logLetters.Skip(6).Take(5).ToArray();
		Debug.LogFormat(@"[Party Time #{0}] ({5})-({4})-({3})-({2})-({1})", moduleId, nowLog[0], nowLog[1], nowLog[2], nowLog[3], nowLog[4]);
		nowLog = logLetters.Skip(11).Take(5).ToArray();
		Debug.LogFormat(@"[Party Time #{0}] ({1})-({2})-({3})-({4})-({5})", moduleId, nowLog[0], nowLog[1], nowLog[2], nowLog[3], nowLog[4]);
		nowLog = logLetters.Skip(16).Take(5).ToArray();
		Debug.LogFormat(@"[Party Time #{0}] ({5})-({4})-({3})-({2})-({1})", moduleId, nowLog[0], nowLog[1], nowLog[2], nowLog[3], nowLog[4]);

		Dice.OnInteract += delegate() {
			OnDicePress();

			return false;
		};

		for (int i = 0; i < Spaces.Length; i++) {
			int j = i;

			Spaces[i].OnInteract += delegate () {
				OnSpacePress(j);

				return false;
			};
		}
	}

	void Update() {
		if (ranDoNum) {
			int rngNumber = prevNumber;

			do {
				rngNumber = Random.Range(1, 7);
			} while (rngNumber == prevNumber);

			prevNumber = rngNumber;
			Dice.transform.GetChild(0).GetComponent<Renderer>().material.SetTexture("_MainTex", diceNum[rngNumber]);
		} else {
			if (moveTimer == 20) {
				if (prevNumber == 0) {
					moveTimer = 0;

					switch (nowSpcTex[nowSpace]) {
						case 0:
						case 1:
						case 5:
						case 6:
						case 8:
						case 9:
						case 10:
						case 11:
							StopMoving();
							break;

						case 2:
							if (!reverseMove) {
								landedBat = true;
								prevNumber += Mathf.Clamp(batCountD, 1, 6);
							} else {
								StopMoving();
							}
							break;

						case 3:
							if (!reverseMove) {
								landedBat = true;
								prevNumber += Mathf.Clamp(batCountAA, 1, 6);
							} else {
								StopMoving();
							}
							break;
						
						case 4:
							if (!landedBat) {
								reverseMove = true;
								prevNumber += Mathf.Clamp(indCount, 1, 6);
							} else {
								StopMoving();
							}
							break;
					}

					Dice.transform.GetChild(0).GetComponent<Renderer>().material.SetTexture("_MainTex", diceNum[prevNumber]);
				} else {
					if (nowSpace < 19) {
						Spaces[nowSpace].transform.GetChild(0).GetComponent<Renderer>().material.SetTexture("_MainTex", spaceTexA[nowSpcTex[nowSpace]]);
						nowSpace += (!reverseMove) ? 1 : -1;
						Spaces[nowSpace].transform.GetChild(0).GetComponent<Renderer>().material.SetTexture("_MainTex", spaceTexB[nowSpcTex[nowSpace]]);

						if (nowSpace != 0) {
							prevNumber--;
						} else {
							prevNumber = 0;
							landedBat = false;
							reverseMove = false;
						}

						Dice.transform.GetChild(0).GetComponent<Renderer>().material.SetTexture("_MainTex", diceNum[prevNumber]);

						if (nowSpcTex[nowSpace] != 5 && nowSpcTex[nowSpace] != 6) {
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
				BombModule.HandlePass();
				Debug.LogFormat(@"[Party Time #{0}] Module solved!", moduleId);
				moduleSolved = true;
			}
		} else {
            if (tpRoll) {
                if (tpDie.Contains(nowSpace)) {
                    OnDicePress();
                    tpDie.Remove(nowSpace);
                }

                if (tpSpaces.Contains(nowSpace)) {
                    OnSpacePress(nowSpace);
                    tpSpaces.Remove(nowSpace);
                }
            }
		}
	}

	void StopMoving() {
		if (nowSpace != 19) {
			if (canRoll == 0) {
				Spaces[nowSpace].transform.GetChild(0).GetComponent<Renderer>().material.SetTexture("_MainTex", spaceTexA[nowSpcTex[nowSpace]]);
				nowSpace = 0;
				Spaces[nowSpace].transform.GetChild(0).GetComponent<Renderer>().material.SetTexture("_MainTex", spaceTexB[nowSpcTex[nowSpace]]);

				canRoll = 5;
				Screen.transform.GetChild(0).GetComponent<Renderer>().material.SetTexture("_MainTex", screenNum[canRoll]);
			}

			ranDoNum = true;
			landedBat = false;
			reverseMove = false;

			if (tpRoll) {
				prevNumber = Random.Range(1, 7);
				Dice.transform.GetChild(0).GetComponent<Renderer>().material.SetTexture("_MainTex", diceNum[prevNumber]);
				OnDicePress();
			}
		}
	}

	void OnDicePress() {
		BombAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
		GetComponent<KMSelectable>().AddInteractionPunch();

		if (moduleSolved) {
			return;
		}
		;

		if (ranDoNum) {
			moveTimer = 0;

			if (canRoll > 0) {
				canRoll--;

				Screen.transform.GetChild(0).GetComponent<Renderer>().material.SetTexture("_MainTex", screenNum[canRoll]);
			}

			ranDoNum = false;
		} else {
			if (moveTimer == 21 && pressDice) {
				if (isSpcT[nowSpace] == 0) {
					if (nowSpcTex[nowSpace] == 5) {
						Spaces[nowSpace].transform.GetChild(0).GetComponent<Renderer>().material.SetTexture("_MainTex", spaceTexB[8]);
						nowSpcTex[nowSpace] = 8;
					} else {
						if (nowSpcTex[nowSpace] == 6) {
							Spaces[nowSpace].transform.GetChild(0).GetComponent<Renderer>().material.SetTexture("_MainTex", spaceTexB[10]);
							nowSpcTex[nowSpace] = 10;
						}
					}

					BombAudio.PlaySoundAtTransform("SoundCorrect", transform);
					Debug.LogFormat(@"[Party Time #{0}] You pressed the die when space #{1} was incorrect, which is correct.", moduleId, nowSpace);
				} else {
					if (nowSpcTex[nowSpace] == 5) {
						Spaces[nowSpace].transform.GetChild(0).GetComponent<Renderer>().material.SetTexture("_MainTex", spaceTexB[9]);
						nowSpcTex[nowSpace] = 9;
					} else {
						if (nowSpcTex[nowSpace] == 6) {
							Spaces[nowSpace].transform.GetChild(0).GetComponent<Renderer>().material.SetTexture("_MainTex", spaceTexB[11]);
							nowSpcTex[nowSpace] = 11;
						}
					}

                    tpRoll = false;
					BombModule.HandleStrike();
					Debug.LogFormat(@"[Party Time #{0}] You pressed the die when space #{1} was correct, which is incorrect.", moduleId, nowSpace);
				}

				moveTimer = 0;
				pressDice = false;
			}
		}
	}

	void OnSpacePress(int spacePressed) {
		if (nowSpace == spacePressed && (nowSpcTex[nowSpace] == 5 || nowSpcTex[nowSpace] == 6)) {
			if (isSpcT[nowSpace] != -1) {
				BombAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
				GetComponent<KMSelectable>().AddInteractionPunch();

				if (isSpcT[nowSpace] == 1) {
					if (nowSpcTex[nowSpace] == 5) {
						Spaces[nowSpace].transform.GetChild(0).GetComponent<Renderer>().material.SetTexture("_MainTex", spaceTexB[8]);
						nowSpcTex[nowSpace] = 8;
					} else {
						if (nowSpcTex[nowSpace] == 6) {
							Spaces[nowSpace].transform.GetChild(0).GetComponent<Renderer>().material.SetTexture("_MainTex", spaceTexB[10]);
							nowSpcTex[nowSpace] = 10;
						}
					}

					BombAudio.PlaySoundAtTransform("SoundCorrect", transform);
					Debug.LogFormat(@"[Party Time #{0}] You pressed space #{1} when it was correct, which is correct.", moduleId, spacePressed);
				} else {
					if (nowSpcTex[nowSpace] == 5) {
						Spaces[nowSpace].transform.GetChild(0).GetComponent<Renderer>().material.SetTexture("_MainTex", spaceTexB[9]);
						nowSpcTex[nowSpace] = 9;
					} else {
						if (nowSpcTex[nowSpace] == 6) {
							Spaces[nowSpace].transform.GetChild(0).GetComponent<Renderer>().material.SetTexture("_MainTex", spaceTexB[11]);
							nowSpcTex[nowSpace] = 11;
						}
					}

                    tpRoll = false;
					BombModule.HandleStrike();
					Debug.LogFormat(@"[Party Time #{0}] You pressed space #{1} when it was incorrect, which is incorrect.", moduleId, spacePressed);
				}

				moveTimer = 0;
			}
		}
	}

	#pragma warning disable 414
	private readonly string TwitchHelpMessage = @"!{0} roll start/stop (starts/stops rolling) (it disables after a strike) | !{0} die 1 2 3... (presses the die when you land on the specified spaces [Star space is 0] [die 0 to clear list]) | !{0} space 1 2 3... (presses the specified spaces when you land on them [Star space is 0] [space 0 to clear list]) | if auto-roll is disabled, space/die lists will not activate when landing in a space; make sure to enable it again after a strike.";
	#pragma warning restore 414

	KMSelectable[] ProcessTwitchCommand(string command) {
		command = command.ToLowerInvariant().Trim();

		if (Regex.IsMatch(command, @"^roll (start|stop)$")) {
			command = command.Substring(5).Trim();
			tpRoll = (command.Equals("start"));

			return (moveTimer != 21 && tpRoll) ? new[] { Dice } : new[] { Spaces[0] };
		}

		if (Regex.IsMatch(command, @"^die +[0-9^, |&]+$")) {
			tpDie.Clear();
			command = command.Substring(4).Trim();

			var spaces = command.Split(new[] { ',', ' ', '|', '&' }, System.StringSplitOptions.RemoveEmptyEntries);

			for (int i = 0; i < spaces.Length; i++) {
				if (Regex.IsMatch(spaces[i], @"^[0-9]{1,2}$")) {
					var spaceInt = int.Parse(spaces[i].ToString());

					if (spaceInt > 0 && spaceInt < 19 && !tpSpaces.Contains(spaceInt)) {
						tpDie.Add(spaceInt);
					}
				}
			}

			for (int i = 0; i < Spaces.Length; i++) {
				if (!tpSpaces.Contains(i)) {
					Spaces[i].GetComponent<Renderer>().material.color = (tpDie.Contains(i)) ? tpColors[2] : tpColors[0];
				}
			}

			return (tpDie.Count > 0) ? new[] { Spaces[0] } : null;
		}

		if (Regex.IsMatch(command, @"^space +[0-9^, |&]+$")) {
			tpSpaces.Clear();
			command = command.Substring(6).Trim();

			var spaces = command.Split(new[] { ',', ' ', '|', '&' }, System.StringSplitOptions.RemoveEmptyEntries);

			for (int i = 0; i < spaces.Length; i++) {
				if (Regex.IsMatch(spaces[i], @"^[0-9]{1,2}$")) {
					var spaceInt = int.Parse(spaces[i].ToString());

					if (spaceInt > 0 && spaceInt < 19 && !tpDie.Contains(spaceInt)) {
						tpSpaces.Add(spaceInt);
					}
				}
			}

			for (int i = 0; i < Spaces.Length; i++) {
				if (!tpDie.Contains(i)) {
					Spaces[i].GetComponent<Renderer>().material.color = (tpSpaces.Contains(i)) ? tpColors[1] : tpColors[0];
				}
			}

			return (tpSpaces.Count > 0) ? new[] { Spaces[0] } : null;
		}

		return null;
	}
}