using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Shapes;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using GEditor.Views;

namespace GEditor.Controllers
{
    [Serializable]
    public class LinkController
    {
        //---------------------------------------------
        /// <summary>
        /// first node - start position
        /// </summary>
        private DeviceController from;

        /// <summary>
        /// second node - finish position
        /// </summary>
        private DeviceController to;

        /// <summary>
        /// For add this shape to global container
        /// </summary>
        private MainController graph;

        /// <summary>
        /// main shape
        /// </summary>
        [NonSerialized]
        private Line line;

        /// <summary>
        /// Type of this shape(connection)
        /// </summary>
        private ConnectionType cType;


        /// <summary>
        /// Repaint Line - "lightning". Serial connection
        /// </summary>
        [NonSerialized]
        private Line leftLine;
        [NonSerialized]
        private Line rightLine;
        [NonSerialized]
        private Line startLeftLine;
        [NonSerialized]
        private Line startRightLine;
        [NonSerialized]
        private Brush opaqueBrush;

        /// <summary>
        /// list of sublines
        /// </summary>       
        [NonSerialized]
        private List<Line> lines;

        //---------------------------------------------
        /// <summary>
        /// constructor. 
        /// </summary>
        /// <param name="graph"> ref ro MainController</param>
        /// <param name="from">from device</param>
        /// <param name="to">to device</param>
        /// 
        public LinkController(MainController graph, DeviceController from, DeviceController to, ConnectionType cType)
        {
            this.graph = graph;
            this.cType = cType;
            //lol
            if (from == to)
            {
                return;
            }

            line = new Line();

            line.MouseEnter += new MouseEventHandler(line_MouseEnter);
            line.MouseLeave += new MouseEventHandler(line_MouseLeave);
            line.MouseLeftButtonDown += new MouseButtonEventHandler(line_MouseLeftButtonDown);
            if (cType == ConnectionType.cSerial)
            {
                line.Stroke = Brushes.Red;
                line.StrokeThickness = 0;
            }
            else
            {
                line.StrokeThickness = 2;
                line.Stroke = Brushes.White;
            }

            //create line
            ((Line)line).X1 = 0;
            ((Line)line).Y1 = 0;
            if (cType == ConnectionType.cSerial)
            {
                leftLine = new Line();
                leftLine.Stroke = Brushes.Red;
                leftLine.MouseEnter += new MouseEventHandler(line_MouseEnter);
                leftLine.MouseLeave += new MouseEventHandler(line_MouseLeave);
                leftLine.MouseLeftButtonDown += new MouseButtonEventHandler(line_MouseLeftButtonDown);
                rightLine = new Line();
                rightLine.Stroke = Brushes.Red;
                rightLine.MouseEnter += new MouseEventHandler(line_MouseEnter);
                rightLine.MouseLeave += new MouseEventHandler(line_MouseLeave);
                rightLine.MouseLeftButtonDown += new MouseButtonEventHandler(line_MouseLeftButtonDown);
                this.startLeftLine = new Line();
                this.startLeftLine.Stroke = Brushes.Red;
                this.startLeftLine.MouseEnter += new MouseEventHandler(line_MouseEnter);
                this.startLeftLine.MouseLeave += new MouseEventHandler(line_MouseLeave);
                this.startLeftLine.MouseLeftButtonDown += new MouseButtonEventHandler(line_MouseLeftButtonDown);
                this.startRightLine = new Line();
                this.startRightLine.Stroke = Brushes.Red;
                this.startRightLine.MouseEnter += new MouseEventHandler(line_MouseEnter);
                this.startRightLine.MouseLeave += new MouseEventHandler(line_MouseLeave);
                this.startRightLine.MouseLeftButtonDown += new MouseButtonEventHandler(line_MouseLeftButtonDown);

                rightLine.StrokeThickness = leftLine.StrokeThickness = 2;
                startRightLine.StrokeThickness = startLeftLine.StrokeThickness = 2;
                graph.GraphCanvas.Children.Add(leftLine);
                graph.GraphCanvas.Children.Add(rightLine);
                graph.GraphCanvas.Children.Add(startLeftLine);
                graph.GraphCanvas.Children.Add(startRightLine);
            }

            if (lines == null)
            {
                lines = new List<Line>();
                lines.Add(line);
                if (cType == ConnectionType.cSerial)
                {
                    lines.Add(leftLine);
                    lines.Add(rightLine);
                    lines.Add(startLeftLine);
                    lines.Add(startRightLine);
                }
            }
            opaqueBrush = new SolidColorBrush(Colors.White);
            opaqueBrush.Opacity = 0;
            graph.GraphCanvas.Children.Add(line);

            Canvas.SetZIndex(line, 0);

            this.from = from;
            this.to = to;

            to.DevicePositionChange += new MainController.PointPositionChanged(OnNodePositionChanged);
            from.DevicePositionChange += new MainController.PointPositionChanged(OnNodePositionChanged);
            OnNodePositionChanged(to);
            OnNodePositionChanged(from);
        }
        //-------------------------------------------------------------------------------------
        #region Properties

