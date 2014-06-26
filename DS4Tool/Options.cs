﻿using System;
using System.Drawing;
using System.Windows.Forms;
using DS4Library;
using DS4Control;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
namespace ScpServer
{
    public partial class Options : Form
    {
        private DS4Control.Control scpDevice;
        public int device;
        public string filename;
        Byte[] oldLedColor, oldLowLedColor, oldChargingColor;
        public Timer inputtimer = new Timer(), sixaxisTimer = new Timer();
        public List<Button> buttons = new List<Button>();
        private Button lastSelected;
        private int alphacolor;
        private Color reg, full;
        private Image colored, greyscale;
        ToolTip tp = new ToolTip();
        Graphics g;
        ScpForm root;
        public Options(DS4Control.Control bus_device, int deviceNum, string name, ScpForm rt)
        {
            InitializeComponent();
            device = deviceNum;
            scpDevice = bus_device;
            filename = name;
            colored = pBRainbow.Image;
            root = rt;
            g = CreateGraphics();
            greyscale = GreyscaleImage((Bitmap)pBRainbow.Image);
            if (deviceNum < 4)
            nUDSixaxis.Value = deviceNum + 1;
            if (filename != "")
            {
                Global.setAProfile(4, name);
                Global.LoadProfile(deviceNum);
                DS4Color color = Global.loadColor(device);
                redBar.Value = color.red;
                greenBar.Value = color.green;
                blueBar.Value = color.blue;
                
                cBLightbyBattery.Checked = Global.getLedAsBatteryIndicator(device);
                nUDflashLED.Value = Global.getFlashAt(device);
                lowBatteryPanel.Visible = cBLightbyBattery.Checked;
                lbFull.Text = (cBLightbyBattery.Checked ? "Full:" : "Color:");
                FullPanel.Location = (cBLightbyBattery.Checked ? new Point(FullPanel.Location.X, 42) : new Point(FullPanel.Location.X, 48));

                DS4Color lowColor = Global.loadLowColor(device);
                lowRedBar.Value = lowColor.red;
                lowGreenBar.Value = lowColor.green;
                lowBlueBar.Value = lowColor.blue;

                DS4Color cColor = Global.loadChargingColor(device);
                btnChargingColor.BackColor = Color.FromArgb(cColor.red, cColor.green, cColor.blue);
                nUDRumbleBoost.Value = Global.loadRumbleBoost(device);
                nUDTouch.Value = Global.getTouchSensitivity(device);
                cBSlide.Checked = Global.getTouchSensitivity(device) > 0;
                nUDScroll.Value = Global.getScrollSensitivity(device);
                cBScroll.Checked = Global.getScrollSensitivity(device) >0;
                nUDTap.Value = Global.getTapSensitivity(device);
                cBTap.Checked = Global.getTapSensitivity(device) > 0;
                cBDoubleTap.Checked = Global.getDoubleTap(device);
                nUDL2.Value = (decimal)Global.getLeftTriggerMiddle(device)/255;
                nUDR2.Value = (decimal)Global.getRightTriggerMiddle(device)/255;
                cBTouchpadJitterCompensation.Checked = Global.getTouchpadJitterCompensation(device);
                cBlowerRCOn.Checked = Global.getLowerRCOn(device);
                cBFlushHIDQueue.Checked = Global.getFlushHIDQueue(device);
                nUDIdleDisconnect.Value = Math.Round((decimal)(Global.getIdleDisconnectTimeout(device) / 60d), 1);
                cBIdleDisconnect.Checked = Global.getIdleDisconnectTimeout(device) > 0;
                numUDMouseSens.Value = Global.getButtonMouseSensitivity(device);
                // Force update of color choosers    
                alphacolor = Math.Max(redBar.Value, Math.Max(greenBar.Value, blueBar.Value));
                reg = Color.FromArgb(color.red, color.green, color.blue);
                full = HuetoRGB(reg.GetHue(), reg.GetBrightness(), reg);
                pBController.BackColor = Color.FromArgb((alphacolor > 205 ? 255 : (alphacolor + 50)), full);

                alphacolor = Math.Max(lowRedBar.Value, Math.Max(greenBar.Value, blueBar.Value));
                reg = Color.FromArgb(lowColor.red, lowColor.green, lowColor.blue);
                full = HuetoRGB(reg.GetHue(), reg.GetBrightness(), reg);
                lowColorChooserButton.BackColor = Color.FromArgb((alphacolor > 205 ? 255 : (alphacolor + 50)), full);
                nUDRainbow.Value = (decimal)Global.getRainbow(device);
                switch (Global.getChargingType(deviceNum))
                {
                    case 1: rBFade.Checked = true; break;
                    case 2: rBRainbow.Checked = true; break;
                    case 3: rBColor.Checked = true; break;
                    default: rBNormal.Checked = true; break;
                }
                if (Global.getRainbow(device) == 0)
                {
                    pBRainbow.Image = greyscale;
                    ToggleRainbow(false);
                }
                else
                {
                    pBRainbow.Image = colored;
                    ToggleRainbow(true);
                }
                nUDLS.Value = Math.Round((decimal)(Global.getLSDeadzone(device) / 127d ), 3);
                nUDRS.Value = Math.Round((decimal)(Global.getRSDeadzone(device) / 127d ), 3);
                nUDSX.Value = (decimal)Global.getSXDeadzone(device);
                nUDSZ.Value = (decimal)Global.getSZDeadzone(device);
            }
            else
                Set();
            foreach (System.Windows.Forms.Control control in MainPanel.Controls)
                if (control is Button)
                    if (!((Button)control).Name.Contains("btn"))
                        buttons.Add((Button)control);
            foreach (System.Windows.Forms.Control control in SticksPanel.Controls)
                if (control is Button)
                    if (!((Button)control).Name.Contains("btn"))
                        buttons.Add((Button)control);
            foreach (System.Windows.Forms.Control control in fLPTiltControls.Controls)
                if (control is Button)
                    if (!((Button)control).Name.Contains("btn"))
                        buttons.Add((Button)control);
            foreach (Button b in buttons)
                b.MouseHover += button_MouseHover;
            if (filename != "" && filename != "New Profile")
                Global.LoadProfile(device, buttons.ToArray());
            tp.SetToolTip(cBlowerRCOn, "Best used with right side as a mouse function");
            tp.SetToolTip(cBDoubleTap, "Tap and hold to drag, slight delay with single taps");
            tp.SetToolTip(lBControlTip, "You can also use your controller to change controls");
            tp.SetToolTip(cBTouchpadJitterCompensation, "Use Sixaxis to help calculate touchpad movement");
            tp.SetToolTip(pBRainbow, "Always on Rainbow Mode");
            tp.SetToolTip(cBFlushHIDQueue, "Flush HID Queue after each reading");
            tp.SetToolTip(cBLightbyBattery, "Also dim light by idle timeout if on");
            tp.SetToolTip(lB6Gryo, "Click to see readout of Sixaxis Gyro");
            tp.SetToolTip(tBsixaxisGyroX, "GyroX, Left and Right Tilt");
            tp.SetToolTip(tBsixaxisGyroY, "GyroY, Forward and Back Tilt");
            tp.SetToolTip(tBsixaxisGyroZ, "GyroZ, Up and Down Tilt");
            tp.SetToolTip(tBsixaxisAccelX, "AccelX");
            tp.SetToolTip(tBsixaxisAccelY, "AccelY");
            tp.SetToolTip(tBsixaxisAccelZ, "AccelZ");
            advColorDialog.OnUpdateColor += advColorDialog_OnUpdateColor;
            btnLeftStick.Enter += btnSticks_Enter;
            btnRightStick.Enter += btnSticks_Enter;
            UpdateLists();
            inputtimer.Start();
            inputtimer.Tick += InputDS4;
            sixaxisTimer.Tick += sixaxisTimer_Tick;
            sixaxisTimer.Interval = 1000 / 60;
        }

