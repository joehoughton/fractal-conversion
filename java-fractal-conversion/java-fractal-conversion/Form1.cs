﻿using System;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Xml.Linq;
using System.IO;
using System.Xml;
using System.Drawing.Imaging;
using System.Collections.Generic;

namespace java_fractal_conversion
{
    using System.Linq;

    public partial class Form1 : Form
    {
        private const int MAX = 256;      // max iterations
        private const double SX = -2.025; // start value real
        private const double SY = -1.125; // start value imaginary
        private const double EX = 0.6;    // end value real
        private const double EY = 1.125;  // end value imaginary
        private const int ScaleUp = 255;
        private int j; // sets colour of fractal - default to 0
        private static int x1, y1, xs, ys, xe, ye;
        private static double xstart, ystart, xende, yende, xzoom, yzoom;
        private static bool action, rectangle;
        private static float xy;
        private readonly Graphics g1;
        private readonly Bitmap bitmap;
        private Pen pen;
        private bool cycleForwards = true;
        public List<State> States;
        private State currentState;
        private State nextState;
        private int ticks = 0;
        // private static bool finished;  // djm not needed - can reset state without in resetToolStripMenuItem_Click()
        // private Cursor c1, c2; // djm not needed // now changed in picture_MouseEnter and picture_MouseLeave 
        // private Image picture; // djm not needed 
        // private HSB HSBcol = new HSB(); // djm not needed

        public Form1()
        {
            InitializeComponent();
            // finished = false; // djm not needed - can reset state without in resetToolStripMenuItem_Click()
            // addMouseListener(this); // djm not needed
            // addMouseMotionListener(this); // djm not needed
            // c1 = Cursors.WaitCursor; // replaced by picture_MouseEnter() // c1 = new Cursor(Cursor.WAIT_CURSOR); // djm original java
            // c2 = Cursors.Cross; // replaced by picture_MouseLeave() // c1 = new Cursor(Cursor.WAIT_CURSOR); // djm original java
            x1 = picture.Width;  // x1 = getSize().width; // djm original java
            y1 = picture.Height;  // y1 = getSize().height; // djm original java
            xy = (float)x1 / (float)y1;

            this.bitmap = new Bitmap(x1, y1); // picture = createImage(x1, y1); // djm original java
            g1 = Graphics.FromImage(this.bitmap); // g1 = picture.getGraphics(); // djm original java

            // finished = true; // djm not needed - can reset state without in resetToolStripMenuItem_Click()
            Start();
        }

        public void Start() // djm original start()
        {
            action = false;
            rectangle = false;
            Initvalues();
            xzoom = (xende - xstart) / (double)x1;
            yzoom = (yende - ystart) / (double)y1;
            Mandelbrot();
        }

        private void Initvalues() // reset start values // djm original initValues()
        {
            xstart = SX;
            ystart = SY;
            xende = EX;
            yende = EY;
            if ((float)((xende - xstart) / (yende - ystart)) != xy)
                xstart = xende - (yende - ystart) * (double)xy;
        }

        // randomly generates a known system colour - used for rectangle
        private Color RandomColour()
        {
            var randomGenerator = new Random();
            var systemColours = (KnownColor[])Enum.GetValues(typeof(KnownColor)); // array of system colour names
            var randomColorName = systemColours[randomGenerator.Next(systemColours.Length)]; // get random colour name
            var randomColor = Color.FromKnownColor(randomColorName); // create color object from name
            return randomColor;
        }

