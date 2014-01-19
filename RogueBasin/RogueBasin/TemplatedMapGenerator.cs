﻿using GraphMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueBasin
{

    /** Builds up a template map and connectivity graph, exposing methods for room aligned placement */
    public class TemplatedMapGenerator
    {
        public class DoorInfo
        {
            public TemplatePositioned OwnerRoom { get; private set; }
            public int DoorIndexInRoom { get; private set; }
            public int OwnerRoomIndex { get; private set; }
            public RoomTemplate.DoorLocation DoorLocation { get; private set; }

            public DoorInfo(TemplatePositioned ownerRoom, int ownerRoomIndex, int doorIndex, RoomTemplate.DoorLocation doorLocation)
            {
                OwnerRoom = ownerRoom;
                DoorIndexInRoom = doorIndex;
                OwnerRoomIndex = ownerRoomIndex;
                DoorLocation = doorLocation;
            }

            public Point MapCoords
            {
                get
                {
                    return OwnerRoom.PotentialDoors[DoorIndexInRoom];
                }
            }
        }

        List<DoorInfo> potentialDoors = new List<DoorInfo>();
        int nextRoomIndex = 0;

        TemplatedMapBuilder mapBuilder;
        ConnectivityMap connectivityMap;

        public TemplatedMapGenerator(TemplatedMapBuilder builder)
        {
            this.mapBuilder = builder;
            this.connectivityMap = new ConnectivityMap();
        }

        private int NextRoomIndex()
        {
            return nextRoomIndex;
        }

        private void IncreaseNextRoomIndex()
        {
            nextRoomIndex++;
        }

        public bool HaveRemainingPotentialDoors()
        {
            return potentialDoors.Count > 0;
        }

        public List<DoorInfo> PotentialDoors
        {
            get
            {
                return potentialDoors;
            }
        }

        public ConnectivityMap ConnectivityMap
        {
            get
            {
                return connectivityMap;
            }
        }

        public bool PlaceRoomTemplateAtPosition(RoomTemplate roomTemplate, Point point)
        {
            var roomIndex = NextRoomIndex();
            var positionedRoom = new TemplatePositioned(point.x, point.y, 0, roomTemplate, roomIndex);
            bool placementSuccess = mapBuilder.AddPositionedTemplate(positionedRoom);

            if (!placementSuccess)
                return false;

            IncreaseNextRoomIndex();
            AddNewDoorsToPotentialDoors(positionedRoom, roomIndex);

            return true;
        }

        private void AddNewDoorsToPotentialDoors(TemplatePositioned positionedRoom, int roomIndex)
        {
            //Store a reference to each potential door in the room
            int noDoors = positionedRoom.PotentialDoors.Count();
            //var currentDoorLocations = potentialDoors.Select(d => d.MapCoords);

            for (int i = 0; i < noDoors; i++)
            {
                //if (!currentDoorLocations.Contains(positionedRoom.PotentialDoors[i]))
                    potentialDoors.Add(new DoorInfo(positionedRoom, roomIndex, i, RoomTemplateUtilities.GetDoorLocation(positionedRoom.Room, i)));
            }
        }

        /// <summary>
        /// Join 2 doors with a corridor. They must be on the opposite sides of their parent rooms (for now)
        /// </summary>
        public bool JoinDoorsWithCorridor(DoorInfo firstDoor, DoorInfo secondDoor, RoomTemplate corridorTemplate)
        {
            try
            {
                var firstDoorLoc = RoomTemplateUtilities.GetDoorLocation(firstDoor.OwnerRoom.Room, firstDoor.DoorIndexInRoom);
                var secondDoorLoc = RoomTemplateUtilities.GetDoorLocation(secondDoor.OwnerRoom.Room, secondDoor.DoorIndexInRoom);

                var firstDoorCoord = firstDoor.MapCoords;
                var secondDoorCoord = secondDoor.MapCoords;

                var corridorTermini = RoomTemplateUtilities.CorridorTerminalPointsBetweenDoors(firstDoor.MapCoords, firstDoor.DoorLocation, secondDoor.MapCoords, secondDoor.DoorLocation);

                bool canDoLSharedCorridor = RoomTemplateUtilities.CanBeConnectedWithLShapedCorridor(firstDoorCoord, firstDoorLoc, secondDoorCoord, secondDoorLoc);
                bool canDoBendCorridor = RoomTemplateUtilities.CanBeConnectedWithBendCorridor(firstDoorCoord, firstDoorLoc, secondDoorCoord, secondDoorLoc);
                bool canDoStraightCorridor = RoomTemplateUtilities.CanBeConnectedWithStraightCorridor(firstDoorCoord, firstDoorLoc, secondDoorCoord, secondDoorLoc);
                bool areAdjacent = corridorTermini.Item1 == secondDoorCoord && corridorTermini.Item2 == firstDoorCoord;
                bool areOverlapping = firstDoorCoord == secondDoorCoord;

                if (!canDoLSharedCorridor && !canDoBendCorridor && !canDoStraightCorridor && !areAdjacent && !areOverlapping)
                    throw new ApplicationException("No corridor available to connect this type of door");

                if (areAdjacent || areOverlapping)
                {
                    //Add a direct connection in the connectivity graph
                    connectivityMap.AddRoomConnection(firstDoor.OwnerRoomIndex, secondDoor.OwnerRoomIndex);
                }
                else
                {
                    //Create template

                    var horizontal = false;
                    if (firstDoorLoc == RoomTemplate.DoorLocation.Left || firstDoorLoc == RoomTemplate.DoorLocation.Right)
                    {
                        horizontal = true;
                    }

                    int xOffset = corridorTermini.Item2.x - corridorTermini.Item1.x;
                    int yOffset = corridorTermini.Item2.y - corridorTermini.Item1.y;

                    RoomTemplate expandedCorridor;
                    Point corridorTerminus1InTemplate;

                    if (canDoBendCorridor)
                    {
                        int transition = (int)Math.Floor(yOffset / 2.0);
                        if (horizontal == true)
                        {
                            transition = (int)Math.Floor(xOffset / 2.0);
                        }
                        var expandedCorridorAndPoint = RoomTemplateUtilities.ExpandCorridorTemplateBend(xOffset, yOffset, transition, horizontal, corridorTemplate);
                        expandedCorridor = expandedCorridorAndPoint.Item1;
                        corridorTerminus1InTemplate = expandedCorridorAndPoint.Item2;
                    }
                    else if (canDoLSharedCorridor)
                    {
                        var expandedCorridorAndPoint = RoomTemplateUtilities.ExpandCorridorTemplateLShaped(xOffset, yOffset, horizontal, corridorTemplate);
                        expandedCorridor = expandedCorridorAndPoint.Item1;
                        corridorTerminus1InTemplate = expandedCorridorAndPoint.Item2;
                    }
                    else
                    {
                        var offsetToUse = horizontal ? xOffset : yOffset;
                        var expandedCorridorAndPoint = RoomTemplateUtilities.ExpandCorridorTemplateStraight(offsetToUse, horizontal, corridorTemplate);
                        expandedCorridor = expandedCorridorAndPoint.Item1;
                        corridorTerminus1InTemplate = expandedCorridorAndPoint.Item2;
                    }

                    //Place corridor

                    //Match corridor tile to location of door
                    Point topLeftCorridor = corridorTermini.Item1 - corridorTerminus1InTemplate;

                    var corridorRoomIndex = NextRoomIndex();
                    var positionedCorridor = new TemplatePositioned(topLeftCorridor.x, topLeftCorridor.y, 0, expandedCorridor, corridorRoomIndex);

                    if (!mapBuilder.CanBePlacedWithoutOverlappingOtherTemplates(positionedCorridor))
                        return false;

                    //Place the corridor
                    mapBuilder.AddPositionedTemplate(positionedCorridor);
                    IncreaseNextRoomIndex();

                    //Add connections to the old and new rooms
                    connectivityMap.AddRoomConnection(firstDoor.OwnerRoomIndex, corridorRoomIndex);
                    connectivityMap.AddRoomConnection(corridorRoomIndex, secondDoor.OwnerRoomIndex);
                }
                
                //Remove both doors from the potential list
                potentialDoors.Remove(firstDoor);
                potentialDoors.Remove(secondDoor);

                return true;
            }
            catch (ApplicationException ex)
            {
                LogFile.Log.LogEntryDebug("Failed to join doors: " + ex.Message, LogDebugLevel.Medium);
                return false;
            }
        }

        public bool PlaceRoomTemplateAlignedWithExistingDoor(RoomTemplate roomTemplateToPlace, RoomTemplate corridorTemplate, DoorInfo existingDoor, int distanceApart)
        {
            int newRoomDoorIndex = Game.Random.Next(roomTemplateToPlace.PotentialDoors.Count);
            return PlaceRoomTemplateAlignedWithExistingDoor(roomTemplateToPlace, corridorTemplate, existingDoor, newRoomDoorIndex, distanceApart);
        }

        public bool PlaceRoomTemplateAlignedWithExistingDoor(RoomTemplate roomTemplateToPlace, RoomTemplate corridorTemplate, DoorInfo existingDoor, int newRoomDoorIndex, int distanceApart)
        {
            var newRoomIndex = NextRoomIndex();
            

            Point existingDoorLoc = existingDoor.MapCoords;

            Tuple<TemplatePositioned, Point> newRoomTuple = RoomTemplateUtilities.AlignRoomOnDoor(roomTemplateToPlace, newRoomIndex, existingDoor.OwnerRoom,
                newRoomDoorIndex, existingDoor.DoorIndexInRoom, distanceApart);

            var alignedNewRoom = newRoomTuple.Item1;
            var alignedDoorLocation = newRoomTuple.Item2;

            //In order to place this successfully, we need to be able to both place the room and a connecting corridor

            if (!mapBuilder.CanBePlacedWithoutOverlappingOtherTemplates(alignedNewRoom))
                return false;

            //Increase next room for any corridor we may add
            IncreaseNextRoomIndex();

            TemplatePositioned corridorTemplateConnectingRooms = null;

            if (distanceApart > 1)
            {
                //Need points that are '1-in' from the doors
                var doorOrientation = RoomTemplateUtilities.GetDoorLocation(existingDoor.OwnerRoom.Room, existingDoor.DoorIndexInRoom);
                bool isHorizontal = doorOrientation == RoomTemplate.DoorLocation.Left || doorOrientation == RoomTemplate.DoorLocation.Right;

                var corridorTermini = RoomTemplateUtilities.CorridorTerminalPointsBetweenDoors(existingDoorLoc, existingDoor.DoorLocation, alignedDoorLocation, RoomTemplateUtilities.GetOppositeDoorLocation(existingDoor.DoorLocation));
                var corridorIndex = NextRoomIndex();

                if (corridorTermini.Item1 == corridorTermini.Item2)
                {
                    corridorTemplateConnectingRooms =
                        RoomTemplateUtilities.GetTemplateForSingleSpaceCorridor(corridorTermini.Item1,
                        RoomTemplateUtilities.ArePointsOnVerticalLine(corridorTermini.Item1, corridorTermini.Item2), 0, corridorTemplate, corridorIndex);
                }
                else
                {
                    corridorTemplateConnectingRooms =
                        RoomTemplateUtilities.GetTemplateForCorridorBetweenPoints(corridorTermini.Item1, corridorTermini.Item2, 0, corridorTemplate, corridorIndex);
                }

                //Implicit guarantee that the corridor won't overlap with the new room we're about to place
                //(but it may overlap other previously placed rooms or corridors)
                if (!mapBuilder.CanBePlacedWithoutOverlappingOtherTemplates(corridorTemplateConnectingRooms))
                    return false;

                //Place the corridor
                mapBuilder.AddPositionedTemplate(corridorTemplateConnectingRooms);
                IncreaseNextRoomIndex();

                //Add connections to the old and new rooms
                connectivityMap.AddRoomConnection(existingDoor.OwnerRoomIndex, corridorIndex);
                LogFile.Log.LogEntryDebug("Addiing connection: " + existingDoor.OwnerRoomIndex + " to " + corridorIndex, LogDebugLevel.Medium);
                connectivityMap.AddRoomConnection(corridorIndex, newRoomIndex);
                LogFile.Log.LogEntryDebug("Addiing connection: " + corridorIndex + " to " + newRoomIndex, LogDebugLevel.Medium);
            }
            else
            {
                //No corridor - a direct connection between the rooms
                connectivityMap.AddRoomConnection(existingDoor.OwnerRoomIndex, newRoomIndex);
                LogFile.Log.LogEntryDebug("Addiing connection: " + existingDoor.OwnerRoomIndex + " to " + newRoomIndex, LogDebugLevel.Medium);
            }

            //Place the room
            bool successfulPlacement = mapBuilder.AddPositionedTemplate(alignedNewRoom);
            if (!successfulPlacement)
            {
                LogFile.Log.LogEntryDebug("Room failed to place because overlaps own corridor - bug", LogDebugLevel.High);
                return false;
            }

            //Add the new potential doors (excluding the one we are linked on)
            //Can't find a nice linq alternative
            int noDoors = alignedNewRoom.PotentialDoors.Count();
            for (int i = 0; i < noDoors; i++)
            {
                if (alignedNewRoom.PotentialDoors[i] == alignedDoorLocation)
                    continue;
                potentialDoors.Add(new DoorInfo(alignedNewRoom, newRoomIndex, i, RoomTemplateUtilities.GetDoorLocation(alignedNewRoom.Room, i)));
            }

            //If successful, remove the candidate door from the list
            potentialDoors.Remove(existingDoor);

            return true;
        }

        public void ReplaceDoorsWithTerrain(RoomTemplateTerrain roomTemplateTerrain)
        {
            foreach (var door in PotentialDoors)
            {
                mapBuilder.AddOverrideTerrain(door.MapCoords, roomTemplateTerrain);
            }

            PotentialDoors.Clear();
        }
    }
}