        void sixaxisTimer_Tick(object sender, EventArgs e)
        {
            // MEMS gyro data is all calibrated to roughly -1G..1G for values -0x2000..0x1fff
            // Enough additional acceleration and we are no longer mostly measuring Earth's gravity...
            // We should try to indicate setpoints of the calibration when exposing this measurement....
            SetDynamicTrackBarValue(tBsixaxisGyroX, (scpDevice.ExposedState[(int)nUDSixaxis.Value - 1].GyroX + tBsixaxisGyroX.Value * 2) / 3);
            SetDynamicTrackBarValue(tBsixaxisGyroY, (scpDevice.ExposedState[(int)nUDSixaxis.Value - 1].GyroY + tBsixaxisGyroY.Value * 2) / 3);
            SetDynamicTrackBarValue(tBsixaxisGyroZ, (scpDevice.ExposedState[(int)nUDSixaxis.Value - 1].GyroZ + tBsixaxisGyroZ.Value * 2) / 3);
            SetDynamicTrackBarValue(tBsixaxisAccelX, (scpDevice.ExposedState[(int)nUDSixaxis.Value - 1].AccelX + tBsixaxisAccelX.Value * 2) / 3);
            SetDynamicTrackBarValue(tBsixaxisAccelY, (scpDevice.ExposedState[(int)nUDSixaxis.Value - 1].AccelY + tBsixaxisAccelY.Value * 2) / 3);
            SetDynamicTrackBarValue(tBsixaxisAccelZ, (scpDevice.ExposedState[(int)nUDSixaxis.Value - 1].AccelZ + tBsixaxisAccelZ.Value * 2) / 3);
        }
        private void InputDS4(object sender, EventArgs e)
        {
            #region DS4Input
            if (Form.ActiveForm == root && cBControllerInput.Checked)
            switch (scpDevice.GetInputkeys((int)nUDSixaxis.Value - 1))
                {
                    case ("Cross"): Show_ControlsBn(bnCross, e); break;
                    case ("Circle"): Show_ControlsBn(bnCircle, e); break;
                    case ("Square"): Show_ControlsBn(bnSquare, e); break;
                    case ("Triangle"): Show_ControlsBn(bnTriangle, e); break;
                    case ("Options"): Show_ControlsBn(bnOptions, e); break;
                    case ("Share"): Show_ControlsBn(bnShare, e); break;
                    case ("Up"): Show_ControlsBn(bnUp, e); break;
                    case ("Down"): Show_ControlsBn(bnDown, e); break;
                    case ("Left"): Show_ControlsBn(bnLeft, e); break;
                    case ("Right"): Show_ControlsBn(bnRight, e); break;
                    case ("PS"): Show_ControlsBn(bnPS, e); break;
                    case ("L1"): Show_ControlsBn(bnL1, e); break;
                    case ("R1"): Show_ControlsBn(bnR1, e); break;
                    case ("L2"): Show_ControlsBn(bnL2, e); break;
                    case ("R2"): Show_ControlsBn(bnR2, e); break;
                    case ("L3"): Show_ControlsBn(bnL3, e); break;
                    case ("R3"): Show_ControlsBn(bnR3, e); break;
                    case ("Touch Left"): Show_ControlsBn(bnTouchLeft, e); break;
                    case ("Touch Right"): Show_ControlsBn(bnTouchRight, e); break;
                    case ("Touch Multi"): Show_ControlsBn(bnTouchMulti, e); break;
                    case ("Touch Upper"): Show_ControlsBn(bnTouchUpper, e); break;
                    case ("LS Up"): Show_ControlsBn(bnLSUp, e); break;
                    case ("LS Down"): Show_ControlsBn(bnLSDown, e); break;
                    case ("LS Left"): Show_ControlsBn(bnLSLeft, e); break;
                    case ("LS Right"): Show_ControlsBn(bnLSRight, e); break;
                    case ("RS Up"): Show_ControlsBn(bnRSUp, e); break;
                    case ("RS Down"): Show_ControlsBn(bnRSDown, e); break;
                    case ("RS Left"): Show_ControlsBn(bnRSLeft, e); break;
                    case ("RS Right"): Show_ControlsBn(bnRSRight, e); break;
                    case ("GyroXP"): Show_ControlsBn(bnGyroXP, e); break;
                    case ("GyroXN"): Show_ControlsBn(bnGyroXN, e); break;
                    case ("GyroZP"): Show_ControlsBn(bnGyroZP, e); break;
                    case ("GyroZN"): Show_ControlsBn(bnGyroZN, e); break;
                }
            #endregion
        }
        private void button_MouseHover(object sender, EventArgs e)
        {
            switch (((Button)sender).Name)
            {
                #region
                case ("bnCross"): lBControls.SelectedIndex = 0; break;
                case ("bnCircle"): lBControls.SelectedIndex = 1; break;
                case ("bnSquare"): lBControls.SelectedIndex = 2; break;
                case ("bnTriangle"): lBControls.SelectedIndex = 3; break;
                case ("bnOptions"): lBControls.SelectedIndex = 4; break;
                case ("bnShare"): lBControls.SelectedIndex = 5; break;
                case ("bnUp"): lBControls.SelectedIndex = 6; break;
                case ("bnDown"): lBControls.SelectedIndex = 7; break;
                case ("bnLeft"): lBControls.SelectedIndex = 8; break;
                case ("bnRight"): lBControls.SelectedIndex = 9; break;
                case ("bnPS"): lBControls.SelectedIndex = 10; break;
                case ("bnL1"): lBControls.SelectedIndex = 11; break;
                case ("bnR1"): lBControls.SelectedIndex = 12; break;
                case ("bnL2"): lBControls.SelectedIndex = 13; break;
                case ("bnR2"): lBControls.SelectedIndex = 14; break;
                case ("bnL3"): lBControls.SelectedIndex = 15; break;
                case ("bnR3"): lBControls.SelectedIndex = 16; break;
                case ("bnTouchLeft"): lBControls.SelectedIndex = 17; break;
                case ("bnTouchRight"): lBControls.SelectedIndex = 18; break;
                case ("bnTouchMulti"): lBControls.SelectedIndex = 19; break;
                case ("bnTouchUpper"): lBControls.SelectedIndex = 20; break;
                case ("bnLSUp"): lBControls.SelectedIndex = 21; break;
                case ("bnLSDown"): lBControls.SelectedIndex = 22; break;
                case ("bnLSLeft"): lBControls.SelectedIndex = 23; break;
                case ("bnLSRight"): lBControls.SelectedIndex = 24; break;
                case ("bnRSUp"): lBControls.SelectedIndex = 25; break;
                case ("bnRSDown"): lBControls.SelectedIndex = 26; break;
                case ("bnRSLeft"): lBControls.SelectedIndex = 27; break;
                case ("bnRSRight"): lBControls.SelectedIndex = 28; break;
                case ("bnGyroZN"): lBControls.SelectedIndex = 29; break;
                case ("bnGyroZP"): lBControls.SelectedIndex = 30; break;
                case ("bnGyroXP"): lBControls.SelectedIndex = 31; break;
                case ("bnGyroXN"): lBControls.SelectedIndex = 32; break;

                #endregion
            }
        }

