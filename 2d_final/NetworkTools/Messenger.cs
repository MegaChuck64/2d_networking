using Lidgren.Network;

namespace NetworkTools
{
    public class Messenger
    {
        public string Name { get; set; }
        public NetConnection Connection { get; set; }


        public Messenger(string name, NetConnection conn)
        {
            Name = name;
            Connection = conn;
        }

    }
}
