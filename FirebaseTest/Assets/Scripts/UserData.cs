using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserData
{
    public string Name;
    public int Score;
    public int Health;

    public UserData(string name, int score, int health)
    {
        Name = name;
        Score = score;
        Health = health;
    }
}
