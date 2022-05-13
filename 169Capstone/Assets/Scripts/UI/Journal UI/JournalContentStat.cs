﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "JournalUI/JournalContentStat")]
public class JournalContentStat : JournalContent
{
    [SerializeField] private string associatedSecondaryStats;

    public string AssociatedSecondaryStats()
    {
        return associatedSecondaryStats;
    }
}
