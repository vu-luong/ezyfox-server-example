﻿using System;
using System.Threading;
using System.Collections.Generic;
using com.tvd12.ezyfoxserver.client.io;
using com.tvd12.ezyfoxserver.client.evt;
using com.tvd12.ezyfoxserver.client.request;
using com.tvd12.ezyfoxserver.client.config;
using com.tvd12.ezyfoxserver.client.setup;
using com.tvd12.ezyfoxserver.client.handler;
using com.tvd12.ezyfoxserver.client.constant;
using com.tvd12.ezyfoxserver.client.entity;
using com.tvd12.ezyfoxserver.client.factory;

using com.tvd12.ezyfoxserver.client.logger;

namespace com.tvd12.ezyfoxserver.client
{
    public sealed class Commands
    {

        public static readonly String ERROR = "err";
        public static readonly String GREET = "greet";
        public static readonly String ROOM_INFO = "roomInfo";
        public static readonly String CREATE_ROOM = "createRoom";
        public static readonly String LEAVE_ROOM = "leaveRoom";
        public static readonly String JOIN_ROOM = "joinRoom";
        public static readonly String JOIN_ROOM_RANDOM = "joinRoomRandom";
        public static readonly String JOIN_OR_CREATE_ROOM = "joinOrCreateRoom";
        public static readonly String SYNC_POSITION = "p";
        public static readonly String ACCESS_LOBBY_ROOM = "accessLobbyRoom";
        public static readonly String RECONNECT = "reconnect";
        public static readonly String RECONNECT_FAILED = "reconnectFailed";

        private Commands()
        {
        }

    }

    class MainClass
    {
        public static void Main(string[] args)
        {
            Thread.CurrentThread.Name = "main";

            entityTest();
            datetimeTest();

            EzyLoggerFactory.setLoggerLevel(EzyLoggerLevel.DEBUG);

            EzyClientConfig clientConfig = EzyClientConfig
                .builder()
                .clientName("freetanks")
                .zoneName("example")
                .build();
            EzyClients clients = EzyClients.getInstance();
            //EzyClient client = clients.newDefaultClient(clientConfig);
            EzyClient client = new EzyUTClient(clientConfig);
            clients.addClient(client);
            EzySetup setup = client.setup();
            setup.addEventHandler(EzyEventType.CONNECTION_SUCCESS, new EzyConnectionSuccessHandler());
            setup.addEventHandler(EzyEventType.CONNECTION_FAILURE, new EzyConnectionFailureHandler());
            setup.addEventHandler(EzyEventType.DISCONNECTION, new EzyDisconnectionHandler());
            setup.addDataHandler(EzyCommand.HANDSHAKE, new ExHandshakeEventHandler());
            setup.addDataHandler(EzyCommand.LOGIN, new ExLoginSuccessHandler());
            //setup.addDataHandler(EzyCommand.LOGIN_ERROR, new ExLoginErrorHandler());
            setup.addDataHandler(EzyCommand.APP_ACCESS, new ExAccessAppHandler());
            setup.addDataHandler(EzyCommand.UDP_HANDSHAKE, new UdpHandshakeHandler());

            EzyAppSetup appSetup = setup.setupApp("hello-world");
            //appSetup.addDataHandler(Commands.ERROR, new ErrorResponseHandler());
            appSetup.addDataHandler(Commands.ACCESS_LOBBY_ROOM, new AccessLobbyResponseHandler());
            appSetup.addDataHandler(Commands.ROOM_INFO, new RoomInfoResponseHandler());
            appSetup.addDataHandler(Commands.SYNC_POSITION, new SyncPositionHandler());

            client.connect("ws.tvd12.com", 3005);
            //client.connect("127.0.0.1", 3005);

            int time = 0;

            while (true)
            {
                Thread.Sleep(3);
                client.processEvents();
                time += 3;
                if (time > 5000)
                {
                    //client.disconnect(401);
                    time = 0;
                    //break;
                }
            }

            //mainEventsLoopTest();
        }

        private static void mainEventsLoopTest()
        {
            //Thread.Sleep(3000);

            //Console.WriteLine("client shutted down");

            //EzyMainEventsLoop mainEventsLoop = new EzyMainEventsLoop();
            //mainEventsLoop.start();

            //IList<EzyClient> cachedClients = new List<EzyClient>();

            //while(true) 
            //{
            //    Thread.Sleep(3);
            //    clients.getClients(cachedClients);
            //    foreach (EzyClient one in cachedClients)
            //        one.processEvents();
            //}
        }

