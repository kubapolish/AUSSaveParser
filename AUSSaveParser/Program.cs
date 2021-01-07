using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Specialized;
using System.Reflection;
using System.Globalization;
using static AUS.Exts;

namespace AUS
{
    using System.Collections;
#if DEBUG
    using System.Diagnostics;

    internal class Program
    {
        private static void Main()
        {
            var sd = SaveData.LoadFromFile("UntitledSave1");
            Console.WriteLine(sd.ToString());
            Console.ReadLine();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "e")]
        static void CollectInfo(string path)
        {
            Console.WriteLine("hello");
            //var fsw = new FileSystemWatcher(Path.GetDirectoryName(path), Path.GetFileName(path));
            //fsw.NotifyFilter = NotifyFilters.LastWrite;
            //fsw.Error += (s, e) =>
            //{
            //    Console.WriteLine(e.GetException()?.ToString() ?? "exception is null mkay");
            //    fsw.EnableRaisingEvents = true;
            //};
            SaveData fold, fnew;
            fold = SaveData.LoadFromFile(path);
            while (true)
            {//i dont know how, or why, but EVERY single time out of 30+ tries
             //after saving in skytown for second time or later the filesystemwatcher would stop responding completely
             //not a coincidence either, tried doing the same thing in other areas and it didn't break immediately
             //what to heck

                //fsw.WaitForChanged(WatcherChangeTypes.Changed);
                //Console.WriteLine("file changed!");
                //var fs = WaitForFile(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                //if (fs == null) throw new Exception("lol fuck");
                //var sr = new StreamReader(fs);
                //var txt = sr.ReadToEnd();
                //fs.Dispose();
                //sr.Dispose();

                Console.ReadKey(true);
                var txt = File.ReadAllText(path);
                fnew = SaveData.LoadFromString(txt);

                //Console.WriteLine($"save coords: stage:{fnew.StageX}x{fnew.StageY}, birb:{fnew.X}x{fnew.Y}");
                void forpair<T1, T2>(T1[] left, T2[] right, Action<T1, T2, int> act)
                {
                    for (int i = 0; i < left.Length; i++)
                    {
                        act(left[i], right[i], i);
                    }
                }
                forpair(fold.Buys, fnew.Buys, (old, @new, i) => { if (old != @new) Console.WriteLine($"Buy {i + 1}, old:{old},new:{@new}"); });
                forpair(fold.Secrets, fnew.Secrets, (old, @new, i) => { if (old != @new) Console.WriteLine($"Secret {i + 1}, old:{old}, new:{@new}"); });
                if (fold.JumpHeight != fnew.JumpHeight) Console.WriteLine($"old jh:{fold.JumpHeight}, new jh:{fnew.JumpHeight}");
                if (fold.DoubleJumpHeight != fnew.DoubleJumpHeight) Console.WriteLine($"old djh:{fold.DoubleJumpHeight}, new djh:{fnew.DoubleJumpHeight}");
                if (fold.MaxAir != fnew.MaxAir) Console.WriteLine($"old maxair:{fold.MaxAir}, new maxair:{fnew.MaxAir}");
                forpair(fold.Hearts, fnew.Hearts, (old, @new, i) => { if (old != @new) Console.WriteLine($"Heart {i + 1}, old:{old}, new:{@new}"); });

                fold = fnew;
                Console.WriteLine();
            }
        }
        //static FileStream WaitForFile(string fullPath, FileMode mode, FileAccess access, FileShare share)
        //{
        //    for (int numTries = 0; numTries < 20; numTries++)
        //    {
        //        try
        //        {
        //            return new FileStream(fullPath, mode, access, share);
        //        }
        //        catch (IOException)
        //        {
        //            Task.Delay(69).Wait();
        //        }
        //    }
        //    return null;
        //}
    }
