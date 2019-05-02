using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SpeedBallServer.Test
{

    public struct FakeData
    {
        public FakeEndPoint endPoint;
        public byte[] data;
    }

    public class FakeQueueEmpty : Exception
    {

    }

    public class FakeTransport : IGameTransport
    {
        //transport address
        private FakeEndPoint bindedEndPoint;

        //hold packet received from server/sended from clients
        private Queue<FakeData> recvQueue;

        //hold packets sended from server
        private Queue<FakeData> sendQueue;

        public EndPoint BindedEndPoint
        {
            get { return bindedEndPoint; }
        }

        public int GetRecvQueueCount()
        {
            return recvQueue.Count;
        }

        public int GetSendQueueCount()
        {
            return sendQueue.Count;
        }

        public FakeTransport()
        {
            recvQueue = new Queue<FakeData>();
            sendQueue = new Queue<FakeData>();
        }

        public void ClientEnqueue(FakeData data)
        {
            recvQueue.Enqueue(data);
        }

        public FakeData ClientDequeue()
        {
            if (sendQueue.Count <= 0)
                throw new FakeQueueEmpty();
            return sendQueue.Dequeue();
        }

        public void Bind(string address, int port)
        {
            bindedEndPoint = new FakeEndPoint(address, port);
        }

        public EndPoint CreateEndPoint()
        {
            return new FakeEndPoint();
        }

        public byte[] Recv(int bufferSize, ref EndPoint sender)
        {
            if (recvQueue.Count <= 0)
                return null;
            FakeData fakeData = recvQueue.Dequeue();
            if (fakeData.data.Length > bufferSize)
                return null;
            sender = fakeData.endPoint;
            return fakeData.data;
        }

        public bool Send(byte[] data, EndPoint endPoint)
        {
            FakeData fakeData = new FakeData();
            fakeData.data = data;
            fakeData.endPoint = endPoint as FakeEndPoint;
            sendQueue.Enqueue(fakeData);
            return true;
        }

    }

}
