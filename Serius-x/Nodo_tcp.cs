using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
namespace Serius
{
    class Nodo_tcp
    {
        TcpClient client;
        public Nodo_tcp()
        {
            client = new TcpClient("localhost", 5858);
        }
        public void send(String message)
        {
            message = "Content-Length:" + message.Length + "\r\n\r\n" + message;
            byte[] datas = Encoding.UTF8.GetBytes(message);
            client.GetStream().Write(datas, 0, datas.Length);
            n++;
            read();
        }
        public void read()
        {
            MemoryStream ms = new MemoryStream();
            NetworkStream ns = client.GetStream();
            byte[] datas = new byte[256];
            do{
                int size = ns.Read(datas, 0, datas.Length);
                if (size == 0) break;
                ms.Write(datas, 0, size);
            } while (ns.DataAvailable);
            String response = Encoding.UTF8.GetString(ms.ToArray());
            ms.Close();
        }
        public String array(params Duo[] values)
        {
            String str = "{";
            for (int i = 0; i < values.Length; i++)
            {
                str += values[i].key + ":" + values[i].value;
                if (i == values.Length - 1) str += "}";
                else str += ",";
            }
            return str;
        }
        int n = 0;
        public const String seq = "seq", type = "type", command = "command", arguments = "arguments";
        public const String request = "request";
        public void version()
        {
           send(array(
               new Duo(seq, n),
               new Duo(type, request),
               new Duo(command, "version")
           ));
        }
        public void lookup()
        {
            send(array(
                new Duo(seq, n),
                new Duo(type, request),
                new Duo(arguments, array(
                    new Duo("handles", "")
                ))
            ));
        }
        public void backtrace()
        {
            send(array(
                new Duo(seq, n),
                new Duo(type, request),
                new Duo(command, "backtrace"),
                new Duo(arguments, array(
                    new Duo("fromFrame", -1),
                    new Duo("toFrame", -1),
                    new Duo("totalFrames", -1),
                    new Duo("frames", -1)
                ))
            ));
        }
        public void frame()
        {
            send(array(
                new Duo(seq, n),
                new Duo(type, request),
                new Duo(command, "frame"),
                new Duo(arguments, array(
                    new Duo("number", -1)
                ))
            ));
        }
        public void receive()
        {
        }
    }
    class Duo
    {
        public String key, value;
        public Duo(String key, String value)
        {
            this.key = key;
            this.value = value;
        }
        public Duo(String key, int value) :
            this(key, value.ToString())
        {
        }
    }
}
