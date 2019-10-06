using UnityEngine;

[CreateAssetMenu]
public class NPC_Data : ScriptableObject
{
    public static string[] OfferStrings = {
        "I see you have nothing, come back if you have something to trade",
        "Ahh is it (%)? that's what i needed, let's trade it",
        "That (%) seems familiar :) I also sell it, can't trade it",
        "Wanna trade that (%)?",
    };

    public string NeedSentences;
    public string[] GreetingSentences;

    public string[] ItemList;
    public int[] ItemValues;
}