        private static void entityTest()
        {
            IDictionary<String, Double> dict1 = new Dictionary<String, Double>();
            dict1["a"] = 1.0D;
            dict1["b"] = 2.0F;
            dict1["sbyte"] = (sbyte)100;
            dict1["byte"] = (byte)101;
            dict1["double"] = (double)102.3D;
            dict1["float"] = (float)103.4F;
            dict1["int"] = (int)104;
            dict1["long"] = (long)105;
            dict1["short"] = (short)106;

            IList<Object> list1 = new List<Object>();
            list1.Add(dict1);
            list1.Add((Int64)100);

            IDictionary<Object, Object> dict2 = new Dictionary<Object, Object>();
            dict2[list1] = dict1;
            dict2["Hello"] = "World";
            dict2["c"] = 3.0D;
            dict2[dict1] = list1;

            List<Object> list2 = new List<object>();
            list2.Add(dict1);
            list2.Add(dict2);
            list2.Add(list1);

            EzyArray array = EzyEntityFactory.newArrayBuilder()
                .append(dict1)
                .append(dict2)
                .append(list1)
                .appendAll(list2)
                .build();
            EzyObject obj = EzyEntityFactory.newObjectBuilder()
                .append(dict1)
                .append(dict2)
                .append("list1", list1)
                .append("list2", list2)
                .build();

            int expectedListCount = 1 + 1 + 1 + list2.Count;
            if (array.size() != expectedListCount)
            {
                throw new ArgumentException("expected: " + expectedListCount + " but: " + array.size());
            }

            int expectedDictCount = dict1.Count + dict2.Count + 1 + 1;
            if (obj.size() != expectedDictCount)
            {
                throw new ArgumentException("expected: " + expectedDictCount + " but: " + array.size());
            }
            Console.WriteLine("array: " + array);
            Console.WriteLine("\n\nobj: " + obj);

            Console.WriteLine("\n\nclone array: " + array.Clone());
            Console.WriteLine("\n\nclone obj: " + obj.Clone());

            Console.WriteLine("\n\nlong from double" + obj.get<long>("a"));

            Console.WriteLine("sbyte from sbyte: " + obj.get<sbyte>("sbyte"));
            Console.WriteLine("byte from sbyte: " + obj.get<byte>("sbyte"));
            Console.WriteLine("double from sbyte: " + obj.get<double>("sbyte"));
            Console.WriteLine("float from sbyte: " + obj.get<float>("sbyte"));
            Console.WriteLine("int from sbyte: " + obj.get<int>("sbyte"));
            Console.WriteLine("long from sbyte: " + obj.get<long>("sbyte"));
            Console.WriteLine("short from sbyte: " + obj.get<short>("sbyte"));

            Console.WriteLine("sbyte from byte: " + obj.get<sbyte>("byte"));
            Console.WriteLine("byte from byte: " + obj.get<byte>("byte"));
            Console.WriteLine("double from byte: " + obj.get<double>("byte"));
            Console.WriteLine("float from byte: " + obj.get<float>("byte"));
            Console.WriteLine("int from byte: " + obj.get<int>("byte"));
            Console.WriteLine("long from byte: " + obj.get<long>("byte"));
            Console.WriteLine("short from byte: " + obj.get<short>("byte"));

            Console.WriteLine("sbyte from double: " + obj.get<double>("double"));
            Console.WriteLine("byte from double: " + obj.get<byte>("double"));
            Console.WriteLine("double from double: " + obj.get<double>("double"));
            Console.WriteLine("float from double: " + obj.get<float>("double"));
            Console.WriteLine("int from double: " + obj.get<int>("double"));
            Console.WriteLine("long from double: " + obj.get<long>("double"));
            Console.WriteLine("short from double: " + obj.get<short>("double"));

            Console.WriteLine("sbyte from float: " + obj.get<float>("float"));
            Console.WriteLine("byte from float: " + obj.get<byte>("float"));
            Console.WriteLine("double from float: " + obj.get<double>("float"));
            Console.WriteLine("float from float: " + obj.get<float>("float"));
            Console.WriteLine("int from float: " + obj.get<int>("float"));
            Console.WriteLine("long from float: " + obj.get<long>("float"));
            Console.WriteLine("short from float: " + obj.get<short>("float"));

            Console.WriteLine("sbyte from int: " + obj.get<int>("int"));
            Console.WriteLine("byte from int: " + obj.get<byte>("int"));
            Console.WriteLine("double from int: " + obj.get<double>("int"));
            Console.WriteLine("float from int: " + obj.get<float>("int"));
            Console.WriteLine("int from int: " + obj.get<int>("int"));
            Console.WriteLine("long from int: " + obj.get<long>("int"));
            Console.WriteLine("short from int: " + obj.get<short>("int"));

            Console.WriteLine("sbyte from long: " + obj.get<long>("long"));
            Console.WriteLine("byte from long: " + obj.get<byte>("long"));
            Console.WriteLine("double from long: " + obj.get<double>("long"));
            Console.WriteLine("float from long: " + obj.get<float>("long"));
            Console.WriteLine("int from long: " + obj.get<int>("long"));
            Console.WriteLine("long from long: " + obj.get<long>("long"));
            Console.WriteLine("short from long: " + obj.get<short>("long"));

            Console.WriteLine("sbyte from short: " + obj.get<short>("short"));
            Console.WriteLine("byte from short: " + obj.get<byte>("short"));
            Console.WriteLine("double from short: " + obj.get<double>("short"));
            Console.WriteLine("float from short: " + obj.get<float>("short"));
            Console.WriteLine("int from short: " + obj.get<int>("short"));
            Console.WriteLine("long from short: " + obj.get<long>("short"));
            Console.WriteLine("short from short: " + obj.get<short>("short"));

            List<object> l = array.toList<object>();
            IDictionary<object, object> d = obj.toDict<object, object>();

            if (l.Count != expectedListCount)
            {
                throw new ArgumentException("expected: " + expectedListCount + " but: " + l.Count);
            }
            if (d.Count != expectedDictCount)
            {
                throw new ArgumentException("expected: " + expectedDictCount + " but: " + d.Count);
            }

            Console.WriteLine("list: " + l);
            Console.WriteLine("dict: " + d);
        }

