using AUS;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace SaveEditor
{
    public partial class Form1 : Form
    {//todo: make code less horrible, split stuff into files?
        public Form1()
        {
            InitializeComponent();
            tabPage2.AutoScroll = false;
            tabPage2.VerticalScroll.Enabled = false;
            tabPage2.VerticalScroll.Visible = false;
            tabPage2.VerticalScroll.Maximum = 0;
            tabPage2.AutoScroll = true;
        }

        string savefilepath;
        SaveData f;

        private void buttonLoad_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() != DialogResult.OK) return;

            //File.WriteAllText(".dialogpath", openFileDialog1.FileName);
            saveFileDialog1.FileName = openFileDialog1.FileName;
            timer1.Enabled = false;
            buttonLoad.Font = new Font(buttonLoad.Font, FontStyle.Regular);
            buttonLoad.ForeColor = Color.FromKnownColor(KnownColor.ControlText);

            savefilepath = openFileDialog1.FileName;
            loading = true;
            try
            {
                load(File.ReadAllText(savefilepath));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error loading file!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            loading = false;
            setmodified(false);
            currentfile = Path.GetFileName(savefilepath);
            Text = $"AUS Save File Editor ({currentfile})";
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() != DialogResult.OK) return;
            openFileDialog1.FileName = saveFileDialog1.FileName;
            var p = saveFileDialog1.FileName;
            var f = Path.GetFileName(p);
            bool bad = false;
            bool badname = false;
            if (f.ToLowerInvariant().StartsWith("untitledsave") && f.Length == "untitledsave".Length + 1 && char.IsDigit(f[f.Length - 1]))
            {
                if (checkBoxCorrectSaveSlot.Checked) numeric_other_saveslot.Value = f[f.Length - 1] - '0';
                if (numeric_other_saveslot.Value != f[f.Length - 1] - '0') bad = true;
            }
            else badname = true;
            if (bad) if (MessageBox.Show("SaveSlot doesn't match the filename!\n" +
                                         "This will make the game overwrite irrelevant save file!\n" +
                                         "Are you sure you want to continue?", "!!!WARNING!!!", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)
                                         != DialogResult.Yes) return;
                else { }
            else if (badname) if (MessageBox.Show("You aren't saving to UntitledSaveX file.\n" +
                                                  "Please remember about the SaveSlot bug.", "!!!WARNING!!!",
                                                  MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) != DialogResult.OK) return;
            var str = save();
            File.WriteAllText(p, str);
            setmodified(false);
            currentfile = f;
            Text = $"AUS Save File Editor ({currentfile})";
        }
        string currentfile = "";
        void load(string savdat)
        {
            f = SaveData.LoadFromString(savdat);

            label62.Visible = !f.IsChecksumValid;

            var reenable = checkBoxSyncVars.Checked;
            checkBoxSyncVars.Checked = false;

            var a = paa.ToArray();
            for (int i = 0; i < f.Abilities.Length; i++)
            {
                if (i != (int)a[i].Tag) throw new InvalidOperationException();
                a[i].Checked = f.Abilities[i];
            }
            var h = pah.ToArray();
            for (int i = 0; i < f.Hearts.Length; i++)
            {
                if (i != (int)h[i].Tag) throw new InvalidOperationException();
                h[i].Checked = f.Hearts[i];
            }
            var g = pag.ToArray();
            for (int i = 0; i < f.Gold.Length; i++)
            {
                if (i != (int)g[i].Tag) throw new InvalidOperationException();
                g[i].Checked = f.Gold[i];
            }
            var s = pas.ToArray();
            for (int i = 0; i < f.Secrets.Length; i++)
            {
                if (i != (((int index, bool istru, int max))s[i].Tag).index) throw new InvalidOperationException();
                s[i].Value = f.Secrets[i];
            }
            var b = pabu.ToArray();
            for (int i = 0; i < f.Buys.Length; i++)
            {
                if (i != (((int index, int cost))b[i].Tag).index) throw new InvalidOperationException();
                b[i].Checked = f.Buys[i];
            }
            var bb = pab.ToArray();
            for (int i = 0; i < f.Bosses.Length; i++)
            {
                if (i != (int)bb[i].Tag) throw new InvalidOperationException();
                bb[i].Checked = f.Bosses[i];
            }
            checkBoxMusic.Checked = !f.Mute;
            checkBoxFullscreen.Checked = f.FullScreen;
            numericPausem.Value = f.PauseM;
            difficultyCombobox.SelectedIndex = f.Difficulty;
            onehitCheckbox.Checked = f.OneHit;
            for (int x = 0; x < f.Map.GetLength(0); x++)
                for (int y = 0; y < f.Map.GetLength(1); y++)
                {
                    var c = Color.FromArgb(f.Map[x, y]);
                    c = Color.FromArgb(byte.MaxValue, c.B, c.G, c.R);
                    settile(x, y, c);
                    setexits(x, y, f.MapExitRight[x, y], f.MapExitDown[x, y]);
                    setsave(x, y, f.MapSave[x, y]);
                }
            //i didnt think of this while i was manually making score controls so... here we go!
            numeric_scores_ac_0.Value = (decimal)f.AstroCrashScores[0];
            numeric_scores_ac_1.Value = (decimal)f.AstroCrashScores[1];
            numeric_scores_ac_2.Value = (decimal)f.AstroCrashScores[2];
            numeric_scores_ac_3.Value = (decimal)f.AstroCrashScores[3];
            numeric_scores_ac_4.Value = (decimal)f.AstroCrashScores[4];
            numeric_scores_jb_0.Value = (decimal)f.JumpBoxScores[0];
            numeric_scores_jb_1.Value = (decimal)f.JumpBoxScores[1];
            numeric_scores_jb_2.Value = (decimal)f.JumpBoxScores[2];
            numeric_scores_jb_3.Value = (decimal)f.JumpBoxScores[3];
            numeric_scores_jb_4.Value = (decimal)f.JumpBoxScores[4];
            numeric_scores_kg_0.Value = (decimal)f.KeepGoingScores[0];
            numeric_scores_kg_1.Value = (decimal)f.KeepGoingScores[1];
            numeric_scores_kg_2.Value = (decimal)f.KeepGoingScores[2];
            numeric_scores_kg_3.Value = (decimal)f.KeepGoingScores[3];
            numeric_scores_kg_4.Value = (decimal)f.KeepGoingScores[4];
            //god
            numericGold.Value = (decimal)f.GoldTotal;
            numericHearts.Value = (decimal)f.HeartsTotal;
            numericAbilities.Value = (decimal)f.AbilitiesTotal;
            numericFlowers.Value = (decimal)f.FlowersTotal;
            numericMaxhp.Value = (decimal)f.MaxHealth;
            numericMaxair.Value = (decimal)f.MaxAir;
            numericJump.Value = (decimal)f.JumpHeight;
            numericDoubleJump.Value = (decimal)f.DoubleJumpHeight;
            numericCrystals.Value = (decimal)f.Crystals;
            numericPlayed.Value = (decimal)f.TimePlayed;
            numericDeaths.Value = (decimal)f.Deaths;
            numericSaves.Value = (decimal)f.Saves;
            numericDamage.Value = (decimal)f.Damage;
            numericStageX.Value = (decimal)f.StageX;
            numericStageY.Value = (decimal)f.StageY;
            numericX.Value = (decimal)f.X;
            numericY.Value = (decimal)f.Y;
            numeric_other_saveslot.Value = (decimal)f.SaveSlot;
            textBoxName.Text = f.Name;
            numeric_other_gp.Value = (decimal)f.gp;
            numeric_other_kb.Value = (decimal)f.kb;
            cbcsis = true;
            comboBoxControls.SelectedIndex = (int)numeric_other_kb.Value;
            cbcsis = false;
            numeric_other_kb_jump.Value = (decimal)f.kb_jump;
            numeric_other_kb_shoot.Value = (decimal)f.kb_shoot;
            numeric_other_kb_action.Value = (decimal)f.kb_action;
            numeric_other_kb_pause.Value = (decimal)f.kb_pause;
            numeric_other_gp_jump.Value = (decimal)f.gp_jump;
            numeric_other_gp_shoot.Value = (decimal)f.gp_shoot;
            numeric_other_gp_action.Value = (decimal)f.gp_action;
            numeric_other_gp_pause.Value = (decimal)f.gp_pause;

            checkBoxSyncVars.Checked = reenable;
        }
        string save()
        {
            f = new SaveData();

            var a = paa.ToArray();
            for (int i = 0; i < f.Abilities.Length; i++)
            {
                if (i != (int)a[i].Tag) throw new InvalidOperationException();
                f.Abilities[i] = a[i].Checked;
            }
            var h = pah.ToArray();
            for (int i = 0; i < f.Hearts.Length; i++)
            {
                if (i != (int)h[i].Tag) throw new InvalidOperationException();
                f.Hearts[i] = h[i].Checked;
            }
            var g = pag.ToArray();
            for (int i = 0; i < f.Gold.Length; i++)
            {
                if (i != (int)g[i].Tag) throw new InvalidOperationException();
                f.Gold[i] = g[i].Checked;
            }
            var s = pas.ToArray();
            for (int i = 0; i < f.Secrets.Length; i++)
            {
                if (i != (((int index, bool, int))s[i].Tag).index) throw new InvalidOperationException();
                f.Secrets[i] = (byte)s[i].Value;
            }
            var b = pabu.ToArray();
            for (int i = 0; i < f.Buys.Length; i++)
            {
                if (i != (((int, int))b[i].Tag).Item1) throw new InvalidOperationException();
                f.Buys[i] = b[i].Checked;
            }
            var bb = pab.ToArray();
            for (int i = 0; i < f.Bosses.Length; i++)
            {
                if (i != (int)bb[i].Tag) throw new InvalidOperationException();
                f.Bosses[i] = bb[i].Checked;
            }
            f.Mute = !checkBoxMusic.Checked;
            f.FullScreen = checkBoxFullscreen.Checked;
            f.PauseM = (byte)numericPausem.Value;
            f.Difficulty = (byte)difficultyCombobox.SelectedIndex;
            f.OneHit = onehitCheckbox.Checked;
            for (int x = 0; x < f.Map.GetLength(0); x++)
                for (int y = 0; y < f.Map.GetLength(1); y++)
                {
                    var c = Color.FromArgb(maptiles[x, y]);
                    c = Color.FromArgb(byte.MinValue, c.B, c.G, c.R);
                    f.Map[x, y] = c.ToArgb();
                    f.MapExitRight[x, y] = mapexitright[x, y];
                    f.MapExitDown[x, y] = mapexitdown[x, y];
                    f.MapSave[x, y] = mapsave[x, y];
                }
            //aaaaaaaahhhhhhhhhhhhhhhhhhhhhhhhhhhh
            f.AstroCrashScores[0] = (double)numeric_scores_ac_0.Value;
            f.AstroCrashScores[1] = (double)numeric_scores_ac_1.Value;
            f.AstroCrashScores[2] = (double)numeric_scores_ac_2.Value;
            f.AstroCrashScores[3] = (double)numeric_scores_ac_3.Value;
            f.AstroCrashScores[4] = (double)numeric_scores_ac_4.Value;
            f.JumpBoxScores[0] = (double)numeric_scores_jb_0.Value;
            f.JumpBoxScores[1] = (double)numeric_scores_jb_1.Value;
            f.JumpBoxScores[2] = (double)numeric_scores_jb_2.Value;
            f.JumpBoxScores[3] = (double)numeric_scores_jb_3.Value;
            f.JumpBoxScores[4] = (double)numeric_scores_jb_4.Value;
            f.KeepGoingScores[0] = (double)numeric_scores_kg_0.Value;
            f.KeepGoingScores[1] = (double)numeric_scores_kg_1.Value;
            f.KeepGoingScores[2] = (double)numeric_scores_kg_2.Value;
            f.KeepGoingScores[3] = (double)numeric_scores_kg_3.Value;
            f.KeepGoingScores[4] = (double)numeric_scores_kg_4.Value;
            //cbt!
            f.GoldTotal = (double)numericGold.Value;
            f.HeartsTotal = (double)numericHearts.Value;
            f.AbilitiesTotal = (double)numericAbilities.Value;
            f.FlowersTotal = (double)numericFlowers.Value;
            f.MaxHealth = (double)numericMaxhp.Value;
            f.MaxAir = (double)numericMaxair.Value;
            f.JumpHeight = (double)numericJump.Value;
            f.DoubleJumpHeight = (double)numericDoubleJump.Value;
            f.Crystals = (double)numericCrystals.Value;
            f.TimePlayed = (double)numericPlayed.Value;
            f.Deaths = (double)numericDeaths.Value;
            f.Saves = (double)numericSaves.Value;
            f.Damage = (double)numericDamage.Value;
            f.StageX = (double)numericStageX.Value;
            f.StageY = (double)numericStageY.Value;
            f.X = (double)numericX.Value;
            f.Y = (double)numericY.Value;
            f.SaveSlot = (double)numeric_other_saveslot.Value;
            f.Name = textBoxName.Text;
            f.gp = (double)numeric_other_gp.Value;
            f.kb = (double)numeric_other_kb.Value;
            f.kb = (double)comboBoxControls.SelectedIndex;
            f.kb_jump = (double)numeric_other_kb_jump.Value;
            f.kb_shoot = (double)numeric_other_kb_shoot.Value;
            f.kb_action = (double)numeric_other_kb_action.Value;
            f.kb_pause = (double)numeric_other_kb_pause.Value;
            f.gp_jump = (double)numeric_other_gp_jump.Value;
            f.gp_shoot = (double)numeric_other_gp_shoot.Value;
            f.gp_action = (double)numeric_other_gp_action.Value;
            f.gp_pause = (double)numeric_other_gp_pause.Value;

            return f.SaveToString();
        }

        #region if debug (why doesnt vs add collapse button for #ifs...)