        public bool IsValid { get; set; }

        public DeviceController From
        {
            get { return from; }
        }

        public DeviceController To
        {
            get { return to; }
        }
        public List<Line> Edge
        {
            get
            {
                return lines;
            }
        }
        public ConnectionType CType
        {
            get { return cType; }
            set { cType = value; }
        }

        #endregion
        //-------------------------------------------------------------------------------------
        /// <summary>
        /// When node position change, rib's position also need to change  
        /// </summary>
        /// <param name="top"></param>
        public void OnNodePositionChanged(DeviceController top)
        {
            int index_i = 0;
            int index_j = 0;
            for (int i = 0; i < from.Connected.Count; i++)
            {
                for (int j = 0; j < to.Connected.Count; j++)
                {
                    if (from.Connected[i] == to.Connected[j])
                    {
                        line = from.Connected[i].Edge[0];
                        index_i = i;
                        index_j = j;
                        break;
                    }
                }
            }
            if (line == null) return;
            if (top == from)
            {
                Canvas.SetLeft(line, from.RelativePosition.X + from.View.Width / 2);
                Canvas.SetTop(line, from.RelativePosition.Y + DeviceView.Radius / 2 + 5.0);
            }
            ((Line)line).X2 = to.RelativePosition.X - from.RelativePosition.X;
            ((Line)line).Y2 = to.RelativePosition.Y - from.RelativePosition.Y;

            //fucking trigonometry
            if (this.cType == ConnectionType.cSerial)
            {
                double u_l = Math.Atan2(((Line)line).X1 - ((Line)line).X2, ((Line)line).Y1 - ((Line)line).Y2);
                double u = Math.PI / 5;//angle
                double u1 = Math.PI / 10;//angle
                try
                {
                    if (from.Connected.Count != 0 && to.Connected.Count != 0)
                    {
                        if (top == from)
                        {
                            leftLine = from.Connected[index_i].Edge[1];
                            rightLine = from.Connected[index_i].Edge[2];
                            startLeftLine = from.Connected[index_i].Edge[3];
                            startRightLine = from.Connected[index_i].Edge[4];
                        }
                        else
                        {
                            if (top == to)
                            {
                                leftLine = to.Connected[index_j].Edge[1];
                                rightLine = to.Connected[index_j].Edge[2];
                                startLeftLine = to.Connected[index_j].Edge[3];
                                startRightLine = to.Connected[index_j].Edge[4];
                            }
                            else
                            {
                                return;
                            }
                        }
                    }
                }
                catch(Exception)
                {
                    //return;
                }

                leftLine.X1 = 3.0 * ((Line)line).X2 / 5.0 + 40 * Math.Sin(u_l);
                leftLine.Y1 = 3.0 * ((Line)line).Y2 / 5.0 + 40 * Math.Cos(u_l);
                leftLine.X2 = 3.0 * ((Line)line).X2 / 5.0 + 60 * Math.Sin(u_l + u1);
                leftLine.Y2 = 3.0 * ((Line)line).Y2 / 5.0 + 60 * Math.Cos(u_l + u1);

                rightLine.X2 = 3.0 * ((Line)line).X2 / 5.0 + 30 * Math.Sin(u_l + u - 1.5);
                rightLine.Y2 = 3.0 * ((Line)line).Y2 / 5.0 + 30 * Math.Cos(u_l + u - 1.5);
                rightLine.X1 = 3.0 * ((Line)line).X2 / 5.0 + 40 * Math.Sin(u_l);
                rightLine.Y1 = 3.0 * ((Line)line).Y2 / 5.0 + 40 * Math.Cos(u_l);

                this.startLeftLine.X1 = 0;
                this.startLeftLine.Y1 = 0;

                this.startLeftLine.X2 = rightLine.X2;
                this.startLeftLine.Y2 = rightLine.Y2;

                this.startRightLine.X1 = leftLine.X2;
                this.startRightLine.Y1 = leftLine.Y2;
                this.startRightLine.X2 = ((Line)line).X2;
                this.startRightLine.Y2 = ((Line)line).Y2;

                Canvas.SetLeft(leftLine, from.RelativePosition.X + to.View.Width / 2);
                Canvas.SetTop(leftLine, from.RelativePosition.Y + DeviceView.Radius / 2 + 5.0);
                Canvas.SetLeft(rightLine, from.RelativePosition.X + to.View.Width / 2);
                Canvas.SetTop(rightLine, from.RelativePosition.Y + DeviceView.Radius / 2 + 5.0);
                Canvas.SetLeft(startLeftLine, from.RelativePosition.X + to.View.Width / 2);
                Canvas.SetTop(startLeftLine, from.RelativePosition.Y + DeviceView.Radius / 2 + 5.0);
                Canvas.SetLeft(startRightLine, from.RelativePosition.X + to.View.Width / 2);
                Canvas.SetTop(startRightLine, from.RelativePosition.Y + DeviceView.Radius / 2 + 5.0);
            }
        }
        //-------------------------------------------------------------------------------------
        /// <summary>
        /// When graph is loaded(deserialize).Alternative of constructor
        /// </summary>
        ///dont work! REFACTOR PLEASE!!!!
        public void OnDeserialized()
        {
            this.line = new Line();
            line.Stroke = Brushes.White;
            line.MouseEnter += new MouseEventHandler(line_MouseEnter);
            line.MouseLeave += new MouseEventHandler(line_MouseLeave);
            line.MouseLeftButtonDown += new MouseButtonEventHandler(line_MouseLeftButtonDown);
            ((Line)line).X1 = 0;
            ((Line)line).Y1 = 0;

            if (this.cType == ConnectionType.cSerial)
            {
                leftLine = new Line();
                leftLine.Stroke = Brushes.Red;
                leftLine.MouseEnter += new MouseEventHandler(line_MouseEnter);
                leftLine.MouseLeave += new MouseEventHandler(line_MouseLeave);
                leftLine.MouseLeftButtonDown += new MouseButtonEventHandler(line_MouseLeftButtonDown);
                leftLine.StrokeThickness = 2;
                ((Line)leftLine).X1 = 0;
                ((Line)leftLine).Y1 = 0;
                rightLine = new Line();
                rightLine.Stroke = Brushes.Red;
                rightLine.MouseEnter += new MouseEventHandler(line_MouseEnter);
                rightLine.MouseLeave += new MouseEventHandler(line_MouseLeave);
                rightLine.MouseLeftButtonDown += new MouseButtonEventHandler(line_MouseLeftButtonDown);
                rightLine.StrokeThickness = 2;
                this.startLeftLine = new Line();
                this.startLeftLine.Stroke = Brushes.Red;
                this.startLeftLine.MouseEnter += new MouseEventHandler(line_MouseEnter);
                this.startLeftLine.MouseLeave += new MouseEventHandler(line_MouseLeave);
                this.startLeftLine.MouseLeftButtonDown += new MouseButtonEventHandler(line_MouseLeftButtonDown);
                startLeftLine.StrokeThickness = 2;
                this.startRightLine = new Line();
                this.startRightLine.Stroke = Brushes.Red;
                this.startRightLine.MouseEnter += new MouseEventHandler(line_MouseEnter);
                this.startRightLine.MouseLeave += new MouseEventHandler(line_MouseLeave);
                this.startRightLine.MouseLeftButtonDown += new MouseButtonEventHandler(line_MouseLeftButtonDown);
                startRightLine.StrokeThickness = 2;
            }
            if (cType == ConnectionType.cSerial)
            {
                line.StrokeThickness = 0;
            }
            else
            {
                line.StrokeThickness = 2;
            }

            //if (isLine)
            //{

            //    if (this.cType == ConnectionType.cSerial)
            //    {
            //        leftLine = new Line();
            //        rightLine = new Line();
            //        rightLine.Stroke = Brushes.Green;
            //        leftLine.Stroke = Brushes.Blue;
            //        rightLine.StrokeThickness = leftLine.StrokeThickness = 2;
            //    }
            //}
            //else
            //{
            //    ((Ellipse)line).Width = 50;
            //    ((Ellipse)line).Height = 50;
            //}
            if (lines == null)
            {
                lines = new List<Line>();
                lines.Add(line);
                if (this.cType == ConnectionType.cSerial)
                {
                    lines.Add(leftLine);
                    lines.Add(rightLine);
                    lines.Add(this.startLeftLine);
                    lines.Add(this.startRightLine);
                }
            }

            to.DevicePositionChange += new MainController.PointPositionChanged(OnNodePositionChanged);
            from.DevicePositionChange += new MainController.PointPositionChanged(OnNodePositionChanged);
            opaqueBrush = new SolidColorBrush(Colors.White);
            opaqueBrush.Opacity = 0;
            OnNodePositionChanged(to);
            OnNodePositionChanged(from);
        }
        //-------------------------------------------------------------------------------------
        /// <summary>
        ///  connetcion's events
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        #region Events
        private void line_MouseLeave(object sender, MouseEventArgs e)
        {
            OnMouseLeave();
        }

