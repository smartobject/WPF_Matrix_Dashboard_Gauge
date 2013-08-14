using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ModernUIApp2
{
    [TemplatePart(Name = "LayoutRoot", Type = typeof(Grid))]
    public partial class ScreenControl : UserControl
    {
        // The actual data being tracked ...
        public List<TelemetryObject> telemetryCollection;

        // This is just for additional background movement
        public List<GhostObject> ghostCollection;

        // Use a bgworker to read files, so your gui doesnt drag / stop ...
        public BackgroundWorker bgwRefresh = new BackgroundWorker();

        private static Random rand = new Random();

        // Time of last screen refresh...
        private DateTime timeLast = DateTime.Now;

        // Time of last file read / data update...
        private DateTime timeLastRefresh = DateTime.Now;

        // This marks the tracks that are in use so the active data does not get overwritten...
        private int[] xPos = new int[1000];

        public ScreenControl()
        {
            InitializeComponent();
            this.ScreenGrid.Height = m_BoxEnd.Y;
            this.ScreenGrid.Width = m_BoxEnd.X;
            this.telemetryCollection = new List<TelemetryObject>();
            this.ghostCollection = new List<GhostObject>();
            InitializeBackgroundWorker();

            //-- Create Random trailing lines ...
            for (int trailCount = 0; trailCount < 15; trailCount++)
            {
                GhostObject go = new GhostObject();
                int nextGhostTrailx = 0;
                nextGhostTrailx = getNextGhostTrailx();
                int randHeight = rand.Next((int)(m_BoxEnd.Y - (20 * charTall)), 100);
                go.xPos = nextGhostTrailx;
                go.yPos = randHeight;
                ghostCollection.Add(go);
            }

            RefreshTelemetry();
            UpdateScreen();
        }

        public void UpdateScreen()
        {
            // -- This slows it down and scrolls smoothly ...
            int millisecSpeed = 100;
            if (DateTime.Now.AddMilliseconds(-millisecSpeed) >= timeLast)
            {
                timeLast = DateTime.Now;
                InvalidateVisual();
            }

            int refreshSpan = 10000;  // Ten seconds...
            if (DateTime.Now.AddMilliseconds(-refreshSpan) >= timeLastRefresh)
            {
                timeLastRefresh = DateTime.Now;
                if (!bgwRefresh.IsBusy)
                    bgwRefresh.RunWorkerAsync();
            }
        }

        public void RefreshTelemetry()
        {
            // Read telemetry data from files...
            // This is the very-specific part that will be different for each application...
            //int fileCounter = 0;

            //int maxFiles = 5;

            //// -- Catch File/Dir not found exception when not connected to drive --
            //DirectoryInfo dirInfo = new DirectoryInfo(@"F:\u\dist\fixes\xmitlog\");
            //FileSystemInfo[] fileList = dirInfo.GetFileSystemInfos();
            //var orderedFiles = fileList.OrderBy(f => f.Name);

            //foreach (var file in orderedFiles)
            //{
            //    fileCounter++;

            //    if (fileCounter > maxFiles) return;

            //    // read file
            //    string inputLine = "";
            //    using (System.IO.StreamReader fileStream = new System.IO.StreamReader(file.FullName))
            //    {
            //        while ((inputLine = fileStream.ReadLine()) != null)
            //        {
            //            string[] dataArray = inputLine.Split(' ');
            //            if (dataArray.Length >= 6 && dataArray[6] == "Xmit")
            //            {
            //                TelemetryObject newTelem = new TelemetryObject();
            //                newTelem.telemetryData = dataArray[9];
            //                newTelem.xPos = nextPos();
            //                telemetryCollection.Add(newTelem);
            //            }
            //        }
            //        fileStream.Close();
            //    }
            //    // move file away
            //    file.Delete();
            //}

            // -- Debug / Load when not connected to telemetry source --
            TelemetryObject to1 = new TelemetryObject();
            to1.xPos = nextPos();
            to1.yPos = charTall + BorderWidth;
            to1.telemetryData = "alpha";
            telemetryCollection.Add(to1);

            TelemetryObject to2 = new TelemetryObject();
            to2.xPos = nextPos();
            to2.yPos = charTall + BorderWidth;
            to2.telemetryData = "beta";
            telemetryCollection.Add(to2);

            TelemetryObject to3 = new TelemetryObject();
            to3.xPos = nextPos();
            to3.yPos = charTall + BorderWidth;
            to3.telemetryData = "gamma";
            telemetryCollection.Add(to3);
        }

        private float nextPos()
        {
            // Gets the next x position - makes sure items are not too close to each other
            // if too far right, then start back over at left
            bool tooClose = false;
            int tryNext = 2;
            int numTries = 0;
            while (true)
            {
                //-- 2 is horizontal separation setting
                numTries++;
                tooClose = false;
                tryNext = rand.Next((int)this.m_BoxEnd.X - m_BorderWidth - m_RightMargin);
                for (int tryCount = Math.Max(0, tryNext - (int)(2 * charWidth)); tryCount < Math.Min(this.m_BoxEnd.X - m_BorderWidth, tryNext + (int)(2 * charWidth)); tryCount++)
                {
                    if (xPos[tryCount] != 0) tooClose = true;
                }

                if (!tooClose)
                {
                    xPos[tryNext] = 1;
                    return tryNext;
                }
                if (numTries > 500) return 0;
            }
        }

        static ScreenControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ScreenControl), new FrameworkPropertyMetadata(typeof(ScreenControl)));
        }

        // Starting point from which to draw the control
        // Note: there is no ClientRectangle in WPF, so a manual Rectangle is built
        private Point m_Start = new Point(0, 0);
        private Point m_BoxEnd = new Point(300, 300);
        private Brush m_BackColor = new SolidColorBrush(Colors.Black);
        private int m_YStart = 10;

        // Points at which to draw the status text box
        private Point m_txtStatusStart = new Point(100, 15);

        private Point m_txtStatusEnd = new Point(200, 25);
        private Brush m_TextBoxBrush = new SolidColorBrush(Colors.Black);

        // Main outer border
        private Brush m_BorderColor = new SolidColorBrush(Colors.Red);

        private int m_BorderWidth = 2;

        private int m_LeftMargin = 10;

        // Colors for the various data items
        private SolidColorBrush activeBrush = new SolidColorBrush(Colors.White);

        private SolidColorBrush printBrush = new SolidColorBrush(Colors.LightGreen);
        private SolidColorBrush ghostBrush = new SolidColorBrush(Colors.Green);
        private SolidColorBrush trailBrush = new SolidColorBrush(Colors.LightGreen);

        //-- Behaviour parameters --
        private float charTall = 12.5F;

        private float charWidth = 5F;
        private float m_ScrollSpeed = 7F;
        private int m_RightMargin = 20;
        private float m_GhostScrollSpeed = 5F;
        private string m_GhostFont = "Katakana";  // Note: Need to install this font
        private float m_SpeedMultiplier = 2F;
        private int m_TailFrontLength = 4;
        private static int m_TailSize = 20;

        //---------- Public Properties ---------
        //
        public Brush BackColor
        {
            get
            {
                return m_BackColor;
            }
            set
            {
                m_BackColor = value;
            }
        }

        public System.Windows.Media.FontFamily Font
        {
            get
            {
                return Font;
            }
            set
            {
                Font = value;
            }
        }

        [System.ComponentModel.Browsable(true),
        System.ComponentModel.Category("Appearance"),
        System.ComponentModel.Description("The color of the outer border.")]
        public Brush BorderColor
        {
            get
            {
                return m_BorderColor;
            }
            set
            {
                m_BorderColor = value;
            }
        }

        [System.ComponentModel.Browsable(true),
        System.ComponentModel.Category("Appearance"),
        System.ComponentModel.Description("The width of the outer border.")]
        public int BorderWidth
        {
            get
            {
                return m_BorderWidth;
            }
            set
            {
                m_BorderWidth = value;
            }
        }

        [System.ComponentModel.Browsable(true),
        System.ComponentModel.Category("Appearance"),
        System.ComponentModel.Description("The left side clear space.")]
        public int LeftMargin
        {
            get
            {
                return m_LeftMargin;
            }
            set
            {
                m_LeftMargin = value;
            }
        }

        [System.ComponentModel.Browsable(true),
        System.ComponentModel.Category("Appearance"),
        System.ComponentModel.Description("The start of the control relative to client area.")]
        public Point StartingPoint
        {
            get
            {
                return m_Start;
            }
            set
            {
                if (m_Start != value)
                {
                    m_Start = value;
                }
            }
        }

        //  --------- Events ---------
        //
        public class ValueChangedEventArgs : EventArgs
        {
            public Int32 valueInRange;

            public ValueChangedEventArgs(Int32 valueInRange)
            {
                this.valueInRange = valueInRange;
            }
        }

        protected override void OnRender(DrawingContext dc)
        {
            m_BoxEnd.X = this.ScreenGrid.Width;
            m_BoxEnd.Y = this.ScreenGrid.Height;

            SolidColorBrush backgroundBrush = new SolidColorBrush();
            backgroundBrush = BackColor as SolidColorBrush;
            Pen outerRectPen = new Pen(m_BorderColor, m_BorderWidth);

            // Main outer rectangle ...
            Rect outerRect = new Rect(m_Start, m_BoxEnd);
            dc.DrawRectangle(backgroundBrush, outerRectPen, outerRect);

            FormattedText printText;

            List<TelemetryObject> dispTelemetry = new List<TelemetryObject>();
            dispTelemetry = telemetryCollection;

            foreach (TelemetryObject thisObj in dispTelemetry.ToList<TelemetryObject>())
            {
                // Increment Y value
                thisObj.yPos++;

                //------ Advance the active item -------
                thisObj.yPos = thisObj.yPos + (m_ScrollSpeed / m_SpeedMultiplier);
                printBrush = activeBrush;

                // -- Display the active data ...
                char[] printArray = thisObj.telemetryData.ToCharArray();
                for (int printCount = 0; printCount < thisObj.telemetryData.Length; printCount++)
                {
                    // Print the Item Vertical
                    printText = new FormattedText(
                        printArray[printCount].ToString().ToUpper(),
                        CultureInfo.GetCultureInfo("en-us"),
                        FlowDirection.LeftToRight,
                        new Typeface("Georgia"),
                        10,
                        printBrush);
                    // Do not draw outside the box ...
                    //          if ( (thisObj.yPos + (printCount * charTall)) < m_BoxEnd.Y)
                    if (((thisObj.yPos - (int)(printCount * charTall)) + BorderWidth < m_BoxEnd.Y) && ((thisObj.yPos - (int)printCount * charTall) > m_Start.Y + m_YStart))
                        dc.DrawText(printText, new Point(thisObj.xPos, thisObj.yPos + (printCount * charTall)));
                }

                //-- Trail behind the active item ...
                //if (thisObj.yPos > (thisObj.telemetryData.Length * charTall))
                for (float trailCount = 0; trailCount < (thisObj.tailArray.Length - 1); trailCount++)
                {
                    if (trailCount <= m_TailFrontLength) printBrush = trailBrush;
                    else
                        printBrush = ghostBrush;
                    printText = new FormattedText(
                        thisObj.tailArray[(int)trailCount].ToString(),
                        CultureInfo.GetCultureInfo("en-us"),
                        FlowDirection.LeftToRight,
                        new Typeface("Verdana"),
                        10,
                        printBrush);
                    // Do not draw outside the box ...  Bottom && Top
                    if (((thisObj.yPos - (int)(trailCount * charTall)) + BorderWidth < m_BoxEnd.Y) && ((thisObj.yPos - (int)trailCount * charTall) > m_Start.Y + m_YStart))
                    {
                        dc.DrawText(printText, new Point(thisObj.xPos, ((thisObj.yPos - charTall) - (int)trailCount * charTall)));
                    }
                }
            }

            // -- Clear out any items that have scrolled off screen...
            List<TelemetryObject> cleanedCollection = new List<TelemetryObject>();
            foreach (TelemetryObject thisObj in telemetryCollection)
            {
                if ((m_BoxEnd.Y + (thisObj.tailArray.Length * charTall) + (thisObj.telemetryData.Length * charTall)) > thisObj.yPos)
                {
                    cleanedCollection.Add(thisObj);
                }
                else
                {
                    xPos[(int)thisObj.xPos] = 0;
                    //printStatus("Remove: " + thisObj.telemetryData, dc);
                }
            }
            telemetryCollection = cleanedCollection;

            foreach (GhostObject ghostTrail in ghostCollection)
            {
                // Advance ghost scrollers
                ghostTrail.yPos = ghostTrail.yPos + (m_GhostScrollSpeed / m_SpeedMultiplier);
                printBrush = ghostBrush;
                char[] printArray = ghostTrail.tailArray;
                for (int printCount = 0; printCount < ghostTrail.tailArray.Length; printCount++)
                {
                    // Print the Item Vertical
                    printText = new FormattedText(
                    printArray[printCount].ToString(),
                              CultureInfo.GetCultureInfo("en-us"),
                              FlowDirection.LeftToRight,
                              new Typeface(m_GhostFont),
                              10,
                              printBrush);
                    // Do not draw outside the box ...  Bottom && Top
                    if (((ghostTrail.yPos - (int)(printCount * charTall)) + BorderWidth < m_BoxEnd.Y) && ((ghostTrail.yPos - (int)printCount * charTall) > m_Start.Y + m_YStart))
                    {
                        dc.DrawText(printText, new Point(ghostTrail.xPos, ((ghostTrail.yPos - charTall) - (int)printCount * charTall)));
                    }
                }
            }
            // -- Clear out any items that have scrolled off screen...
            List<GhostObject> cleanedGhostCollection = new List<GhostObject>();
            foreach (GhostObject thisObj in ghostCollection)
            {
                if ((m_BoxEnd.Y + (thisObj.tailArray.Length * charTall)) > thisObj.yPos)
                {
                    cleanedGhostCollection.Add(thisObj);
                }
                else
                {
                    // Replace the one removed...
                    GhostObject newGO = new GhostObject();
                    int nextGhostTrailx = 0;
                    nextGhostTrailx = getNextGhostTrailx();
                    int randHeight = rand.Next((int)(m_BoxEnd.Y - (20 * charTall)), 100);
                    newGO.xPos = nextGhostTrailx;
                    newGO.yPos = randHeight;
                    cleanedGhostCollection.Add(newGO);
                }
            }
            ghostCollection = cleanedGhostCollection;
        }

        private int getNextGhostTrailx()
        {
            int nextNum = 0;
            for (int newCount = 0; newCount < m_BoxEnd.X - BorderWidth; newCount++)
            {
                nextNum = rand.Next((int)m_BoxEnd.X - BorderWidth);
                if (xPos[nextNum] == 0) return nextNum;
            }
            return 0;
        }

        public static string getRandomString()
        {
            StringBuilder strRand = new StringBuilder();
            int t;
            int randLength = m_TailSize;
            char nextChar = ' ';

            for (int iCount = 0; iCount < randLength; iCount++)
            {
                t = rand.Next(10);
                if (t <= 2)
                    nextChar = (char)('0' + rand.Next(10));
                else if (t <= 4)
                    nextChar = (char)('a' + rand.Next(27));
                else if (t <= 6)
                    nextChar = (char)('A' + rand.Next(27));
                else
                    nextChar = (char)(rand.Next(32, 125));
                if (nextChar != ' ')
                    strRand.Append(nextChar);
            }

            return strRand.ToString();
        }

        private void InitializeBackgroundWorker()
        {
            bgwRefresh.WorkerReportsProgress = false;
            bgwRefresh.WorkerSupportsCancellation = false;

            bgwRefresh.DoWork +=
                new DoWorkEventHandler(bgwRefresh_DoWork);
            bgwRefresh.RunWorkerCompleted +=
                new RunWorkerCompletedEventHandler(bgwRefresh_WorkCompleted);
        }

        private void bgwRefresh_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            RefreshTelemetry();
        }

        private void bgwRefresh_WorkCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
        }
    }

    public class TelemetryObject
    {
        public float xPos { get; set; }
        public float yPos { get; set; }
        public string telemetryData { get; set; }
        public char[] tailArray { get; set; }

        public TelemetryObject()
        {
            this.tailArray = ScreenControl.getRandomString().ToCharArray();
        }
    }

    // Ghost object is the random chars dropping in background...
    public class GhostObject
    {
        public float xPos { get; set; }
        public float yPos { get; set; }
        public char[] tailArray { get; set; }

        public GhostObject()
        {
            this.tailArray = ScreenControl.getRandomString().ToCharArray();
        }
    }
}