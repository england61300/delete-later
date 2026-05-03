using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "SkinsDatabase", menuName = "Game/Skins Database")]
public class SkinDatabase : ScriptableObject
{
    public List<SkinData> playerSkins;
    public List<SkinData> enemySkins;
}
