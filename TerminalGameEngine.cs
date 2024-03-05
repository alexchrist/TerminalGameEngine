namespace TerminalGameEngine
{
    using System.Dynamic;

    public class Engine
    {
        static int height = 16;
        static int width = 62;
        static Random random = new Random();
        public static char[,] frameCoords = new char[height, width];
        static List<FrameEntry> gameEntities = new List<FrameEntry>();
        static Dictionary<string, string[]> backgroundFrames = new Dictionary<string, string[]>();
        static List<MenuItem> menuItems = new List<MenuItem>();
        static string[]? currentFrame;
        static string[] gameState = new string[] { "menu", "running", "paused" };
        static string currentGameState = gameState[0];
        static string[] menuState = new string[] { "start", "settings", "controls", "pause", "default" };
        
        public void Run()
        {
            // Game loop
            Initializers.Initialize();

            while (Definitions.isRunning)
            {
                // Frame.FrameUpdater();

                if (!Definitions.isPaused)
                {
                    Pickup.SpawnPickup();

                    Pickup.PickupCheck();

                    Update.UpdateScore();
                }

                Input.CheckInput();

                // Frame.FrameUpdater();

                Update.CheckGameState();

                Frame.FrameUpdater();

                Frame.FramePainter();

                while (Definitions.isPaused)
                {
                    

                    Input.MenuSelect();

                    Menu.MenuSelection();

                    Frame.FrameUpdater();

                    Frame.FramePainter();

                    Update.CheckGameState();
                }
                // Frame.FramePainter();
                Thread.Sleep(Settings.System.FrameRate);
            }

            ExitGame();
        }

        public Engine()
        {

        }

        public void ChangeSystemSettings(int height = 0, int width = 0, double fps = 0)
        {
            if (!(height <= 0))
            {
                Settings.System.Height = height;
            }

            if (!(width <= 0))
            {
                Settings.System.Width = width;
            }

            if (!(fps < 24) && !(fps > 60))
            {
                Settings.System.FPS = fps;
            }
        }

        public void ChangeUserSettings(string? pickupIcon = null, int pickupAmount = 0, string? playerCharacter = null)
        {
            if (pickupIcon != null)
            {
                Settings.User.pickupIcon = pickupIcon;
            }

            if (!(pickupAmount <= 0) && !(pickupAmount > 20))
            {
                Settings.User.pickupAmount = pickupAmount;
            }

            if (playerCharacter != null)
            {
                Settings.User.playerCharacter = playerCharacter;
            }
        }

        public class Settings
        {
            public class System
            {
                public static int Height = height;
                public static int Width = width;
                private static double fps = 35.00;
                public static double FPS { get { return fps; } set { fps = value; FrameRate = (int)Math.Round(1000 / fps); } }
                public static int FrameRate = (int)Math.Round(1000 / FPS);

            }

            public static class User
            {
                // Pickup variables
                public static string pickupIcon = "*";
                public static int pickupAmount = 5;

                // Player variables
                public static string playerCharacter = "(*-*)";

            }
        }

        public static class Definitions
        {
            public static bool isRunning = true;
            public static bool isPaused = false;
            public static int gameScore = 0;

            public static FrameEntry gameScoreEntry = new FrameEntry();
            public static FrameEntry frameRateInfo = new FrameEntry();

            public static int playerX;
            public static int playerY;
            public static FrameEntry player = new FrameEntry("player", playerX, playerY, Settings.User.playerCharacter);
        }

        public static void ExitGame()
        {
            Console.Clear();
            Console.CursorVisible = true;
            Console.WriteLine();
            Console.SetCursorPosition(Settings.System.Width / 2 - 1, (Settings.System.Height / 2) - 1);
            Console.WriteLine("Game quit");
            Environment.Exit(0);
        }

        public static class Initializers
        {
            public static void Initialize()
            {
                Console.CursorVisible = false;
                Console.Clear();
                Console.SetCursorPosition(0, 0);
                PopulateFrameTemplate();

                ShowDebugInfo();
                ShowGameInfo();
                // InitializePickups();
                InitializePlayer();

                Frame.CreateFrameTemplateFile();
                PopulateCustomFrames();
                Update.CheckGameState();

                Menu.MenuSelection();

                Frame.FrameUpdater();

                Frame.FramePainter();
            }

            static void PopulateFrameTemplate()
            {
                // int width = 62;
                // int height = 17;
                // string[] frameCell = new string[3];
                // string[] frameLine = new string[width];
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        if (y == 1 || y == height - 1)
                        {
                            frameCoords[y, x] = '-';
                        }
                        else if (x == 0 || x == width - 1)
                        {
                            frameCoords[y, x] = '|';
                        }
                        else
                        {
                            frameCoords[y, x] = ' ';
                        }
                    }
                }
            }

            static void ShowDebugInfo()
            {
                string debugString = Settings.System.FrameRate.ToString() + " ms/f";
                Definitions.frameRateInfo = new FrameEntry("framerate", Settings.System.Width - debugString.Length - 2, 0, debugString);
                gameEntities.Add(Definitions.frameRateInfo);
            }

            static void ShowGameInfo()
            {
                // gameScoreEntry = new FrameEntry("gamescore", 2, 0, number: gameScore, isNumber: true);
                Definitions.gameScoreEntry = new FrameEntry("gamescore", 2, 0, Definitions.gameScore.ToString() + (Definitions.gameScore == 1 ? " point" : " points"));
                gameEntities.Add(Definitions.gameScoreEntry);
            }

            static void InitializePlayer()
            {
                Definitions.playerX = 1;
                Definitions.playerY = Settings.System.Height - 2;

                Definitions.player = new FrameEntry("player", Definitions.playerX, Definitions.playerY, Settings.User.playerCharacter);
                gameEntities.Add(Definitions.player);
            }

            static void PopulateCustomFrames()
            {
                foreach (string frame in menuState)
                {
                    Frame.ReadFrameFile(frame);
                }
            }
        }

        public static class Frame
        {
            public static void FrameUpdater()
            {
                if (currentFrame != null)
                {
                    for (int y = 0; y < currentFrame.Length - 1; y++)
                    {
                        for (int x = 0; x < currentFrame[y].Length; x++)
                        {
                            if (currentFrame[y][x] == 'X')
                            {
                                x++;
                                int[] menuItemLocation = new int[2];
                                switch (currentFrame[y][x])
                                {
                                    case 'p':
                                        Menu.AddMenuItem("play", y, x - 1, true);
                                        x++;
                                        break;

                                    case 's':
                                        Menu.AddMenuItem("settings", y, x - 1);
                                        x++;
                                        break;

                                    case 'e':
                                        Menu.AddMenuItem("exit", y, x - 1);
                                        x++;
                                        break;

                                    case 'r':
                                        Menu.AddMenuItem("resume", y, x - 1, true);
                                        x++;
                                        break;

                                    case 'm':
                                        Menu.AddMenuItem("mainmenu", y, x - 1);
                                        x++;
                                        break;

                                }

                            }
                            frameCoords[y, x] = currentFrame[y][x];
                        }
                    }
                }
                else
                {
                    for (int y = 0; y < Settings.System.Height - 1; y++)
                    {
                        for (int x = 1; x < Settings.System.Width - 1; x++)
                        {
                            if (y != 1)
                            {
                                frameCoords[y, x] = ' ';
                            }
                            else
                                frameCoords[y, x] = '-';
                        }
                    }
                }

                if (!Definitions.isPaused)
                {
                    if (gameEntities.Count == 0)
                    {
                        return;
                    }
                    else
                    {
                        for (int entries = 0; entries < gameEntities.Count; entries++)
                        {
                            FrameEntry currentEntry = gameEntities[entries];
                            string entryIcon;
                            if (currentEntry.Icon != null)
                            {
                                entryIcon = currentEntry.Icon;
                            }
                            else
                            {
                                entryIcon = " ";
                            }
                            for (int entryChar = 0; entryChar < entryIcon.Length; entryChar++)
                                frameCoords[currentEntry.Y, currentEntry.X + entryChar] = entryIcon[entryChar];
                        }
                    }
                }

                if (Definitions.isPaused)
                {
                    Menu.MenuSelection();
                }
            }

            public static void FramePainter()
            {
                Console.Clear();
                for (int y = 0; y < height; y++)
                {
                    string frameLine = "";

                    for (int x = 0; x < width; x++)
                    {
                        frameLine += frameCoords[y, x];
                    }

                    Console.SetCursorPosition(0, y);
                    Console.WriteLine(frameLine);
                }
            }

            public static FrameEntry? GetFrameEntry(FrameEntry currentFrameEntry)
            {
                int entryIndex = gameEntities.IndexOf(currentFrameEntry);

                if (entryIndex != -1)
                {
                    return gameEntities[entryIndex];
                }
                else
                    return null;
            }

            public static void CreateFrameTemplateFile()
            {
                string path = @".\frames\template.txt";
                string frameLine = "";
                string[] fullFrame = new string[height];
                for (int y = 0; y < height; y++)
                {
                    frameLine = "";
                    for (int x = 0; x < width; x++)
                    {
                        frameLine += frameCoords[y, x];
                    }
                    fullFrame[y] += frameLine;
                }

                File.WriteAllLines(path, fullFrame);
            }

            public static void ReadFrameFile(string file)
            {
                string path = $".\\frames\\{file}.txt";
                string[] fullFrame;
                if (File.Exists(path))
                {
                    fullFrame = File.ReadAllLines(path);
                    backgroundFrames.Add(file, fullFrame);
                }
            }
        }

        public static class Pickup
        {
            public static void SpawnPickup()
            {
                while (GetPlacedPickups().Count < Settings.User.pickupAmount)
                {
                    List<FrameEntry> placedPickups = GetPlacedPickups();
                    int currentIndex;
                    int randX = random.Next(1, Settings.System.Width - 1);
                    int randY = random.Next(2, Settings.System.Height - 1);

                    if (placedPickups.Count != 0)
                    {
                        string[] usedIndexes = new string[placedPickups.Count];

                        for (int i = 0; i < placedPickups.Count; i++)
                        {
                            while (placedPickups[i].X == randX || placedPickups[i].Y == randY || Collision.ColidesWithPlayer(new FrameEntry(xPos: randX, yPos: randY)))
                            {
                                randX = random.Next(1, Settings.System.Width - 1);
                                randY = random.Next(2, Settings.System.Height - 1);
                            }
                        }
                    }
                    currentIndex = UniquePickupIndex();
                    FrameEntry pickup = new FrameEntry("pickup" + currentIndex, randX, randY, Settings.User.pickupIcon);
                    gameEntities.Add(pickup);
                }
            }

            static List<FrameEntry> GetPlacedPickups()
            {
                return gameEntities.FindAll(FindPickup);
            }

            static int UniquePickupIndex()
            {
                int currentIndex = 0;
                List<FrameEntry> placedPickups = GetPlacedPickups();

                if (placedPickups.Count != 0)
                {
                    string[] usedIndexes = new string[placedPickups.Count];

                    for (int i = 0; i < usedIndexes.Length; i++)
                    {
                        usedIndexes[i] = placedPickups[i].Name.ToString().Substring(6, 1);
                    }

                    Array.Sort(usedIndexes);

                    for (int i = 0; i <= usedIndexes.Length; i++)
                    {
                        if (!usedIndexes.Contains(i.ToString()))
                        {
                            currentIndex = i;
                        }
                    }
                }

                return currentIndex;
            }

            static bool FindPickup(FrameEntry entry)
            {
                if (entry.Name.Contains("pickup"))
                {
                    return true;
                }
                return false;
            }

            public static void PickupCheck()
            {
                FrameEntry? playerEntry = Frame.GetFrameEntry(Definitions.player);
                List<FrameEntry> placedPickups = GetPlacedPickups();

                if (placedPickups.Count != 0)
                {
                    for (int i = 0; i < placedPickups.Count; i++)
                    {
                        FrameEntry? currentPickup = Frame.GetFrameEntry(placedPickups[i]);
                        if (currentPickup != null && Collision.ColidesWithPlayer(currentPickup))
                        {
                            Definitions.gameScore++;
                            gameEntities.Remove(currentPickup);
                        }
                    }
                }
            }
        }

        public static class Collision
        {
            public static bool ColidesWithPlayer(FrameEntry otherEntry)
            {
                FrameEntry? playerEntry = Frame.GetFrameEntry(Definitions.player);
                if (playerEntry != null)
                {
                    if (playerEntry.Y == otherEntry.Y)
                    {
                        for (int x = playerEntry.X; x < playerEntry.X + playerEntry.Icon.Length; x++)
                        {
                            if (x == otherEntry.X)
                            {
                                return true;
                            }
                        }
                    }
                }
                return false;
            }

            public static bool ColidesWithBorders(FrameEntry currentEntry)
            {
                if (currentEntry.Y == 1 || currentEntry.Y == Settings.System.Height - 1 || currentEntry.X == 0 || currentEntry.X + currentEntry.Icon.Length == Settings.System.Width)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public static class Update
        {
            public static void UpdateScore()
            {
                FrameEntry? scoreEntry = Frame.GetFrameEntry(Definitions.gameScoreEntry);
                if (scoreEntry != null)
                {
                    scoreEntry.Icon = Definitions.gameScore.ToString() + (Definitions.gameScore == 1 ? " point" : " points");
                }
            }

            public static void CheckGameState()
            {
                switch (currentGameState)
                {
                    case "running":
                        currentFrame = backgroundFrames["default"];
                        Definitions.isPaused = false;
                        Definitions.isRunning = true;
                        break;

                    case "menu":
                        currentFrame = backgroundFrames["start"];
                        Definitions.isPaused = true;
                        Definitions.isRunning = true;
                        break;

                    case "paused":
                        currentFrame = backgroundFrames["pause"];
                        Definitions.isPaused = true;
                        Definitions.isRunning = true;
                        break;

                    case "exiting":
                        Definitions.isPaused = false;
                        Definitions.isRunning = false;
                        ExitGame();
                        break;

                    default:
                        Definitions.isPaused = false;
                        Definitions.isRunning = true;
                        break;
                }
            }
        }

        public static class Input
        {
            public static void CheckInput()
            {
                ConsoleKeyInfo keyEntry;
                FrameEntry? playerEntry = Frame.GetFrameEntry(Definitions.player);

                if (playerEntry != null)
                {
                    if (Console.KeyAvailable)
                    {
                        keyEntry = Console.ReadKey();
                        switch (keyEntry.Key)
                        {
                            case ConsoleKey.RightArrow:
                                if (!Definitions.isPaused)
                                    MoveRight(playerEntry);
                                break;
                            case ConsoleKey.LeftArrow:
                                if (!Definitions.isPaused)
                                    MoveLeft(playerEntry);
                                break;
                            case ConsoleKey.UpArrow:
                                if (!Definitions.isPaused)
                                    MoveUp(playerEntry);
                                break;
                            case ConsoleKey.DownArrow:
                                if (!Definitions.isPaused)
                                    MoveDown(playerEntry);
                                break;
                            case ConsoleKey.Escape:
                                Menu.Clear();
                                if (currentGameState == "paused")
                                    currentGameState = "running";
                                else
                                {
                                    currentFrame = backgroundFrames["pause"];
                                    currentGameState = "paused";
                                }
                                break;

                            default:
                                break;
                        }
                    }
                }
            }

            public static void MenuSelect()
            {
                ConsoleKeyInfo keyEntry = Console.ReadKey();
                switch (keyEntry.Key)
                {
                    case ConsoleKey.DownArrow:
                        MenuDown();
                        break;

                    case ConsoleKey.UpArrow:
                        MenuUp();
                        break;

                    case ConsoleKey.Enter:
                        MenuConfirm();
                        Menu.Clear();
                        break;

                    case ConsoleKey.Escape:
                        if (currentGameState != "menu")
                        {
                            Menu.Clear();
                            if (currentGameState == "paused")
                                currentGameState = "running";
                            else
                                currentGameState = "paused";
                        }
                        break;
                }
            }

            static void MenuDown()
            {
                foreach (MenuItem item in menuItems)
                {
                    if (item.CurrentSelection)
                    {
                        if (menuItems.IndexOf(item) < menuItems.Count - 1)
                        {
                            item.Toggle();
                            menuItems[menuItems.IndexOf(item) + 1].Toggle();
                            break;
                        }
                    }
                }
            }

            static void MenuUp()
            {
                foreach (MenuItem item in menuItems)
                {
                    if (item.CurrentSelection)
                    {
                        if (menuItems.IndexOf(item) >= 1)
                        {
                            item.Toggle();
                            menuItems[menuItems.IndexOf(item) - 1].Toggle();
                            break;
                        }
                    }
                }
            }

            static void MenuConfirm()
            {
                foreach (MenuItem item in menuItems)
                {
                    if (item.CurrentSelection)
                    {
                        switch (item.Name)
                        {
                            case "play":
                                currentGameState = "running";
                                break;

                            case "settings":
                                break;

                            case "resume":
                                currentGameState = "running";
                                break;

                            case "mainmenu":
                                currentGameState = "menu";
                                currentFrame = backgroundFrames["start"];
                                break;

                            case "exit":
                                ExitGame();
                                currentGameState = "exiting";
                                break;
                        }
                    }
                }
                // Menu.Clear();
            }

            static void MoveRight(FrameEntry entry)
            {
                entry.X++;
                if (Collision.ColidesWithBorders(entry))
                    entry.X--;
            }

            static void MoveLeft(FrameEntry entry)
            {
                entry.X--;
                if (Collision.ColidesWithBorders(entry))
                    entry.X++;
            }

            static void MoveUp(FrameEntry entry)
            {
                entry.Y--;
                if (Collision.ColidesWithBorders(entry))
                    entry.Y++;
            }

            static void MoveDown(FrameEntry entry)
            {
                entry.Y++;
                if (Collision.ColidesWithBorders(entry))
                    entry.Y--;
            }
        }

        public static class Menu
        {
            public static void AddMenuItem(string item, int y, int x, bool current = false)
            {
                MenuItem menuItem = new MenuItem(item, x, y, current);
                foreach (MenuItem existingItem in menuItems)
                {
                    if (existingItem.Name == item)
                    {
                        return;
                    }
                }
                menuItems.Add(menuItem);
            }

            public static void Clear()
            {
                menuItems.Clear();
            }

            public static void MenuSelection()
            {

                if (menuItems.Count == 0)
                {
                    return;
                }
                else
                {
                    for (int entries = 0; entries < menuItems.Count; entries++)
                    {
                        FrameEntry currentEntry = menuItems[entries];
                        string itemIcon;
                        if (currentEntry.Icon != null)
                        {
                            itemIcon = currentEntry.Icon;
                        }
                        else
                        {
                            itemIcon = " ";
                        }
                        for (int entryChar = 0; entryChar < itemIcon.Length; entryChar++)
                            frameCoords[currentEntry.Y, currentEntry.X + entryChar] = itemIcon[entryChar];
                    }
                }
            }
        }

        public class MenuItem : FrameEntry
        {
            bool current = false;
            public bool CurrentSelection
            {
                get { return current; }
                set
                {
                    current = value;
                    this.Icon = current ? "X" : " ";
                }
            }

            public void Toggle()
            {
                current = !current;
                if (current)
                {
                    this.Icon = "X";
                }
                else
                {
                    this.Icon = " ";
                }
            }

            public MenuItem(string name, int xPos, int yPos, bool current = false)
            {
                this.Name = name;
                this.X = xPos;
                this.Y = yPos;
                this.CurrentSelection = current;

                if (current)
                {
                    this.Icon = "X";
                }
                else
                {
                    this.Icon = " ";
                }
            }
        }

        public class FrameEntry
        {
            public string Name { get; set; } = "";
            public int X { get; set; } = 0;
            public int Y { get; set; } = 0;
            private bool isNumber = false;
            public bool IsNumber
            {
                get { return isNumber; }
                set { isNumber = value; }
            }
            private string icon = "";
            public string Icon { get; set; }

            public bool IsAlive { get; set; } = true;
            private int number;

            public int Number
            {
                get { return number; }
                set
                {
                    if (isNumber)
                    {
                        icon = value.ToString();
                    }
                    else
                    {
                        number = value;
                    }
                }
            }

            public FrameEntry(string name = "", int xPos = 0, int yPos = 0, string icon = "", bool isAlive = true, int number = 0, bool isNumber = false)
            {
                this.Name = name;
                this.X = xPos;
                this.Y = yPos;
                this.IsAlive = isAlive;
                if (isNumber)
                {
                    this.Icon = number.ToString();
                }
                else
                {
                    this.Icon = icon;
                }
                this.Number = number;
                this.IsNumber = isNumber;
            }
        }
    }
}