        private void SetDynamicTrackBarValue(TrackBar trackBar, int value)
        {
            if (trackBar.Maximum < value)
                trackBar.Maximum = value;
            else if (trackBar.Minimum > value)
                trackBar.Minimum = value;
            trackBar.Value = value;
        }

        public void Set()
        {
            lowBatteryPanel.Visible = cBLightbyBattery.Checked;
            lbFull.Text = (cBLightbyBattery.Checked ? "Full:" : "Color:");
            FullPanel.Location = (cBLightbyBattery.Checked ? new Point(FullPanel.Location.X, 42) : new Point(FullPanel.Location.X, 48));
            Global.saveColor(device, (byte)redBar.Value, (byte)greenBar.Value, (byte)blueBar.Value);
            Global.saveLowColor(device, (byte)lowRedBar.Value, (byte)lowGreenBar.Value, (byte)lowBlueBar.Value);
            Global.setLeftTriggerMiddle(device, (byte)Math.Round((nUDL2.Value * 255), 0));
            Global.setRightTriggerMiddle(device, (byte)Math.Round((nUDR2.Value * 255), 0));
            Global.saveRumbleBoost(device, (byte)nUDRumbleBoost.Value);
            Global.setTouchSensitivity(device, (byte)nUDTouch.Value);
            Global.setTouchpadJitterCompensation(device, cBTouchpadJitterCompensation.Checked);
            Global.setLowerRCOn(device, cBlowerRCOn.Checked);
            Global.setScrollSensitivity(device, (byte)nUDScroll.Value);
            Global.setDoubleTap(device, cBDoubleTap.Checked);
            Global.setTapSensitivity(device, (byte)nUDTap.Value);
            Global.setIdleDisconnectTimeout(device, (int)(nUDIdleDisconnect.Value * 60));
            Global.setRainbow(device, (int)nUDRainbow.Value);
            Global.setRSDeadzone(device, (byte)Math.Round((nUDRS.Value * 127), 0));
            Global.setLSDeadzone(device, (byte)Math.Round((nUDLS.Value * 127), 0));
            Global.setButtonMouseSensitivity(device, (int)numUDMouseSens.Value);
            Global.setFlashAt(device, (int)nUDflashLED.Value);
            Global.setSXDeadzone(device, (double)nUDSX.Value);
            Global.setSZDeadzone(device, (double)nUDSZ.Value);
            Global.setMouseAccel(device, cBMouseAccel.Checked);
            if (nUDRainbow.Value == 0) pBRainbow.Image = greyscale;
            else pBRainbow.Image = colored;
        }

        KBM360 kbm360 = null;

        private void Show_ControlsBn(object sender, EventArgs e)
        {
            lastSelected = (Button)sender;
            kbm360 = new KBM360(scpDevice, device, this, lastSelected);
            kbm360.Icon = this.Icon;
            kbm360.ShowDialog();
        }

