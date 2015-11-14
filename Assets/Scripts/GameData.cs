using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameData {

    public static int[] shotDamageTable = {
        //Player
        100,
        //Enemy1
        20,
        //Enemy2
        20,
        //Enemy3
        20,
        //Enemy4
        20,
        //Enemy5
        20 };
    public static float[] shotMoveSpeedTable = {
        //Player - NormalShot
        10,
        //Player - ChargedShot
        20,
        //Enemy2
        10,
        //Enemy3
        10,
        //Enemy4
        10 };

    public enum Team {
        Player = 0,
        Enemy = 1,
        Neutral = 2
    }

    public enum ShotType {
        Normal = 0,
        Charged = 1,
        Special2 = 2,
        Special3 = 3
    }

    public enum EnemyType {
        Navi = 0,
        Wasp = 1
    }

    
}