#if DEBUG//show console
        private static bool consoleCreated = false;
        public static void CreateConsole()
        {
            if (consoleCreated) return;
            else consoleCreated = true;

            AllocConsole();

            // stdout's handle seems to always be equal to 7
            IntPtr defaultStdout = new IntPtr(7);
            IntPtr currentStdout = GetStdHandle(StdOutputHandle);

            if (currentStdout != defaultStdout)
                // reset stdout
                SetStdHandle(StdOutputHandle, defaultStdout);

            // reopen stdout
            TextWriter writer = new StreamWriter(Console.OpenStandardOutput())
            { AutoFlush = true };
            Console.SetOut(writer);
        }

        // P/Invoke required:
        private const UInt32 StdOutputHandle = 0xFFFFFFF5;

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetStdHandle(UInt32 nStdHandle);
        [DllImport("kernel32.dll")]
        private static extern void SetStdHandle(UInt32 nStdHandle, IntPtr handle);
        [DllImport("kernel32")]
        static extern bool AllocConsole();
#endif
        #endregion
        private const string ConfigPath = ".aussecfg";
        void loadconfig()
        {
            if (File.Exists(ConfigPath))
            {
                var f = File.ReadAllLines(ConfigPath);
                openFileDialog1.FileName = saveFileDialog1.FileName = f[0];
                openFileDialog2.FileName = f[1];
                primaryColor = button1.BackColor = colorDialog1.Color = Color.FromArgb(int.Parse(f[2]));
                secondaryColor = button2.BackColor = colorDialog2.Color = Color.FromArgb(int.Parse(f[3]));
                int[] colors = f[4].Split(',').Select(x => int.Parse(x)).ToArray();
                colorDialog1.CustomColors = colorDialog2.CustomColors = colors;
                egg = f.Length > 5 && f[5] == "egg";
            }
        }
        void saveconfig()
        {
            File.WriteAllLines(ConfigPath, new[]
            {
                openFileDialog1.FileName,
                openFileDialog2.FileName,
                primaryColor.ToArgb().ToString(),
                secondaryColor.ToArgb().ToString(),
                string.Join(",", colorDialog1.CustomColors.Select(x => x.ToString())),
                egg ? "egg" : "",
            });
        }
        private void Form1_Load(object __, EventArgs ___)
        {
#if DEBUG
            //show console if debug
            CreateConsole();
#endif
            //if (File.Exists(".dialogpath")) openFileDialog1.FileName = saveFileDialog1.FileName = File.ReadAllText(".dialogpath");
            loadconfig();
            Height += SystemInformation.HorizontalScrollBarHeight;
            tabControl1.Height += SystemInformation.HorizontalScrollBarHeight;
            Color c1 = Color.FromArgb(140, 140, 140), c2 = Color.FromArgb(120, 120, 120), c3 = Color.FromArgb(110, 110, 110), c4 = Color.FromArgb(130, 130, 130);
            #region load button gradient stuff
            //animate load button text color
            buttonLoad.Font = new Font(buttonLoad.Font, FontStyle.Bold);
            byte r = 255, g = 0, b = 0;
            byte which = 0;
            timer1.Tick += (____, _so_many_placeholder_variables) =>
            {
                buttonLoad.ForeColor = Color.FromArgb(r, g, b);
                const int increment = 3;
                switch (which)
                {
                    case 0:
                        if ((g += increment) == 255) which++;
                        break;
                    case 1:
                        if ((r -= increment) == 0) which++;
                        break;
                    case 2:
                        if ((b += increment) == 255) which++;
                        break;
                    case 3:
                        if ((g -= increment) == 0) which++;
                        break;
                    case 4:
                        if ((r += increment) == 255) which++;
                        break;
                    case 5:
                        if ((b -= increment) == 0) which = 0;
                        break;
                }
            };

            #endregion

            //pausem stuff
            pausempbs = new[] { picture_opt0, picture_opt1, picture_opt2, picture_opt3, picture_opt4 };
            for (int i = 0; i < pausempbs.Length; i++)
            {
                int index = i;
                pausempbs[index].Tag = index;
                pausempbs[index].Click += delegate (object _sender, EventArgs _)
                {
                    numericPausem.Value = (int)(_sender as PictureBox).Tag;
                };
            }

            //idk how to avoid copy and paste here honestly
            #region set up panel checkboxes stuff
            #region abilities
            var saa = new CheckBox()
            {
                Text = "Select All",
                Location = new Point(0, 0),
                AutoSize = true,
                MinimumSize = new Size(panelAbilities.Width, 24),
                ForeColor = Color.White,
            };
            bool saac = false;
            saa.CheckedChanged += delegate
            {
                if (saac || saa.CheckState == CheckState.Indeterminate) return;
                saac = true;
                foreach (var item in paa)
                {
                    if (item == saa) continue;
                    item.CheckState = saa.CheckState;
                }
                saac = false;
            };
            panelAbilities.Controls.Add(saa);
            int ypos = saa.Height;
            int y = 0;
            var tiem = !egg ? new Timer() { Interval = 40 } : null;
            var ss = false;
            panelAbilities.AutoScroll = false;
            panelAbilities.HorizontalScroll.Enabled = false;
            panelAbilities.HorizontalScroll.Visible = false;
            panelAbilities.HorizontalScroll.Maximum = 0;
            panelAbilities.AutoScroll = true;
            var maxwidtha = panelAbilities.Width - SystemInformation.VerticalScrollBarWidth;
            for (int i = 0; i < 24; i++)
            {
                int index = i;
                var cb = new CheckBox()
                {
                    Text = ab[index] + " [" + (index + 1) + "]" + "\n" + ab_d[index],
                    Location = new Point(0, ypos),
                    //AutoSize = true,
                    Tag = index,
                    BackColor = i % 2 == 0 ? c1 : c2,
                    //MinimumSize = new Size(panelAbilities.Width, 0),
                    Width = maxwidtha,
                    ForeColor = Color.White,
                };
                var width = TextRenderer.MeasureText(cb.Text, cb.Font, new Size(0, 0)).Width;
                cb.Height += (int)(cb.Height * ((width) / (maxwidtha - (8d + SystemInformation.MenuCheckSize.Width))));
                cb.Height -= 4;
                cb.CheckedChanged += delegate
                {
                    if (checkBoxSyncVars.Checked)
                    {
                        numericAbilities.Value += cb.Checked ? 1 : -1;

                        switch (index)
                        {
                            case 1 - 1: numericJump.Value += cb.Checked ? 1 : -1; break;
                            case 2 - 1: numericJump.Value += cb.Checked ? 1 : -1; break;
                            case 3 - 1: numericJump.Value += cb.Checked ? 1 : -1; break;
                            case 6 - 1: numericDoubleJump.Value += cb.Checked ? 1 : -1; break;
                            case 7 - 1: numericDoubleJump.Value += cb.Checked ? 1 : -1; break;
                            case 8 - 1: numericDoubleJump.Value += cb.Checked ? 1 : -1; break;
                            case 18 - 1: numericMaxair.Value += cb.Checked ? 50 : -50; break;
                            case 19 - 1: numericMaxair.Value += cb.Checked ? 50 : -50; break;
                        }
                    }
                    if (saac) return;
                    if (paa.All(x => x.CheckState == CheckState.Checked || x == saa)) saa.CheckState = CheckState.Checked;
                    else if (paa.All(x => x.CheckState == CheckState.Unchecked || x == saa)) saa.CheckState = CheckState.Unchecked;
                    else saa.CheckState = CheckState.Indeterminate;
                };
                panelAbilities.Controls.Add(cb);
                ypos += cb.Height;
            }
            paa = panelAbilities.GetAll<CheckBox>().Where(x => x != saa);
            #endregion
            #region hearts
            var sah = new CheckBox()
            {
                Text = "Select All",
                Location = new Point(0, 0),
                AutoSize = true,
                MinimumSize = new Size(panelHearts.Width, 24),
                ForeColor = Color.White,
            };
            bool saah = false;
            sah.CheckedChanged += delegate
            {
                if (saah || sah.CheckState == CheckState.Indeterminate) return;
                saah = true;
                foreach (var item in pah)
                {
                    if (item == sah) continue;
                    item.CheckState = sah.CheckState;
                }
                saah = false;
            };
            panelHearts.Controls.Add(sah);
            ypos = sah.Height;
            panelHearts.AutoScroll = false;
            panelHearts.HorizontalScroll.Enabled = false;
            panelHearts.HorizontalScroll.Visible = false;
            panelHearts.HorizontalScroll.Maximum = 0;
            panelHearts.AutoScroll = true;
            var maxwidth = panelHearts.Width - SystemInformation.VerticalScrollBarWidth;
            for (int i = 0; i < 95; i++)
            {
                int index = i;
                var cb = new CheckBox()
                {
                    Text = "Heart " + (index + 1) + Environment.NewLine + h[index],
                    Location = new Point(0, ypos),
                    //AutoSize = false,
                    Tag = index,
                    BackColor = i % 2 == 1 ? c1 : c2,
                    //Width = panelHearts.Width - SystemInformation.VerticalScrollBarWidth,
                    //AutoSize = true,
                    //MinimumSize=new Size(minwidth,0),
                    //MaximumSize=new Size(0,0),
                    Width = maxwidth,
                    ForeColor = Color.White,
                };//i spent over half of my day trying to have control height depending on the text AKA somehow fucking text wrap this shit and yet it still doesnt quite work as it should but i dont even care anymore
                var width = TextRenderer.MeasureText(cb.Text, cb.Font, new Size(0, 0)).Width;
                //Console.WriteLine(index + " " + width);
                cb.Height += (int)(cb.Height * ((width) / (maxwidth - (8d + SystemInformation.MenuCheckSize.Width))));
                cb.Height -= 4;
                if (index == 7) cb.Height += 6;//fucking
                //cb.Height += cb.Height + cb.Height * (TextRenderer.MeasureText(cb.Text, cb.Font, new Size()).Width / cb.Width);
                cb.CheckedChanged += delegate
                {
                    if (checkBoxSyncVars.Checked)
                    {
                        numericHearts.Value += cb.Checked ? 1 : -1;
                        //switch (index)
                        //{
                        //    //case 73 - 1: pas.First(x => (((int index, bool istru))x.Tag).index == 48 - 1).Value = cb.Checked ? 1 : 0; break;
                        //    //case 74 - 1: pas.First(x => (((int index, bool istru))x.Tag).index == 50 - 1).Value = cb.Checked ? 1 : 0; break;
                        //}
                    }
                    if (saah) return;
                    if (pah.All(x => x.CheckState == CheckState.Checked || x == sah)) sah.CheckState = CheckState.Checked;
                    else if (pah.All(x => x.CheckState == CheckState.Unchecked || x == sah)) sah.CheckState = CheckState.Unchecked;
                    else sah.CheckState = CheckState.Indeterminate;
                };
                panelHearts.Controls.Add(cb);
                ypos += cb.Height;
            }
            var sssss = !egg ? new MemoryStream(Properties.Resources.huh) : null;
            var fghj = !egg ? new PictureBox() { Parent = tabPage4, Location = new Point(label11.Location.X + label11.Size.Width, label11.Location.Y - 4), SizeMode = PictureBoxSizeMode.StretchImage, Size = new Size(28, 28), Visible = false, Image = Image.FromStream(sssss), Tag = sssss } : null;
            pah = panelHearts.GetAll<CheckBox>().Where(x => x != sah);
            #endregion
            #region boss
            var sab = new CheckBox()
            {
                Text = "Select All",
                Location = new Point(0, 0),
                AutoSize = true,
                MinimumSize = new Size(panelBosses.Width, 24),
                ForeColor = Color.White,
            };
            bool saab = false;
            sab.CheckedChanged += delegate
            {
                if (saab || sab.CheckState == CheckState.Indeterminate) return;
                saab = true;
                foreach (var item in pab)
                {
                    if (item == sab) continue;
                    item.CheckState = sab.CheckState;
                }
                saab = false;
            };
            panelBosses.Controls.Add(sab);
            ypos = sab.Height;
            panelBosses.AutoScroll = false;
            panelBosses.HorizontalScroll.Enabled = false;
            panelBosses.HorizontalScroll.Visible = false;
            panelBosses.HorizontalScroll.Maximum = 0;
            panelBosses.AutoScroll = true;
            if (!egg) tiem.Tick += (xzcxzc, xzczxc) => { if (!ss && y > 0) { y -= 1; if (y < 1) tiem.Enabled = false; } if (y > 12) ss = true; if (ss) { y++; fghj.Visible = !fghj.Visible; if (y >= 69) { fghj.Top += 1 * ((y - 68) / 10); } if (fghj.Top > Height + 100) { tiem.Enabled = false; tiem.Dispose(); tabPage4.Controls.Remove(fghj); fghj.Image.Dispose(); ((IDisposable)fghj.Tag).Dispose(); fghj.Dispose(); egg = true; } } };
            for (int i = 0; i < 17; i++)
            {
                int index = i;
                var cb = new CheckBox
                {
                    Text = "Boss " + (index + 1) + "\n",
                    Location = new Point(0, ypos),
                    AutoSize = true,
                    Tag = index,
                    BackColor = i % 2 == 0 ? c1 : c2,
                    MinimumSize = new Size(panelBosses.Width, 0),
                    ForeColor = Color.White,
                };
                cb.CheckedChanged += delegate
                {
                    updateBossIcon((int)cb.Tag);
                    if (saab) return;
                    if (pab.All(x => x.CheckState == CheckState.Checked || x == sab)) sab.CheckState = CheckState.Checked;
                    else if (pab.All(x => x.CheckState == CheckState.Unchecked || x == sab)) sab.CheckState = CheckState.Unchecked;
                    else sab.CheckState = CheckState.Indeterminate;
                };
                string n = null;
                switch (index)
                {
                    case 0: n = "Grotto"; break;
                    case 1: n = "DeepTower"; break;
                    case 2: n = "ColdKeep"; break;
                    case 3: n = "NightClimb"; break;
                    case 4: n = "StoneCastle"; break;
                    case 5: n = "Grotto 2"; break;
                    case 6: n = "FireCage"; break;
                    case 7: n = "FarFall"; break;
                    case 8: n = "StoneCastle 2"; break;
                    case 9: n = "SkySand"; break;
                    case 10: n = "CloudRun"; break;
                    case 11: n = "DeepDive"; break;
                    case 12: n = "DarkGrotto"; break;
                    case 13: n = "The Curtain"; break;
                    case 14: n = "IceCastle"; break;
                    case 15: n = "BlancLand"; break;
                    case 16: n = "BlackCastle"; break;
                }
                cb.Text += n;
                panelBosses.Controls.Add(cb);
                ypos += cb.Height;
            }
            pab = panelBosses.GetAll<CheckBox>().Where(x => x != sab);
            #endregion
            #region gold
            var sag = new CheckBox()
            {
                Text = "Select All",
                Location = new Point(0, 0),
                AutoSize = true,
                MinimumSize = new Size(panelGold.Width, 24),
                ForeColor = Color.White,
            };
            bool saag = false;
            sag.CheckedChanged += delegate
            {
                if (saag || sag.CheckState == CheckState.Indeterminate) return;
                saag = true;
                foreach (var item in pag)
                {
                    if (item == sag) continue;
                    item.CheckState = sag.CheckState;
                }
                saag = false;
            };
            panelGold.Controls.Add(sag);
            ypos = sag.Height;
            if (!egg) tabPage4.Controls.Add(fghj);
            panelGold.AutoScroll = false;
            panelGold.HorizontalScroll.Enabled = false;
            panelGold.HorizontalScroll.Visible = false;
            panelGold.HorizontalScroll.Maximum = 0;
            panelGold.AutoScroll = true;
            for (int i = 0; i < 10; i++)
            {
                int index = i;
                var cb = new CheckBox
                {
                    Text = "Gold " + (index + 1) + "\n",
                    Location = new Point(0, ypos),
                    AutoSize = true,
                    Tag = index,
                    BackColor = i % 2 == 1 ? c1 : c2,
                    MinimumSize = new Size(panelGold.Width, 0),
                    ForeColor = Color.White,
                };
                string n = null;
                switch (index)
                {
                    case 1 - 1: n = "Buy in Sky Town shop."; break;
                    case 2 - 1: n = "Beat the FarFall boss."; break;
                    case 3 - 1: n = "Beat the StoneCastle 2nd boss."; break;
                    case 4 - 1: n = "Beat the SkySand boss."; break;
                    case 5 - 1: n = "Beat the CloudRun boss."; break;
                    case 6 - 1: n = "Beat the DeepDive boss."; break;
                    case 7 - 1: n = "Beat the DarkGrotto boss."; break;
                    case 8 - 1: n = "Beat the The Curtain boss."; break;
                    case 9 - 1: n = "Beat the IceCastle boss."; break;
                    case 10 - 1: n = "Beat the BlancLand boss."; break;
                }
                cb.Text += n;
                cb.CheckedChanged += delegate
                {
                    if (checkBoxSyncVars.Checked)
                        numericGold.Value += cb.Checked ? 1 : -1;
                    if (saag) return;
                    if (pag.All(x => x.CheckState == CheckState.Checked || x == sag)) sag.CheckState = CheckState.Checked;
                    else if (pag.All(x => x.CheckState == CheckState.Unchecked || x == sag)) sag.CheckState = CheckState.Unchecked;
                    else sag.CheckState = CheckState.Indeterminate;
                };
                panelGold.Controls.Add(cb);
                ypos += cb.Height;
            }
            pag = panelGold.GetAll<CheckBox>().Where(x => x != sag);
            #endregion
            #region secrets
            var sas = new CheckBox()
            {
                Text = "Set all to natural max",
                Location = new Point(0, 0),
                AutoSize = true,
                MinimumSize = new Size(panelSecrets.Width, 24),
                ForeColor = Color.White,
            };
            bool saas = false;
            sas.CheckedChanged += delegate
            {
                if (saas || sas.CheckState == CheckState.Indeterminate) return;
                saas = true;
                foreach (var item in pas)
                {
                    item.Value = sas.CheckState == CheckState.Checked ? (((int index, bool istru, int max))item.Tag).max : 0;
                }
                saas = false;
            };
            panelSecrets.Controls.Add(sas);
            ypos = sas.Height;
            panelSecrets.AutoScroll = false;
            panelSecrets.HorizontalScroll.Enabled = false;
            panelSecrets.HorizontalScroll.Visible = false;
            panelSecrets.HorizontalScroll.Maximum = 0;
            panelSecrets.AutoScroll = true;
            var minsize = 0;
            var lbs = new HashSet<Label>();
            for (int i = 0; i < 95; i++)
            {
                int index = i;
                var cb = new Label
                {
                    Text = "Secret " + (index + 1) + "\n",
                    Location = new Point(0, ypos),
                    AutoSize = true,
                    BackColor = i % 2 == 0 ? c1 : c2,
                    MinimumSize = new Size(panelSecrets.Width, 0),
                    ForeColor = Color.White,
                };
                cb.MinimumSize = new Size(cb.MinimumSize.Width, cb.MinimumSize.Height + 4);
                lbs.Add(cb);
                ypos += cb.Height;
                var n = "???";
                int c = 1;
                switch (index)
                {
                    case 1 - 1: n = "Prayed to gods in NightWalk."; break;
                    case 2 - 1: n = "Bought 1st heart in Sky Town shop."; break;
                    case 3 - 1: n = "Bought 2nd heart."; break;
                    case 4 - 1: n = "Bought 3rd heart."; break;
                    case 5 - 1: n = "Bought 4th heart."; break;
                    case 6 - 1: n = "Bought 5th heart."; break;
                    case 7 - 1: n = "Bought Energy Jump 1 ability."; break;
                    case 8 - 1: n = "Bought Long Shots ability."; break;
                    case 9 - 1: n = "Bought a Gold Orb."; break;
                    case 10 - 1: n = "Bought the property in Sky Town."; break;
                    case 11 - 1: n = "Opened the heart door in StoneCastle."; break;
                    case 12 - 1: n = "Paid the tollbooth in FarFall."; break;
                    case 13 - 1: n = "Paid the tollbooth in NightWalk."; break;
                    case 14 - 1: n = "Pushed the button in NightWalk."; break;
                    case 15 - 1: n = "Opened the (heart) heart door in FarFall."; break;
                    case 16 - 1: n = "Opened the (ability) heart door in FarFall."; break;
                    case 17 - 1: n = "Opened the first heart door in FireCage."; break;

                    case 19 - 1: n = "Paid the tollbooth in FireCage."; break;
                    case 20 - 1: n = "Opened the second heart door in FireCage."; break;
                    case 21 - 1: n = "Pressed the button in FireCage."; break;
                    case 22 - 1: n = "Bandit walked through the boss in FireCage."; break;
                    case 23 - 1: n = "Ghost cutscene with Bandit in FireCage."; break;
                    case 24 - 1: n = "Talked to the guy in Bonus."; break;

                    case 26 - 1: n = "Broke the fire barrier in SkySand entrance."; break;
                    case 27 - 1: n = "Opened the first heart door in SkySand."; break;
                    case 28 - 1: n = "Opened the second heart door in SkySand."; break;
                    case 29 - 1: n = "Talked to Adven in SkySand."; break;
                    case 30 - 1: n = "Ghost cutscene with Adven in SkySand."; break;
                    case 31 - 1: n = "Solved treasure chest puzzle in FarFall."; break;
                    case 32 - 1: n = "Treasure chest introduction."; break;
                    case 33 - 1: n = "Solved treasure chest puzzle in SkySand."; break;
                    case 34 - 1: n = "Solved treasure chest puzzle in DeepDive."; break;
                    case 35 - 1: n = "Solved treasure chest puzzle in NightClimb."; break;
                    case 36 - 1: n = "Destroyed the hidden block in HighLands."; break;
                    case 37 - 1: n = "Opened the heart door in NightWalk challenge."; break;
                    case 38 - 1: n = "Solved treasure chest puzzle in NightWalk challenge."; break;
                    case 39 - 1: n = "Destroyed the hidden block in The Curtain."; break;
                    case 40 - 1: n = "Opened a heart door in IceCastle."; break;
                    case 41 - 1: n = "Opened a heart door in IceCastle."; break;
                    case 42 - 1: n = "Opened a heart door in IceCastle."; break;
                    case 43 - 1: n = "Opened a heart door in IceCastle."; break;

                    case 45 - 1: n = "Talked to Girl. [0-2]"; c = 2; break;
                    case 46 - 1: n = "Talked to Floretta."; break;
                    case 47 - 1: n = "Talked to Floretta with 5 flowers."; break;
                    case 48 - 1: n = "Talked to Floretta with 10 flowers."; break;
                    case 49 - 1: n = "Talked to Floretta with 15 flowers."; break;
                    case 50 - 1: n = "Talked to Floretta with 20 flowers."; break;
                    case var v when index >= 51 - 1 && index <= 70 - 1:
                        n = "Flower - " + new[] {
                            "The Curtain",
                            "LongBeach",
                            "IceCastle",
                            "BlancLand",
                            "StoneCastle",
                            "FarFall",
                            "NightWalk (savepoint)",
                            "BlackCastle",
                            "NightClimb",
                            "DeepDive",
                            "TheBottom",
                            "MountSide",
                            "Library",
                            "DarkGrotto",
                            "NightWalk (NightWalk challenge)",
                            "CloudRun",
                            "Grotto",
                            "SkySand",
                            "Sky Town",
                            "NightWalk (starting area)",
                        }[v - (51 - 1)]; break;
                    case 71 - 1: n = "Talked to Crystal Ball."; break;
                    case 72 - 1: n = "Claimed 3rd RainbowDive reward."; break;
                    case 73 - 1: n = "Claimed 4th RainbowDive reward."; break;
                    case 74 - 1: n = "Solved treasure chest puzzle in SkyLands."; break;
                    case 75 - 1: n = "Paid the tollbooth in SkyLands."; break;
                    case 76 - 1: n = "Opened the heart door in MountSide."; break;
                    case 77 - 1: n = "BlackCastle entrance barrier opened."; break;
                    case 78 - 1: n = "Ninja cutscene after passing BlackCastle barrier."; break;
                    case 79 - 1: n = "Current suit. [0-3]"; c = 3; break;
                    case 80 - 1: n = "Vending machine suits bought. [0-3]"; c = 3; break;
                    case 81 - 1: n = "Golden orb inserted into 1st slot in BlackCastle."; break;
                    case 82 - 1: n = "Golden orb inserted into 2nd slot in BlackCastle."; break;
                    case 83 - 1: n = "Golden orb inserted into 3rd slot in BlackCastle."; break;
                    case 84 - 1: n = "Golden orb inserted into 4th slot in BlackCastle."; break;
                    case 85 - 1: n = "Golden orb inserted into 5th slot in BlackCastle."; break;
                    case 86 - 1: n = "Golden orb inserted into 6th slot in BlackCastle."; break;
                    case 87 - 1: n = "Golden orb inserted into 7th slot in BlackCastle."; break;
                    case 88 - 1: n = "Golden orb inserted into 8th slot in BlackCastle."; break;
                    case 89 - 1: n = "Golden orb inserted into 9th slot in BlackCastle."; break;
                    case 90 - 1: n = "Golden orb inserted into 10th slot in BlackCastle."; break;
                    case 91 - 1: n = "Opened the heart door in StrangeCastle."; break;
                    case 92 - 1: n = "Destroyed the hidden block in LongBeach."; break;
                    case 93 - 1: n = "Opened the heart door on the right in UnderTomb."; break;
                    case 94 - 1: n = "Opened the heart door on the left in UnderTomb."; break;
                    case 95 - 1: n = "Opened the heart door in SkyLands."; break;
                }
                var thecb = new CustomNumericUpDown
                {
                    Location = new Point(0, ypos),
                    Tag = (index, istru: false, max: c),
                    BackColor = i % 2 != 0 ? c3 : c4,
                    Minimum = 0,
                    Maximum = 9,
                    //MaximumSize = new Size((int)Font.Size + SystemInformation.VerticalScrollBarWidth, int.MaxValue),
                    ForeColor = Color.White,
                };
                cb.Text += n;
                thecb.ValueChanged += delegate
                {
                    var h = ((int index, bool istru, int max))thecb.Tag;
                    var changedbool = (h.istru ^ thecb.Value > 0);
                    if (checkBoxSyncVars.Checked)
                    {
                        if (changedbool)
                        {
                            switch (index)
                            {
                                case (47 - 1): numericCrystals.Value += thecb.Value > 0 ? 200 : -200; break;
                                //case (48 - 1): pah.First(x => (73 - 1).Equals(x.Tag)).Checked = thecb.Value > 0; break;
                                case (49 - 1): numericCrystals.Value += thecb.Value > 0 ? 400 : -400; break;
                                //case (50 - 1): pah.First(x => (74 - 1).Equals(x.Tag)).Checked = thecb.Value > 0; break;
                                case (72 - 1): numericCrystals.Value += thecb.Value > 0 ? 300 : -300; break;
                                case (73 - 1): numericCrystals.Value += thecb.Value > 0 ? 100 : -100; break;
                            }
                            if (index >= 51 - 1 && index <= 70 - 1) numericFlowers.Value += thecb.Value > 0 ? 1 : -1;
                        }
                    }
                    h.istru = thecb.Value > 0;
                    thecb.Tag = h;
                    if (saas) return;
                    if (pas.All(x => x.Value == (((int index, bool istru, int max))x.Tag).max)) sas.CheckState = CheckState.Checked;
                    else if (pas.All(x => x.Value == 0)) sas.CheckState = CheckState.Unchecked;
                    else sas.CheckState = CheckState.Indeterminate;
                };
                panelSecrets.Controls.Add(cb);
                panelSecrets.Controls.Add(thecb);
                if (cb.Width > minsize) minsize = cb.Width;
                ypos += thecb.Height;
            }
            pas = panelSecrets.GetAll<CustomNumericUpDown>();
            if (!egg) label11.MouseEnter += (s, e) => { if (ss) return; fghj.Visible = true; Timer t = null; t = new Timer() { Interval = 40 }; t.Tick += (ssssss, ee) => { y += 4; fghj.Visible = false; t.Dispose(); }; t.Enabled = true; tiem.Enabled = true; };
            panelSecrets.Width = minsize + SystemInformation.VerticalScrollBarWidth;
            foreach (var item in pas) item.MinimumSize = new Size(minsize, 0);
            foreach (var item in lbs) item.MinimumSize = new Size(minsize, 0);
            sas.MinimumSize = new Size(minsize, 0);
            lbs.Clear();
            #endregion
            #region buys
            var sabu = new CheckBox()
            {
                Text = "Select All",
                Location = new Point(0, 0),
                AutoSize = true,
                MinimumSize = new Size(panelBuys.Width, 24),
                ForeColor = Color.White,
            };
            bool saabu = false;
            sabu.CheckedChanged += delegate
            {
                if (saabu || sabu.CheckState == CheckState.Indeterminate) return;
                saabu = true;
                foreach (var item in pabu)
                {
                    if (item == sabu) continue;
                    item.CheckState = sabu.CheckState;
                }
                saabu = false;
            };
            panelBuys.Controls.Add(sabu);
            ypos = sabu.Height;
            panelBuys.AutoScroll = false;
            panelBuys.HorizontalScroll.Enabled = false;
            panelBuys.HorizontalScroll.Visible = false;
            panelBuys.HorizontalScroll.Maximum = 0;
            panelBuys.AutoScroll = true;
            for (int i = 0; i < 24; i++)
            {
                int index = i;
                string n = null;
                int c = 0;
                switch (index)
                {
                    case 1 - 1: n = "Mesmerizing Box"; c = 24; break;
                    case 2 - 1: n = "Strange Heart Poster"; c = 3; break;
                    case 3 - 1: n = "Stuffed Abomination"; c = 6; break;
                    case 4 - 1: n = "Time Keeper"; c = 9; break;
                    case 5 - 1: n = "Blue Throne"; c = 30; break;
                    case 6 - 1: n = "Green Throne"; c = 30; break;
                    case 7 - 1: n = "Wisdom Shelf"; c = 45; break;
                    case 8 - 1: n = "Strange Lantern"; c = 11; break;
                    case 9 - 1: n = "SkyTown Flag"; c = 7; break;
                    case 10 - 1: n = "Strange Square Poster"; c = 9; break;
                    case 11 - 1: n = "Unusable Machine"; c = 57; break;
                    case 12 - 1: n = "Ancient Sign"; c = 85; break;
                    case 13 - 1: n = "Work of Art"; c = 118; break;
                    case 14 - 1: n = "JumpBox Arcade Game"; c = 112; break;
                    case 15 - 1: n = "Trendy Couch"; c = 62; break;
                    case 16 - 1: n = "Smallish Table"; c = 55; break;
                    case 17 - 1: n = "Potted NightWalk Tree"; c = 92; break;
                    case 18 - 1: n = "Potted SkySand Cactus"; c = 103; break;
                    case 19 - 1: n = "Keep Going!! Arcade Game"; c = 196; break;
                    case 20 - 1: n = "Better Work of Art"; c = 126; break;
                    case 21 - 1: n = "(Sorta) Heavenly Bed"; c = 173; break;
                    case 22 - 1: n = "Amazing Work of Art"; c = 202; break;
                    case 23 - 1: n = "Solid Gold Statue"; c = 1000; break;
                    case 24 - 1: n = "[Unused]"; c = 0; break;
                }
                var cb = new CheckBox
                {
                    Text = "Buy " + (index + 1) + "\n",
                    Location = new Point(0, ypos),
                    AutoSize = true,
                    Tag = (index, cost: c),
                    BackColor = i % 2 == 1 ? c1 : c2,
                    MinimumSize = new Size(panelBuys.Width, 0),
                    ForeColor = Color.White,
                };
                cb.Text += n;
                cb.CheckedChanged += delegate
                {
                    if (saabu) return;
                    if (checkBoxSyncVars.Checked)
                    {
                        var cost = (((int index, int cost))cb.Tag).cost;
                        numericCrystals.Value += cb.Checked ? -cost : cost;
                    }
                    if (pabu.All(x => x.CheckState == CheckState.Checked || x == sabu)) sabu.CheckState = CheckState.Checked;
                    else if (pabu.All(x => x.CheckState == CheckState.Unchecked || x == sabu)) sabu.CheckState = CheckState.Unchecked;
                    else sabu.CheckState = CheckState.Indeterminate;
                };
                panelBuys.Controls.Add(cb);
                ypos += cb.Height;
            }
            pabu = panelBuys.GetAll<CheckBox>().Where(x => x != sabu);
            #endregion

            #endregion
            initmap();
            load(new SaveData().SaveToString());
            setRandomName();
            setmodified(false);
            foreach (var ctrl in this.GetAll())
            {
                //Console.WriteLine(ctrl.GetType() + ctrl.Name);

                switch (ctrl)
                {
                    case CustomNumericUpDown c:
                        c.ValueChanged += (_, fuck) => setmodified(true);
                        c.Enter += (_, fuck) => c.Select();
                        //c.DecimalPlaces = 2;
                        break;
                    case TextBox c:
                        c.TextChanged += (_, fuck) => setmodified(true);
                        c.Enter += (_, fuck) => c.SelectAll();
                        break;
                    case CheckBox c:
                        if (c == checkBoxSyncVars || c == checkBoxShowBosses || c == checkBoxCorrectSaveSlot) continue;
                        c.CheckedChanged += (_, fuck) => setmodified(true);
                        break;
                    case ComboBox c:
                        c.SelectedIndexChanged += (_, fuck) => setmodified(true);
                        break;
                }
            }
        }
        static readonly Icon normal = Properties.Resources.ausfileeditor;
        static readonly Icon crack = Properties.Resources.ausfileeditorcrack;
        bool loading = false;
        void setmodified(bool yez)
        {
            if (yez)
            {
                if (!loading)
                {
                    if (!havechanged) { Text += "*"; Icon = crack; }
                    havechanged = true;
                }
            }
            else
            {
                if (havechanged) { Icon = normal; }
                havechanged = false;
            }
        }
        IEnumerable<CheckBox> paa = null;
        IEnumerable<CheckBox> pah = null;
        IEnumerable<CheckBox> pab = null;
        IEnumerable<CheckBox> pag = null;
        IEnumerable<CustomNumericUpDown> pas = null;
        IEnumerable<CheckBox> pabu = null;
        static readonly string[] n = new[]
        {
            "Rook",
            "Pawn",
            "King",
            "Bishop",
            "GostBot",
            "Edify",
            "Woe",
            "Happy",
            "YMM",
            "Matt",
            "Kurstyn",
            "MJK",
            "Queen",
            "Horse",
            "Check",
            "Mart",
            "Unnamed",
            "Coolio",
            "Languish",
            "Jumper",
            "Hollow",
            "Rather",
            "Temper",
            "Random",
            "Douglas",
            "Nurse",
            "Wisdom",
            "Pisces",
            "Liquid",
            "Adams",
            "Lateral",
            "Million",
            "Gabe",
            "Axis",
            "zyhrllos",
            "Return",
            "Current",
            "Ritual",
            "Red",
            "Blue",
            "Black",
            "Depth",
            "Gnarly",
            "KMoney",
            "TheHammer",
            "Needles",
            "TheButcher",
        };
        readonly Random rnd = new Random();
        void setRandomName()
        {
            string t;
            do { t = n[rnd.Next(0, n.Length)]; } while (t == textBoxName.Text);
            textBoxName.Text = t;
        }

        PictureBox[] pausempbs;
        private void numericPausem_ValueChanged(object sender, EventArgs e)
        {
            var val = numericPausem.Value;
            for (int i = 0; i < pausempbs.Length; i++)
            {
                pausempbs[i].Visible = (i != val);
            }
        }

        static readonly string[] ab = new string[24], ab_d = new string[24], h = new string[95];

        private void numeric_other_kb_ValueChanged(object sender, EventArgs e)
        {
            if (cbcsis) return;
            if (numeric_other_kb.Value < 0 || numeric_other_kb.Value >= 4)
            {
                cbcsis = true;
                comboBoxControls.SelectedIndex = -1;
                cbcsis = false;
                return;
            }
            if (!checkBoxSyncVars.Checked) cbcsis = true;
            comboBoxControls.SelectedIndex = (int)numeric_other_kb.Value;
            if (!checkBoxSyncVars.Checked) cbcsis = false;
        }

        bool cbcsis = false;

        private void difficultyCombobox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (checkBoxSyncVars.Checked)
            {
                if (difficultyCombobox.SelectedIndex == 4)
                    onehitCheckbox.CheckState = CheckState.Checked;
                else
                    onehitCheckbox.CheckState = CheckState.Unchecked;
            }
        }

        private void onehitCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxSyncVars.Checked)
                if (onehitCheckbox.CheckState == CheckState.Checked) difficultyCombobox.SelectedIndex = 4;
                else if (difficultyCombobox.SelectedIndex == 4) difficultyCombobox.SelectedIndex = 3;
        }

        private void textBoxName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.R) { e.SuppressKeyPress = true; setRandomName(); }
        }

        //////MAP STUFF
        Color primaryColor = Color.White;
        Color secondaryColor = Color.Black;
        PictureBox maptilespb, mapsavepb;
        Graphics maptilesg, mapsaveg;
        PictureBox mapbosspb;
        Graphics mapbossg;
        readonly int[,] maptiles = new int[30, 25];
        readonly bool[,] mapsave = new bool[30, 25];
        readonly bool[,] mapexitright = new bool[30, 25], mapexitdown = new bool[30, 25];
        void initmap()
        {
            colorDialog1.Color = primaryColor;
            colorDialog2.Color = secondaryColor;
            maptilespb = new PictureBox() { BackColor = Color.Transparent, Location = new Point(1, 1), Size = panelMap.Size - new Size(1, 1) };
            mapsavepb = new PictureBox() { BackColor = Color.Transparent, Location = new Point(0, 0), Size = panelMap.Size - new Size(1, 1) };
            maptilespb.Image = new Bitmap(panelMap.Size.Width - 1, panelMap.Size.Height - 1);
            mapsavepb.Image = new Bitmap(panelMap.Size.Width - 1, panelMap.Size.Height - 1);
            maptilesg = Graphics.FromImage(maptilespb.Image);
            mapsaveg = Graphics.FromImage(mapsavepb.Image);
            panelMap.Controls.Add(maptilespb);
            panelMap.Controls.Add(mapsavepb);
            maptilespb.Parent = panelMap;
            mapsavepb.Parent = maptilespb;
            maptilespb.BringToFront();
            mapsavepb.BringToFront();
            maptilespb.MouseDown += (s, e) => panelMap_MouseDown(s, e);
            mapsavepb.MouseDown += (s, e) => panelMap_MouseDown(s, e);
            maptilespb.MouseUp += (s, e) => panelMap_MouseUp(s, e);
            mapsavepb.MouseUp += (s, e) => panelMap_MouseUp(s, e);
            maptilespb.MouseMove += (s, e) => panelMap_MouseMove(s, e);
            mapsavepb.MouseMove += (s, e) => panelMap_MouseMove(s, e);
            mapbosspb = new PictureBox() { BackColor = Color.Transparent, Location = new Point(0, 0), Size = panelMap.Size - new Size(1, 1) };
            mapbosspb.Image = new Bitmap(panelMap.Size.Width - 1, panelMap.Size.Height - 1);
            mapbossg = Graphics.FromImage(mapbosspb.Image);
            panelMap.Controls.Add(mapbosspb);
            mapbosspb.Parent = mapsavepb;
            mapbosspb.BringToFront();
            mapbosspb.MouseDown += (s, e) => panelMap_MouseDown(s, e);
            mapbosspb.MouseUp += (s, e) => panelMap_MouseUp(s, e);
            mapbosspb.MouseMove += (s, e) => panelMap_MouseMove(s, e);
            for (int x = 0; x < 30; x++)
                for (int y = 0; y < 25; y++)
                {
                    maptiles[x, y] = -16777216;
                    updateBossIcon(x, y);
                }
        }
        void fill(PictureBox pb, Graphics g, int x, int y, int w, int h, Color c)
        {
            var b = new SolidBrush(c);
            g.FillRectangle(b, x, y, w, h);
            b.Dispose();
            pb.Invalidate(new Rectangle(x, y, w, h));
        }
        void settile(int x, int y, Color color)
        {
            var c = color.ToArgb();
            if (maptiles[x, y] == c) return;
            setmodified(true);
            maptiles[x, y] = c;
            if (color.A != byte.MaxValue) color = Color.FromArgb(byte.MaxValue, color);
            fill(maptilespb, maptilesg, x * 15, y * 15, 14, 14, color);
            setexits(x, y, mapexitright[x, y], mapexitdown[x, y], true);
            if (color.ToArgb() == Color.Black.ToArgb())
            {
                if (x > 0) setexits(x - 1, y, mapexitright[x - 1, y], mapexitdown[x - 1, y], true);
                if (y > 0) setexits(x, y - 1, mapexitright[x, y - 1], mapexitdown[x, y - 1], true);
            }
            updateBossIcon(x, y);
            setsave(x, y, mapsave[x, y], true);
        }
        void setexits(int x, int y, bool right, bool down, bool forceredraw = false)
        {
            if (!forceredraw) if (mapexitright[x, y] == right && mapexitdown[x, y] == down) return;
            setmodified(true);
            mapexitright[x, y] = right;
            mapexitdown[x, y] = down;
            int lr = x == 29 || maptiles[x + 1, y] != -16777216 ? 1 : 3;
            int ld = y == 24 || maptiles[x, y + 1] != -16777216 ? 1 : 3;
            if (!right) fill(maptilespb, maptilesg, x * 15 + 14, y * 15 + 5, lr, 3, Color.Black);
            if (!down) fill(maptilespb, maptilesg, x * 15 + 5, y * 15 + 14, 3, ld, Color.Black);
            if (!right && !down) return;
            var c = Color.FromArgb(maptiles[x, y]);
            //c = Color.FromArgb(byte.MaxValue, c.B, c.G, c.R);
            c = Color.FromArgb(byte.MaxValue, c.R, c.G, c.B);
            if (right) fill(maptilespb, maptilesg, x * 15 + 14, y * 15 + 5, lr, 3, c);
            if (down) fill(maptilespb, maptilesg, x * 15 + 5, y * 15 + 14, 3, ld, c);
        }
        const byte opacity = unchecked((byte)-96);
        static readonly Brush dimblackbrush = new SolidBrush(Color.FromArgb(opacity, 0, 0, 0));
        void setsave(int x, int y, bool show, bool redraw = false)
        {
            if (mapsave[x, y] == show && !redraw) return;
            setmodified(true);
            mapsave[x, y] = show;
            mapsaveg.SetClip(new Rectangle(x * 15, y * 15, 15, 15));
            mapsaveg.Clear(Color.Transparent);
            mapsaveg.ResetClip();
            if (show)
            {
                mapsaveg.DrawImage(Properties.Resources.mapIconSave, x * 15, y * 15);
                if (maptiles[x, y] == -16777216 && (!checkBoxShowBosses.Checked || !bosscoords.Contains((x, y)))) mapsaveg.FillRectangle(dimblackbrush, x * 15, y * 15, 15, 15);
            }
            mapsavepb.Invalidate(new Rectangle(x * 15, y * 15, 15, 15));
        }

        bool mousepressd = false;
        private void panelMap_MouseDown(object sender, MouseEventArgs e)
        {
            int x = e.X / 15, y = e.Y / 15;
            if (x < 0 || y < 0 || x > 29 || y > 24) return;
            if (e.Button == MouseButtons.Middle)
            {
                numericStageX.Value = x + 1; numericStageY.Value = y + 1;
                if (mapsave[x, y])
                {
                    var loc = savecoords.FirstOrDefault(z => z.Key.Equals((x, y)));
                    if (!loc.Key.Equals((0, 0))) { numericX.Value = loc.Value.x; numericY.Value = loc.Value.y; }
                }
            }
            if (e.Button != MouseButtons.Left && e.Button != MouseButtons.Right) return;
            lastx = x; lasty = y;
            which = e.Button == MouseButtons.Right;
            if (ModifierKeys.HasFlag(Keys.Alt))
            {
                var c = Color.FromArgb(maptiles[x, y]);
                var cc = Color.FromArgb(byte.MaxValue, c.R, c.G, c.B);
                if (!which) { primaryColor = cc; button1.BackColor = primaryColor; colorDialog1.Color = cc; }
                else { secondaryColor = cc; button2.BackColor = secondaryColor; colorDialog2.Color = cc; }
                return;
            }
            mousepressd = true;
            moder = !mapexitright[x, y];
            moded = !mapexitdown[x, y];
            modes = !mapsave[x, y];
            Console.WriteLine($"{which} {moder} {moded} {modes}");
            panelMap_MouseMove(sender, e);
        }

        private void panelMap_MouseUp(object sender, MouseEventArgs e)
        {
            mousepressd = false;
        }

        bool which;
        bool moder, moded, modes;

        bool? dothing = null;
        private void button4_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left && e.Button != MouseButtons.Right) return;
            which = e.Button != MouseButtons.Left;
            dothing = true;
        }

        private void button4_MouseMove(object sender, MouseEventArgs e)
        {
            if (dothing != null)
            {
                var c = e.Location;
                if (c.X < 0 || c.Y < 0 || c.X > button4.Width || c.Y > button4.Height) { dothing = false; Console.WriteLine("left"); }
                else dothing = true;
            }
        }

        private void button4_MouseUp(object sender, MouseEventArgs e)
        {
            Console.WriteLine("up");
            Console.WriteLine(dothing);
            if (dothing == true && which) button4.PerformClick();
            which = false;
            dothing = null;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (!ModifierKeys.HasFlag(Keys.Shift) && !confirm("Fill entire map with selection?\n(hold shift to skip prompt)")) return;
            for (int y = 0; y < 25; y++)
                for (int x = 0; x < 30; x++)
                {
                    if (radioButtonColor.Checked) settile(x, y, !which ? primaryColor : secondaryColor);
                    else if (radioButtonRight.Checked) setexits(x, y, moder, mapexitdown[x, y]);
                    else if (radioButtonDown.Checked) setexits(x, y, mapexitright[x, y], moded);
                    else if (radioButtonSave.Checked) setsave(x, y, modes);
                }
            if (radioButtonRight.Checked) moder = !moder;
            else if (radioButtonDown.Checked) moded = !moded;
            else if (radioButtonSave.Checked) modes = !modes;
        }

        int lastx, lasty;

        private void checkBoxShowBosses_CheckedChanged(object sender, EventArgs e)
        {
            mapbosspb.Visible = checkBoxShowBosses.Checked;
            foreach (var (x, y) in bosscoords) setsave(x, y, mapsave[x, y], true);
        }

        readonly (int x, int y)[] bosscoords = new[]
        {//these coords start at 0x0
            (14, 10),
            (8, 9),
            (7, 8),
            (6, 4),
            (4, 13),
            (15, 11),
            (14, 13),
            (1, 20),
            (5, 12),
            (2, 2),
            (13, 23),
            (18, 15),
            (17, 11),
            (19, 0),
            (28, 3),
            (7, 15),
            (27, 18),
            (27, 13),
        };
        readonly Dictionary<(int sx, int sy), (int x, int y)> savecoords = new Dictionary<(int sx, int sy), (int x, int y)>()
        {//stage x and y also begin at 0
            { (16, 15), (165, 428) },//disappears at >difficult
            { (25, 22), (104, 158) },//same here
            { (0,  15), (75,  324) },
            { (0,  24), (120, 444) },
            { (1,  5),  (343, 444) },
            { (1,  7),  (240, 294) },
            { (1,  19), (375, 369) },
            { (1,  23), (90,  414) },
            { (2,  13), (150, 369) },
            { (4,  9),  (120, 398) },
            { (4,  17), (360, 128) },
            { (4,  20), (315, 218) },
            { (5,  11), (510, 128) },
            { (5,  12), (314, 444) },
            { (6,  7),  (165, 444) },
            { (6,  15), (374, 204) },
            { (7,  4),  (180, 174) },
            { (8,  23), (315, 248) },
            { (9,  0),  (120, 354) },
            { (9,  10), (180, 369) },
            { (9,  13), (269, 294) },
            { (9,  20), (510, 384) },
            { (10, 16), (300, 158) },
            { (10, 17), (90,  398) },
            { (12, 13), (210, 354) },
            { (12, 23), (269, 218) },
            { (14, 6),  (555, 218) },
            { (15, 9),  (374, 278) },
            { (16, 9),  (44,  414) },
            { (17, 5),  (570, 444) },
            { (18, 11), (90,  369) },
            { (19, 4),  (555, 294) },
            { (19, 10), (226, 308) },
            { (19, 15), (434, 398) },
            { (20, 0),  (106, 294) },
            { (21, 3),  (254, 188) },
            { (22, 13), (254, 204) },
            { (23, 2),  (300, 204) },
            { (25, 7),  (150, 324) },
            { (25, 23), (494, 264) },
            { (27, 3),  (240, 354) },
            { (27, 14), (300, 340) },
            { (27, 17), (210, 428) },
            { (27, 20), (270, 428) },
            { (27, 24), (46,  444) },
            { (28, 6),  (542, 308) },
            { (28, 23), (510, 234) },
            { (29, 2),  (300, 294) },
        };
        void updateBossIcon(int bossNum) => drawBossIcon(bossNum, bosscoords[bossNum].x, bosscoords[bossNum].y);
        void updateBossIcon(int x, int y)
        {
            var i = Array.FindIndex(bosscoords, z => z.x == x && z.y == y);
            if (i == -1) return;
            drawBossIcon(i, x, y);
        }

        bool havechanged = false;
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (havechanged) if (!confirm("You have unsaved changes. Are you sure you want to exit?")) { e.Cancel = true; return; }
            saveconfig();
        }
        bool confirm(string str) => MessageBox.Show(str, "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
        private void button5_Click(object sender, EventArgs e)
        {
            var dirty = false;
            for (int y = 0; y < 25; y++)
            {//why do i worry about possible performance hits in irrelevant places instead of where it actually ~~might be~~ is a problem...
                for (int x = 0; x < 30; x++)
                {
                    if (mapsave[x, y] || mapexitright[x, y] || mapexitdown[x, y] || maptiles[x, y] != -16777216) { dirty = true; break; }
                }
                if (dirty) break;
            }
            if (dirty) if (!confirm("Overwrite current map?")) return;
            var sav = SaveData.LoadFromString(Properties.Resources.fullmap);
            for (int x = 0; x < 30; x++)
                for (int y = 0; y < 25; y++)
                {
                    var c = Color.FromArgb(sav.Map[x, y]);
                    settile(x, y, Color.FromArgb(255, c.B, c.G, c.R));
                    setexits(x, y, sav.MapExitRight[x, y], sav.MapExitDown[x, y]);
                    setsave(x, y, sav.MapSave[x, y]);
                }
            if (difficultyCombobox.SelectedIndex < 2)
                foreach (var item in savecoords.Take(2))
                {
                    setsave(item.Key.sx, item.Key.sy, true);
                }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (!confirm("Load default save file?")) return;
            load(new SaveData().SaveToString());
            setRandomName();
            setmodified(false);
            currentfile = "";
            Text = "AUS Save File Editor";
        }

        void drawBossIcon(int bossNum, int x, int y)
        {
            //var killed = bossNum != 17 ? pab.First(z => bossNum.Equals(z.Tag)).Checked : false;//finalboss shows up too
            var killed = bossNum != 17 && pab.First(z => bossNum.Equals(z.Tag)).Checked;//finalboss shows up too
            mapbossg.SetClip(new Rectangle(x * 15, y * 15, 15, 15));
            mapbossg.Clear(Color.Transparent);
            mapbossg.ResetClip();
            mapbossg.DrawImage(killed ? Properties.Resources.mapIconDeadBoss : Properties.Resources.mapIconBoss, x * 15, y * 15);
            if (maptiles[x, y] == -16777216) mapbossg.FillRectangle(dimblackbrush, x * 15, y * 15, 15, 15);
            mapbosspb.Invalidate(new Rectangle(x * 15, y * 15, 15, 15));
        }

        private void panelMap_MouseMove(object sender, MouseEventArgs e)
        {
            int _x = e.X / 15, _y = e.Y / 15;
            if (_x < 0 || _y < 0 || _x > 29 || _y > 24) return;
            labelCoords.Text = $"{_x + 1}x{_y + 1}";
            if (!mousepressd) return;
            Utils.BresenhamsLine(lastx, lasty, _x, _y, (x, y) =>
            {
                if (x < 0 || y < 0 || x > 29 || y > 24) return;
                //if (e.Button != MouseButtons.Left && e.Button != MouseButtons.Right) return;
                //Console.WriteLine($"move at {e.Location}, corrected {x}x{y}");
                if (radioButtonColor.Checked) settile(x, y, !which ? primaryColor : secondaryColor);
                else if (radioButtonRight.Checked) setexits(x, y, moder, mapexitdown[x, y]);
                else if (radioButtonDown.Checked) setexits(x, y, mapexitright[x, y], moded);
                else if (radioButtonSave.Checked) setsave(x, y, modes);
            });
            lastx = _x; lasty = _y;
        }

        private void label66_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(((Label)sender).Text);
            System.Media.SystemSounds.Asterisk.Play();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() != DialogResult.OK) return;
            primaryColor = colorDialog1.Color;
            button1.BackColor = primaryColor;
            colorDialog2.CustomColors = colorDialog1.CustomColors;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (colorDialog2.ShowDialog() != DialogResult.OK) return;
            secondaryColor = colorDialog2.Color;
            button2.BackColor = secondaryColor;
            colorDialog1.CustomColors = colorDialog2.CustomColors;
        }

        private void button3_Click(object sender, EventArgs e)
        {//import img
            if (openFileDialog2.ShowDialog() != DialogResult.OK) return;
            Image img;
            try
            {
                img = Image.FromFile(openFileDialog2.FileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error loading image", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            /// <summary>
            /// Resize the image to the specified width and height.
            /// </summary>
            /// <param name="image">The image to resize.</param>
            /// <param name="width">The width to resize to.</param>
            /// <param name="height">The height to resize to.</param>
            /// <returns>The resized image.</returns>
            Bitmap ResizeImage(Image image, int width, int height)
            {
                var destRect = new Rectangle(0, 0, width, height);
                var destImage = new Bitmap(width, height);

                destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

                using (var graphics = Graphics.FromImage(destImage))
                {
                    graphics.CompositingMode = CompositingMode.SourceCopy;
                    graphics.CompositingQuality = CompositingQuality.HighQuality;
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.SmoothingMode = SmoothingMode.HighQuality;
                    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    //graphics.CompositingQuality = CompositingQuality.HighQuality;
                    //graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                    //graphics.SmoothingMode = SmoothingMode.HighQuality;
                    //graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                    using (var wrapMode = new ImageAttributes())
                    {
                        wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                        graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                    }
                }

                return destImage;
            }

            var h = ResizeImage(img, 30, 25);

            for (int x = 0; x < 30; x++)
                for (int y = 0; y < 25; y++)
                    settile(x, y, Color.FromArgb(255, h.GetPixel(x, y)));

            img.Dispose();
            h.Dispose();
        }
        public static class Utils
        {
            public static void BresenhamsLine(int x1, int y1, int x2, int y2, Action<int, int> place)
            {
                int width = x2 - x1;
                int height = y2 - y1;

                int dirX1 = (width < 0 ? -1 : (width > 0 ? 1 : 0)), dirX2 = dirX1;
                int dirY1 = (height < 0 ? -1 : (height > 0 ? 1 : 0)), dirY2 = dirY1;

                var longest = Math.Abs(width);
                var shortest = Math.Abs(height);
                if (!(longest > shortest))
                {
                    var tmp = shortest;
                    shortest = longest;
                    longest = tmp;

                    dirX2 = 0;
                }
                else
                {
                    dirY2 = 0;
                }

                var numerator = longest / 2;
                int x = x1, y = y1;
                for (var i = 0; i <= longest; i++)
                {
                    place(x, y);
                    numerator += shortest;

                    if (numerator < longest)
                    {
                        x += dirX2;
                        y += dirY2;
                    }
                    else
                    {
                        numerator -= longest;
                        x += dirX1;
                        y += dirY1;
                    }
                }
            }
        }
        /////END
        decimal totalheartsoldval = 0;
        private void numericHearts_ValueChanged(object sender, EventArgs e)
        {
            if (checkBoxSyncVars.Checked) numericMaxhp.Value += (numericHearts.Value - totalheartsoldval) * 10;
            totalheartsoldval = numericHearts.Value;
        }

        private void comboBoxControls_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbcsis) return;
            cbcsis = true;
            var index = comboBoxControls.SelectedIndex;
            numeric_other_kb.Value = index;
            //comboBoxControls.SelectedIndex = -1;
            cbcsis = false;
            if (index < 0 && index > 3) return;
            numeric_other_kb.Value = index;
            const int vk_up = 38, vk_space = 32, vk_enter = 13, ordz = 90, ordx = 88, ordc = 67;
            switch (index)
            {
                case 0:
                    numeric_other_kb_jump.Value = vk_up; numeric_other_kb_shoot.Value = ordz;
                    numeric_other_kb_action.Value = ordx; numeric_other_kb_pause.Value = vk_space;
                    break;
                case 1:
                    numeric_other_kb_jump.Value = ordz; numeric_other_kb_shoot.Value = ordx;
                    numeric_other_kb_action.Value = ordc; numeric_other_kb_pause.Value = vk_space;
                    break;
                case 2:
                    numeric_other_kb_jump.Value = vk_space; numeric_other_kb_shoot.Value = ordz;
                    numeric_other_kb_action.Value = ordx; numeric_other_kb_pause.Value = vk_enter;
                    break;
                case 3:
                    numeric_other_kb_jump.Value = vk_up; numeric_other_kb_shoot.Value = vk_space;
                    numeric_other_kb_action.Value = ordz; numeric_other_kb_pause.Value = vk_enter;
                    break;
            }
        }

        static Form1()
        {
            //Ability stuff
            ab[1 - 1] = "Jump Upgrade 1";
            ab_d[1 - 1] = "Increases height of your jump!";
            ab[2 - 1] = "Jump Upgrade 2";
            ab_d[2 - 1] = "Increases height of your jump!";
            ab[3 - 1] = "Jump Upgrade 3";
            ab_d[3 - 1] = "Increases height of your jump!";
            ab[4 - 1] = "Energy Jump 1";
            ab_d[4 - 1] = "Grab red energy balls for a boost!";
            ab[5 - 1] = "Lucky Pots";
            ab_d[5 - 1] = "More crystals are found when you break pots!";
            ab[6 - 1] = "Double Jump";
            ab_d[6 - 1] = "Jump again in mid-air!";
            ab[7 - 1] = "Double Jump Upgrade 1";
            ab_d[7 - 1] = "Increases height of your double jump!";
            ab[8 - 1] = "Double Jump Upgrade 2";
            ab_d[8 - 1] = "Increases height of your double jump!";
            ab[9 - 1] = "Ducking";
            ab_d[9 - 1] = "You can duck by holding DOWN!";
            ab[10 - 1] = "sticking";
            ab_d[10 - 1] = "You can stick to ceilings by holding JUMP!";
            ab[11 - 1] = "stick slide";
            ab_d[11 - 1] = "You can move left and right while stuck to a ceiling!";
            ab[12 - 1] = "Teleport";
            ab_d[12 - 1] = "Press ACTION at save points to teleport!";
            ab[13 - 1] = "Dive Bomb";
            ab_d[13 - 1] = "Hold DOWN in mid-air to dive bomb!";
            ab[14 - 1] = "shoot Fire";
            ab_d[14 - 1] = "Press sHOOT to shoot fire!";
            ab[15 - 1] = "Long shots";
            ab_d[15 - 1] = "Your shots travel much farther!";
            ab[16 - 1] = "Energy Jump 2";
            ab_d[16 - 1] = "Grab yellow energy balls for a boost!";
            ab[17 - 1] = "Hatch";
            ab_d[17 - 1] = "Your egg has hatched!";
            ab[18 - 1] = "Air Upgrade 1";
            ab_d[18 - 1] = "You can breathe longer underwater!";
            ab[19 - 1] = "Air Upgrade 2";
            ab_d[19 - 1] = "You can breathe longer underwater!";
            ab[20 - 1] = "Magnetism";
            ab_d[20 - 1] = "Crystals are magnetically attracted to you!";
            ab[21 - 1] = "shoot Ice";
            ab_d[21 - 1] = "Hold sHOOT to charge, then release to shoot ice!";
            ab[22 - 1] = "Toughness 1";
            ab_d[22 - 1] = "You take less damage!";
            ab[23 - 1] = "Toughness 2";
            ab_d[23 - 1] = "You take less damage!";
            ab[24 - 1] = "Toughness 3";
            ab_d[24 - 1] = "You take less damage!";

            //Hearts
            h[1 - 1] = "Crossed the spike pit in DeepTower.";
            h[2 - 1] = "Reached the branch in ColdKeep.";
            h[3 - 1] = "Lit the lanterns in NightWalk.";
            h[4 - 1] = "Crossed the big spike pit in skyTown.";
            h[5 - 1] = "Found on the way up NightClimb.";
            h[6 - 1] = "Found behind the cannons in NightClimb.";
            h[7 - 1] = "Found the secret passage in Grotto.";
            h[8 - 1] = "scaled NightClimb.";
            h[9 - 1] = "Got on top of the lookout tower in skyTown.";
            h[10 - 1] = "Used the red energy in skyTown.";
            h[11 - 1] = "Used the yellow energy in skyTown.";
            h[12 - 1] = "Bought from the shop in skyTown.";
            h[13 - 1] = "Bought from the shop in skyTown.";
            h[14 - 1] = "Bought from the shop in skyTown.";
            h[15 - 1] = "Bought from the shop in skyTown.";
            h[16 - 1] = "Bought from the shop in skyTown.";
            h[17 - 1] = "Climbed the clouds in NightWalk.";
            h[18 - 1] = "Defeated the flying enemies in FarFall.";
            h[19 - 1] = "Found the secret passage in stoneCastle.";
            h[20 - 1] = "Dive bombed the grass in NightWalk.";
            h[21 - 1] = "Used the yellow energy in FarFall.";
            h[22 - 1] = "Reached the bottom of the pit in FarFall.";
            h[23 - 1] = "Unlocked the first heart door in FireCage.";
            h[24 - 1] = "Passed the crushers in FireCage.";
            h[25 - 1] = "Found the secret passage in FireCage.";
            h[26 - 1] = "Ducked between the statues in NightClimb.";
            h[27 - 1] = "Jumped up the shaft in FireCage.";
            h[28 - 1] = "Unlocked the second heart door in FireCage.";
            h[29 - 1] = "slid along the roof in FireCage.";
            h[30 - 1] = "Popped the special balloon in FarFall.";
            h[31 - 1] = "Reached the Bottom";
            h[32 - 1] = "Froze the cannon in ColdKeep.";
            h[33 - 1] = "Avoided a firey death in stoneCastle.";
            h[34 - 1] = "Popped the balloons clockwise in FarFall.";
            h[35 - 1] = "Won in the game in Bonus.";
            h[36 - 1] = "Climbed the frozen fish in DeepDive.";
            h[37 - 1] = "Unlocked the first heart door in skysand.";
            h[38 - 1] = "Found the secret passage in skysand.";
            h[39 - 1] = "Used the yellow energy in skysand.";
            h[40 - 1] = "Unlocked the second heart door in skysand.";
            h[41 - 1] = "slid along the bottom of the cloud in CloudRun.";
            h[42 - 1] = "Got halfway through CloudRun.";
            h[43 - 1] = "Jumped past the end in CloudRun.";
            h[44 - 1] = "Found by the wreckage in DeepDive.";
            h[45 - 1] = "Descended in DeepDive.";
            h[46 - 1] = "Found by the temple in NightWalk.";
            h[47 - 1] = "Opened the treasure chest in FarFall.";
            h[48 - 1] = "Opened the treasure chest in skysand.";
            h[49 - 1] = "Opened the treasure chest in DeepDive.";
            h[50 - 1] = "Opened the treasure chest in NightClimb.";
            h[51 - 1] = "Explored the ss Eternity in DeepDive.";
            h[52 - 1] = "Got the top score in AstroCrash.";
            h[53 - 1] = "Beat the boss in Grotto.";
            h[54 - 1] = "Got to the top of the biggest chamber in Dark Grotto.";
            h[55 - 1] = "Found the secret passage near the camp in Dark Grotto.";
            h[56 - 1] = "Relit the lanterns in Dark Grotto.";
            h[57 - 1] = "Jumped off the cliff in HighLands.";
            h[58 - 1] = "Pulled off some tricky jumps in NightWalk.";
            h[59 - 1] = "smashed the floor in The Curtain.";
            h[60 - 1] = "Opened the treasure chest in NightWalk.";
            h[61 - 1] = "Defeated the flying enemies in The Curtain.";
            h[62 - 1] = "Got the top score in JumpBox.";
            h[63 - 1] = "Got the top score in Keep Going!!";
            h[64 - 1] = "Froze the fish in DeepDive.";
            h[65 - 1] = "Jumped into the platform in DeepDive.";
            h[66 - 1] = "Fell on the right side of the pit in IceCastle.";
            h[67 - 1] = "Found a secret passage in IceCastle.";
            h[68 - 1] = "Opened the first heart door in IceCastle.";
            h[69 - 1] = "Opened the second heart door in IceCastle.";
            h[70 - 1] = "Opened the third heart door in IceCastle.";
            h[71 - 1] = "Opened the fourth heart door in IceCastle.";
            h[72 - 1] = "Found a secret passage in IceCastle.";
            h[73 - 1] = "Returned 10 ghostly flowers in staircase.";
            h[74 - 1] = "Returned 20 ghostly flowers in staircase.";
            h[75 - 1] = "Beat the easy score in RainbowDive.";
            h[76 - 1] = "Beat the hard score in RainbowDive.";
            h[77 - 1] = "Crossed the bottom in IceCastle.";
            h[78 - 1] = "Slid along the bottom of IceCastle.";
            h[79 - 1] = "Opened the treasure chest in skyLands.";
            h[80 - 1] = "Copied the statues in skyLands.";
            h[81 - 1] = "Popped the twelve balloons in skyLands.";
            h[82 - 1] = "Warped into skyLands.";
            h[83 - 1] = "Browsed the books in Library.";
            h[84 - 1] = "Jumped through the spike trap in DeepDive.";
            h[85 - 1] = "Jumped through another spike trap in DeepDive.";
            h[86 - 1] = "Shot all the triangles in BlancLand.";
            h[87 - 1] = "Froze the triangles in BlancLand.";
            h[88 - 1] = "Paid the toll in skyLands.";
            h[89 - 1] = "Found the really secret passage in IceCastle.";
            h[90 - 1] = "Found the hidden chamber in BlackCastle.";
            h[91 - 1] = "Opened the heart door in strangeCastle.";
            h[92 - 1] = "Got through the spike maze in skyLands.";
            h[93 - 1] = "Opened the right heart door in UnderTomb.";
            h[94 - 1] = "Got through the left path in UnderTomb.";
            h[95 - 1] = "Opened the left heart door in UnderTomb.";

            string fixCase(string str)
            {
                StringBuilder sb = new StringBuilder(str);
                sb[0] = char.ToUpperInvariant(sb[0]);
                var words = str.Split(' ');
                int n = words[0].Length + 1;
                for (int i = 1; i < words.Length; i++)
                {
                    var word = words[i];
                    if (char.IsLower(word[0]) && word.Any(x => char.IsUpper(x))) sb[n] = char.ToUpperInvariant(word[0]);
                    n += word.Length + 1;
                }
                return sb.ToString();
            }
            for (int i = 0; i < ab.Length; i++) ab[i] = fixCase(ab[i]);
            for (int i = 0; i < ab_d.Length; i++) ab_d[i] = fixCase(ab_d[i]);
            for (int i = 0; i < h.Length; i++) h[i] = fixCase(h[i]);
        }
        bool egg = false;
    }
    static class Extensions
    {
        public static IEnumerable<T> GetAll<T>(this Control control) => GetAll(control, typeof(T)).Cast<T>();
        public static IEnumerable<Control> GetAll(this Control control, Type type)
        {
            var controls = control.Controls.Cast<Control>();

            return controls.SelectMany(ctrl => GetAll(ctrl, type))
                                      .Concat(controls)
                                      .Where(c => c.GetType() == type);
        }
        public static IEnumerable<Control> GetAll(this Control control)
        {
            var controls = control.Controls.Cast<Control>();

            return controls.SelectMany(ctrl => GetAll(ctrl))
                                      .Concat(controls);
        }
    }
}
