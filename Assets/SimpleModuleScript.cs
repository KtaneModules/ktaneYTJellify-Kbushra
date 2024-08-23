using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using KModkit;
using Newtonsoft.Json;
using System.Linq;
using System.Text.RegularExpressions;
using Rnd = UnityEngine.Random;

public class SimpleModuleScript : MonoBehaviour {

	public KMAudio audio;
	public KMBombInfo info;
	public KMBombModule module;
	public KMSelectable[] competitions;
	static int ModuleIdCounter = 1;
	int ModuleId;

	public TextMesh[] screenTexts;

	public int textMessage1;
	public float number;
	public string textFinder1;

	public GameObject[] Podiums;

	bool _isSolved = false;
	bool incorrect = false;


	void Awake() 
	{
		ModuleId = ModuleIdCounter++;

		foreach (KMSelectable button in competitions)
		{
			KMSelectable pressedButton = button;
			button.OnInteract += delegate () { competitionDecider(pressedButton); return false; };
		}
	}

	void Start ()
	{
		textMessage1 = Rnd.Range(1, 55);
		Calculate ();
		for (int i = 0; i < textMessage1; i++)
			textFinder1 = textMessage1.ToString();
		screenTexts[0].text = textFinder1;

		Debug.LogFormat("[Contest IDs #{0}] You are contestant no {1}, your ID is {2}", ModuleId, textMessage1, number);
	}

	void Calculate()
	{
		number = textMessage1;
		if (number > 40)
		{
			if (info.GetBatteryCount () < 4) 
			{
				number = number * (float) 0.25 ;
			}
			if (info.GetBatteryCount () > 3) 
			{
				number = number * 4;
			}
		}
		if (number <= 40)
		{
			if (info.GetBatteryCount () < 2) 
			{
				number = number * (float) 0.5;
			}
			if (info.GetBatteryCount () > 1) 
			{
				number = number * 2;
			}
		}

		int[] array = info.GetSerialNumberNumbers ().ToArray();

		if (array [0] % 4 == 0) 
		{
			number = number - 2;
		}
		else if (array [0] % 2 == 0) 
		{
			number = number + 3;
		}


		number = number % 5;
		number = Mathf.Round (number + 0.01f);
	}
	void competitionDecider(KMSelectable pressedButton)
	{
		int buttonPosition = new int();
		for(int i = 0; i < competitions.Length; i++)
		{
			if (pressedButton == competitions[i])
			{
				buttonPosition = i;
				break;
			}
		}
		if (_isSolved == false) 
		{
			audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, competitions[buttonPosition].transform);
			competitions [buttonPosition].AddInteractionPunch ();
			switch (buttonPosition) 
			{
			case 0:
				if (number != 4) 
				{
					incorrect = true;
				} 
				else 
				{
					Podiums [0].transform.localPosition = new Vector3 (0, 1, 0);
				}
				break;
			case 1:
				if (number != 2 && number != 0) 
				{
					incorrect = true;
				}
				else 
				{
					Podiums [1].transform.localPosition = new Vector3 (0, 1, 0);
				}
				break;
			case 2: 
				if (number == 2 || number == 4 || number == 0) 
				{
					incorrect = true;
				}
				break;
			}
			if (incorrect) 
			{
				module.HandleStrike ();
				Log ("Your ID does not match the competition chosen");
				incorrect = false;
			}
			else 
			{
				module.HandlePass ();
				Log ("Seems like you've been entered in...");
				_isSolved = true;
			}
		}
	}

	void Log(string message)
	{
		Debug.LogFormat("[Contest IDs #{0}] {1}", ModuleId, message);
	}

	#pragma warning disable 414
	private readonly string TwitchHelpMessage = @"!{0} co1/quit/co2 [Presses the button labelled CO1, QUIT, or CO2]";
	#pragma warning restore 414

	IEnumerator ProcessTwitchCommand(string command)
	{
		if (command.EqualsIgnoreCase("co1"))
		{
			yield return null;
			competitions[0].OnInteract();
		}
		else if (command.EqualsIgnoreCase("co2"))
		{
			yield return null;
			competitions[1].OnInteract();
		}
		else if (command.EqualsIgnoreCase("quit"))
		{
			yield return null;
			competitions[2].OnInteract();
		}
	}

	IEnumerator TwitchHandleForcedSolve()
	{
		if (number == 4)
		{
			competitions[0].OnInteract();
			yield return new WaitForSeconds(.1f);
		}
		else if (number == 2 || number == 0)
		{
			competitions[1].OnInteract();
			yield return new WaitForSeconds(.1f);
		}
		else
		{
			competitions[2].OnInteract();
			yield return new WaitForSeconds(.1f);
		}
	}
}