        public void ChangeButtonText(string controlname, object tag)
        {
            lastSelected.Text = controlname;
            int value;
            if (Int32.TryParse(tag.ToString(), out value))
                lastSelected.Tag = value;
            else if (tag is Int32[])
                lastSelected.Tag = tag;
            else
                lastSelected.Tag = tag.ToString();
        }
        public void ChangeButtonText(string controlname)
        {
            lastSelected.Text = controlname;
            lastSelected.Tag = controlname;
        }
        public void Toggle_Bn(bool SC, bool TG, bool MC,  bool MR)
        {
            if (lastSelected.Tag is int || lastSelected.Tag is UInt16 || lastSelected.Tag is int[])
                lastSelected.Font = new Font(lastSelected.Font, 
                    (SC ? FontStyle.Bold : FontStyle.Regular) | (TG ? FontStyle.Italic : FontStyle.Regular) | 
                    (MC ? FontStyle.Underline : FontStyle.Regular) | (MR ? FontStyle.Strikeout : FontStyle.Regular));
            else if (lastSelected.Tag is string)
                if (lastSelected.Tag.ToString().Contains("Mouse Button"))
                    lastSelected.Font = new Font(lastSelected.Font, TG ? FontStyle.Italic : FontStyle.Regular);
            else
                lastSelected.Font = new Font(lastSelected.Font, FontStyle.Regular);
        }
        private void btnSticks_Enter(object sender, EventArgs e)
        {
            SticksPanel.Visible = true;
            MainPanel.Visible = false;
        }

        private void btnFullView_Click(object sender, EventArgs e)
        {
            SticksPanel.Visible = false;
            MainPanel.Visible = true;
        }
        private void btnLightbar_Click(object sender, EventArgs e)
        {
            advColorDialog.Color = Color.FromArgb(redBar.Value, greenBar.Value, blueBar.Value);
            advColorDialog_OnUpdateColor(pBController.BackColor, e);
            if (advColorDialog.ShowDialog() == DialogResult.OK)
            {
                pBController.BackColor = advColorDialog.Color;
                redBar.Value = advColorDialog.Color.R;
                greenBar.Value = advColorDialog.Color.G;
                blueBar.Value = advColorDialog.Color.B;
            }
            else Global.saveColor(device, oldLedColor[0], oldLedColor[1], oldLedColor[2]);
            Global.saveChargingColor(device, oldChargingColor[0], oldChargingColor[1], oldChargingColor[2]);
            Global.saveLowColor(device, oldLowLedColor[0], oldLowLedColor[1], oldLowLedColor[2]);
            oldChargingColor = null;
            oldLedColor = null;
            oldLowLedColor = null;
        }
        private void lowColorChooserButton_Click(object sender, EventArgs e)
        {
            advColorDialog.Color = lowColorChooserButton.BackColor;
            advColorDialog_OnUpdateColor(lowColorChooserButton.BackColor, e);
            if (advColorDialog.ShowDialog() == DialogResult.OK)
            {
                lowColorChooserButton.BackColor = advColorDialog.Color;
                lowRedBar.Value = advColorDialog.Color.R;
                lowGreenBar.Value = advColorDialog.Color.G;
                lowBlueBar.Value = advColorDialog.Color.B;
            }
            else Global.saveLowColor(device, oldLowLedColor[0], oldLowLedColor[1], oldLowLedColor[2]);
            Global.saveChargingColor(device, oldChargingColor[0], oldChargingColor[1], oldChargingColor[2]);
            Global.saveColor(device, oldLedColor[0], oldLedColor[1], oldLedColor[2]);
            oldChargingColor = null;
            oldLedColor = null;
            oldLowLedColor = null;
        }


        private void btnChargingColor_Click(object sender, EventArgs e)
        {
            advColorDialog.Color = btnChargingColor.BackColor;
            advColorDialog_OnUpdateColor(btnChargingColor.BackColor, e);
            if (advColorDialog.ShowDialog() == DialogResult.OK)
            {
                btnChargingColor.BackColor = advColorDialog.Color;
            }
            else Global.saveChargingColor(device, oldChargingColor[0], oldChargingColor[1], oldChargingColor[2]);
            Global.saveLowColor(device, oldLowLedColor[0], oldLowLedColor[1], oldLowLedColor[2]);
            Global.saveColor(device, oldLedColor[0], oldLedColor[1], oldLedColor[2]);
            oldChargingColor = null;
            oldLedColor = null;
            oldLowLedColor = null;
        }
        private void advColorDialog_OnUpdateColor(object sender, EventArgs e)
        {
            if (oldLedColor == null || oldLowLedColor == null || oldChargingColor == null)
            {
                DS4Color color = Global.loadColor(device);
                oldLedColor = new Byte[] { color.red, color.green, color.blue };
                color = Global.loadLowColor(device);
                oldLowLedColor = new Byte[] { color.red, color.green, color.blue };
                color = Global.loadChargingColor(device);
                oldChargingColor = new Byte[] { color.red, color.green, color.blue };
            }
            if (sender is Color)
            {
                Color color = (Color)sender;
                Global.saveColor(device, color.R, color.G, color.B);
                Global.saveLowColor(device, color.R, color.G, color.B);
                Global.saveChargingColor(device, color.R, color.G, color.B);
            }
        }
        int bgc = 255; //Color of the form background, If greyscale color
        private void redBar_ValueChanged(object sender, EventArgs e)
        {
            int value = ((TrackBar)sender).Value;
            int sat = bgc - (value < bgc ? value : bgc);
            int som = bgc + 11 * (int)(value * 0.0039215);
            ((TrackBar)sender).BackColor = Color.FromArgb(som, sat, sat);
            alphacolor = Math.Max(redBar.Value, Math.Max(greenBar.Value, blueBar.Value));
            reg = Color.FromArgb(redBar.Value, greenBar.Value, blueBar.Value);
            full = HuetoRGB(reg.GetHue(), reg.GetBrightness(), reg);
            pBController.BackColor = Color.FromArgb((alphacolor > 205 ? 255 : (alphacolor + 50)), full);
            Global.saveColor(device, (byte)redBar.Value, (byte)greenBar.Value, (byte)blueBar.Value);
            if (g.DpiX == 120)
                tp.Show(((TrackBar)sender).Value.ToString(), ((TrackBar)sender), 125, 0, 2000);
            else
                tp.Show(((TrackBar)sender).Value.ToString(), ((TrackBar)sender), 100, 0, 2000);
        }
        private void greenBar_ValueChanged(object sender, EventArgs e)
        {
            int value = ((TrackBar)sender).Value;
            int sat = bgc - (value < bgc ? value : bgc);
            int som = bgc + 11 * (int)(value * 0.0039215);
            ((TrackBar)sender).BackColor = Color.FromArgb(sat, som, sat);
            alphacolor = Math.Max(redBar.Value, Math.Max(greenBar.Value, blueBar.Value));
            reg = Color.FromArgb(redBar.Value, greenBar.Value, blueBar.Value);
            full = HuetoRGB(reg.GetHue(), reg.GetBrightness(), reg);
            pBController.BackColor = Color.FromArgb((alphacolor > 205 ? 255 : (alphacolor + 50)), full);
            Global.saveColor(device, (byte)redBar.Value, (byte)greenBar.Value, (byte)blueBar.Value);
            if (g.DpiX == 120)
                tp.Show(((TrackBar)sender).Value.ToString(), ((TrackBar)sender), 125, 0, 2000);
            else
                tp.Show(((TrackBar)sender).Value.ToString(), ((TrackBar)sender), 100, 0, 2000);
        }
        private void blueBar_ValueChanged(object sender, EventArgs e)
        {
            int value = ((TrackBar)sender).Value;
            int sat = bgc - (value < bgc ? value : bgc);
            int som = bgc + 11 * (int)(value * 0.0039215);
            ((TrackBar)sender).BackColor = Color.FromArgb(sat, sat, som);
            alphacolor = Math.Max(redBar.Value, Math.Max(greenBar.Value, blueBar.Value));
            reg = Color.FromArgb(redBar.Value, greenBar.Value, blueBar.Value);
            full = HuetoRGB(reg.GetHue(), reg.GetBrightness(), reg);
            pBController.BackColor = Color.FromArgb((alphacolor > 205 ? 255 : (alphacolor + 50)), full);
            Global.saveColor(device, (byte)redBar.Value, (byte)greenBar.Value, (byte)blueBar.Value);
            if (g.DpiX == 120)
                tp.Show(((TrackBar)sender).Value.ToString(), ((TrackBar)sender), 125, 0, 2000);
            else
                tp.Show(((TrackBar)sender).Value.ToString(), ((TrackBar)sender), 100, 0, 2000);
        }

