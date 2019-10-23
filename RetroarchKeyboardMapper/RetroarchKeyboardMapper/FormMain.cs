using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace RetroarchKeyboardMapper
{
    public partial class FormMain : Form
    {
        int A2DSelected => menuItemA2DLeft.Checked ? 1 : (menuItemA2DRight.Checked ? 2 : 0);

        Dictionary<Button, Button> buttons;
        Button selectedButton;

        public FormMain()
        {
            InitializeComponent();
            Icon = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);
            buttons = new Dictionary<Button, Button>();
            Reset();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            panelEdit.BackColor = panelEdit.BackColor == Color.Red ? Color.White : Color.Red;
        }


        // CLICK //


        void DiselectButton()
        {
            selectedButton = null;
            panelEdit.Location = new Point(-100, -100);
        }

        void SelectButton(Button button)
        {
            selectedButton = button;
            panelEdit.Location = new Point(selectedButton.Location.X - 6, selectedButton.Location.Y - 3);
            panelEdit.Width = button.Width < 40 ? 46 : 86;
        }

        private void panelBackgroundTop_Click(object sender, EventArgs e)
        {
            DiselectButton();
        }

        private void button_Click(object sender, EventArgs e)
        {
            if (selectedButton != (Button)sender)
            {
                SelectButton((Button)sender);
                if (buttons[selectedButton] != null)
                    buttons[selectedButton].BackColor = selectedButton.BackColor;
            }
            else
                DiselectButton();
        }

        private void key_Click(object sender, EventArgs e)
        {
            ActiveControl = selectedButton ?? (Control)panelBackgroundTop;
            if (selectedButton == null)
                return;
            if (((Button)sender).BackColor != SystemColors.Control)
            {
                if (buttons[selectedButton] != null)
                    buttons[selectedButton].BackColor = SystemColors.Control;
                buttons[selectedButton] = null;
                return;
            }
            if (buttons[selectedButton] != null)
                buttons[selectedButton].BackColor = SystemColors.Control;
            buttons[selectedButton] = (Button)sender;
            buttons[selectedButton].BackColor = selectedButton.BackColor;
            DiselectButton();
        }


        // RESET //


        void Reset()
        {
            if (selectedButton != null)
            {
                if (buttons[selectedButton] != null)
                    buttons[selectedButton].BackColor = SystemColors.Control;
            }
            foreach (Control control in Controls)
                if (control is Button && control.Tag != null)
                    control.BackColor = SystemColors.Control;
            DiselectButton();
            buttons[buttonDown] = null;
            buttons[buttonUp] = null;
            buttons[buttonLeft] = null;
            buttons[buttonRight] = null;
            buttons[buttonSE] = null;
            buttons[buttonST] = null;
            buttons[buttonLB] = null;
            buttons[buttonLT] = null;
            buttons[buttonL3] = null;
            buttons[buttonRB] = null;
            buttons[buttonRT] = null;
            buttons[buttonR3] = null;
            buttons[buttonA] = null;
            buttons[buttonB] = null;
            buttons[buttonX] = null;
            buttons[buttonY] = null;
            A2DSelect(menuItemA2DNone);
        }

        private void menuItemReset_Click(object sender, EventArgs e)
        {
            Reset();
        }


        // LOAD //


        Button FindButtonByTag(int tag)
        {
            foreach (Control control in Controls)
                if (control is Button && control.Tag.ToString() == tag.ToString())
                    return (Button)control;
            return null;
        }

        void ReadLine(string line, Button button, string buttonString)
        {
            try
            {
                if (line.Contains("input_player1_key_" + buttonString + " "))
                {
                    buttons[button] = FindButtonByTag(int.Parse(line.Split('=')[1].Replace(" ", "").Replace("\"", "")));
                    buttons[button].BackColor = button.BackColor;
                }
            }
            catch { }
        }

        private void menuItemLoad_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Remap file (*.rmp)|*.rmp|All files (*.*)|*.*";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                Reset();
                string[] lines = File.ReadAllLines(dialog.FileName);
                foreach (string line in lines)
                {
                    try
                    {
                        if (line.Contains("input_player1_analog_dpad_mode"))
                        {
                            switch (int.Parse(line.Split('=')[1].Replace(" ", "").Replace("\"", "")))
                            {
                                case 0: A2DSelect(menuItemA2DNone); break;
                                case 1: A2DSelect(menuItemA2DLeft); break;
                                case 2: A2DSelect(menuItemA2DRight); break;
                            }
                        }
                    }
                    catch { }
                    ReadLine(line, buttonLeft, "left");
                    ReadLine(line, buttonRight, "right");
                    ReadLine(line, buttonUp, "up");
                    ReadLine(line, buttonDown, "down");
                    ReadLine(line, buttonA, "a");
                    ReadLine(line, buttonB, "b");
                    ReadLine(line, buttonX, "x");
                    ReadLine(line, buttonY, "y");
                    ReadLine(line, buttonSE, "select");
                    ReadLine(line, buttonST, "start");
                    ReadLine(line, buttonLB, "l");
                    ReadLine(line, buttonLT, "l2");
                    ReadLine(line, buttonL3, "l3");
                    ReadLine(line, buttonRB, "r");
                    ReadLine(line, buttonRT, "r2");
                    ReadLine(line, buttonR3, "r3");
                }
            }
        }


        // SAVE //


        void WriteLine(List<string> lines, Button button, string buttonString)
        {
            if (buttons[button] != null)
                lines.Add("input_player1_key_" + buttonString + " = \"" + buttons[button].Tag + "\"");
        }

        private void menuItemSave_Click(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Remap file (*.rmp)|*.rmp";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                List<string> lines = new List<string>();
                lines.Add("input_libretro_device_p1 = \"3\"");
                lines.Add("input_player1_analog_dpad_mode = \"" + A2DSelected + "\"");
                WriteLine(lines, buttonLeft, "left");
                WriteLine(lines, buttonRight, "right");
                WriteLine(lines, buttonUp, "up");
                WriteLine(lines, buttonDown, "down");
                WriteLine(lines, buttonA, "a");
                WriteLine(lines, buttonB, "b");
                WriteLine(lines, buttonX, "x");
                WriteLine(lines, buttonY, "y");
                WriteLine(lines, buttonSE, "select");
                WriteLine(lines, buttonST, "start");
                WriteLine(lines, buttonLB, "l");
                WriteLine(lines, buttonLT, "l2");
                WriteLine(lines, buttonL3, "l3");
                WriteLine(lines, buttonRB, "r");
                WriteLine(lines, buttonRT, "r2");
                WriteLine(lines, buttonR3, "r3");
                File.WriteAllLines(dialog.FileName, lines.ToArray());
            }
        }


        // ANALOG TO DIGITAL TYPE //


        void A2DSelect(MenuItem menuItem)
        {
            menuItemA2DNone.Checked = false;
            menuItemA2DLeft.Checked = false;
            menuItemA2DRight.Checked = false;
            menuItem.Checked = true;
            labelA2D.Text = (string)menuItem.Tag;
        }

        private void menuItemA2D_Click(object sender, EventArgs e)
        {
            A2DSelect((MenuItem)sender);
        }


        // OTHER //


        private void menuItemExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void menuItemHow_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Create and edit RetroArch keyboard remap files easily using this program. Remaps are configuration files for the Controls menu inside Quick Menu.\nPlace your remap file to \\config\\remaps\\<core>\\<game>.rmp\n\nGamepad (top half of window): Select a button you wish to remap by clicking on it. Click again to deselect it.\n\nKeyboard (bottom half of window): After you selected a gamepad button, click on a keyboard key. It will change color, showing you which button you assigned it to. To remove the connection, select the same button-key combination again.\n\nFile menu: Create, load or save remap file.\nOptions menu: Choose analog to digital method for this user device. This program only works for the first user device.", "How to use");
        }

        private void menuItemAbout_Click(object sender, EventArgs e)
        {
            MessageBox.Show("RetroArch Keyboard Mapper 1.0\n© 2019 SeriousCsaba", "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