        private void line_MouseEnter(object sender, MouseEventArgs e)
        {
            OnMouseEnter();
        }

        private void line_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (graph.IsLinkDelete)
            {
                graph.DeleteLink(this);
                graph.FinishDeleteLink();
            }
        }
        #endregion
        //-------------------------------------------------------------------------------------
        /// <summary>
        /// when shape focused
        /// </summary>
        public void OnMouseEnter()
        {
            line.Stroke = Brushes.Orange;
            if (this.cType == ConnectionType.cSerial)
            {
                rightLine.Stroke = Brushes.Orange;
                leftLine.Stroke = Brushes.Orange;
                this.startLeftLine.Stroke = Brushes.Orange;
                this.startRightLine.Stroke = Brushes.Orange;
            }

            foreach (Shape shape in lines)
            {
                Canvas.SetZIndex(line, 1);
            }
        }
        //-------------------------------------------------------------------------------------
        /// <summary>
        /// when focus leave
        /// </summary>
        public void OnMouseLeave()
        {
            if (this.cType == ConnectionType.cSerial)
            {
                rightLine.Stroke = Brushes.Red;
                leftLine.Stroke = Brushes.Red;
                this.startLeftLine.Stroke = Brushes.Red;
                this.startRightLine.Stroke = Brushes.Red;
            }
            else
            {
                line.Stroke = Brushes.White;
            }
            foreach (Shape shape in lines)
            {
                Canvas.SetZIndex(line, 0);
            }
        }
        //-------------------------------------------------------------------------------------
    }
}