        private void lowRedBar_ValueChanged(object sender, EventArgs e)
        {
            int value = ((TrackBar)sender).Value;
            int sat = bgc - (value < bgc ? value : bgc);
            int som = bgc + 11 * (int)(value * 0.0039215);
            ((TrackBar)sender).BackColor = Color.FromArgb(som, sat, sat);
            alphacolor = Math.Max(lowRedBar.Value, Math.Max(lowGreenBar.Value, lowBlueBar.Value));
            reg = Color.FromArgb(lowRedBar.Value, lowGreenBar.Value, lowBlueBar.Value);
            full = HuetoRGB(reg.GetHue(), reg.GetBrightness(), reg);
            lowColorChooserButton.BackColor = Color.FromArgb((alphacolor > 205 ? 255 : (alphacolor + 50)), full);
            Global.saveLowColor(device, (byte)lowRedBar.Value, (byte)lowGreenBar.Value, (byte)lowBlueBar.Value);
            if (g.DpiX == 120)
                tp.Show(((TrackBar)sender).Value.ToString(), ((TrackBar)sender), 125, 0, 2000);
            else
                tp.Show(((TrackBar)sender).Value.ToString(), ((TrackBar)sender), 100, 0, 2000);
        }

        private void lowGreenBar_ValueChanged(object sender, EventArgs e)
        {
            int value = ((TrackBar)sender).Value;
            int sat = bgc - (value < bgc ? value : bgc);
            int som = bgc + 11 * (int)(value * 0.0039215);
            ((TrackBar)sender).BackColor = Color.FromArgb(sat, som, sat);
            alphacolor = Math.Max(lowRedBar.Value, Math.Max(lowGreenBar.Value, lowBlueBar.Value));
            reg = Color.FromArgb(lowRedBar.Value, lowGreenBar.Value, lowBlueBar.Value);
            full = HuetoRGB(reg.GetHue(), reg.GetBrightness(), reg);
            lowColorChooserButton.BackColor = Color.FromArgb((alphacolor > 205 ? 255 : (alphacolor + 50)), full);
            Global.saveLowColor(device, (byte)lowRedBar.Value, (byte)lowGreenBar.Value, (byte)lowBlueBar.Value);
            if (g.DpiX == 120)
                tp.Show(((TrackBar)sender).Value.ToString(), ((TrackBar)sender), 125, 0, 2000);
            else
                tp.Show(((TrackBar)sender).Value.ToString(), ((TrackBar)sender), 100, 0, 2000);
        }

        private void lowBlueBar_ValueChanged(object sender, EventArgs e)
        {
            int value = ((TrackBar)sender).Value;
            int sat = bgc - (value < bgc ? value : bgc);
            int som = bgc + 11 * (int)(value * 0.0039215);
            ((TrackBar)sender).BackColor = Color.FromArgb(sat, sat, som);
            alphacolor = Math.Max(lowRedBar.Value, Math.Max(lowGreenBar.Value, lowBlueBar.Value));
            reg = Color.FromArgb(lowRedBar.Value, lowGreenBar.Value, lowBlueBar.Value);
            full = HuetoRGB(reg.GetHue(), reg.GetBrightness(), reg);
            lowColorChooserButton.BackColor = Color.FromArgb((alphacolor > 205 ? 255 : (alphacolor + 50)), full);
            Global.saveLowColor(device, (byte)lowRedBar.Value, (byte)lowGreenBar.Value, (byte)lowBlueBar.Value);
            if (g.DpiX == 120)
                tp.Show(((TrackBar)sender).Value.ToString(), ((TrackBar)sender), 125, 0, 2000);
            else
                tp.Show(((TrackBar)sender).Value.ToString(), ((TrackBar)sender), 100, 0, 2000);
        }