        private void Mandelbrot() // calculate all points // djm original mandelbrot()
        {
            int x, y;
            float h, b, alt = 0.0f;

            action = false;
            // Cursor = Cursors.WaitCursor; //jh not needed // setCursor(c1); // djm original java
            Text = "C3375905";
            message.Text = "Mandelbrot-Set will be produced - please wait..."; // showStatus("Mandelbrot-Set will be produced - please wait..."); // djm original java

            for (x = 0; x < x1; x += 2) // x less than width - draw lines from left to right
                for (y = 0; y < y1; y++) // draw 1 pixel at a time
                {
                    h = Pointcolour(xstart + xzoom * (double)x, ystart + yzoom * (double)y); // color value
                    if (h != alt)
                    {
                        b = 1.0f - h * h; // brightness

                        var customColour = new HSBColor(h * ScaleUp, 0.8f * ScaleUp, b * ScaleUp); // hsb colour
                        var convertedColour = HSBColor.FromHSB(customColour); // convert hsb to rgb then make a Java Color

                        // Color col = Color.getHSBColor(h,0.8f,b); // djm not needed
                        // int red = col.getRed(); // djm not needed
                        // int green = col.getGreen(); // djm not needed
                        // int blue = col.getBlue(); // djm not needed
                        pen = new Pen(convertedColour);
                        alt = h;
                    }
                    g1.DrawLine(pen, x, y, x + 1, y);
                }

            // Cursor = Cursors.Cross; //jh not needed // setCursor(c1); // djm original java
            message.Text = "Mandelbrot-Set ready - please select zoom area with pressed mouse."; // showStatus("Mandelbrot-Set ready - please select zoom area with pressed mouse."); // djm original java
            action = true;
        }

        private float Pointcolour(double xwert, double ywert) // color value from 0.0 to 1.0 by iterations  // djm original pointcolour()
        {
            double r = 0.0, i = 0.0, m = 0.0;

            var jReference = j;

            while ((jReference < MAX) && (m < 4.0)) // djm original while ((j < MAX) && (m < 4.0))
            {
                jReference++; // djm original jReference++;
                m = r * r - i * i;
                i = 2.0 * r * i + ywert;
                r = m + xwert;
            }
            return (float)jReference / (float)MAX; // djm original return (float)j / (float)MAX;
        }

        public void Paint(Graphics g)
        {
            Update(g);
        }

        public void Update(Graphics g)
        {
            g.DrawImage(bitmap, 0, 0);
            if (rectangle)
            {
                var randomColour = RandomColour();
                pen = new Pen(randomColour); // use a new pen to prevent memory leak // pen.Color = Color.White; throws Parameter is not valid exception
                if (xs < xe)
                {
                    if (ys < ye) g.DrawRectangle(pen, xs, ys, (xe - xs), (ye - ys));
                    else g.DrawRectangle(pen, xs, ye, (xe - xs), (ys - ye));
                }
                else
                {
                    if (ys < ye)
                    {
                        g.DrawRectangle(pen, xe, ys, (xs - xe), (ye - ys));
                    }
                    else
                    {
                        g.DrawRectangle(pen, xe, ye, (xs - xe), (ys - ye));
                    }
                }

                pen.Dispose(); // release all pen resources
            }
        }

        private void picture_Paint(object sender, PaintEventArgs e)
        {
            Paint(e.Graphics);
        }

        private void picture_MouseDown(object sender, MouseEventArgs e) // public void mousePressed(MouseEvent e) // djm original
        {
            // e.consume(); // djm not needed

            if (action && e.Button == MouseButtons.Left) // e.consume(); // djm original Java
            {
                xs = e.X; // xs = e.getX(); // djm original Java
                ys = e.Y; // ys = e.getY(); // djm original Java
            }
        }

        private void picture_MouseUp(object sender, MouseEventArgs e) // public void mouseReleased(MouseEvent e) // djm original java
        {
            int z, w;

            if (action)
            {
                xe = e.X; // xe = e.getX(); // djm original Java
                ye = e.Y; // ye = e.getY(); // djm original Java
                if (xs > xe)
                {
                    z = xs;
                    xs = xe;
                    xe = z;
                }
                if (ys > ye)
                {
                    z = ys;
                    ys = ye;
                    ye = z;
                }
                w = (xe - xs);
                z = (ye - ys);
                if ((w < 2) && (z < 2)) Initvalues();
                else
                {
                    if (((float)w > (float)z * xy)) ye = (int)((float)ys + (float)w / xy);
                    else xe = (int)((float)xs + (float)z * xy);
                    xende = xstart + xzoom * (double)xe;
                    yende = ystart + yzoom * (double)ye;
                    xstart += xzoom * (double)xs;
                    ystart += yzoom * (double)ys;
                }
                xzoom = (xende - xstart) / (double)x1;
                yzoom = (yende - ystart) / (double)y1;
                Mandelbrot();
                rectangle = false;
                Refresh();  // redraw picture and child components // repaint(); // djm original Java
            }
        }

