﻿using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin
{
    /// <summary>
    /// Handles showing the intro screen and getting user input
    /// </summary>
    public class GameIntro
    {
        public string PlayerName { get; private set; }

        public GameIntro() {
            PlayerName = null;
        }

        /// <summary>
        /// Run the intro sequence. After this returns use properties to decide whether to load a game or start a new one
        /// </summary>
        public void ShowIntroScreen() {

            OpeningScreen();

            PlayerNameScreen();
        }

        Point preambleTL;

        private void PlayerNameScreen()
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            //Clear screen
            rootConsole.Clear();

            //Draw frame
            //Why xpos 2 here?
            rootConsole.DrawFrame(1, 2, Screen.Instance.Width - 2, Screen.Instance.Height - 3, true);

            //Draw preample
            preambleTL = new Point(5, 5);

            int height;
            List<string> preamble = Utility.LoadTextFile("text/introPreamble", Screen.Instance.Width - 2 * preambleTL.x, out height);

            for (int i = 0; i < preamble.Count; i++)
            {
                rootConsole.PrintLineRect(preamble[i], preambleTL.x, preambleTL.y + i, Screen.Instance.Width - 2 * preambleTL.x, 1, LineAlignment.Left);
            }

            Point nameIntro = new Point(5, 5 + preamble.Count + 2);
            do {
                PlayerName = Screen.Instance.GetUserString("Rogue name", nameIntro, 5);
                LogFile.Log.LogEntry("Player name: " + PlayerName);
            } while(PlayerName.Contains(" ") || PlayerName == "");


        }

        /// <summary>
        /// Title screen. Press any key to continue
        /// </summary>

        Point titleCentre;
        Point anyKeyLocation;
        
        private void OpeningScreen()
        {
            Screen screen = Screen.Instance;

            //Get screen handle

            RootConsole rootConsole = RootConsole.GetInstance();

            //Draw title

            titleCentre = new Point(screen.Width / 2, screen.Height / 2);
            rootConsole.PrintLineRect("Welcome to DDRogue", titleCentre.x, titleCentre.y, screen.Width, 1, LineAlignment.Center);

            //Any key to continue

            anyKeyLocation = new Point(screen.Width / 2, screen.Height - 5);
            rootConsole.PrintLineRect("Press any key to continue", anyKeyLocation.x, anyKeyLocation.y, screen.Width, 1, LineAlignment.Center);
            
            //Update screen
            Screen.Instance.FlushConsole();

            //Wait for key
            KeyPress userKey = Keyboard.WaitForKeyPress(true);
        }
    }
}