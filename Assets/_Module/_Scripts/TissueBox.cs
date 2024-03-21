using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

public class TissueBox : MonoBehaviour {
	public KMBombModule module;
	public KMBombInfo edgework;
	public KMAudio audio;
	private bool isSolved;
	private static int moduleCount = 1;
	private int moduleID;
	public KMSelectable[] tissue;
	public TextMesh displayText;
	public GameObject[] boxes;
	public Texture[] boxTextures;
	private int Click = 0;
	private float Seconds = 0;
	private bool isCounting = false;
	private int targetClick = 0;

	void Awake() 
	{
        boxes[0].GetComponent<MeshRenderer>().material.SetTexture("_MainTex", boxTextures.PickRandom());
        boxes[1].GetComponent<MeshRenderer>().material.SetTexture("_MainTex", boxTextures.PickRandom());
    }

    void Start() {
		foreach (KMSelectable button in tissue) {
			button.OnInteract += delegate ()
			{ pressTissue(); return false; };
		}
		moduleID = moduleCount++;

        displayText.text = " ";
		if (UnityEngine.Random.value < 0.5f)
		{
			boxes[0].SetActive(false);
			if (edgework.GetSerialNumberNumbers().Last() < 6 && edgework.GetSerialNumberNumbers().Last() != 0)
			{
				Seconds = 45;
                Debug.LogFormat("[The Tissue Box™ #{0}] Number of seconds: {1}", moduleID, Seconds);
                targetClick = (edgework.GetBatteryCount() * edgework.GetPortCount() + 10) * edgework.GetSerialNumberNumbers().Last();
                Debug.LogFormat("[The Tissue Box™ #{0}] Target number of tissues to pull: {1}", moduleID, targetClick);

            }
			else
			{
				Seconds = edgework.GetSerialNumberNumbers().Last() * 3.5f;
				if (edgework.GetSerialNumberNumbers().Last() > 0)
				{ Debug.LogFormat("[The Tissue Box™ #{0}] Number of seconds: {1}", moduleID, Seconds); }
                targetClick = 10 * edgework.GetSerialNumberNumbers().Last();
				if (edgework.GetSerialNumberNumbers().Last() > 0)
				{ Debug.LogFormat("[The Tissue Box™ #{0}] Target number of tissues to pull: {1}", moduleID, targetClick); }

            }
		}
		else
		{
			boxes[1].SetActive(false);
            if (edgework.GetSerialNumberNumbers().Last() < 6 && edgework.GetSerialNumberNumbers().Last() != 0)
            {
                Seconds = edgework.GetSerialNumberNumbers().Last() * 3.5f;
                Debug.LogFormat("[The Tissue Box™ #{0}] Number of seconds: {1}", moduleID, Seconds);
                targetClick = 10 * edgework.GetSerialNumberNumbers().Last();
                Debug.LogFormat("[The Tissue Box™ #{0}] Target number of tissues to pull: {1}", moduleID, targetClick);
            }
            else
            {
				Seconds = edgework.GetBatteryCount() + edgework.GetPortCount() + edgework.GetIndicators().ToList().Count + edgework.GetSerialNumberNumbers().Last() + 5;
				if (edgework.GetSerialNumberNumbers().Last() > 0)
				{ Debug.LogFormat("[The Tissue Box™ #{0}] Number of seconds: {1}", moduleID, Seconds); }
                targetClick = (int)Mathf.Pow(3, (edgework.GetSerialNumberNumbers().Last() - 5));
				if (edgework.GetSerialNumberNumbers().Last() > 0)
				{ Debug.LogFormat("[The Tissue Box™ #{0}] Target number of tissues to pull: {1}", moduleID, targetClick); }
            }
        }
        if (edgework.GetSerialNumberNumbers().Last() == 0)
        {
            Seconds = (edgework.GetModuleNames().ToList().Count % 10) * 20;
            if (Seconds == 0)
            { Seconds = 35; }
            Debug.LogFormat("[The Tissue Box™ #{0}] Number of seconds: {1}", moduleID, Seconds);
            targetClick = edgework.GetSerialNumber().Select(c => char.IsDigit(c) ? c - '0' : c - '@').ToList().Sum();
            Debug.LogFormat("[The Tissue Box™ #{0}] Target number of tissues to pull: {1}", moduleID, targetClick);
        }
        if (Seconds > 150)
        { Seconds = Seconds % 150; }
        if (Seconds == 0)
        { Seconds = Seconds + 1; }
        Debug.LogFormat("[The Tissue Box™ #{0}] Modified number of seconds after global % 150: {1}", moduleID, Seconds);
        if (targetClick > 150)
        { targetClick = targetClick % 150; }
        if (targetClick == 0)
        { targetClick = targetClick + 1; }
        Debug.LogFormat("[The Tissue Box™ #{0}] Modified target number of tissues to pull after global % 150: {1}", moduleID, targetClick); 
		if (targetClick / Seconds > 5)
		{
			targetClick = 80;
			Seconds = 20;
            Debug.LogFormat("[The Tissue Box™ #{0}] The calculated clicks per second was greater than 5. Therefore, the override rule applies. The target number of tissues to pull is 80 and the number of seconds is 20.", moduleID);
        }
    }
	private void pressTissue()
	{
		Click++;
		displayText.text = Click.ToString();
		if (!isCounting)
		{
            if (Seconds > edgework.GetTime())
            {
                Seconds = edgework.GetTime() - 0.2f;
            }
            isCounting = true;
			StartCoroutine(countSeconds(Seconds));
		}
	}
	private IEnumerator countSeconds(float time)
	{
		yield return new WaitForSeconds(time);
		if (Click != targetClick)
		{
            module.HandleStrike();
            isCounting = false;
            displayText.text = "NOPE";
            Debug.LogFormat("[The Tissue Box™ #{0}] You pulled {1} tissues. Strike!", moduleID, Click); 
			Click = 0;
        }
		else
		{
			module.HandlePass();
			isSolved = true;
            Debug.LogFormat("[The Tissue Box™ #{0}] Hooray! You are so good at this bomb defusal stuff :)", moduleID);
        }

    }

    //twitch plays
    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} pull <1-150> [Pulls out the specified number of tissues]";
    #pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string command)
    {
        string[] parameters = command.Split(' ');
        if (parameters[0].EqualsIgnoreCase("pull"))
        {
            if (parameters.Length > 2)
                yield return "sendtochaterror Too many parameters!";
            else if (parameters.Length == 2)
            {
                int temp = -1;
                if (!int.TryParse(parameters[1], out temp))
                {
                    yield return "sendtochaterror!f The specified number of tissues '" + parameters[1] + "' is invalid!";
                    yield break;
                }
                if (temp < 1 || temp > 150)
                {
                    yield return "sendtochaterror The specified number of tissues '" + parameters[1] + "' is invalid!";
                    yield break;
                }
                yield return null;
                while (Click < temp)
                {
                    tissue[0].OnInteract();
                    yield return new WaitForSeconds(.01f);
                }
                if (Click == targetClick)
                    yield return "solve";
                else
                    yield return "strike";
            }
            else if (parameters.Length == 1)
                yield return "sendtochaterror Please specify a number of tissues to pull!";
        }
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        if (Click > targetClick)
        {
            module.HandlePass();
            isSolved = true;
            StopAllCoroutines();
            yield break;
        }
        while (Click < targetClick)
        {
            tissue[0].OnInteract();
            yield return new WaitForSeconds(.01f);
        }
        while (!isSolved) yield return true;
    }
}