        public Color HuetoRGB(float hue, float light, Color rgb)
        {
            float L = (float)Math.Max(.5, light);
            float C = (1 - Math.Abs(2 * L - 1));
            float X = (C * (1 - Math.Abs((hue / 60) % 2 - 1)));
            float m = L - C / 2;
            float R =0, G=0, B=0;
            if (light == 1) return Color.FromName("White");
            else if (rgb.R == rgb.G && rgb.G == rgb.B) return Color.FromName("White");
            else if (0 <= hue && hue < 60)    { R = C; G = X; }
            else if (60 <= hue && hue < 120)  { R = X; G = C; }
            else if (120 <= hue && hue < 180) { G = C; B = X; }
            else if (180 <= hue && hue < 240) { G = X; B = C; }
            else if (240 <= hue && hue < 300) { R = X; B = C; }
            else if (300 <= hue && hue < 360) { R = C; B = X; }
            return Color.FromArgb((int)((R + m) * 255), (int)((G + m) * 255), (int)((B + m) * 255));
        }
        private void rumbleBoostBar_ValueChanged(object sender, EventArgs e)
        {
            Global.saveRumbleBoost(device, (byte)nUDRumbleBoost.Value);
            scpDevice.setRumble((byte)nUDHeavyRumble.Value, (byte)nUDLightRumble.Value, device);
        }
                
        private void numUDLightRumble_ValueChanged(object sender, EventArgs e)
        {
            if (btnRumbleTest.Text == "Stop")
                scpDevice.setRumble((byte)nUDHeavyRumble.Value, (byte)nUDLightRumble.Value, device);
        }

        private void numUDHeavyRumble_ValueChanged(object sender, EventArgs e)
        {
            if (btnRumbleTest.Text == "Stop")
                scpDevice.setRumble((byte)nUDHeavyRumble.Value, (byte)nUDLightRumble.Value, device);
        }

        private void btnRumbleTest_Click(object sender, EventArgs e)
        {
            if (((Button)sender).Text == "Test")
            {
                scpDevice.setRumble(255, 255, (int)nUDSixaxis.Value - 1);
                ((Button)sender).Text = "Stop";
            }
            else
            {
                scpDevice.setRumble(0, 0, (int)nUDSixaxis.Value - 1);
                ((Button)sender).Text = "Test";
            }                
        }

        private void numUDTouch_ValueChanged(object sender, EventArgs e)
        {
            Global.setTouchSensitivity(device, (byte)nUDTouch.Value);
        }

        private void numUDTap_ValueChanged(object sender, EventArgs e)
        {
            Global.setTapSensitivity(device, (byte)nUDTap.Value);
        }

        private void numUDScroll_ValueChanged(object sender, EventArgs e)
        {
            Global.setScrollSensitivity(device, (int)nUDScroll.Value);
        }
        private void ledAsBatteryIndicator_CheckedChanged(object sender, EventArgs e)
        {
            Global.setLedAsBatteryIndicator(device, cBLightbyBattery.Checked);
            lowBatteryPanel.Visible = cBLightbyBattery.Checked;
            FullPanel.Location = (cBLightbyBattery.Checked ? new Point(FullPanel.Location.X, 42) : new Point(FullPanel.Location.X, 48));
            lbFull.Text = (cBLightbyBattery.Checked ? "Full:" : "Color:");
        }

        private void lowerRCOffCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Global.setLowerRCOn(device, cBlowerRCOn.Checked);
        }

        private void touchpadJitterCompensation_CheckedChanged(object sender, EventArgs e)
        {
            Global.setTouchpadJitterCompensation(device, cBTouchpadJitterCompensation.Checked);
        }
        
        private void flushHIDQueue_CheckedChanged(object sender, EventArgs e)
        {
            Global.setFlushHIDQueue(device, cBFlushHIDQueue.Checked);
        }

        private void nUDIdleDisconnect_ValueChanged(object sender, EventArgs e)
        {
            Global.setIdleDisconnectTimeout(device, (int)(nUDIdleDisconnect.Value * 60));
            //if (nUDIdleDisconnect.Value == 0)
                //cBIdleDisconnect.Checked = false;
        }

        private void cBIdleDisconnect_CheckedChanged(object sender, EventArgs e)
        {
            if (cBIdleDisconnect.Checked)
                nUDIdleDisconnect.Value = 5;
            else
                nUDIdleDisconnect.Value = 0;
            nUDIdleDisconnect.Enabled = cBIdleDisconnect.Checked;
        }

        private void Options_Closed(object sender, FormClosedEventArgs e)
        {
            if (sixaxisTimer.Enabled)
                sixaxisTimer.Stop();
            for (int i = 0; i < 4; i++)
                Global.LoadProfile(i); //Refreshes all profiles in case other controllers are using the same profile
            inputtimer.Stop();
            sixaxisTimer.Stop();
        }

        private void cBSlide_CheckedChanged(object sender, EventArgs e)
        {
            if (cBSlide.Checked)
                nUDTouch.Value = 100;
            else
                nUDTouch.Value = 0;
            nUDTouch.Enabled = cBSlide.Checked;
        }

        private void cBScroll_CheckedChanged(object sender, EventArgs e)
        {
            if (cBScroll.Checked)
                nUDScroll.Value = 5;
            else
                nUDScroll.Value = 0;
            nUDScroll.Enabled = cBScroll.Checked;
        }

        private void cBTap_CheckedChanged(object sender, EventArgs e)
        {
            if (cBTap.Checked)
                nUDTap.Value = 100;
            else
                nUDTap.Value = 0;
            nUDTap.Enabled = cBTap.Checked;
            cBDoubleTap.Enabled = cBTap.Checked;
        }

        private void cBDoubleTap_CheckedChanged(object sender, EventArgs e)
        {
            Global.setDoubleTap(device, cBDoubleTap.Checked);
        }