        private static void datetimeTest()
        {
            Thread.Sleep(10);
            DateTime now = DateTime.Now;
            Thread.Sleep(100);
            long offset = EzyDateTimes.getOffsetMillis(now, DateTime.Now);
            Console.WriteLine("offset: " + offset);
            Console.WriteLine("status int: " + (int)EzySocketStatus.CONNECTED);
        }
    }

    class ExHandshakeEventHandler : EzyHandshakeHandler
    {

        protected override EzyRequest getLoginRequest()
        {
            return new EzyLoginRequest("example", "dungtv", "123456");
        }
    }

    class ExLoginSuccessHandler : EzyLoginSuccessHandler
    {

        protected override void handleLoginSuccess(EzyData responseData)
        {
            client.send(new EzyAppAccessRequest("hello-world"));
        }
    }

    class ExAccessAppHandler : EzyAppAccessHandler
    {

        protected override void postHandle(EzyApp app, EzyArray data)
        {
            //app.send(Commands.ACCESS_LOBBY_ROOM);
            //this.client.send(new EzyAppExitRequest(app.getId()));
            this.client.udpConnect(2611);
        }
    }

    class AccessLobbyResponseHandler : EzyAppDataHandler
    {

        public void handle(EzyApp app, EzyData d)
        {
            EzyObject data = (EzyObject)d;
            long currentRoomId = data.get<long>("currentRoomId");
            if (currentRoomId <= 0)
            {
                app.send(Commands.JOIN_OR_CREATE_ROOM);
            }
            else
            {
                app.send(Commands.RECONNECT);
            }
        }
    }

    class RoomInfoResponseHandler : EzyAppDataHandler
    {

        public void handle(EzyApp app, EzyData d)
        {
            EzyArray pos = EzyEntityFactory.newArrayBuilder()
                                           .append(1.0D, 2.0D, 3.0D)
                                           .build();
            app.send(Commands.SYNC_POSITION, pos);
        }
    }

    class SyncPositionHandler : EzyAppDataHandler
    {

        public void handle(EzyApp app, EzyData d)
        {
            //System.out.println("syn pos: " + data);
        }
    }

    class UdpHandshakeHandler : EzyUdpHandshakeHandler
    {

        protected override void onAuthenticated(EzyArray data)
        {
            EzyApp app = client.getZone().getApp();
            app.udpSend("udpGreet", EzyEntityFactory.newObjectBuilder()
                    .append("who", "Dzung")
                    .build());
        }

    }

}
