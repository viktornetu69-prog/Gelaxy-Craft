using System;
using UnityEngine;

/// <summary>
/// Данные об игроке.
/// </summary>
[Serializable]
public class PlayerInfo
{
    public string PlayerId;
    public string Username;
    public int Level;
    public float X, Y, Z;
}
