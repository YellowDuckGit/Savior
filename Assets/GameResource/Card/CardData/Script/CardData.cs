using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface CardData
{
    string id { get; set; }
    string name { get; set; }
    string description { get; set; }
    int cost { get; set; }

}
