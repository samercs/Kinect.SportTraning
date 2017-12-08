using System.Linq;
using LightBuzz.Vitruvius;
using Microsoft.Kinect;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;

namespace LightBuzz.Vituvius.Samples.WPF
{
    /// <summary>
    /// Interaction logic for AnglePage.xaml
    /// </summary>
    public partial class AnglePage03 : Page
    {
        KinectSensor _sensor;
        MultiSourceFrameReader _reader;
        PlayersController _playersController;

        JointType _start1 = JointType.ElbowRight;
        JointType _center1 = JointType.SpineShoulder;
        JointType _end1 = JointType.SpineBase;

        JointType _start2 = JointType.KneeLeft;
        JointType _center2 = JointType.SpineBase;
        JointType _end2 = JointType.KneeRight;

        private int PassCount { get; set; } = 0;
        private bool StartOne { get; set; } = false;

        public AnglePage03()
        {
            InitializeComponent();

            _sensor = KinectSensor.GetDefault();

            if (_sensor != null)
            {
                _sensor.Open();

                _reader = _sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Depth | FrameSourceTypes.Infrared | FrameSourceTypes.Body);
                _reader.MultiSourceFrameArrived += Reader_MultiSourceFrameArrived;

                _playersController = new PlayersController();
                _playersController.BodyEntered += UserReporter_BodyEntered;
                _playersController.BodyLeft += UserReporter_BodyLeft;
                _playersController.Start();
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_playersController != null)
            {
                _playersController.Stop();
            }

            if (_reader != null)
            {
                _reader.Dispose();
            }

            if (_sensor != null)
            {
                _sensor.Close();
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }

        void Reader_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            var reference = e.FrameReference.AcquireFrame();

            // Color
            using (var frame = reference.ColorFrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    if (viewer.Visualization == Visualization.Color)
                    {
                        viewer.Image = frame.ToBitmap();
                    }
                }
            }

            // Body
            using (var frame = reference.BodyFrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    var bodies = frame.Bodies();

                    _playersController.Update(bodies);

                    Body body = bodies.First();

                    if (body != null)
                    {
                        viewer.DrawBody(body);

                        angle1.Update(body.Joints[_start1], body.Joints[_center1], body.Joints[_end1], 50);
                        angle2.Update(body.Joints[_start2], body.Joints[_center2], body.Joints[_end2], 50);

                        tblAngle1.Text = ((int)angle1.Angle).ToString();
                        tblAngle2.Text = ((int)angle2.Angle).ToString();

                        if ((int)angle1.Angle >= 95)
                        {
                            lblMsg.Content = "Please move your hand down";
                            lblMsg.Foreground = Brushes.Red;
                        }
                        else if ((int)angle1.Angle <= 95 && (int)angle1.Angle >= 80)
                        {
                            if ((int) angle2.Angle <= 310 && (int) angle2.Angle >= 300)
                            {
                                lblMsg.Content = "Greate ... !";
                                lblMsg.Foreground = Brushes.Green;
                                if (StartOne)
                                {
                                    ++PassCount;
                                    TblCount.Text = PassCount.ToString();
                                    StartOne = false;
                                }
                            }
                            else if ((int) angle2.Angle > 310)
                            {
                                lblMsg.Content = "Please move the two legs away from each other";
                                lblMsg.Foreground = Brushes.Red;
                            }
                            else
                            {
                                lblMsg.Content = "Please move the two legs closer";
                                lblMsg.Foreground = Brushes.Red;
                            }
                        }
                        else if ((int)angle1.Angle <= 40)
                        {
                            StartOne = true;
                            lblMsg.Content = "Please move your hand up";
                            lblMsg.Foreground = Brushes.Red;
                        }
                        else
                        {
                            lblMsg.Content = "Please move your hand up";
                            lblMsg.Foreground = Brushes.Red;
                        }

                    }
                }
            }
        }

        void UserReporter_BodyEntered(object sender, PlayersControllerEventArgs e)
        {
            if (e.Players.Count >= 2)
            {
                viewer.Clear();
                angle1.Clear();
                tblAngle1.Text = "-";
                PassCount = 0;
                TblCount.Text = PassCount.ToString();
                lblMsg.Content = "We Just Allow Single Player";
            }
        }

        void UserReporter_BodyLeft(object sender, PlayersControllerEventArgs e)
        {
            viewer.Clear();
            angle1.Clear();
            tblAngle1.Text = "-";
            EmailService.SendEmail(PassCount, "Shoulder And Legs");
            PassCount = 0;
            TblCount.Text = PassCount.ToString();
        }
    }
}
