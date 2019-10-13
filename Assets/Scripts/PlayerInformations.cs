using UnityEngine;
using System.Collections;
using Mirror;
using System;

namespace FPSNetwork
{
    [Serializable]
    public struct PlayerInformations
    {

        public int Kills;

        public int Deaths;

        public int Damages;

        public PlayerInformations(PlayerInformations infos)
        {
            Kills = infos.Kills;
            Deaths = infos.Deaths;
            Damages = infos.Damages;
        }
    }
}
