using System;
using UnityEngine;

namespace TheBCMHT{
[CreateAssetMenu(fileName = "New read me", menuName = "CM Assets/Read me", order = 1)]
public class Readme : ScriptableObject
{
    [TextArea]
    public string message;
}
}