        private void picture_MouseMove(object sender, MouseEventArgs e) // public void mouseDragged(MouseEvent e) // djm original java
        {
            // e.consume(); // djm not needed
            if (action && e.Button == MouseButtons.Left) // if (action); // djm original Java
            {
                xe = e.X; // xe = e.getX(); // djm original Java
                ye = e.Y; // ye = e.getY(); // djm original Java
                rectangle = true; // set to true so selected area drawn in Update method
                Refresh();  // redraw picture and child components
            }
        }

        // cursors for picture box
        private void picture_MouseEnter(object sender, EventArgs e)
        {
            Cursor = Cursors.Cross;
        }

        private void picture_MouseLeave(object sender, EventArgs e)
        {
            Cursor = Cursors.Default;
        }

        // cursors for all buttons, sliders, checkboxes, menu items
        private void cursor_MouseEnter(object sender, EventArgs e)
        {
            Cursor = Cursors.Hand; // set cursor to hand on reset button hover
        }

        private void cursor_MouseLeave(object sender, EventArgs e)
        {
            Cursor = Cursors.Default; // set cursor to default arrow on leave of reset button
        }

        private void cursor_MouseClick(object sender, MouseEventArgs e)
        {
            Cursor = Cursors.Hand; // set cursor to hand on click
        }

        // close form - file menu item
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // ========================================================================
        // SAVE BITMAP TO FILE AND SAVES STATE TO ALLOW REZOOMING AND COLOUR RELOAD
        // ========================================================================

        // save state to xml - file menu item
        private void saveStateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var saveFileDialog = new SaveFileDialog
                                     {
                                         Filter = "XML files (*.xml)|*.xml", // only allow xml files to be saved
                                         CreatePrompt = true, // make user confirm they want to save file
                                         FileName = "fractal-state" // default name of xml file to be saved
                                     };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var converter = TypeDescriptor.GetConverter(typeof(Bitmap));
                    var image = Convert.ToBase64String((byte[])converter.ConvertTo(this.bitmap, typeof(byte[]))); // convert bitmap to byte array and convert array to string

                    var document = new XDocument( // define xml tree
                        new XElement("state", // parent node - the identifier 
                        new XElement("image", image), // remaining child nodes containing current bitmap values
                        new XElement("xstart", xstart),
                        new XElement("ystart", ystart),
                        new XElement("xzoom", xzoom),
                        new XElement("yzoom", yzoom),
                        new XElement("j", j)
                    ));

