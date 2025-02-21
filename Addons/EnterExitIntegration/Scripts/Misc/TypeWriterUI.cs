// Script for having a typewriter effect for UI
// Prepared by Nick Hwang (https://www.youtube.com/nickhwang)
// Want to get creative? Try a Unicode leading character(https://unicode-table.com/en/blocks/block-elements/)
// Copy Paste from page into Inpector

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TypeWriterUI : MonoBehaviour
{
    Text _text;
    string writer;

    [SerializeField] float delayBeforeStart = 0f;
    [SerializeField] float timeBtwChars = 0.1f;
    [SerializeField] string leadingChar = "";
    [SerializeField] bool leadingCharBeforeDelay = false;

    void OnEnable()
    {
        _text = GetComponent<Text>()!;

        if (_text != null)
        {
            writer = _text.text;
            _text.text = "";

            StartCoroutine("TypeWriterText");
        }
    }

    IEnumerator TypeWriterText()
    {
        _text.text = leadingCharBeforeDelay ? leadingChar : "";

        yield return new WaitForSeconds(delayBeforeStart);

        foreach (char c in writer)
        {
            if (_text.text.Length > 0)
            {
                _text.text = _text.text.Substring(0, _text.text.Length - leadingChar.Length);


                //TODO Sound Calling
                SoundManager.Instance.PlayTypeSound();
            }
            _text.text += c;
            _text.text += leadingChar;

            yield return new WaitForSeconds(timeBtwChars);
        }

        if (leadingChar != "")
        {
            _text.text = _text.text.Substring(0, _text.text.Length - leadingChar.Length);
        }

        if (GameManager.GetInstance().state == GameplayScreens.CutScene) yield break;

        yield return new WaitForSeconds(.5f);

        GameManager.GetInstance().OnObjectiveOKButton();
    }
}