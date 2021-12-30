using GameServer.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Metagame.GameRoom
{
    public class GameRoom : IGameRoom
    {
        public RoomData Data { get; private set; }
        public bool IsAvailableToJoin { get; set; } = true;

        public GameRoom(RoomData data)
        {
            Data = data;
        }

        public void Launch(List<User> usersToAdd)
        {
            //launch new process here
        }

        public void RoomStarted()
        {
            //connect here all users
        }

        public Task<ApiResult> Join(User user)
        {
            Data.Users.Add(user);

            return Task.FromResult(ApiResult.Ok);
        }

        public void Stop()
        {

        } 
    }
}