        public void UpdateLists()
        {
            lBControls.Items[0] = "Cross : " + bnCross.Text;
            lBControls.Items[1] = "Circle : " + bnCircle.Text;
            lBControls.Items[2] = "Sqaure : " + bnSquare.Text;
            lBControls.Items[3] = "Triangle : " + bnTriangle.Text;
            lBControls.Items[4] = "Options : " + bnOptions.Text;
            lBControls.Items[5] = "Share : " + bnShare.Text;
            lBControls.Items[6] = "Up : " + bnUp.Text;
            lBControls.Items[7] = "Down : " + bnDown.Text;
            lBControls.Items[8] = "Left : " + bnLeft.Text;
            lBControls.Items[9] = "Right : " + bnRight.Text;
            lBControls.Items[10] = "PS : " + bnPS.Text;
            lBControls.Items[11] = "L1 : " + bnL1.Text;
            lBControls.Items[12] = "R1 : " + bnR1.Text;
            lBControls.Items[13] = "L2 : " + bnL2.Text;
            lBControls.Items[14] = "R2 : " + bnR2.Text;
            lBControls.Items[15] = "L3 : " + bnL3.Text;
            lBControls.Items[16] = "R3 : " + bnR3.Text;
            lBControls.Items[17] = "Left Touch : " + bnTouchLeft.Text;
            lBControls.Items[18] = "Right Touch : " + bnTouchRight.Text;
            lBControls.Items[19] = "Multitouch : " + bnTouchMulti.Text;
            lBControls.Items[20] = "Upper Touch : " + bnTouchUpper.Text;
            lBControls.Items[21] = "LS Up : " + bnLSUp.Text;
            lBControls.Items[22] = "LS Down : " + bnLSDown.Text;
            lBControls.Items[23] = "LS Left : " + bnLSLeft.Text;
            lBControls.Items[24] = "LS Right : " + bnLSRight.Text;
            lBControls.Items[25] = "RS Up : " + bnRSUp.Text;
            lBControls.Items[26] = "RS Down : " + bnRSDown.Text;
            lBControls.Items[27] = "RS Left : " + bnRSLeft.Text;
            lBControls.Items[28] = "RS Right : " + bnRSRight.Text;
            lBControls.Items[29] = "Tilt Up : " + UpdateGyroList(bnGyroZN);
            lBControls.Items[30] = "Tilt Down : " + UpdateGyroList(bnGyroZP);
            lBControls.Items[31] = "Tilt Left : " + UpdateGyroList(bnGyroXP);
            lBControls.Items[32] = "Tilt Right : " + UpdateGyroList(bnGyroXN);
            bnGyroZN.Text = "Tilt Up";
            bnGyroZP.Text = "Tilt Down";
            bnGyroXP.Text = "Tilt Left";
            bnGyroXN.Text = "Tilt Right";
        }

        private string UpdateGyroList(Button button)
        {
            if (button.Tag is String && (String)button.Tag == "Unbound")
                return "Unbound";
            else if (button.Tag is IEnumerable<int> || button.Tag is Int32[] || button.Tag is UInt16[])
                return "Macro";
            else if (button.Tag is Int32 || button.Tag is UInt16)
                return ((Keys)(ushort)button.Tag).ToString();
            else if (button.Tag is string)
                return button.Tag.ToString();
            else
                return string.Empty;
        }
        private void Show_ControlsList(object sender, EventArgs e)
        {
            if (lBControls.SelectedIndex == 0) Show_ControlsBn(bnCross, e);
            if (lBControls.SelectedIndex == 1) Show_ControlsBn(bnCircle, e);
            if (lBControls.SelectedIndex == 2) Show_ControlsBn(bnSquare, e);
            if (lBControls.SelectedIndex == 3) Show_ControlsBn(bnTriangle, e);
            if (lBControls.SelectedIndex == 4) Show_ControlsBn(bnOptions, e);
            if (lBControls.SelectedIndex == 5) Show_ControlsBn(bnShare, e);
            if (lBControls.SelectedIndex == 6) Show_ControlsBn(bnUp, e);
            if (lBControls.SelectedIndex == 7) Show_ControlsBn(bnDown, e);
            if (lBControls.SelectedIndex == 8) Show_ControlsBn(bnLeft, e);
            if (lBControls.SelectedIndex == 9) Show_ControlsBn(bnRight, e);
            if (lBControls.SelectedIndex == 10) Show_ControlsBn(bnPS, e);
            if (lBControls.SelectedIndex == 11) Show_ControlsBn(bnL1, e);
            if (lBControls.SelectedIndex == 12) Show_ControlsBn(bnR1, e);
            if (lBControls.SelectedIndex == 13) Show_ControlsBn(bnL2, e);
            if (lBControls.SelectedIndex == 14) Show_ControlsBn(bnR2, e);
            if (lBControls.SelectedIndex == 15) Show_ControlsBn(bnL3, e);
            if (lBControls.SelectedIndex == 16) Show_ControlsBn(bnR3, e);

            if (lBControls.SelectedIndex == 17) Show_ControlsBn(bnTouchLeft, e);
            if (lBControls.SelectedIndex == 18) Show_ControlsBn(bnTouchRight, e);
            if (lBControls.SelectedIndex == 19) Show_ControlsBn(bnTouchMulti, e);
            if (lBControls.SelectedIndex == 20) Show_ControlsBn(bnTouchUpper, e);

            if (lBControls.SelectedIndex == 21) Show_ControlsBn(bnLSUp, e);
            if (lBControls.SelectedIndex == 22) Show_ControlsBn(bnLSDown, e);
            if (lBControls.SelectedIndex == 23) Show_ControlsBn(bnLSLeft, e);
            if (lBControls.SelectedIndex == 24) Show_ControlsBn(bnLSRight, e);
            if (lBControls.SelectedIndex == 25) Show_ControlsBn(bnRSUp, e);
            if (lBControls.SelectedIndex == 26) Show_ControlsBn(bnRSDown, e);
            if (lBControls.SelectedIndex == 27) Show_ControlsBn(bnRSLeft, e);
            if (lBControls.SelectedIndex == 28) Show_ControlsBn(bnRSRight, e);

            if (lBControls.SelectedIndex == 29) Show_ControlsBn(bnGyroZN, e);
            if (lBControls.SelectedIndex == 30) Show_ControlsBn(bnGyroZP, e);
            if (lBControls.SelectedIndex == 31) Show_ControlsBn(bnGyroXP, e);
            if (lBControls.SelectedIndex == 32) Show_ControlsBn(bnGyroXN, e);
        }

