namespace KaizerWaldCode
{
    /// <summary>
    /// Represents a single player on the game server
    /// </summary>
    public struct PlayerData
    {
        public string PlayerName;  //name of the player
        public ulong ClientID; //the identifying id of the client

        public PlayerData(string playerName, ulong clientId)
        {
            PlayerName = playerName;
            ClientID = clientId;
        }
    }
}