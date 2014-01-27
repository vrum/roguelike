﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RogueBasin;
using GraphMap;
using System.Collections.Generic;
using System.Linq;

namespace DDRogueTest
{
    [TestClass]
    public class MapInfoTest
    {
        [TestMethod]
        public void MapsFromDifferentLevelsCanBeConnected()
        {
            var mapInfo = new MapInfoBuilder();

            var l1ConnectivityMap = new ConnectivityMap();
            l1ConnectivityMap.AddRoomConnection(1, 2);
            l1ConnectivityMap.AddRoomConnection(2, 3);

            var l2ConnectivityMap = new ConnectivityMap();
            l2ConnectivityMap.AddRoomConnection(5, 6);
            l2ConnectivityMap.AddRoomConnection(6, 7);

            mapInfo.AddConstructedLevel(0, l1ConnectivityMap, new List<TemplatePositioned>(), 0);
            mapInfo.AddConstructedLevel(1, l2ConnectivityMap, new List<TemplatePositioned>(), new Connection(3, 5));

            ConnectivityMap fullMap = mapInfo.FullConnectivityMap;

            CollectionAssert.AreEquivalent(new List<Connection>(new Connection[] { new Connection(1, 2), new Connection(2, 3),
                new Connection(3, 5), new Connection(5, 6), new Connection(6, 7)}), fullMap.GetAllConnections().ToList());
        }

        [TestMethod]
        public void RoomsCanBeRetrievedByIndex()
        {
            var newTemplate = new TemplatePositioned(9, 9, 0, null, 100);
            var mapInfoBuilder = new MapInfoBuilder();

            var templateList = new List<TemplatePositioned>();
            templateList.Add(newTemplate);

            var map = new ConnectivityMap();
            map.AddRoomConnection(new Connection(100, 101));

            mapInfoBuilder.AddConstructedLevel(0, map, templateList, 100);

            var mapInfo = new MapInfo(mapInfoBuilder);

            Assert.AreEqual(new Point(9,9), mapInfo.GetRoom(100).Location);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public void AddConstructedLevelCantBeCalledForFirstLevel()
        {
            var mapInfo = new MapInfoBuilder();
            mapInfo.AddConstructedLevel(0, new ConnectivityMap(), null, new Connection(1, 2));
        }
    }
}