#endif
    /// <summary>
    /// An Untitled Story save data.
    /// </summary>
    public class SaveData
    {
        /// <summary>
        /// Represents a single digit in range from 0 to 9.
        /// </summary>
        public struct Digit
        {
            /// <summary>
            /// Initializes a new <see cref="Digit"/> with the value.
            /// </summary>
            /// <param name="v">A number which represents a Digit.</param>
            public Digit(byte v) => this.v = EnsureRange(v);
            private const string oormsg = "The save will physically break if this is outside the range of 0 and 9.";
            private static byte EnsureRange(int v) => v >= 0 && v < 10 ? (byte)v : throw new ArgumentOutOfRangeException(oormsg);
            private static byte EnsureRange(byte v) => v < 10 ? v
                : throw new ArgumentOutOfRangeException(oormsg);
            private byte v;
            /// <summary>
            /// The value of the digit.
            /// </summary>
            public byte Value { get => v; set => v = EnsureRange(value); }
            /// <summary>
            /// Converts a Digit into a byte.
            /// </summary>
            /// <param name="v">The digit.</param>
            public static implicit operator byte(Digit v) => v.v;//^.^
            /// <summary>
            /// Converts a Digit into an int.
            /// </summary>
            /// <param name="v">The digit.</param>
            public static implicit operator int(Digit v) => v.v;
            /// <summary>
            /// Converts a byte into a Digit.
            /// </summary>
            /// <param name="v">The byte value.</param>
            public static implicit operator Digit(byte v) => new Digit(v);
            /// <summary>
            /// Converts an int into a Digit.
            /// </summary>
            /// <param name="v">The int value.</param>
            public static implicit operator Digit(int v) => new Digit() { v = EnsureRange(v) };
            /// <summary>
            /// Checks for equality between digits.
            /// </summary>
            /// <param name="left">The left value.</param>
            /// <param name="right">The right value.</param>
            /// <returns>A bool whether the digits are equal.</returns>
            public static bool operator ==(Digit left, Digit right) => left.Equals(right);
            /// <summary>
            /// Checks for inequality between digits.
            /// </summary>
            /// <param name="left">The left value.</param>
            /// <param name="right">The right value.</param>
            /// <returns>A bool whether the digits are inequal.</returns>
            public static bool operator !=(Digit left, Digit right) => !left.Equals(right);
            /// <summary>
            /// Converts the digit into a string.
            /// </summary>
            /// <returns>The digit as a string.</returns>
            public override string ToString() => v.ToString(ic);
            /// <summary>
            /// Checks for equality.
            /// </summary>
            /// <param name="obj">The other object.</param>
            /// <returns>A bool whether the objects are equal.</returns>
            public override bool Equals(object obj) => v.Equals(obj);
            /// <summary>
            /// Gets hash code of the digit.
            /// </summary>
            /// <returns>The hash code.</returns>
            public override int GetHashCode() => v.GetHashCode();
        }
        /// <summary>
        /// Whether the file's checksum is valid.
        /// </summary>
        public bool IsChecksumValid { get; protected set; } = true;
        #region Values
        /// <summary>
        /// The file's checksum.
        /// </summary>
        public double Checksum { get; protected set; }
        /// <summary>
        /// Obtained abilities.
        /// </summary>
        public bool[] Abilities { get; protected set; } = new bool[24];
        /// <summary>
        /// Obtained hearts.
        /// </summary>
        public bool[] Hearts { get; protected set; } = new bool[95];
        /// <summary>
        /// Obtained gold orbs.
        /// </summary>
        public bool[] Gold { get; protected set; } = new bool[10];
        /// <summary>
        /// State of secrets.
        /// </summary>
        public Digit[] Secrets { get; protected set; } = new Digit[95];
        /// <summary>
        /// Bought SkyTown furnitures.
        /// </summary>
        public bool[] Buys { get; protected set; } = new bool[24];
        /// <summary>
        /// Defeated bosses.
        /// </summary>
        public bool[] Bosses { get; protected set; } = new bool[17];
        /// <summary>
        /// Whether the music is muted.
        /// </summary>
        public bool Mute { get; set; } = false;
        /// <summary>
        /// Whether the game is fullscreen.
        /// </summary>
        public bool FullScreen { get; set; } = false;
        /// <summary>
        /// Whether the player dies in one hit, like in Insanity difficulty.
        /// </summary>
        public bool OneHit { get; set; } = false;
        /// <summary>
        /// Last opened menu tab index.
        /// </summary>
        public Digit PauseM { get; set; } = 0;
        /// <summary>
        /// The game's difficulty.
        /// </summary>
        public Digit Difficulty { get; set; } = 1;
        /// <summary>
        /// The map size.
        /// </summary>
        protected const int _mapW = 30, _mapH = 25;
        /// <summary>
        /// The map data.
        /// </summary>
        public int[,] Map { get; protected set; } = new int[_mapW, _mapH];
        /// <summary>
        /// Map save flags.
        /// </summary>
        public bool[,] MapSave { get; protected set; } = new bool[_mapW, _mapH];
        /// <summary>
        /// Map right exit flags.
        /// </summary>
        public bool[,] MapExitRight { get; protected set; } = new bool[_mapW, _mapH];
        /// <summary>
        /// Map exit down flags.
        /// </summary>
        public bool[,] MapExitDown { get; protected set; } = new bool[_mapW, _mapH];
        /// <summary>
        /// AstroCrash minigame scores.
        /// </summary>
        public double[] AstroCrashScores { get; protected set; } = new double[5] { 17500, 12000, 8000, 4000, 2000 };
        /// <summary>
        /// JumpBox minigame scores.
        /// </summary>
        public double[] JumpBoxScores { get; protected set; } = new double[5] { 7500, 7000, 6000, 4000, 2000 };
        /// <summary>
        /// KeepGoing minigame scores.
        /// </summary>
        public double[] KeepGoingScores { get; protected set; } = new double[5] { 300, 200, 100, 60, 20 };
        /// <summary>
        /// Total amount of abilities.
        /// </summary>
        public double AbilitiesTotal { get; set; } = 0;
        /// <summary>
        /// Total amount of hearts.
        /// </summary>
        public double HeartsTotal { get; set; } = 0;
        /// <summary>
        /// Total amount of gold orbs.
        /// </summary>
        public double GoldTotal { get; set; } = 0;
        /// <summary>
        /// Total amount of flowers.
        /// </summary>
        public double FlowersTotal { get; set; } = 0;
        /// <summary>
        /// Max health.
        /// </summary>
        public double MaxHealth { get; set; } = 100;
        /// <summary>
        /// Max air.
        /// </summary>
        public double MaxAir { get; set; } = 100;
        /// <summary>
        /// Jump height.
        /// </summary>
        public double JumpHeight { get; set; } = 5;
        /// <summary>
        /// Doublejump height.
        /// </summary>
        public double DoubleJumpHeight { get; set; } = 0;//4 for first
        /// <summary>
        /// Amount of crystals.
        /// </summary>
        public double Crystals { get; set; } = 0;
        /// <summary>
        /// Time played in seconds.
        /// </summary>
        public double TimePlayed { get; set; } = 0;
        /// <summary>
        /// Times died.
        /// </summary>
        public double Deaths { get; set; } = 0;
        /// <summary>
        /// Times saved.
        /// </summary>
        public double Saves { get; set; } = 0;
        /// <summary>
        /// Amount of taken damage.
        /// </summary>
        public double Damage { get; set; } = 0;
        /// <summary>
        /// Stage X coordinate.
        /// </summary>
        public double StageX { get; set; } = 10;
        /// <summary>
        /// Stage Y coordinate.
        /// </summary>
        public double StageY { get; set; } = 10;
        /// <summary>
        /// X coordinate of player on stage.
        /// </summary>
        public double X { get; set; } = 255;
        /// <summary>
        /// Y coordinate of player on stage.
        /// </summary>
        public double Y { get; set; } = 270;
        /// <summary>
        /// Save file slot. This is important.
        /// </summary>
        public double SaveSlot { get; set; } = 0;
        /// <summary>
        /// Save name.
        /// </summary>
        public string Name { get; set; } = "zyhrllos";
        /// <summary>
        /// Keyboard layout type.
        /// </summary>
        public double kb { get; set; } = 0;
        /// <summary>
        /// Keyboard jump button id.
        /// </summary>
        public double kb_jump { get; set; } = 38;
        /// <summary>
        /// Keyboard shoot button id.
        /// </summary>
        public double kb_shoot { get; set; } = 90;
        /// <summary>
        /// Keyboard action button id.
        /// </summary>
        public double kb_action { get; set; } = 88;
        /// <summary>
        /// Keyboard pause button id.
        /// </summary>
        public double kb_pause { get; set; } = 32;
        /// <summary>
        /// Gamepad layout type.
        /// </summary>
        public double gp { get; set; } = 0;
        /// <summary>
        /// Gamepad jump button id.
        /// </summary>
        public double gp_jump { get; set; } = 2;
        /// <summary>
        /// Gamepad shoot button id.
        /// </summary>
        public double gp_shoot { get; set; } = 1;
        /// <summary>
        /// Gamepad action button id.
        /// </summary>
        public double gp_action { get; set; } = 3;
        /// <summary>
        /// Gamepad pause button id.
        /// </summary>
        public double gp_pause { get; set; } = 10;
        #endregion

        #region stuff
        /// <summary>
        /// Converts map data into ascii art.
        /// </summary>
        /// <param name="consoleFriendly">Whether to use characters friendly for windows console.</param>
        /// <returns>Map data in ascii art.</returns>
        public string DumpMap(bool consoleFriendly = false)
        {//finding proper characters for the ends was hard and it still doesnt satisfy me
            char re = (consoleFriendly ? '~' : '╶'), de = (consoleFriendly ? ',' : '╷'),
                 le = (consoleFriendly ? '¬' : '╴'), ue = (consoleFriendly ? 'ˈ' : '╵');
            const char rd = '┌', ld = '┐', ur = '└', ul = '┘',
                       ud = '│', lr = '─', lurd = '┼',
                       urd = '├', lrd = '┬', uld = '┤', ulr = '┴';
            const char e = ' ';
            StringBuilder sb = new StringBuilder();
            for (int y = 0; y < _mapH; y++)
            {
                for (int x = 0; x < _mapW; x++)
                {// ┘ ┐ ┌ └ ┤ ┴ ┬ ├ ─ │ ┼
                    if (MapSave[x, y])
                    {
                        sb.Append('S');
                        continue;
                    }

                    if (Map[x, y] == 0 || Map[x, y] == -16777216)
                    {
                        sb.Append(e);
                        continue;
                    }

                    if (x == 0)
                        if (y == 0)
                            sb.Append(MapExitRight[x, y] && MapExitDown[x, y] ? rd : MapExitRight[x, y] ? re : MapExitDown[x, y] ? de : e);
                        else
                            sb.Append(MapExitDown[x, y - 1]
                                 ? MapExitRight[x, y] && MapExitDown[x, y] ? urd : MapExitRight[x, y] ? ur : MapExitDown[x, y] ? ud : ue
                                 : MapExitRight[x, y] && MapExitDown[x, y] ? rd : MapExitRight[x, y] ? re : MapExitDown[x, y] ? de : e);
                    else
                    if (y == 0)
                        sb.Append(MapExitRight[x - 1, y]
                             ? MapExitRight[x, y] && MapExitDown[x, y] ? lrd : MapExitRight[x, y] ? lr : MapExitDown[x, y] ? ld : le
                             : MapExitRight[x, y] && MapExitDown[x, y] ? rd : MapExitRight[x, y] ? re : MapExitDown[x, y] ? de : e);
                    else
                        sb.Append(MapExitRight[x - 1, y] && MapExitDown[x, y - 1]
                             ? MapExitRight[x, y] && MapExitDown[x, y] ? lurd : MapExitRight[x, y] ? ulr : MapExitDown[x, y] ? uld : ul
                             : MapExitRight[x - 1, y]
                                 ? MapExitRight[x, y] && MapExitDown[x, y] ? lrd : MapExitRight[x, y] ? lr : MapExitDown[x, y] ? ld : le
                                 : MapExitDown[x, y - 1]
                                     ? MapExitRight[x, y] && MapExitDown[x, y] ? urd : MapExitRight[x, y] ? ur : MapExitDown[x, y] ? ud : ue
                                     : MapExitRight[x, y] && MapExitDown[x, y] ? rd : MapExitRight[x, y] ? re : MapExitDown[x, y] ? de : e);
                }
                if (y != _mapH - 1) sb.AppendLine();
            }
            return sb.ToString();
        }
        /// <summary>
        /// Converts save data into text, also dumping map in console friendly mode.
        /// </summary>
        /// <returns>Text containing information about the save.</returns>
        public override string ToString() => ToString(true, true);
        /// <summary>
        /// Converts save data into text.
        /// </summary>
        /// <param name="dumpMap">Whether to dump the map.</param>
        /// <param name="consoleFriendly">If map is being dumped, whether to use characters friendly for windows console.</param>
        /// <returns>Text containing information about the save.</returns>
        public string ToString(bool dumpMap = true, bool consoleFriendly = true)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"File has {(IsChecksumValid ? "valid" : "invalid")} checksum.");
            foreach (var item in typeof(SaveData).GetProperties())
            {
                if (item.Name == nameof(IsChecksumValid)) continue;
                var thisval = item.GetValue(this);
                if (item.PropertyType.IsArray)
                {
                    var array = thisval as Array;
                    if (array.Rank > 1) continue;//dont want to dump map (yet)
                    sb.Append($"{item.Name}: [");
                    var length = array.Length;
                    for (int i = 0; i < length; i++)
                    {
                        sb.Append(array.GetValue(i).ToString() + ", ");
                    }
                    sb.Length -= 2;
                    sb.AppendLine($"]");
                }
                else sb.AppendLine($"{item.Name}: {thisval}");
            }
            if (dumpMap)
            {
                sb.AppendLine();
                sb.AppendLine("Map:");
                sb.Append(DumpMap(consoleFriendly));
            }
            return sb.ToString();
        }
        #endregion

        /// <summary>
        /// List of random file names.
        /// </summary>
        public readonly static System.Collections.ObjectModel.ReadOnlyCollection<string> RandomNames = Array.AsReadOnly(new[]
        {
            "Rook", "Pawn", "King", "Bishop", "GostBot", "Edify",
            "Woe", "Happy", "YMM", "Matt", "Kurstyn", "MJK",
            "Queen", "Horse", "Check", "Mart", "Unnamed", "Coolio",
            "Languish", "Jumper", "Hollow", "Rather", "Temper",
            "Random", "Douglas", "Nurse", "Wisdom", "Pisces", "Liquid",
            "Adams", "Lateral", "Million", "Gabe", "Axis", "Return",
            "Current", "Ritual", "Red", "Blue", "Black", "Depth",
            "Gnarly", "KMoney", "TheHammer", "Needles", "TheButcher",
        });
        /// <summary>
        /// Initializes a new instance of <see cref="SaveData"/>.
        /// </summary>
        public SaveData() { Name = RandomNames[new Random().Next(0, RandomNames.Count)]; }
        private string loadedFilePath;
        /// <summary>
        /// Loads save data from a file, given the path.
        /// </summary>
        /// <param name="path">Path to the file.</param>
        /// <returns>Save data of the file.</returns>
        public static SaveData LoadFromFile(string path)
        {
            var sf = LoadFromString(File.ReadAllText(path));
            sf.loadedFilePath = path;
            return sf;
        }
        /// <summary>
        /// Loads save data from a string.
        /// </summary>
        /// <param name="savdat">String containing save data.</param>
        /// <returns>Save data of the string.</returns>
        public static SaveData LoadFromString(string savdat)
        {
            SaveData save = new SaveData();
            //var a, b, c, d, i, j, k, checksum, ver, chk;
            var file = savdat.Replace("\r", "").Split('\n');
            //string a;
            int i = 0; i += 2;

            //init vars
            int k, j;
            double chk, checksum;
            int ver;

            // ---------------- v1.00
            //Read abilities
            for (int b = 0; b < 23; b++)
                save.Abilities[b] = file[i][b] != '0';
            i++;

            //Read Hearts
            for (int b = 0; b < 90; b++)
                save.Hearts[b] = file[i][b] != '0';
            i++;

            //Read Gold
            for (int b = 0; b < 10; b++)
                save.Gold[b] = file[i][b] != '0';
            i++;

            //Read secrets
            for (int b = 0; b < 90; b++)
                save.Secrets[b] = (byte)(file[i][b] - '0');
            i++;

            //Read Buys
            for (int b = 0; b < 24; b++)
                save.Buys[b] = file[i][b] != '0';
            i++;

            //Read Bosses
            for (int b = 0; b < 17; b++)
                save.Bosses[b] = file[i][b] != '0';
            i++;

            //Read options
            {
                string a = file[i];
                save.Mute = a[0] != '0';
                save.FullScreen = a[1] != '0';
                save.PauseM = (byte)(a[2] - '0');
                save.Difficulty = (byte)(a[3] - '0');
                save.OneHit = a[4] != '0';
                i++;
            }

            //Read map stuff
            for (j = 0; j < 30; j++)
                for (k = 0; k < 25; k++)
                {
                    save.Map[j, k] = int.Parse(file[i]);
                    i++;
                    save.MapSave[j, k] = (file[i][0] != '0');
                    save.MapExitRight[j, k] = (file[i][1] != '0');
                    save.MapExitDown[j, k] = (file[i][2] != '0');
                    i++;
                }

            //Read mini1, mini5 and mini6 scores
            for (int b = 0; b < 5; b++)
            {
                save.AstroCrashScores[b] = double.Parse(file[i++], ic);
                save.JumpBoxScores[b] = double.Parse(file[i++], ic);
                save.KeepGoingScores[b] = double.Parse(file[i++], ic);
            }

            //Read totals
            save.GoldTotal = double.Parse(file[i++], ic);
            save.HeartsTotal = double.Parse(file[i++], ic);
            save.AbilitiesTotal = double.Parse(file[i++], ic);
            save.FlowersTotal = double.Parse(file[i++], ic);

            //Read jump heights and life
            //save.MaxHealth = int.Parse(file[i++]);
            save.MaxHealth = double.Parse(file[i++], ic);
            save.MaxAir = double.Parse(file[i++], ic);
            save.JumpHeight = double.Parse(file[i++], ic);
            save.DoubleJumpHeight = double.Parse(file[i++], ic);

            //Read the rest
            save.Crystals = double.Parse(file[i++], ic);
            save.TimePlayed = double.Parse(file[i++], ic);
            save.Deaths = double.Parse(file[i++], ic);
            save.Saves = double.Parse(file[i++], ic);
            save.Damage = double.Parse(file[i++], ic);
            save.StageX = double.Parse(file[i++], ic);
            save.StageY = double.Parse(file[i++], ic);
            save.X = /*j =*/   double.Parse(file[i++], ic);
            save.Y = /*k =*/   double.Parse(file[i++], ic);
            save.SaveSlot = double.Parse(file[i++], ic);
            save.Name = file[i++];

            //Read Controls
            save.gp = double.Parse(file[i++], ic);
            save.kb = double.Parse(file[i++], ic);
            save.kb_jump = double.Parse(file[i++], ic);
            save.kb_shoot = double.Parse(file[i++], ic);
            save.kb_action = double.Parse(file[i++], ic);
            save.kb_pause = double.Parse(file[i++], ic);
            save.gp_jump = double.Parse(file[i++], ic);
            save.gp_shoot = double.Parse(file[i++], ic);
            save.gp_action = double.Parse(file[i++], ic);
            save.gp_pause = double.Parse(file[i++], ic);
            //Read the checksums
            save.Checksum = chk = int.Parse(file[i++]);

            // ---------------- v1.03

            if (i == file.Length)
            {
                ver = 1;
                Console.WriteLine("v1.00 save file!");
                //abilities[24] = 0;
                //for (int b = 90; b < 95; b++)
                //    hearts[b] = false;
                //for (int b = 90; b < 95; b++)
                //    secrets[b] = false;
                //already set to 0 by default so..
            }
            else
            {
                ver = 2;

                save.Abilities[23] = int.Parse(file[i++]) != 0;

                for (int b = 90; b < 95; b++)
                    save.Hearts[b] = file[i][b - 90] != '0';
                //i++;

                for (int b = 90; b < 95; b++)
                    save.Secrets[b] = (byte)(file[i][b - 90] - '0');
                //i++;
            }

            // ---------------- done

            //file = null;

            //Check the checksums
            //checksum = calc_checksum(save, j, k, ver);
            checksum = calc_checksum(save, ver);

            if (checksum != chk)
            {
                Console.WriteLine("ERROR:  Save data corrupt or tampered with.");
                //Console.WriteLine($"calced: {checksum}, in file: {chk}");
                save.IsChecksumValid = false;
                //return null;
            }

            //Move blackCastle (so much work!)
            if (ver == 1)
                moveBlackCastle(ref save);

            //Change JumpBox highscores
            if (save.JumpBoxScores[1] == 10000)
                save.JumpBoxScores[1] = GetJumpBoxHighScore(save.Difficulty);

            //Done
            return save;
        }//Load()
        /// <summary>
        /// note: overwrites the file. make a backup yourself first.
        /// </summary>
        public virtual void SaveToFile()
        {
            if (loadedFilePath == null) throw new InvalidOperationException("You need to specify file path for savedata loaded from string.");
            SaveToFile(loadedFilePath);
        }
        /// <summary>
        /// note: overwrites the file. make a backup yourself first.
        /// </summary>
        public virtual void SaveToFile(string path) => File.WriteAllText(path, SaveToString());
        /// <summary>
        /// Converts current save data into a string.
        /// </summary>
        /// <returns>A string containing the save data.</returns>
        public virtual string SaveToString()
        {
            //var a,b,i,j,k,checksum;
            StringBuilder a = new StringBuilder();

            // ---------------- v1.00
            a.AppendLine("Untitled Story Save File");
            a.AppendLine("~~~ Saved by kubapolish's AUS Save Editor ~~~");

            //Write abilities
            for (int b = 0; b < 23; b++)
                a.Append(Abilities[b] ? '1' : '0');
            a.AppendLine();

            //Write hearts
            for (int b = 0; b < 90; b++)
                a.Append(Hearts[b] ? '1' : '0');
            a.AppendLine();

            //Write gold
            for (int b = 0; b < 10; b++)
                a.Append(Gold[b] ? '1' : '0');
            a.AppendLine();

            //Write secrets
            for (int b = 0; b < 90; b++)
                a.Append(Secrets[b]);
            a.AppendLine();

            //Write buys
            for (int b = 0; b < 24; b++)
                a.Append(Buys[b] ? '1' : '0');
            a.AppendLine();

            //Write bosses
            for (int b = 0; b < 17; b++)
                a.Append(Bosses[b] ? '1' : '0');
            a.AppendLine();

            //Write options
            a.Append((Mute ? "1" : "0") +
                     (FullScreen ? "1" : "0") +
                     (PauseM.ToString()) + (Difficulty.ToString()) +
                     (OneHit ? "1" : "0"));
            a.AppendLine();

            //Write map stuff
            for (int j = 0; j < 30; j++)
                for (int k = 0; k < 25; k++)
                {
                    a.AppendLine(Map[j, k].ToString(ic));
                    a.AppendLine((MapSave[j, k] ? "1" : "0") +
                                 (MapExitRight[j, k] ? "1" : "0") +
                                 (MapExitDown[j, k] ? "1" : "0"));
                }

            //Write mini1, mini5 and mini6 scores
            for (int b = 0; b < 5; b++)
            {
                a.AppendLine(AstroCrashScores[b].ToString(ic));
                a.AppendLine(JumpBoxScores[b].ToString(ic));
                a.AppendLine(KeepGoingScores[b].ToString(ic));
            }

            //Write the totals
            a.AppendLine(GoldTotal.ToString(ic));
            a.AppendLine(HeartsTotal.ToString(ic));
            a.AppendLine(AbilitiesTotal.ToString(ic));
            a.AppendLine(FlowersTotal.ToString(ic));

            //Write jump heights and life
            a.AppendLine(MaxHealth.ToString(ic));
            a.AppendLine(MaxAir.ToString(ic));
            a.AppendLine(JumpHeight.ToString(ic));
            a.AppendLine(DoubleJumpHeight.ToString(ic));

            //Write the rest
            a.AppendLine(Crystals.ToString(ic));
            a.AppendLine(TimePlayed.ToString(ic));
            a.AppendLine(Deaths.ToString(ic));
            a.AppendLine(Saves.ToString(ic));
            a.AppendLine(Damage.ToString(ic));
            a.AppendLine(StageX.ToString(ic));
            a.AppendLine(StageY.ToString(ic));
            a.AppendLine(X.ToString(ic));
            a.AppendLine(Y.ToString(ic));
            a.AppendLine(SaveSlot.ToString(ic));
            a.AppendLine(Name);

            //Write Controls
            a.AppendLine(gp.ToString(ic));
            a.AppendLine(kb.ToString(ic));
            a.AppendLine(kb_jump.ToString(ic));
            a.AppendLine(kb_shoot.ToString(ic));
            a.AppendLine(kb_action.ToString(ic));
            a.AppendLine(kb_pause.ToString(ic));
            a.AppendLine(gp_jump.ToString(ic));
            a.AppendLine(gp_shoot.ToString(ic));
            a.AppendLine(gp_action.ToString(ic));
            a.AppendLine(gp_pause.ToString(ic));

            var checksum = GetChecksum();

            //Write the checksums
            a.AppendLine(checksum.ToString(ic));

            // ---------------- v1.03

            //Write the 24th ability
            a.AppendLine(this.Abilities[23] ? "1" : "0");

            //Write the 5 new hearts
            for (int b = 90; b < 95; b++)
                a.Append(this.Hearts[b] ? '1' : '0');
            a.AppendLine();

            //Write the 5 new secrets
            for (int b = 90; b < 95; b++)
                a.Append(this.Secrets[b]);

            // ---------------- Done

            return a.ToString();
        }
        /// <summary>
        /// Calculates checksum for current save data.
        /// </summary>
        /// <returns>The checksum.</returns>
        public double GetChecksum() => calc_checksum(this, 2);
        /// <summary>
        /// Calculates the checksum.
        /// </summary>
        /// <param name="save">The save data.</param>
        /// <param name="ver">Version of the save file.</param>
        /// <returns>The checksum.</returns>
        protected static double calc_checksum(SaveData save, /*int j, int k,*/ int ver)
        {
            int b_max;
            double c = 0;

            //Abilities
            b_max = ver == 1 ? 23 : 24;
            for (int b = 0; b < b_max; b++)
                c += (save.Abilities[b] ? 1 : 0);

            //Hearts
            b_max = ver == 1 ? 90 : 95;
            for (int b = 0; b < b_max; b++)
                c += (save.Hearts[b] ? 1 : 0) * (b + 1);

            //Gold
            for (int b = 0; b < 10; b++)
                c += (save.Gold[b] ? 1 : 0) * (b + 1) * (b + 1);

            //Secrets
            //b_max = ver == 1 ? 90 : 95;//duplicate of previous assignment, can be removed
            for (int b = 0; b < b_max; b++)
                c += (save.Secrets[b]);

            //Buys
            for (int b = 0; b < 23; b++)
                c += (save.Buys[b] ? 1 : 0) * 3;

            //Bosses
            for (int b = 0; b < 17; b++)
                c += (save.Bosses[b] ? 1 : 0) * 6;

            //Minigame Scores
            for (int b = 0; b < 5; b++)
            {
                c += gm_div(save.AstroCrashScores[b], 19d);
                c += gm_div(save.JumpBoxScores[b], 13d);
                c += gm_div(save.KeepGoingScores[b], 16d);
            }

            //Difficulty
            c += ((save.Difficulty * 3) + (save.OneHit ? 1 : 0)) * 7;

            //Map
            for (int a = 0; a < SaveData._mapW; a++)
                for (int b = 0; b < SaveData._mapH; b++)
                {
                    c += ((save.Map[a, b] != 0) ? 2 : 0);
                    c += gm_mod(save.Map[a, b], 3);
                    c += (save.MapSave[a, b] ? 3 : 0);
                    c += gm_mod((save.MapSave[a, b] ? 4 : 0), 3);
                }

            //Totals
            c += (save.GoldTotal * 3);
            c += gm_mod(save.GoldTotal, 4);
            c += save.HeartsTotal;
            c += gm_div(save.HeartsTotal, 3d);
            c += (save.AbilitiesTotal * 2);
            c += gm_mod(save.AbilitiesTotal, 3);
            c += (save.FlowersTotal * 6);

            //Jump Heights + Life
            c += gm_div(((save.MaxHealth * 3) + 6), 2);
            c += gm_mod((save.MaxHealth * 11), 5);
            c += gm_div(((save.MaxAir * 4) + 7), 2);
            c += gm_mod((save.MaxAir * 13), 4);
            c += (save.JumpHeight * 11);
            c += gm_mod(((save.JumpHeight * 16) + 3), 4);
            c += (save.DoubleJumpHeight * 7);
            c += gm_div(((save.DoubleJumpHeight * 5) + 14), 2);

            //Other Stuff
            c += save.Crystals;
            c += save.TimePlayed;
            c += save.Deaths;
            c += save.Saves * 4;
            c += save.Damage * 2;
            c += save.StageX;
            c += save.StageY;
            //c += j;
            //c += k;
            c += save.X;
            c += save.Y;
            c += save.SaveSlot;

            c += 4;
            c = Math.Ceiling(c);

            c = gm_mod(c, 51) + gm_div(c, 22) + c;

            return c;
        }//calc_checksum
        /// <summary>
        /// Moves BlackCastle from old save files.
        /// </summary>
        /// <param name="save">The save file.</param>
        protected static void moveBlackCastle(ref SaveData save)
        {//wait, why did i even port this code? do v1.00 saves even exist anymore?
         //also completely untested because im too lazy to make a v1.00 save and i dont think this is ever going to be used anyway

            //Init
            int[,] mc = new int[5, 12];
            bool[,] ms = new bool[5, 12],
                   mr = new bool[5, 12],
                   md = new bool[5, 12];

            //Copy it over
            for (int i = 23; i < 27; i++)
                for (int j = 13; j < 24; j++)
                {
                    mc[i - 23, j - 13] = save.Map[i, j];
                    ms[i - 23, j - 13] = save.MapSave[i, j];
                    mr[i - 23, j - 13] = save.MapExitRight[i, j];
                    md[i - 23, j - 13] = save.MapExitDown[i, j];
                }

            //Clear the area
            for (int i = 23; i < 27; i++)
                for (int j = 13; j < 24; j++)
                {
                    mc[i, j] = 0;
                    ms[i, j] = false;
                    mr[i, j] = false;
                    md[i, j] = false;
                }

            for (int i = 0; i < 5; i++)
                for (int j = 0; j < 12; j++)
                {
                    save.Map[i + 25, j + 13] = mc[i, j];
                    save.MapSave[i + 25, j + 13] = ms[i, j];
                    save.MapExitRight[i + 25, j + 13] = mr[i, j];
                    save.MapExitDown[i + 25, j + 13] = md[i, j];
                }

            //mc = null;
            //ms = mr = md = null;

            if (save.StageX >= 24 && save.StageY >= 14)
                save.StageX += 2;
        }//moveBlackCastle()
        /// <summary>
        /// Gets JumpBox high score for a given difficulty.
        /// </summary>
        /// <param name="difficulty">Difficulty.</param>
        /// <returns>A high score for JumpBox.</returns>
        public static int GetJumpBoxHighScore(int difficulty) => 7500 + (1000 * difficulty);
    }
    /// <summary>
    /// An Untitled Story records data.
    /// </summary>
    public class RecordsData
    {
        /// <summary>
        /// Completion data.
        /// </summary>
        public class CompletionData
        {
            internal CompletionData(string num, string fn, DifficultyType diff) : this(double.Parse(num, ic), fn, diff) { }
            internal CompletionData(string num, string fn, string diff) : this(double.Parse(num, ic), fn, (DifficultyType)int.Parse(diff)) { }
            /// <summary>
            /// Initializes a new instance of <see cref="CompletionData"/>.
            /// </summary>
            public CompletionData() { }
            /// <summary>
            /// Initializes a new instance of <see cref="CompletionData"/>, with given number, filename and difficulty.
            /// </summary>
            /// <param name="number">A number.</param>
            /// <param name="filename">Filename.</param>
            /// <param name="difficulty">Difficulty.</param>
            public CompletionData(double number, string filename, DifficultyType difficulty)
            {
                this.Number = number;
                this.FileName = filename;
                this.Difficulty = difficulty;
            }
            /// <summary>
            /// Difficulty.
            /// </summary>
            public enum DifficultyType : byte
            {
                /// <summary>
                /// Simple difficulty.
                /// </summary>
                Simple = 0,
                /// <summary>
                /// Regular difficulty.
                /// </summary>
                Regular,
                /// <summary>
                /// Difficult difficulty.
                /// </summary>
                Difficult,
                /// <summary>
                /// Masterful difficulty.
                /// </summary>
                Masterful,
                /// <summary>
                /// Insanity difficulty.
                /// </summary>
                Insanity
            }
            /// <summary>
            /// depends on context. time is in seconds. -1 = unset
            /// </summary>
            public double Number { get; set; } = -1;
            private string _fileName = "";
            /// <summary>
            /// Filename.
            /// </summary>
            public string FileName { get => _fileName; set => _fileName = value ?? throw new ArgumentNullException("filename cannot be null."); }
            /// <summary>
            /// Difficulty.
            /// </summary>
            public DifficultyType Difficulty { get; set; }
            /// <summary>
            /// Converts current instance to a string representing it.
            /// </summary>
            /// <returns>A string that represents the current instance.</returns>
            public override string ToString() => $"{nameof(Number)}: {Number}" + Environment.NewLine +
                                                 $"{nameof(FileName)}: {FileName}" + Environment.NewLine +
                                                 $"{nameof(Difficulty)}: {Difficulty}";
        }

        /// <summary>
        /// Heist mode stages data.
        /// </summary>
        public class HeistModeStages
        {
            /// <summary>
            /// Initializes a new instance of <see cref="HeistModeStages"/>.
            /// </summary>
            public HeistModeStages() { }
            /// <summary>
            /// Unlocks all stages.
            /// </summary>
            public void UnlockAll() => SkyTown = FarFall = ColdKeep = FireCage = StoneCastle = DeepDive = CloudRun = SkySand = TheCurtain = DarkGrotto = BlackCastle = true;
            /// <summary>
            /// Locks all stages.
            /// </summary>
            public void LockAll() => SkyTown = FarFall = ColdKeep = FireCage = StoneCastle = DeepDive = CloudRun = SkySand = TheCurtain = DarkGrotto = BlackCastle = false;
            /// <summary>
            /// SkyTown unlock flag.
            /// </summary>
            public bool SkyTown { get; set; }
            /// <summary>
            /// FarFall unlock flag.
            /// </summary>
            public bool FarFall { get; set; }
            /// <summary>
            /// ColdKeep unlock flag.
            /// </summary>
            public bool ColdKeep { get; set; }
            /// <summary>
            /// FireCage unlock flag.
            /// </summary>
            public bool FireCage { get; set; }
            /// <summary>
            /// StoneCastle unlock flag.
            /// </summary>
            public bool StoneCastle { get; set; }
            /// <summary>
            /// DeepDive unlock flag.
            /// </summary>
            public bool DeepDive { get; set; }
            /// <summary>
            /// CloudRun unlock flag.
            /// </summary>
            public bool CloudRun { get; set; }
            /// <summary>
            /// SkySand unlock flag.
            /// </summary>
            public bool SkySand { get; set; }
            /// <summary>
            /// TheCurtain unlock flag.
            /// </summary>
            public bool TheCurtain { get; set; }
            /// <summary>
            /// DarkGrotto unlock flag.
            /// </summary>
            public bool DarkGrotto { get; set; }
            /// <summary>
            /// BlackCastle unlock flag.
            /// </summary>
            public bool BlackCastle { get; set; }
            /// <summary>
            /// Converts current instance to a string that shows the flags.
            /// </summary>
            /// <returns>A string containing status of flags.</returns>
            public override string ToString() => $"{nameof(SkyTown)}: {SkyTown}" + Environment.NewLine +
                                                 $"{nameof(FarFall)}: {FarFall}" + Environment.NewLine +
                                                 $"{nameof(ColdKeep)}: {ColdKeep}" + Environment.NewLine +
                                                 $"{nameof(FireCage)}: {FireCage}" + Environment.NewLine +
                                                 $"{nameof(StoneCastle)}: {StoneCastle}" + Environment.NewLine +
                                                 $"{nameof(DeepDive)}: {DeepDive}" + Environment.NewLine +
                                                 $"{nameof(CloudRun)}: {CloudRun}" + Environment.NewLine +
                                                 $"{nameof(SkySand)}: {SkySand}" + Environment.NewLine +
                                                 $"{nameof(TheCurtain)}: {TheCurtain}" + Environment.NewLine +
                                                 $"{nameof(DarkGrotto)}: {DarkGrotto}" + Environment.NewLine +
                                                 $"{nameof(BlackCastle)}: {BlackCastle}";
        }

        /// <summary>
        /// Whether the checksum is valid for this save file.
        /// </summary>
        public bool IsChecksumValid { get; internal set; } = true;
        #region variables
        /// <summary>
        /// The checksum.
        /// </summary>
        public int Checksum { get; internal set; }

        /// <summary>
        /// Total completions.
        /// </summary>
        public double TotalCompletions { get; set; }
        /// <summary>
        /// Total simple completions.
        /// </summary>
        public double TotalSimpleCompletions { get; set; }
        /// <summary>
        /// Total regular completions.
        /// </summary>
        public double TotalRegularCompletions { get; set; }
        /// <summary>
        /// Total difficult completions.
        /// </summary>
        public double TotalDifficultCompletions { get; set; }
        /// <summary>
        /// Total masterful completions.
        /// </summary>
        public double TotalMasterfulCompletions { get; set; }
        /// <summary>
        /// Total insanity completions.
        /// </summary>
        public double TotalInsanityCompletions { get; set; }

        private CompletionData _highestPercentCompletion = new CompletionData();
        private CompletionData _lowestPercentCompletion = new CompletionData();
        private CompletionData _highestDamageCompletion = new CompletionData();
        private CompletionData _lowestDamageCompletion = new CompletionData();
        private CompletionData _highestDeathsCompletion = new CompletionData();
        private CompletionData _lowestDeathsCompletion = new CompletionData();
        private CompletionData _highestSavesCompletion = new CompletionData();
        private CompletionData _lowestSavesCompletion = new CompletionData();

        private CompletionData _fastestCompletion = new CompletionData();
        private CompletionData _slowestCompletion = new CompletionData();
        private CompletionData _fastestSimpleCompletion = new CompletionData();
        private CompletionData _fastestRegularCompletion = new CompletionData();
        private CompletionData _fastestDifficultCompletion = new CompletionData();
        private CompletionData _fastestMasterfulCompletion = new CompletionData();
        private CompletionData _fastestInsanityCompletion = new CompletionData();

        private CompletionData _fastestHatch = new CompletionData();
        private CompletionData _slowestHatch = new CompletionData();
        private CompletionData _fastestFullCompletion = new CompletionData();
        private CompletionData _slowestFullCompletion = new CompletionData();

        private CompletionData _fastestSimpleFullCompletion = new CompletionData();
        private CompletionData _fastestRegularFullCompletion = new CompletionData();
        private CompletionData _fastestDifficultFullCompletion = new CompletionData();
        private CompletionData _fastestMasterfulFullCompletion = new CompletionData();
        private CompletionData _fastestInsanityFullCompletion = new CompletionData();

        #region mhm
#pragma warning disable CS1591 // I am NOT doing this
        public CompletionData HighestPercentCompletion { get => _highestPercentCompletion; set => _highestPercentCompletion = value ?? throw new ArgumentNullException(); }
        public CompletionData LowestPercentCompletion { get => _lowestPercentCompletion; set => _lowestPercentCompletion = value ?? throw new ArgumentNullException(); }
        public CompletionData HighestDamageCompletion { get => _highestDamageCompletion; set => _highestDamageCompletion = value ?? throw new ArgumentNullException(); }
        public CompletionData LowestDamageCompletion { get => _lowestDamageCompletion; set => _lowestDamageCompletion = value ?? throw new ArgumentNullException(); }
        public CompletionData HighestDeathsCompletion { get => _highestDeathsCompletion; set => _highestDeathsCompletion = value ?? throw new ArgumentNullException(); }
        public CompletionData LowestDeathsCompletion { get => _lowestDeathsCompletion; set => _lowestDeathsCompletion = value ?? throw new ArgumentNullException(); }
        public CompletionData HighestSavesCompletion { get => _highestSavesCompletion; set => _highestSavesCompletion = value ?? throw new ArgumentNullException(); }
        public CompletionData LowestSavesCompletion { get => _lowestSavesCompletion; set => _lowestSavesCompletion = value ?? throw new ArgumentNullException(); }

        public CompletionData FastestCompletion { get => _fastestCompletion; set => _fastestCompletion = value ?? throw new ArgumentNullException(); }
        public CompletionData SlowestCompletion { get => _slowestCompletion; set => _slowestCompletion = value ?? throw new ArgumentNullException(); }
        public CompletionData FastestSimpleCompletion { get => _fastestSimpleCompletion; set => _fastestSimpleCompletion = value ?? throw new ArgumentNullException(); }
        public CompletionData FastestRegularCompletion { get => _fastestRegularCompletion; set => _fastestRegularCompletion = value ?? throw new ArgumentNullException(); }
        public CompletionData FastestDifficultCompletion { get => _fastestDifficultCompletion; set => _fastestDifficultCompletion = value ?? throw new ArgumentNullException(); }
        public CompletionData FastestMasterfulCompletion { get => _fastestMasterfulCompletion; set => _fastestMasterfulCompletion = value ?? throw new ArgumentNullException(); }
        public CompletionData FastestInsanityCompletion { get => _fastestInsanityCompletion; set => _fastestInsanityCompletion = value ?? throw new ArgumentNullException(); }

        public CompletionData FastestHatch { get => _fastestHatch; set => _fastestHatch = value ?? throw new ArgumentNullException(); }
        public CompletionData SlowestHatch { get => _slowestHatch; set => _slowestHatch = value ?? throw new ArgumentNullException(); }
        public CompletionData FastestFullCompletion { get => _fastestFullCompletion; set => _fastestFullCompletion = value ?? throw new ArgumentNullException(); }
        public CompletionData SlowestFullCompletion { get => _slowestFullCompletion; set => _slowestFullCompletion = value ?? throw new ArgumentNullException(); }

        public CompletionData FastestSimpleFullCompletion { get => _fastestSimpleFullCompletion; set => _fastestSimpleFullCompletion = value ?? throw new ArgumentNullException(); }
        public CompletionData FastestRegularFullCompletion { get => _fastestRegularFullCompletion; set => _fastestRegularFullCompletion = value ?? throw new ArgumentNullException(); }
        public CompletionData FastestDifficultFullCompletion { get => _fastestDifficultFullCompletion; set => _fastestDifficultFullCompletion = value ?? throw new ArgumentNullException(); }
        public CompletionData FastestMasterfulFullCompletion { get => _fastestMasterfulFullCompletion; set => _fastestMasterfulFullCompletion = value ?? throw new ArgumentNullException(); }
        public CompletionData FastestInsanityFullCompletion { get => _fastestInsanityFullCompletion; set => _fastestInsanityFullCompletion = value ?? throw new ArgumentNullException(); }
        #endregion
        private HeistModeStages _stagesUnlocked = new HeistModeStages();
        public HeistModeStages StagesUnlocked { get => _stagesUnlocked; set => _stagesUnlocked = value ?? throw new ArgumentNullException(); }
        #endregion
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

        #region overrides
        /// <summary>
        /// Converts current instance to a string describing the data
        /// </summary>
        /// <returns>string</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"File has {(IsChecksumValid ? "valid" : "invalid")} checksum.");
            foreach (var item in typeof(RecordsData).GetProperties())
            {
                if (item.Name == nameof(IsChecksumValid)) continue;
                var thisval = item.GetValue(this);
                if (item.PropertyType.Equals(typeof(int)))
                    sb.AppendLine($"{item.Name}: {thisval}");
                else if (item.PropertyType.Equals(typeof(CompletionData)))
                    sb.AppendLine(item.Name + ": " + Environment.NewLine +
                        string.Join("\n", ((CompletionData)thisval).ToString().Split('\n').Select(x => "  " + x)));
                else if (item.PropertyType.Equals(typeof(HeistModeStages)))
                    sb.AppendLine(item.Name + ": " + Environment.NewLine +
                        string.Join("\n", ((HeistModeStages)thisval).ToString().Split('\n').Select(x => "  " + x)));
                else throw new InvalidOperationException();
            }

            return sb.ToString();
        }
        #endregion

        /// <summary>
        /// Initializes a new instance of <see cref="RecordsData"/>.
        /// </summary>
        public RecordsData() { }
        private string loadedFilePath;
        /// <summary>
        /// Loads data from a file given path.
        /// </summary>
        /// <param name="path">Path to a file.</param>
        /// <returns></returns>
        public static RecordsData LoadFromFile(string path)
        {
            var sf = LoadFromString(File.ReadAllText(path));
            sf.loadedFilePath = path;
            return sf;
        }
        /// <summary>
        /// Loads data from a string.
        /// </summary>
        /// <param name="savdat">String containing the data.</param>
        /// <returns></returns>
        public static RecordsData LoadFromString(string savdat)
        {
            var ini = new IniParser.Parser.IniDataParser().Parse(savdat);
            var data = new RecordsData
            {
                TotalCompletions = int.Parse(ini["1"]["1"]),
                TotalSimpleCompletions = int.Parse(ini["1"]["2"]),
                TotalRegularCompletions = int.Parse(ini["1"]["3"]),
                TotalDifficultCompletions = int.Parse(ini["1"]["4"]),
                TotalMasterfulCompletions = int.Parse(ini["1"]["5"]),
                TotalInsanityCompletions = int.Parse(ini["1"]["6"]),

                Checksum = int.Parse(ini["2"]["5"]),

                HighestPercentCompletion = new CompletionData(ini["2"]["11a"], ini["2"]["11b"], ini["2"]["11c"]),
                LowestPercentCompletion = new CompletionData(ini["2"]["12a"], ini["2"]["12b"], ini["2"]["12c"]),
                HighestDamageCompletion = new CompletionData(ini["2"]["21a"], ini["2"]["21b"], ini["2"]["21c"]),
                LowestDamageCompletion = new CompletionData(ini["2"]["22a"], ini["2"]["22b"], ini["2"]["22c"]),
                HighestDeathsCompletion = new CompletionData(ini["2"]["31a"], ini["2"]["31b"], ini["2"]["31c"]),
                LowestDeathsCompletion = new CompletionData(ini["2"]["32a"], ini["2"]["32b"], ini["2"]["32c"]),
                HighestSavesCompletion = new CompletionData(ini["2"]["41a"], ini["2"]["41b"], ini["2"]["41c"]),
                LowestSavesCompletion = new CompletionData(ini["2"]["42a"], ini["2"]["42b"], ini["2"]["42c"]),

                FastestCompletion = new CompletionData(ini["3"]["11a"], ini["3"]["11b"], ini["3"]["11c"]),
                SlowestCompletion = new CompletionData(ini["3"]["12a"], ini["3"]["12b"], ini["3"]["12c"]),
                FastestSimpleCompletion = new CompletionData(ini["3"]["2a"], ini["3"]["2b"], CompletionData.DifficultyType.Simple),
                FastestRegularCompletion = new CompletionData(ini["3"]["3a"], ini["3"]["3b"], CompletionData.DifficultyType.Regular),
                FastestDifficultCompletion = new CompletionData(ini["3"]["4a"], ini["3"]["4b"], CompletionData.DifficultyType.Difficult),
                FastestMasterfulCompletion = new CompletionData(ini["3"]["5a"], ini["3"]["5b"], CompletionData.DifficultyType.Masterful),
                FastestInsanityCompletion = new CompletionData(ini["3"]["6a"], ini["3"]["6b"], CompletionData.DifficultyType.Insanity),

                FastestHatch = new CompletionData(ini["4"]["11a"], ini["4"]["11b"], ini["4"]["11c"]),
                SlowestHatch = new CompletionData(ini["4"]["12a"], ini["4"]["12b"], ini["4"]["12c"]),
                FastestFullCompletion = new CompletionData(ini["4"]["21a"], ini["4"]["21b"], ini["4"]["21c"]),
                SlowestFullCompletion = new CompletionData(ini["4"]["22a"], ini["4"]["22b"], ini["4"]["22c"]),

                FastestSimpleFullCompletion = new CompletionData(ini["6"]["1a"], ini["6"]["1b"], CompletionData.DifficultyType.Simple),
                FastestRegularFullCompletion = new CompletionData(ini["6"]["2a"], ini["6"]["2b"], CompletionData.DifficultyType.Regular),
                FastestDifficultFullCompletion = new CompletionData(ini["6"]["3a"], ini["6"]["3b"], CompletionData.DifficultyType.Difficult),
                FastestMasterfulFullCompletion = new CompletionData(ini["6"]["4a"], ini["6"]["4b"], CompletionData.DifficultyType.Masterful),
                FastestInsanityFullCompletion = new CompletionData(ini["6"]["5a"], ini["6"]["5b"], CompletionData.DifficultyType.Insanity),

                StagesUnlocked = new HeistModeStages()
                {
                    SkyTown = ini["5"]["1"] != "0",
                    FarFall = ini["5"]["2"] != "0",
                    ColdKeep = ini["5"]["3"] != "0",
                    FireCage = ini["5"]["4"] != "0",
                    StoneCastle = ini["5"]["5"] != "0",
                    DeepDive = ini["5"]["6"] != "0",
                    CloudRun = ini["5"]["7"] != "0",
                    SkySand = ini["5"]["8"] != "0",
                    TheCurtain = ini["5"]["9"] != "0",
                    DarkGrotto = ini["5"]["10"] != "0",
                    BlackCastle = ini["5"]["11"] != "0"
                },
            };
            data.IsChecksumValid = data.Checksum == data.GetChecksum();
            return data;
        }
        /// <summary>
        /// Saves data back to the file, if it was loaded from a path.
        /// </summary>
        public void SaveToFile()
        {
            if (loadedFilePath == null) throw new InvalidOperationException("You need to specify file path for savedata loaded from string.");
            SaveToFile(loadedFilePath);
        }
        /// <summary>
        /// Saves data to a file given a path.
        /// </summary>
        /// <param name="path">Path to a file. It will be overwritten.</param>
        public void SaveToFile(string path) => File.WriteAllText(path, SaveToString());
        /// <summary>
        /// Gives a string containing the data.
        /// </summary>
        /// <returns></returns>
        public string SaveToString()
        {
            var ini = new IniParser.Model.IniData();
            ini.Configuration.AssigmentSpacer = "";

            ini["0"]["WARNING:"] = "  TAMPERING WITH THIS FILE WILL RESULT IN zyhrllos";
            ini["1"]["1"] = TotalCompletions.ToString(ic);
            ini["1"]["2"] = TotalSimpleCompletions.ToString(ic);
            ini["1"]["3"] = TotalRegularCompletions.ToString(ic);
            ini["1"]["4"] = TotalDifficultCompletions.ToString(ic);
            ini["1"]["5"] = TotalMasterfulCompletions.ToString(ic);
            ini["1"]["6"] = TotalInsanityCompletions.ToString(ic);

            ini["2"]["5"] = GetChecksum().ToString(ic);

            void set(int sect, int basekey, CompletionData cmpldat)
            {
                var bk = basekey.ToString();
                ini[sect.ToString()][bk + "a"] = cmpldat.Number.ToString(ic);
                ini[sect.ToString()][bk + "b"] = cmpldat.FileName;
                if (bk.Length > 1) ini[sect.ToString()][bk + "c"] = cmpldat.Difficulty.ToNumStr();
            }

            set(2, 11, HighestPercentCompletion);
            set(2, 12, LowestPercentCompletion);
            set(2, 21, HighestDamageCompletion);
            set(2, 22, LowestDamageCompletion);
            set(2, 31, HighestDeathsCompletion);
            set(2, 32, LowestDeathsCompletion);
            set(2, 41, HighestSavesCompletion);
            set(2, 42, LowestSavesCompletion);

            set(3, 11, FastestCompletion);
            set(3, 12, SlowestCompletion);
            set(3, 2, FastestSimpleCompletion);
            set(3, 3, FastestRegularCompletion);
            set(3, 4, FastestDifficultCompletion);
            set(3, 5, FastestMasterfulCompletion);
            set(3, 6, FastestInsanityCompletion);

            set(4, 11, FastestHatch);
            set(4, 12, SlowestHatch);
            set(4, 21, FastestFullCompletion);
            set(4, 22, SlowestFullCompletion);

            set(6, 1, FastestSimpleFullCompletion);
            set(6, 2, FastestRegularFullCompletion);
            set(6, 3, FastestDifficultFullCompletion);
            set(6, 4, FastestMasterfulFullCompletion);
            set(6, 5, FastestInsanityFullCompletion);

            ini["5"]["1"] = StagesUnlocked.SkyTown ? "1" : "0";
            ini["5"]["2"] = StagesUnlocked.FarFall ? "1" : "0";
            ini["5"]["3"] = StagesUnlocked.ColdKeep ? "1" : "0";
            ini["5"]["4"] = StagesUnlocked.FireCage ? "1" : "0";
            ini["5"]["5"] = StagesUnlocked.StoneCastle ? "1" : "0";
            ini["5"]["6"] = StagesUnlocked.DeepDive ? "1" : "0";
            ini["5"]["7"] = StagesUnlocked.CloudRun ? "1" : "0";
            ini["5"]["8"] = StagesUnlocked.SkySand ? "1" : "0";
            ini["5"]["9"] = StagesUnlocked.TheCurtain ? "1" : "0";
            ini["5"]["10"] = StagesUnlocked.DarkGrotto ? "1" : "0";
            ini["5"]["11"] = StagesUnlocked.BlackCastle ? "1" : "0";

            return ini.ToString();
        }
        /// <summary>
        /// Gets checksum for current data.
        /// </summary>
        /// <returns></returns>
        public double GetChecksum() => records_checksum(this);
        /// <summary>
        /// Gets checksum for data.
        /// </summary>
        /// <param name="data">data</param>
        /// <returns>checksum</returns>
        protected static double records_checksum(RecordsData data)
        {
            double a, b, c, d;

            a = 0;

            a += (double)data.TotalCompletions * 5;
            a += (double)data.TotalSimpleCompletions;
            a += (double)data.TotalRegularCompletions * 2;
            a += (double)data.TotalDifficultCompletions * 6;
            a += (double)data.TotalMasterfulCompletions * 4;
            a += (double)data.TotalInsanityCompletions;

            a += (double)data.HighestPercentCompletion.Number * 7;
            a += (double)data.HighestPercentCompletion.Difficulty * 5;
            a += (double)data.LowestPercentCompletion.Number * 3;
            a += (double)data.LowestPercentCompletion.Difficulty;
            a += (double)data.HighestDamageCompletion.Number * 6;
            a += (double)data.HighestDamageCompletion.Difficulty * 8;
            a += (double)data.LowestDamageCompletion.Number * 9;
            a += (double)data.LowestDamageCompletion.Difficulty * 2;
            a += (double)data.HighestDeathsCompletion.Number * 6;
            a += (double)data.HighestDeathsCompletion.Difficulty * 3;
            a += (double)data.LowestDeathsCompletion.Number;
            a += (double)data.LowestDeathsCompletion.Difficulty * 7;
            a += (double)data.HighestSavesCompletion.Number * 3;
            a += (double)data.HighestSavesCompletion.Difficulty * 13;
            a += (double)data.LowestSavesCompletion.Number * 7;
            a += (double)data.LowestSavesCompletion.Difficulty * 3;

            a += (double)data.FastestCompletion.Number * 10;
            a += (double)data.FastestCompletion.Difficulty * 15;
            a += (double)data.SlowestCompletion.Number * 2;
            a += (double)data.SlowestCompletion.Difficulty * 7;
            a += (double)data.FastestSimpleCompletion.Number * 3;
            a += (double)data.FastestRegularCompletion.Number;
            a += (double)data.FastestDifficultCompletion.Number * 6;
            a += (double)data.FastestMasterfulCompletion.Number * 4;
            a += (double)data.FastestInsanityCompletion.Number * 11;

            a += (double)data.FastestHatch.Number * 11;
            a += (double)data.FastestHatch.Difficulty * 14;
            a += (double)data.SlowestHatch.Number * 3;
            a += (double)data.SlowestHatch.Difficulty * 6;
            a += (double)data.FastestFullCompletion.Number * 9;
            a += (double)data.FastestFullCompletion.Difficulty * 16;
            a += (double)data.SlowestFullCompletion.Number;
            a += (double)data.SlowestFullCompletion.Difficulty * 8;

            a += (double)data.FastestSimpleFullCompletion.Number * 6;
            a += (double)data.FastestRegularFullCompletion.Number * 3;
            a += (double)data.FastestDifficultFullCompletion.Number * 4;
            a += (double)data.FastestMasterfulFullCompletion.Number * 4;
            a += (double)data.FastestInsanityFullCompletion.Number * 2;

            a += (data.StagesUnlocked.SkyTown ? 1d : 0d) * 11;
            a += (data.StagesUnlocked.FarFall ? 1d : 0d) * 12;
            a += (data.StagesUnlocked.ColdKeep ? 1d : 0d) * 3;
            a += (data.StagesUnlocked.FireCage ? 1d : 0d) * 18;
            a += (data.StagesUnlocked.StoneCastle ? 1d : 0d) * 7;
            a += (data.StagesUnlocked.DeepDive ? 1d : 0d) * 4;
            a += (data.StagesUnlocked.CloudRun ? 1d : 0d) * 17;
            a += (data.StagesUnlocked.SkySand ? 1d : 0d) * 9;
            a += (data.StagesUnlocked.TheCurtain ? 1d : 0d) * 6;
            a += (data.StagesUnlocked.DarkGrotto ? 1d : 0d) * 13;
            a += (data.StagesUnlocked.BlackCastle ? 1d : 0d) * 2;

            b = a;
            c = a;
            d = a;

            b += 7;
            b = gm_mod(b, 6);

            c -= 18;
            c *= 7;
            c = gm_div(c, 4);

            d *= 2;
            d += 3;

            a = a + b + c + d;

            return a;
        }
    }
    internal static class Exts
    {
        internal static string ToNumStr(this RecordsData.CompletionData.DifficultyType diff) => ((byte)diff).ToString();
        internal static double gm_div(double left, double right) => unchecked((int)Math.Truncate(left / right));
        //todo: this should be implemented to act just how gamemaker7 mod operator behaves for >uint.MaxValue (it's buggy)
        internal static double gm_mod(double left, double right) => left % right;//!
        internal static readonly CultureInfo ic = CultureInfo.InvariantCulture;
    }
}