                    document.Save(saveFileDialog.FileName); // save document to the selected path
                    string selectedFileExtension = Path.GetExtension(saveFileDialog.FileName); // get file extension (in this case xml)
                    message.Text = String.Format("Successfully saved fractal state at {0} in {01} format.", saveFileDialog.FileName, selectedFileExtension);
                }
                catch (Exception ex)
                {
                    message.Text = String.Format("Failed to save fractal state. {0}", ex.Message); // failed to convert to xml and save, display error message
                }
            }
        }

        // ================================================================
        // LOAD BITMAP FROM FILE AND RESTORE STATE INCUDING ZOOM AND COLOUR
        // ================================================================

        // load state from xml - file menu item
        private void loadStateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "XML files (*.xml)|*.xml" // only display xml files in directory
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                // Added as a precaution as the Filter will only display xml files anyway
                if (!String.Equals(Path.GetExtension(openFileDialog.FileName), ".xml", StringComparison.OrdinalIgnoreCase))
                {
                    message.Text = ("You must select an XML file.");
                    return;
                }

                try
                {
                    var streamReader = new StreamReader(openFileDialog.FileName); // initialise stream reader to read selected xml file
                    using (streamReader) // using statement disposes of system resources automatically
                    {
                        var document = new XmlDocument();
                        document.Load(openFileDialog.OpenFile());
                        var xnList = document.SelectNodes("/state"); // select parent node

                        foreach (XmlNode xmlNode in xnList) // loop through child nodes to access stored bitmap attributes
                        {
                            xstart = Convert.ToDouble(xmlNode["xstart"].InnerText);
                            ystart = Convert.ToDouble(xmlNode["ystart"].InnerText);
                            xzoom = Convert.ToDouble(xmlNode["xzoom"].InnerText);
                            yzoom = Convert.ToDouble(xmlNode["yzoom"].InnerText);
                            j = Convert.ToInt32(xmlNode["j"].InnerText);
                        }
                        Mandelbrot();
                        Refresh();  // redraw picture and child components // repaint(); // djm original Java

                        string selectedFileExtension = Path.GetExtension(openFileDialog.FileName); // get file extension (in this case xml)
                        message.Text = String.Format("Successfully loaded fractal state from {0} at {1}.", selectedFileExtension, openFileDialog.FileName);
                    }

                }
                catch (Exception ex)
                {
                    message.Text = String.Format("Failed to load fractal state. {0}", ex.Message); // failed to load, display error message
                }

            }
        }

        // ============================================
        // RESET FORM TO DEFAULT 
        // ============================================

        // reset form to default
        private void reset_Click(object sender, EventArgs e)
        {
            // djm not needed
            /*if (finished)
            {
                removeMouseListener(this);
                removeMouseMotionListener(this);
                picture = null;
                g1 = null;
                c1 = null;
                c2 = null;
                System.gc(); // garbage collection
            }*/
            timerColourCycle.Stop(); // stop timer
            timerColourCycle.Dispose(); // dispose of time
            checkBoxAnimation.Checked = false; // uncheck animation checkbox
            checkBoxColourCycle.Checked = false; // uncheck colour cycle checkbox
            checkBoxPaletteCycle.Checked = false; // uncheck palette cycle checkbox
            checkBoxSequenceOfFractals.Checked = false;
            j = 0; // reset fractal colour to red
            trackBarColourCycle.Value = 0; // reset colour cycle speed
            colourCycleSpeedLabel.Text = "x1"; // reset colour cycle speed label
            colourPaletteLabel.Text = "Select a colour:"; // no colour selected
            messageAnimation.Text = null; // remove text
            domainUpDownSequenceOfFractals.Text = "x1"; // reset text to default speed
            domainUpDownSequenceOfFractals.SelectedItem = "x1";
            Start(); // reset zoom, initial variables, call mandlebrot
            Refresh(); // redraw picture and child components 
        }

        // ============================================
        // SAVE BITMAP STATE TO IMAGE FILE
        // ============================================

        // save bitmap to image - file menu item
        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "JPEG|*.JPG|PNG|*.PNG|BMP|*.BMP", // only allow png, bmp or jpg to be saved
                CreatePrompt = true, // make user confirm they want to save file
                FileName = "fractal-image" // default name of image file to be saved
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string selectedFileExtension = Path.GetExtension(saveFileDialog.FileName); // store the users selected extension 

                ImageFormat format;
                switch (selectedFileExtension)
                {
                    case ".jpg":
                        format = ImageFormat.Jpeg;
                        break;
                    case ".bmp":
                        format = ImageFormat.Bmp;
                        break;
                    default:
                        format = ImageFormat.Png;
                        break;
                }

                try
                {
                    this.bitmap.Save(saveFileDialog.FileName, format); // save image using users file name and selected format
                    message.Text = String.Format("Successfully saved fractal at {0} in {1} format.", saveFileDialog.FileName, selectedFileExtension);
                }
                catch (Exception ex)
                {
                    message.Text = String.Format("Failed to save image. {0}", ex.Message); // failed to save image, display error message
                }
            }
        }

        // ============================================
        // CHANGE COLOUR PALETTE
        // ============================================

        // colour palette selection
        private void colourPaletteRed_Click(object sender, EventArgs e)
        {
            j = 0;
            colourPaletteLabel.Text = "Selected: Red";
            Mandelbrot();
            Refresh(); // redraw picture and child components
        }

        private void colourPaletteOrange_Click(object sender, EventArgs e)
        {
            j = 10;
            colourPaletteLabel.Text = "Selected: Orange";
            Mandelbrot();
            Refresh();
        }

        private void colourPaletteYellow_Click(object sender, EventArgs e)
        {
            j = 30;
            colourPaletteLabel.Text = "Selected: Yellow";
            Mandelbrot();
            Refresh();
        }

        private void colourPaletteGreen_Click(object sender, EventArgs e)
        {
            j = 60;
            colourPaletteLabel.Text = "Selected: Green";
            Mandelbrot();
            Refresh();
        }

        private void colourPaletteTurquoise_Click(object sender, EventArgs e)
        {
            j = 120;
            colourPaletteLabel.Text = "Selected: Turquoise";
            Mandelbrot();
            Refresh();
        }

        private void colourPaletteBlue_Click(object sender, EventArgs e)
        {
            j = 150;
            colourPaletteLabel.Text = "Selected: Blue";
            Mandelbrot();
            Refresh();
        }

        private void colourPalettePurple_Click(object sender, EventArgs e)
        {
            j = 190;
            colourPaletteLabel.Text = "Selected: Purple";
            Mandelbrot();
            Refresh();
        }

        // ============================================
        // COLOUR CYCLING
        // ============================================

        /* colour cycle checkbox - selecting the checkbox starts the timerColourCycle_Tick timer.
           The method is called every 100 milliseconds at default. At each call, the value of j is
           incremended. If j reaches 240 (dark colour) it is decremented to lighten
           the fractal. At 0 (red) it is incremented again. Sliding the slider calls the 
           trackBarColourCycle_Scroll which changes the timer speed in milliseconds. */
        private void checkBoxColourCycle_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxColourCycle.Checked) // cycle checkbox selected
            {
                timerColourCycle.Start();
            }
            else // cycle checkbox not selected
            {
                timerColourCycle.Stop();
                timerColourCycle.Dispose();
            }
        }

        private void timerColourCycle_Tick(object sender, EventArgs e)
        {
            Mandelbrot();
            Refresh(); // redraw picture and child components

            if (j == 240) // if j reaches 240 (black) then set variable to cycle backwards 
            {
                cycleForwards = false;
            }

            if (j == 0) // if j reaches 0 (red) then set variable to cycle forwards 
            {
                cycleForwards = true;
            }

            if (cycleForwards)
            {
                j++; // cycle colours from light to dark
            }
            else
            {
                j--; // cycle colours from dark to light
            }
        }

        // colour cycle speed slider
        private void trackBarColourCycle_Scroll(object sender, EventArgs e)
        {
            switch (trackBarColourCycle.Value) // get value from slider and set timer time in milliseconds
            {
                case 1:
                    timerColourCycle.Interval = 300;
                    colourCycleSpeedLabel.Text = "x2";
                    break;
                case 2:
                    timerColourCycle.Interval = 100;
                    colourCycleSpeedLabel.Text = "x3";
                    break;
                default:
                    timerColourCycle.Interval = 600;
                    colourCycleSpeedLabel.Text = "x1";
                    break;
            }
        }

        // ============================================
        // FULL SMOOTH COLOUR CYCLING / PALETTE CYCLING
        // ============================================

        /* palette cycle checkbox - if checked, the timerPaletteCycle_Tick is called every 100 milliseconds.
           At each call, the current bitmap is saved in memory as a gif image, to access the original bitmaps Pallete Entries (256 colours, 
           not accessible in jpg).
           Each palette entry (colour) is set to the next entry in the array. The colour palette is added to a new bitmap which is drawn. */
        private void checkBoxPaletteCycle_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxPaletteCycle.Checked) // palette cycle checkbox selected
            {
                timerPaletteColourCycle.Start(); // start timer - called every 100 milliseconds
            }
            else // palette cycle checkbox not selected
            {
                timerPaletteColourCycle.Stop();
                timerPaletteColourCycle.Dispose();
            }
        }

        private void timerPaletteCycle_Tick(object sender, EventArgs e)
        {
            try
            {
                var memoryStream = new MemoryStream();
                using (memoryStream) // using statement disposes memoryStream after use
                {
                    bitmap.Save(memoryStream, ImageFormat.Gif); // Save bitmap to stream as gif as gifs have palette entries (unlike jpegs)
                    var bitmap2 = new Bitmap(memoryStream); // copy original bitmap to new bitmap
                    var palette = bitmap2.Palette;

                    for (int i = 0; i < palette.Entries.Length - 5; i++) // exclude last four white colours
                    {
                        var colour = palette.Entries[i];
                        palette.Entries[i] = palette.Entries[i + 1]; // exclude first black colour at palette.Entries[0]
                        palette.Entries[i + 1] = colour;
                    }

                    bitmap2.Palette = palette; // add palette to new bitmap
                    g1.DrawImage(bitmap2, 0, 0); // draw new bitmap
                    Refresh();  // redraw picture and child components
                }
            }
            catch (Exception ex)
            {
                message.Text = String.Format("Failed to cycle through palette colours. {0}", ex.Message); // failed to cycle, display error message
            }
        }

        // ================================================================
        // ANIMATION OF A SEQUENCE OF FRACTALS  / CALCULATING BITMAP STATES 
        // ================================================================

        /* animation checkbox - if selected, a dialog will appear allowing the user to select multiple bitmap states (xml).
           Each state file is loaded into a State model and added to the States list. The AnimateStates method displays each state,
           and calculates the values needed to draw the bitmaps inbetween the current and next state. 
        */
        private void checkBoxAnimation_CheckedChanged(object sender, EventArgs e)
        {
            States = new List<State>(); // instantiate empty list of states

            if (checkBoxAnimation.Checked) // animation checkbox selected
            {
                var openFileDialog = new OpenFileDialog
                {
                    Filter = "XML files (*.xml)|*.xml", // only display xml files in directory
                    Multiselect = true // select multiple files
                };

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    foreach (String file in openFileDialog.FileNames) // read each selected file
                    {
                        try
                        {
                            var streamReader = new StreamReader(file); // initialise stream reader to read selected xml file
                            using (streamReader) // using statement disposes of system resources automatically
                            {
                                var document = new XmlDocument();
                                document.Load(file);
                                var xnList = document.SelectNodes("/state"); // select parent node

                                foreach (XmlNode xmlNode in xnList) // loop through child nodes to access stored bitmap attributes
                                {
                                    States.Add(new State( // add new state to list of states
                                          Convert.ToDouble(xmlNode["xstart"].InnerText),
                                          Convert.ToDouble(xmlNode["ystart"].InnerText),
                                          Convert.ToDouble(xmlNode["xzoom"].InnerText),
                                          Convert.ToDouble(xmlNode["yzoom"].InnerText),
                                          Convert.ToInt32(xmlNode["j"].InnerText),
                                          file
                                        ));
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            message.Text = String.Format("Failed to load fractal state. {0}", ex.Message); // failed to load, display error message
                        }
                    }
                }
                AnimateStates(); // animate through loaded states and calculate inbetween states
                checkBoxAnimation.Checked = false; // uncheck animation checkbox
            }
        }

        public void AnimateStates()
        {
            for (int i = 0; i < States.Count; i++) // loop through all loaded states
            {
                currentState = States[i]; // get single state from list

                xstart = currentState.Xstart;
                ystart = currentState.Ystart;
                xzoom = currentState.Xzoom;
                yzoom = currentState.Yzoom;
                j = currentState.J;

                Mandelbrot(); // draw first state in list
                Refresh();

                if (States.ElementAtOrDefault(i + 1) != null) // if state exists in list after current state
                {
                    nextState = States[i + 1]; // store the next state in the list

                    if (currentState.Xstart < nextState.Xstart && // current state vales lower than next state?
                           currentState.Ystart < nextState.Ystart &&
                           currentState.Xzoom > nextState.Xzoom &&
                           currentState.Yzoom > nextState.Yzoom
                        )
                    {
                        while (
                          currentState.Xstart <= nextState.Xstart &&  // yes - increase values and animate in 
                          currentState.Ystart <= nextState.Ystart &&  // and draw sequences between current and next states
                          currentState.Xzoom >= nextState.Xzoom &&
                          currentState.Yzoom >= nextState.Yzoom)
                        {
                            // calculate difference between states and divide by four
                            // this value will be used to increment current state values until they exceed the next state values
                            // added 0.0001 to ensure they exceed
                            if (currentState.Xstart <= nextState.Xstart)
                            {
                                var difference = (nextState.Xstart - currentState.Xstart) / 4 + 0.00001;
                                currentState.Xstart += difference;
                            }

                            if (currentState.Ystart <= nextState.Ystart)
                            {
                                var difference = (nextState.Ystart - currentState.Ystart) / 4 + 0.00001;
                                currentState.Ystart += difference;
                            }

                            if (currentState.Xzoom >= nextState.Xzoom)
                            {
                                var difference = (currentState.Xzoom - nextState.Xzoom) / 4 + 0.000001;
                                currentState.Xzoom -= difference;
                            }

                            if (currentState.Yzoom >= nextState.Yzoom)
                            {
                                var difference = (currentState.Yzoom - nextState.Yzoom) / 4 + 0.000001;
                                currentState.Yzoom -= difference;
                            }

                            // each time the current state is changed, redraw using its values
                            xstart = currentState.Xstart;
                            ystart = currentState.Ystart;
                            xzoom = currentState.Xzoom;
                            yzoom = currentState.Yzoom;
                            this.Mandelbrot();
                            this.Refresh();
                        }
                    }
                    else // current state vales higher than next state - decreate values and animate out
                    {
                        while (
                          currentState.Xstart >= nextState.Xstart &&
                          currentState.Ystart >= nextState.Ystart &&
                          currentState.Xzoom <= nextState.Xzoom &&
                          currentState.Yzoom <= nextState.Yzoom)
                        {
                            if (currentState.Xstart >= nextState.Xstart)
                            {
                                var difference = (currentState.Xstart - nextState.Xstart) / 4 + 0.00001;
                                currentState.Xstart -= difference;
                            }

                            if (currentState.Ystart >= nextState.Ystart)
                            {
                                var difference = (currentState.Ystart - nextState.Ystart) / 4 + 0.00001;
                                currentState.Ystart -= difference;
                            }

                            if (currentState.Xzoom <= nextState.Xzoom)
                            {
                                var difference = (nextState.Xzoom - currentState.Xzoom) / 4 + 0.000001;
                                currentState.Xzoom += difference;
                            }

                            if (currentState.Yzoom <= nextState.Yzoom)
                            {
                                var difference = (nextState.Yzoom - currentState.Yzoom) / 4 + 0.000001;
                                currentState.Yzoom += difference;
                            }
                            // each time the current state is changed, redraw using its values
                            xstart = currentState.Xstart;
                            ystart = currentState.Ystart;
                            xzoom = currentState.Xzoom;
                            yzoom = currentState.Yzoom;
                            this.Mandelbrot();
                            this.Refresh();
                        }

                    }
                }
            }
        }

        // ======================================================================
        // ANIMATION OF A SEQUENCE OF FRACTALS  / DISPLAYING LOADED BITMAP STATES 
        // ======================================================================

        /* Sequence of fractals - user can select multiple bitmap states in xml from a dialog menu,
           which are stored in States list. Each time the timerSequenceOfFractals_Tick() method is called,
           ticks is incremented. The value of ticks is used to select the next state from the States list.
           The user can change the timerSequenceOfFractals timer interval using the domainUpDown form control.
           The user can also use the trackbar to switch between states.
        */
        private void checkBoxSequenceOfFractals_CheckedChanged(object sender, EventArgs e)
        {
            States = new List<State>(); // instantiate empty list of states

            if (checkBoxSequenceOfFractals.Checked) // animation checkbox selected
            {
                var openFileDialog = new OpenFileDialog
                                         {
                                             Filter = "XML files (*.xml)|*.xml", // only display xml files in directory
                                             Multiselect = true // select multiple files
                                         };

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    foreach (String file in openFileDialog.FileNames) // read each selected file
                    {
                        try
                        {
                            var streamReader = new StreamReader(file); // initialise stream reader to read selected xml file
                            using (streamReader) // using statement disposes of system resources automatically
                            {
                                var document = new XmlDocument();
                                document.Load(file);
                                var xnList = document.SelectNodes("/state"); // select parent node

                                foreach (XmlNode xmlNode in xnList) // loop through child nodes to access stored bitmap attributes
                                {
                                    States.Add(
                                        new State( // add new state to list of states
                                            Convert.ToDouble(xmlNode["xstart"].InnerText),
                                            Convert.ToDouble(xmlNode["ystart"].InnerText),
                                            Convert.ToDouble(xmlNode["xzoom"].InnerText),
                                            Convert.ToDouble(xmlNode["yzoom"].InnerText),
                                            Convert.ToInt32(xmlNode["j"].InnerText),
                                            file));
                                }
                            }
                            timerSequenceOfFractals.Start();

                        }
                        catch (Exception ex)
                        {
                            message.Text = String.Format("Failed to load fractal state. {0}", ex.Message); // failed to load, display error message
                        }
                    }
                }
            }
            else
            {
                timerSequenceOfFractals.Stop(); //stop timer if button not checked
                trackBarSequenceOfFractals.Maximum = 0; // no trackbar marks
            }
        }

        private void trackBarSequenceOfFractals_Scroll(object sender, EventArgs e)
        {
            currentState = States[trackBarSequenceOfFractals.Value]; // take trackbar index and redraw the correct bitmap from list using this value

            xstart = currentState.Xstart;
            ystart = currentState.Ystart;
            xzoom = currentState.Xzoom;
            yzoom = currentState.Yzoom;
            j = currentState.J;

            Mandelbrot(); // redraw selected bitmap
            Refresh();
        }

        private void timerSequenceOfFractals_Tick(object sender, EventArgs e)
        {
            if (!States.Any()) // if no states selected from file, return
            {
                return;
            }

            trackBarSequenceOfFractals.Maximum = timerSequenceOfFractals.Enabled ? States.Count - 1 : 0; // set trackbar levels to amount of selected files
            ticks++; // increment ticks each time timer called

            if (ticks > States.Count - 1) // if ticks exceeds the list count, set it back to zero
            {
                ticks = 0;
            }

            trackBarSequenceOfFractals.Value = ticks; // set slider to current list index
            currentState = States[ticks]; // get single state from list

            xstart = currentState.Xstart;
            ystart = currentState.Ystart;
            xzoom = currentState.Xzoom;
            yzoom = currentState.Yzoom;
            j = currentState.J;

            Mandelbrot(); // draw selected state from list
            Refresh();
        }

        private void domainUpDownSequenceOfFractals_SelectedItemChanged(object sender, EventArgs e)
        {
            if (domainUpDownSequenceOfFractals.SelectedItem == null) // set default selected item 
            {
                domainUpDownSequenceOfFractals.SelectedItem = "x1";
            }

            switch (domainUpDownSequenceOfFractals.SelectedItem.ToString()) // get value from user selection and change timer
            {
                case "x2":
                    timerSequenceOfFractals.Interval = 1000;
                    break;
                case "x3":
                    timerSequenceOfFractals.Interval = 300;
                    break;
                default:
                    timerSequenceOfFractals.Interval = 2000;
                    break;
            }
        }

    }
}
