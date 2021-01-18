using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using Debug = UnityEngine.Debug;

public class LeaderboardScript : MonoBehaviour
{

    private String playerName;
    public int playerHighScore = 0;
    private int highScoreIndex = -1;

    /*
     * This boolean toggles whether the leaderboard is static or "live."
     * I.e. should the leaderboard change if the player's score increases?
     */
    public bool isLive = false;

    // WARNING: THE FOLLOWING 2 LINES ARE HARDCODED
    private List<string> names = new List<string>{ "ominousObelisk", "unusualUkulele", "rudeRainbow", "oilyObjector", "battyBrawler", "outstandingOaf", "royalRabbit", "obtuseOxygen", "sappySadist", "gracefulIron", "timidGuitar", "unsightlyDream", "talentedHoney", "exuberantNight", "terribleRocket", "yellowActor", "giantAmbulance", "itchyNeedle", };
    private List<int> scores = new List<int>{ 64, 57, 51, 45, 39, 34, 29, 25, 21, 17, 14, 11, 9, 7, 5, 4, 3, 2 };
    private Dictionary<float, TextMeshProUGUI> heightToText = new Dictionary<float, TextMeshProUGUI>();

    // color of player's current (if live) or best (if static) score on leaderboard
    static protected Vector3 highlightedColor = new Vector3(60, 60, 20) / (256f);
    // color of all other entries on leaderboard
    static protected Vector3 normalColor = new Vector3(110, 99, 0) / (256f);

    // Start is called before the first frame update
    void Start()
    {
        playerHighScore = PlayerPrefs.GetInt("Score", 0);
        playerName = PlayerPrefs.GetString("Name", "[playtesters]");

        // all children of current GameObject are the text entries in leaderboard
        foreach (Transform child in this.transform)
        {
            // get heights of text objects
            float height = child.transform.localPosition.y;
            heightToText.Add(height, child.transform.GetComponent<TextMeshProUGUI>());
        }

        // If the leaderboard is live, include player's previous high score on the leaderboard.
        // If the player's high score is 0, it's immediately matched by the current game's score and can be ignore.
        if (isLive && 0 < playerHighScore)
        {
            highScoreIndex = GetRank(playerHighScore);
            // insert player's previous high score into the leaderboard
            names.Insert(highScoreIndex, playerName);
            scores.Insert(highScoreIndex, playerHighScore);
        }

        if (!isLive)
        {
            SetStaticLeaderboard(playerHighScore);
        }
    }

    public void SetStaticLeaderboard(int score)
    {
        /*
        if (0 <= highScoreIndex && playerHighScore <= score )
        {
            scores.RemoveAt(highScoreIndex);
            names.RemoveAt(highScoreIndex);

            highScoreIndex = -1;
        }
        */

        List<Entry> entries = ClosestEntries(score);
        int playerIndex = GetPlayerIndex(score);

        SetText(entries, playerIndex);
    }

    private List<Entry> ClosestEntries(int playerScore)
    {
        // WARNING: Number of entries is hardcoded to 10
        List<Entry> entries = new List<Entry>(10);

        int rank = GetRank(playerScore);
        int playerIndex = 0;

        // first add player's entry
        entries.Add(new Entry(rank + 1, playerScore, playerName));

        int guard = Math.Min(rank + 8, scores.Count);
        Debug.Log("Guard:\t" + guard);
        // try to add the 8 entries below players
        for (int i = rank; i < guard; i++)
        {
            // append
            entries.Add(new Entry(i + 2, scores[i], names[i]));
            Debug.Log(entries[entries.Count - 1].ToString());
        }

        // if fewer than 8 entries appear below player on leaderboard
        if (scores.Count - rank < 8)
        {
            // prepend the missing number of entries above player on leaderboard
            guard = rank - (10 - entries.Count);
            for (int i = rank - 1; i >= guard; i--)
            {
                // prepend
                entries.Insert(0, new Entry(i + 1, scores[i], names[i]));
                playerIndex++;
            }
        }
        else
        // if player has best score of all
        if (rank == 0)
        {
            // get the last entry that will appear on list
            entries.Add(new Entry(rank + 10, scores[rank + 8], names[rank + 8]));
        }
        // normal case 
        else
        {
            playerIndex++;
            entries.Insert(0, new Entry(rank, scores[rank - 1], names[rank - 1]));
        }
        return entries;
    }

    // get player's index into name/score lists
    private int GetPlayerIndex(int playerScore)
    {
        int rank = GetRank(playerScore);
        int toReturn;

        if (rank == 0)
        {
            toReturn = 0;
        }
        else
        {
            toReturn = 10 - (Math.Min(rank + 8, scores.Count) - rank) - 1;
        }

        return toReturn;
    }

    /*
     * NOTE: This function is inefficent on a live leaderboard where 100% of entries remain the same color in the normal case.
     * This function could be made more efficient by checking if the position of the player's entry on the leaderboard has changed,
     *      and only altering the colors of the 2 relevant entries (the player's old entry and the player's new entry).
     * Ultimately though, this function can only be called >20 times a game, and only affects 10 elements
    */
    private void SetText(List<Entry> entries, int playerIndex)
    {

        Debug.Log("This is the playerIndex: " + playerIndex);
        if (entries.Count == 10)
        {
            // sorted from top to bottom
            List<float> arrayOfAllKeys = new List<float>(heightToText.Keys);
            arrayOfAllKeys.Sort();
            arrayOfAllKeys.Reverse();

            // iterate over all all entries on leaderboard
            for (int i = 0; i < 10; i++)
            {
                Entry entry = entries[i];
                float key = arrayOfAllKeys[i];
                heightToText[key].text = entry.ToString();

                // if this is the player's entry
                if (i == playerIndex)
                {
                    // highlight text
                    heightToText[key].color = new Color(highlightedColor.x, highlightedColor.y, highlightedColor.z);
                }
                else
                {
                    // normal text
                    heightToText[key].color = new Color(normalColor.x, normalColor.y, normalColor.z);
                }
            }
        }
        else
        {
            Debug.Log("Error in generating entries");
            Debug.Log(entries.Count);
        }
    }

    /*
     * Rank denotes index OF the first score that is less than player's score
     * For example, the best rank is 0.
     * The worst rank is scores.length (i.e. not a valid index becasue there are no scores below player's score)
    */
    // TODO: This could be made O(log(n)) instead of O(n), but O(19) vs O(4) shouldn't affect runtime
    private int GetRank(int value)
    {
        for (int i = 0; i < scores.Count; i++)
        {
            if (value > scores[i])
            {
                return i;
            }
        }

        return scores.Count;
    }

    // Entry defines all data about entries on leaderboard
    // TODO: Whether or not the entry is the highlighted entry could also be a field
    public class Entry
    {
        int rank;
        int score;
        string name;

        public Entry(int rank, int score, string name)
        {
            this.rank = rank;
            this.score = score;
            this.name = name;
        }

        // For formatting purposes, it's important all strings returned are the same length
        override public string ToString()
        {
            // return rank + ". " + name + " -- " + score;
            return (rank + ". ").PadLeft(4, ' ') + name.PadRight(16, ' ') + score;
        }

        // MUTATOR METHOD
        public void SetRank(int newRank)
        {
            rank = newRank;
        }

        // ACCESSORY METHODS
        public int GetRank()
        {
            return rank;
        }

        public int GetScore()
        {
            return score;
        }
    }

}
