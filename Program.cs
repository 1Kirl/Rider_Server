using System;
using LiteNetLib;
using LiteNetLib.Utils;
using System.Net;

class Program
{
    static void Main()
    {
        var netManager = NetworkManager.Instance;
        netManager.Start();
    }
}