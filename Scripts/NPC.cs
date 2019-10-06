using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class NPC : MonoBehaviour
{
    const float OFFSET = 10f;
    const float DURATN = 0.25f;

    public RectTransform Label;
    public RectTransform MainUI;
    public GameObject Button;
    public NPC_Data Data;
    public float Factor = 1f;

    float _currentLabelY;
    float _originalLabelY;

    readonly List<string> _extras = new List<string>();
    readonly List<string> _inventory = new List<string>();

    readonly List<int> _extrasCost = new List<int>();
    readonly List<int> _inventoryCost = new List<int>();

    readonly List<string> _needs = new List<string>();

    int _greetingCounter;

    public static bool CrateRob;
    public static bool FoodRob;
    public static int NeedCounter;

    void Start()
    {
        _originalLabelY = Label.position.y;
        CrateRob = false;
        FoodRob = false;
        NeedCounter = 0;
    }

    void Update()
    {
        
    }

    public void Select()
    {
        if (!Label.gameObject.activeInHierarchy)
        {
            _currentLabelY = _originalLabelY - OFFSET;
            Label.position = new Vector3(Label.position.x, _currentLabelY);
            Label.localScale = Vector3.zero;
            Label.gameObject.SetActive(true);
        }
        else
        {
            StopCoroutine(UnselectAnimation());
        }
        StartCoroutine(SelectAnimation());
    }

    public void Unselect()
    {
        StopCoroutine(SelectAnimation());
        StartCoroutine(UnselectAnimation());
    }

    IEnumerator SelectAnimation()
    {
        Vector3 startingScale = Label.localScale;
        float startingLabelY = _currentLabelY;
        float timer = 0f;
        float duration = (_originalLabelY - _currentLabelY) / OFFSET * DURATN;
        while (timer < duration)
        {
            Label.localScale = Vector3.Lerp(startingScale, Vector3.one, timer / duration);
            _currentLabelY = Mathf.Lerp(startingLabelY, _originalLabelY, timer / duration);
            Label.position = new Vector3(Label.position.x, _currentLabelY);
            yield return null;
            timer += Time.deltaTime;
        }
    }

    IEnumerator UnselectAnimation()
    {
        Vector3 startingScale = Label.localScale;
        float startingLabelY = _currentLabelY;
        float timer = 0f;
        float duration = (OFFSET - _originalLabelY + _currentLabelY) / OFFSET * DURATN;
        while (timer < duration)
        {
            Label.localScale = Vector3.Lerp(startingScale, Vector3.zero, timer / duration);
            _currentLabelY = Mathf.Lerp(startingLabelY, _originalLabelY, timer / duration);
            Label.position = new Vector3(Label.position.x, _currentLabelY);
            yield return null;
            timer += Time.deltaTime;
        }
        Label.gameObject.SetActive(false);
    }

    public void ForceUnselect()
    {
        Label.gameObject.SetActive(false);
    }

    public void ToggleMainMenu(bool decorate)
    {
        if (decorate)
        {
            Decorate();
        }
        StartCoroutine(ToggleMainUI());
    }

    void Decorate()
    {
        ControlManager cm = FindObjectOfType<ControlManager>();

        for (int i = 6; i < MainUI.childCount; i++)
        {
            Destroy(MainUI.GetChild(i).gameObject);
        }
        MainUI.GetChild(1).GetComponent<Text>().text = Data.GreetingSentences[_greetingCounter % Data.GreetingSentences.Length];

        string str = "I need following items: <color=yellow>";
        for (int i = 0; i < _needs.Count; i++)
            str += _needs[i] + ", ";
        str += "</color>";
        if (_needs.Count == 0)
            str = "I don't need anything right now...";
        if (name == "Right Store")
            str = "We have our own suppliers sir.";
        MainUI.GetChild(2).GetComponent<Text>().text = str;

        bool flag = false;
        string s;
        if (cm.Item == "Nothing")
            s = NPC_Data.OfferStrings[0];
        else if (_needs.Contains(cm.Item))
            s = NPC_Data.OfferStrings[1].Replace("%", cm.Item);
        else
        {
            foreach (string item in Data.ItemList)
                flag |= item == cm.Item;

            if (flag)
            {
                s = NPC_Data.OfferStrings[2].Replace("%", cm.Item);
            }
            else
            {
                s = NPC_Data.OfferStrings[3].Replace("%", cm.Item);
            }
        }

        MainUI.GetChild(3).GetComponent<Text>().text = s;

        if (_extras.Count != 0)
        {
            MainUI.GetChild(4).GetComponent<Text>().text = Data.NeedSentences;
            MainUI.GetChild(4).gameObject.SetActive(true);
        }
        else
        {
            MainUI.GetChild(4).gameObject.SetActive(false);
        }

        float ff = _needs.Contains(cm.Item) ? Factor * 0.25f : Factor;
        for (int i=0; i<_inventory.Count; i++)
        {
            string ss = "";
            if (_inventoryCost[i] > 14 + cm.Score)
                ss = " (+++)";
            else if (_inventoryCost[i] > 7 + cm.Score)
                ss = " (++)";
            else if (_inventoryCost[i] > cm.Score)
                ss = " (+)";
            else if (_inventoryCost[i] > cm.Score - 7)
                ss = " (-)";
            else if (_inventoryCost[i] > cm.Score - 14)
                ss = " (--)";
            else if (_inventoryCost[i] <= cm.Score - 14)
                ss = " (---)";

            GameObject button = Instantiate(Button);
            button.transform.SetParent(MainUI);
            button.GetComponent<RectTransform>().anchoredPosition = new Vector3(80f + (i % 3) * 155f, -90f - (i / 3) * 35f, 0f);
            button.transform.localScale = Vector3.one;
            button.transform.GetChild(0).GetComponent<Text>().text = _inventory[i] + ss;
            button.GetComponent<Button>().interactable = Mathf.Ceil(_inventoryCost[i] * ff) <= cm.Score + 1 && !flag;
            int index = i;
            button.GetComponent<Button>().onClick.AddListener(()=>GiveInventoryItem(index));
        }
        for (int i=0; i<_extras.Count; i++)
        {
            string ss = "";
            if (_extrasCost[i] > 14 + cm.Score)
                ss = " (+++)";
            else if (_extrasCost[i] > 7 + cm.Score)
                ss = " (++)";
            else if (_extrasCost[i] > cm.Score)
                ss = " (+)";
            else if (_extrasCost[i] > cm.Score - 7)
                ss = " (-)";
            else if (_extrasCost[i] > cm.Score - 14)
                ss = " (--)";
            else if (_extrasCost[i] <= cm.Score - 14)
                ss = " (---)";

            GameObject button = Instantiate(Button);
            button.transform.SetParent(MainUI);
            button.GetComponent<RectTransform>().anchoredPosition = new Vector3(80f + (i % 3) * 155f, -220f, 0f);
            button.transform.localScale = Vector3.one;
            button.transform.GetChild(0).GetComponent<Text>().text = _extras[i] + ss;
            int index = i;
            button.GetComponent<Button>().onClick.AddListener(() => GiveExtraItem(index));
        }
    }

    IEnumerator ToggleMainUI()
    {
        Vector3 startingScale = MainUI.localScale;
        Vector3 endingScale = Vector3.one;
        if (MainUI.gameObject.activeInHierarchy)
        {
            endingScale = Vector3.zero;
        }
        else
        {
            MainUI.gameObject.SetActive(true);
        }
        float duration = Mathf.Abs(startingScale.x - endingScale.x) * DURATN;
        float timer = 0f;
        while (timer<duration)
        {
            MainUI.localScale = Vector3.Lerp(startingScale, endingScale, timer / duration);
            yield return null;
            timer += Time.deltaTime;
        }
        if (endingScale == Vector3.zero)
        {
            MainUI.gameObject.SetActive(false);
            FindObjectOfType<ControlManager>().Closed();
        }
    }

    public bool Contains (string i) { return _inventory.Contains(i) || _extras.Contains(i); }

    public void AddItem(string i, int c, bool extra = false)
    {
        if (extra)
        {
            _extras.Add(i);
            _extrasCost.Add(c);
        }
        else
        {
            _inventory.Add(i);
            _inventoryCost.Add(c);
        }
    }

    public void GiveInventoryItem(int i)
    {
        ControlManager cm = FindObjectOfType<ControlManager>();
        if (cm.Item != "Nothing" && !_needs.Contains(cm.Item))
        {
            _inventory.Add(cm.Item);
            _inventoryCost.Add(cm.Score);
        }
        if (_needs.Contains(cm.Item))
        {
            NeedCounter++;
            _needs.Remove(cm.Item);
        }
        cm.Give(_inventory[i],  _inventoryCost[i]);
        _inventory.RemoveAt(i);
        _inventoryCost.RemoveAt(i);
        Decorate();
    }

    public void GiveExtraItem(int i)
    {
        ControlManager cm = FindObjectOfType<ControlManager>();
        /*
        if (cm.Item != "Nothing" && !_needs.Contains(cm.Item))
        {
            _inventory.Add(cm.Item);
            _inventoryCost.Add(cm.Score);
        }
        if (_needs.Contains(cm.Item))
        {
            NeedCounter++;
            _needs.Remove(cm.Item);
        }
        */
        if (name == "Crates")
            CrateRob = true;
        if (name == "Food Cart")
            FoodRob = true;
        cm.Give(_extras[i], _extrasCost[i]);
        _extras.RemoveAt(i);
        _extrasCost.RemoveAt(i);
        Decorate();
    }

    public void AddNeed(string s) { _needs.Add(s);  }

    public bool IsNeeded(string s) { return _needs.Contains(s); }

    public string RandomItem()
    {
        if (Random.value < 0.7 || _extras.Count == 0)
        {
            return _inventory[Random.Range(0, _inventory.Count - 1)];
        }
        return _extras[Random.Range(0, _extras.Count - 1)];
    }
}
