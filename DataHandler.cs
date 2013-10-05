/*   
TShock, a server mod for Terraria
Copyright (C) 2011 The TShock Team

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
using System;
using System.Collections.Generic;
using Terraria;
using System.IO;
using TShockAPI;
using System.IO.Streams;

namespace C3Mod
{
    internal delegate bool GetDataHandlerDelegate(GetDataHandlerArgs args);
    internal class GetDataHandlerArgs : EventArgs
    {
        public TSPlayer Player { get; private set; }
        public MemoryStream Data { get; private set; }

        public Player TPlayer
        {
            get { return Player.TPlayer; }
        }

        public GetDataHandlerArgs(TSPlayer player, MemoryStream data)
        {
            Player = player;
            Data = data;
        }
    }
    internal static class GetDataHandlers
    {
        private static Dictionary<PacketTypes, GetDataHandlerDelegate> GetDataHandlerDelegates;

        public static void InitGetDataHandler()
        {
            GetDataHandlerDelegates = new Dictionary<PacketTypes, GetDataHandlerDelegate>
            {
                {PacketTypes.PlayerKillMe, HandlePlayerKillMe},                
                {PacketTypes.PlayerDamage, HandlePlayerDamage},
            };
        }

        public static bool HandlerGetData(PacketTypes type, TSPlayer player, MemoryStream data)
        {
            GetDataHandlerDelegate handler;
            if (GetDataHandlerDelegates.TryGetValue(type, out handler))
            {
                try
                {
                    return handler(new GetDataHandlerArgs(player, data));
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
            }
            return false;
        }

        private static bool HandlePlayerKillMe(GetDataHandlerArgs args)
        {
            int index = args.Player.Index; //Attacking Player
            byte PlayerID = (byte)args.Data.ReadByte();
            byte hitDirection = (byte)args.Data.ReadByte();
            Int16 Damage = (Int16)args.Data.ReadInt16();
            bool PVP = args.Data.ReadBoolean();
            var player = C3Tools.GetC3PlayerByIndex(PlayerID);

            if (player.SpawnProtectionEnabled)
            {
                NetMessage.SendData(4, -1, PlayerID, player.PlayerName, PlayerID, 0f, 0f, 0f, 0);
                return true;
            }

            if (player.GameType == "ffa")
            {
                player.KillingPlayer.FFAScore++;
                C3Tools.BroadcastMessageToGametype("ffa", player.KillingPlayer.PlayerName + " - Score : " + player.KillingPlayer.FFAScore + " -- kills -- " + player.PlayerName + " - Score : " + player.FFAScore, Color.Black);
                player.Dead = true;
                player.TSPlayer.TPlayer.dead = true;
            }

            if (player.KillingPlayer != null)
            {
                C3Events.Death(player.KillingPlayer, player, player.GameType, PVP);
                player.KillingPlayer = null;
            }

            return false;
        }

        private static bool HandlePlayerDamage(GetDataHandlerArgs args)
        {
            int index = args.Player.Index; //Attacking Player
            byte PlayerID = (byte)args.Data.ReadByte(); //Damaged Player
            byte hitDirection = (byte)args.Data.ReadByte();
            Int16 Damage = (Int16)args.Data.ReadInt16();
            var player = C3Tools.GetC3PlayerByIndex(PlayerID);
            bool PVP = args.Data.ReadBoolean();
            byte Crit = (byte)args.Data.ReadByte();

            if (player.SpawnProtectionEnabled)
            {
                C3Tools.GetC3PlayerByIndex(index).TSPlayer.SendData(PacketTypes.PlayerUpdate, "", PlayerID);
                return true;
            }

            if (index != PlayerID)
            {
                player.KillingPlayer = C3Tools.GetC3PlayerByIndex(index);
            }
            else
                player.KillingPlayer = null;

            return false;
        }
    }
}