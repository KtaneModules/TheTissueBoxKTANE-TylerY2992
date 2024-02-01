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
                Debug.LogFormat("[The Tissue Box #{0}] Number of seconds: {1}", moduleID, Seconds);
                targetClick = (edgework.GetBatteryCount() * edgework.GetPortCount() + 10) * edgework.GetSerialNumberNumbers().Last();
                Debug.LogFormat("[The Tissue Box #{0}] Target number of tissues to pull: {1}", moduleID, targetClick);

            }
			else
			{
				Seconds = edgework.GetSerialNumberNumbers().Last() * 3.5f;
				if (edgework.GetSerialNumberNumbers().Last() > 0)
				{ Debug.LogFormat("[The Tissue Box #{0}] Number of seconds: {1}", moduleID, Seconds); }
                targetClick = 10 * edgework.GetSerialNumberNumbers().Last();
				if (edgework.GetSerialNumberNumbers().Last() > 0)
				{ Debug.LogFormat("[The Tissue Box #{0}] Target number of tissues to pull: {1}", moduleID, targetClick); }

            }
		}
		else
		{
			boxes[1].SetActive(false);
            if (edgework.GetSerialNumberNumbers().Last() < 6 && edgework.GetSerialNumberNumbers().Last() != 0)
            {
                Seconds = edgework.GetSerialNumberNumbers().Last() * 3.5f;
                Debug.LogFormat("[The Tissue Box #{0}] Number of seconds: {1}", moduleID, Seconds);
                targetClick = 10 * edgework.GetSerialNumberNumbers().Last();
                Debug.LogFormat("[The Tissue Box #{0}] Target number of tissues to pull: {1}", moduleID, targetClick);
            }
            else
            {
				Seconds = edgework.GetBatteryCount() + edgework.GetPortCount() + edgework.GetIndicators().ToList().Count + edgework.GetSerialNumberNumbers().Last() + 5;
				if (edgework.GetSerialNumberNumbers().Last() > 0)
				{ Debug.LogFormat("[The Tissue Box #{0}] Number of seconds: {1}", moduleID, Seconds); }
                targetClick = (int)Mathf.Pow(3, (edgework.GetSerialNumberNumbers().Last() - 5));
				if (edgework.GetSerialNumberNumbers().Last() > 0)
				{ Debug.LogFormat("[The Tissue Box #{0}] Target number of tissues to pull: {1}", moduleID, targetClick); }
            }
        }
        if (edgework.GetSerialNumberNumbers().Last() == 0)
        {
            Seconds = (edgework.GetModuleNames().ToList().Count % 10) * 20;
            if (Seconds == 0)
            { Seconds = 35; }
            Debug.LogFormat("[The Tissue Box #{0}] Number of seconds: {1}", moduleID, Seconds);
            targetClick = edgework.GetSerialNumber().Select(c => char.IsDigit(c) ? c - '0' : c - '@').ToList().Sum();
            Debug.LogFormat("[The Tissue Box #{0}] Target number of tissues to pull: {1}", moduleID, targetClick);
        }
        Seconds = Seconds % 150;
        Debug.LogFormat("[The Tissue Box #{0}] Modified number of seconds after global % 150: {1}", moduleID, Seconds);
        targetClick = targetClick % 150;
        Debug.LogFormat("[The Tissue Box #{0}] Modified target number of tissues to pull after global % 150: {1}", moduleID, targetClick); 
		if (targetClick / Seconds > 5)
		{
			targetClick = 80;
			Seconds = 20;
            Debug.LogFormat("[The Tissue Box #{0}] The calculated clicks per second was greater than 5. Therefore, the override rule applies. The target number of tissues to pull is 80 and the number of seconds is 20.", moduleID);
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
            Debug.LogFormat("[The Tissue Box #{0}] You pulled {1} tissues. Strike!", moduleID, Click); 
			Click = 0;
        }
		else
		{
			module.HandlePass();
			isSolved = true;
            Debug.LogFormat("[The Tissue Box #{0}] Hooray! You are so good at this bomb defusal stuff :)", moduleID);
        }

    }
}