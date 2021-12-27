using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace KaizerWaldCode.V2
{
    
    /// <summary>
    /// Contains all NetworkVariables and RPCs of a character. This component is present on both client and server objects.
    /// MAKE the Junction between client and server :
    /// Client Send input with ServerRPC wich trigger event on the ServerSide
    /// </summary>
    public class NetworkCharacterState : NetworkBehaviour
    {
        /// Indicates how the character's movement should be depicted.
        //public NetworkVariable<MovementStatus> MovementStatus { get; } = new NetworkVariable<MovementStatus>();

// SERVER RPC
//======================================================================================================================

        public event Action<Vector2> ReceivedClientInput;

        /// <summary>
        /// RPC to send inputs for this character from a client to a server.
        /// Go to <see cref="ServerCharacterMouvement"/> 
        /// </summary>
        /// <param name="movementTarget">The position which this character should move towards.</param>
        [ServerRpc]
        public void SendCharacterInputServerRpc(Vector2 moveInput)
        {
            ReceivedClientInput?.Invoke(moveInput);
        }
    }
}