        private void List_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show_ControlsList(sender, e);
        }

        private void List_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyValue == 13)
                Show_ControlsList(sender, e);
        }

        private void numUDRainbow_ValueChanged(object sender, EventArgs e)
        {
            Global.setRainbow(device, (double)nUDRainbow.Value);
            if ((double)nUDRainbow.Value <= 0.5)
            {
                pBRainbow.Image = greyscale;
                ToggleRainbow(false);
                nUDRainbow.Value = 0;
            }
        }

        private void pbRainbow_Click(object sender, EventArgs e)
        {
            if (pBRainbow.Image == greyscale)
            {
                pBRainbow.Image = colored;
                ToggleRainbow(true);
                nUDRainbow.Value = 5;
            }
            else
            {
                pBRainbow.Image = greyscale;
                ToggleRainbow(false);
                nUDRainbow.Value = 0;
            }
        }

        private void ToggleRainbow(bool on)
        {
            nUDRainbow.Enabled = on;
            if (on)
            {
                //pBRainbow.Location = new Point(216 - 78, pBRainbow.Location.Y);
                pBController.BackgroundImage = Properties.Resources.rainbowC;
                cBLightbyBattery.Text = "Dim by Battery %";
            }
            else
            {
                lowBatteryPanel.Enabled = cBLightbyBattery.Checked;
                //pBRainbow.Location = new Point(216, pBRainbow.Location.Y);
                pBController.BackgroundImage = null;
                cBLightbyBattery.Text = "Color by Battery %";
            }
            lBspc.Enabled = on;
            lowBatteryPanel.Enabled = !on;
            FullPanel.Enabled = !on;
        }

        private Bitmap GreyscaleImage(Bitmap image)
        {
            Bitmap c = (Bitmap)image;
            Bitmap d = new Bitmap(c.Width, c.Height);

            for (int i = 0; i < c.Width; i++)
            {
                for (int x = 0; x < c.Height; x++)
                {
                    Color oc = c.GetPixel(i, x);
                    int grayScale = (int)((oc.R * 0.3) + (oc.G * 0.59) + (oc.B * 0.11));
                    Color nc = Color.FromArgb(oc.A, grayScale, grayScale, grayScale);
                    d.SetPixel(i, x, nc);
                }
            }
            return d;
        }

        private void numUDL2_ValueChanged(object sender, EventArgs e)
        {
            Global.setLeftTriggerMiddle(device, (byte)(nUDL2.Value * 255));
        }

        private void numUDR2_ValueChanged(object sender, EventArgs e)
        {
            Global.setRightTriggerMiddle(device, (byte)(nUDR2.Value * 255));
        }

        private void nUDSX_ValueChanged(object sender, EventArgs e)
        {
            Global.setSXDeadzone(device, (double)nUDSX.Value);
        }

        private void nUDSZ_ValueChanged(object sender, EventArgs e)
        {
            Global.setSZDeadzone(device, (double)nUDSZ.Value);
        }

        Image L = Properties.Resources.LeftTouch;
        Image R = Properties.Resources.RightTouch;
        Image M = Properties.Resources.MultiTouch;
        Image U = Properties.Resources.UpperTouch;
        private void bnTouchLeft_MouseHover(object sender, EventArgs e)
        {
            pBController.Image = L;       
        }

        private void bnTouchMulti_MouseHover(object sender, EventArgs e)
        {
            pBController.Image = M;
        }

        private void bnTouchRight_MouseHover(object sender, EventArgs e)
        {
            pBController.Image = R;
        }

        private void bnTouchUpper_MouseHover(object sender, EventArgs e)
        {
            pBController.Image = U;
        }

        private void Toucpad_Leave(object sender, EventArgs e)
        {
            pBController.Image = Properties.Resources.DS4_Controller;
        }

        private void numUDRS_ValueChanged(object sender, EventArgs e)
        {
            Global.setRSDeadzone(device, (byte)Math.Round((nUDRS.Value * 127),0));
        }

        private void numUDLS_ValueChanged(object sender, EventArgs e)
        {
            Global.setLSDeadzone(device, (byte)Math.Round((nUDLS.Value * 127),0));
        }

        private void numUDMouseSens_ValueChanged(object sender, EventArgs e)
        {
            Global.setButtonMouseSensitivity(device, (int)numUDMouseSens.Value);
        }

        private void LightBar_MouseDown(object sender, MouseEventArgs e)
        {
            if (g.DpiX == 120)
                tp.Show(((TrackBar)sender).Value.ToString(), ((TrackBar)sender), 125, 0, 2000);
            else
                tp.Show(((TrackBar)sender).Value.ToString(), ((TrackBar)sender), 100, 0, 2000);
        }

        private void Lightbar_MouseUp(object sender, MouseEventArgs e)
        {
            tp.Hide(((TrackBar)sender));
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void nUDflashLED_ValueChanged(object sender, EventArgs e)
        {
            if (nUDflashLED.Value % 10 != 0)
                nUDflashLED.Value = Math.Round(nUDflashLED.Value / 10, 0) * 10;
            Global.setFlashAt(device, (int)nUDflashLED.Value);
        }

        private void rBNormal_CheckedChanged(object sender, EventArgs e)
        {
            Global.setChargingType(device, 0);
            btnChargingColor.Visible = false;
        }

        private void rBFade_CheckedChanged(object sender, EventArgs e)
        {
            Global.setChargingType(device, 1);
            btnChargingColor.Visible = false;
        }

        private void rBRainbow_CheckedChanged(object sender, EventArgs e)
        {
            Global.setChargingType(device, 2);
            btnChargingColor.Visible = false;
        }

        private void rBColor_CheckedChanged(object sender, EventArgs e)
        {
            Global.setChargingType(device, 3);
            btnChargingColor.Visible = true;
        }

        private void lB6Gryo_MouseClick(object sender, MouseEventArgs e)
        {
            fLPTiltControls.Visible = !fLPTiltControls.Visible;
            SixaxisPanel.Visible = !SixaxisPanel.Visible;
            if (SixaxisPanel.Visible)
                sixaxisTimer.Start();
            else
                sixaxisTimer.Stop();
        }

        private void cBMouseAccel_CheckedChanged(object sender, EventArgs e)
        {
            Global.setMouseAccel(device, cBMouseAccel.Checked);
        }
    }
}
