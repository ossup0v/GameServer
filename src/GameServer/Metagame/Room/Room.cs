using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Metagame.Room
{
    public class Room
    {
        public RoomData Data { get; private set; }
        public bool IsAvailableToJoin { get; set; } = true;

        public Room(RoomData data)
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

        public void Join(User user)
        {
            //add here user to room
        }

        public void Stop()
        {

        } 
    }
}
