using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class RelicSO : ScriptableObject
{
    public List<PlayerEffects.RelicBuff> relicBuffs;

    public void SetRelicSo(RelicSO relicToSet)
    {
        relicBuffs = relicToSet.relicBuffs;
        foreach (var relicBuff in relicBuffs) relicBuff.currentUsages = 0;
    }
}
