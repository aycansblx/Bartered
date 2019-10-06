using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RoundManager : MonoBehaviour
{
    readonly Color DUSK = new Color(135f / 255f, 197f / 255f, 242f / 255f);
    readonly Color MIDD = new Color(247f / 255f, 228f / 255f, 103f / 255f);
    readonly Color NIGT = new Color( 53f / 255f,  98f / 255f, 148f / 255f);

    readonly Vector3 START = new Vector3(4.3f, 1.5f, 0f);
    readonly Vector3 FINNN = new Vector3(-1.5f, 1.5f, 0f);

    public float RoundDuration;

    public Transform Bazaar;
    public Transform Sun;
    public Transform GameOver;
    public Transform Intro;

    float _timer;

    bool _over;

    SpriteRenderer _spriteRenderer;

    ControlManager _cm;

    Vector3 _mid;

    void Start()
    {
        Time.timeScale = 0f;
        _timer = 0f;
        _spriteRenderer = GetComponent<SpriteRenderer>();
        CreateLevel();
        _cm = FindObjectOfType<ControlManager>();
        _mid = (START + FINNN) / 2f + Vector3.up * 1.9f;
    }

    IEnumerator AnimateIntro()
    {
        float speed = 500f;
        float duration = 0.5f;
        float timer = 0f;
        while(timer < duration)
        {
            Intro.GetChild(0).position += Vector3.down * speed * Time.deltaTime;
            Intro.GetChild(1).position += Vector3.down * speed * Time.deltaTime;
            Intro.GetChild(2).position += Vector3.up * speed * Time.deltaTime;
            yield return null;
            timer += Time.deltaTime;
        }
    }

    void Update()
    {
        if (_timer > RoundDuration && Input.GetKeyDown(KeyCode.S))
        {
            SceneManager.LoadScene(0);
        }
        else if (Time.timeScale == 0f && Input.GetKeyDown(KeyCode.S))
        {
            Time.timeScale = 1f;
            StartCoroutine(AnimateIntro());
        }

        if (_cm.InMenu())
            return;

        _timer += Time.deltaTime;
        if (_timer > RoundDuration && !_over)
        {
            _over = true;
            Time.timeScale = 0f;
            _cm.ForceUnselect();
            GameOver.GetChild(2).GetComponent<Text>().text = _cm.Item;
            string s = "You provided " + NPC.NeedCounter + " needs.";
            if (NPC.NeedCounter >= 4)
                s += " That was impressing.";
            s += "\n";
            if (NPC.CrateRob)
                s += "You robbed poor old homeless man :( You monster!\n";
            if (NPC.FoodRob)
                s += "You robbed food cart guy :( That's not cool.";
            GameOver.GetChild(3).GetComponent<Text>().text = s;
            GameOver.gameObject.SetActive(true);
        }

        if (_timer < RoundDuration / 2f)
        {
            _spriteRenderer.color = Color.Lerp(DUSK, MIDD, _timer / (RoundDuration / 2f));
        }
        else
        {
            _spriteRenderer.color = Color.Lerp(MIDD, NIGT, (_timer - RoundDuration / 2f) / (RoundDuration / 2f));
        }

        float ratio = _timer / RoundDuration;
        Sun.position = (1f - ratio) * (1f - ratio) * START + 2f * ratio * (1f - ratio) * _mid + ratio * ratio * FINNN;
    }

    void CreateLevel()
    {
        List<int> costs = new List<int>();
        for (int i = 0; i < Bazaar.childCount; i++)
        {
            List<int> thisCosts = new List<int>();
            NPC npc = Bazaar.GetChild(i).GetComponent<NPC>();
            int invetorySize = Random.Range(4, 9);

            if (i != 4)
            {
                int price = i == 0 || i == 5 ? 15 : 5;
                int minRange = i == 1 ? 1 : 0;
                int maxrange = i == 3 ? 2 : 3;
                int extraSize = Random.Range(minRange, maxrange);
                for (int j = 0; j < extraSize; j++)
                {
                    string s;
                    int c;
                    int trial = 0;
                    do
                    {
                        int index = Random.Range(0, 18);
                        s = npc.Data.ItemList[index];
                        c = npc.Data.ItemValues[index];
                    } while ((npc.Contains(s) || c >= price) && trial++ < 197);
                    if (!npc.Contains(s) && s != "" && c < price)
                        npc.AddItem(s, c, true);
                }
            }

            for (int j = 0; j < invetorySize; j++)
            {
                int randomCost;
                bool flag = false;
                string s = "";
                int c = 0;
                if (costs.Count > 0 && Random.value < 0.5f)
                {
                    randomCost = costs[Random.Range(0, costs.Count - 1)];
                    for (int k = 0; k < 19; k++)
                    {
                        if (npc.Data.ItemValues[k] - randomCost < 3 && npc.Data.ItemValues[k] - randomCost > 0 && !npc.Contains(npc.Data.ItemList[k]))
                        {
                            flag = true;
                            s = npc.Data.ItemList[k];
                            c = npc.Data.ItemValues[k];
                            costs.Remove(randomCost);
                            break;
                        }
                    }
                }
                if (!flag)
                {
                    do
                    {
                        int index = Random.Range(0, 18);
                        s = npc.Data.ItemList[index];
                        c = npc.Data.ItemValues[index];
                    } while (npc.Contains(s));
                }
               
                if (!npc.Contains(s))
                {
                    npc.AddItem(s, c);
                    thisCosts.Add(c);
                }
            }
            costs.AddRange(thisCosts);
            thisCosts.Clear();
        }

        List<string> needs = new List<string>();

        for (int i = 0; i < Bazaar.childCount; i++)
        {
            if (i == 4)
                continue;
            NPC npc = Bazaar.GetChild(i).GetComponent<NPC>();
            int minRange = i == 2 ? 2 : 1;
            int maxrange = i == 3 ? 3 : 2;
            int needCount = Random.Range(minRange, maxrange);
            for (int j = 0; j < needCount; j++)
            {
                int bazaarElement;
                do
                {
                    bazaarElement = Random.Range(0, Bazaar.childCount - 1);
                } while (bazaarElement == i);
                string s;
                do
                {
                    s = Bazaar.GetChild(bazaarElement).GetComponent<NPC>().RandomItem();
                } while (npc.IsNeeded(s) || needs.Contains(s));
                if (!npc.IsNeeded(s) && s != "" && !needs.Contains(s))
                {
                    npc.AddNeed(s);
                    needs.Add(s);
                }
            }
        }
    }
}
