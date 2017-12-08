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
    public partial class AnglePage04 : Page
    {
        KinectSensor _sensor;
        MultiSourceFrameReader _reader;
        PlayersController _playersController;

        JointType _start1 = JointType.HandRight;
        JointType _center1 = JointType.ElbowRight;
        JointType _end1 = JointType.ShoulderRight;

        JointType _start2 = JointType.AnkleLeft;
        JointType _center2 = JointType.KneeLeft;
        JointType _end2 = JointType.HipLeft;

        private int PassCount { get; set; } = 0;
        private bool StartOne { get; set; } = false;

        public AnglePage04()
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

                        if ((int)angle1.Angle > 70)
                        {
                            lblMsg.Content = "Please move your right hand";
                            lblMsg.Foreground = Brushes.Red;
                        }
                        else if ((int)angle1.Angle <= 70 && (int)angle1.Angle >= 50)
                        {
                            if ((int)angle2.Angle <= 135 && (int)angle2.Angle >= 115)
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
                            else if ((int)angle2.Angle > 135)
                            {
                                lblMsg.Content = "Please move the left legs";
                                lblMsg.Foreground = Brushes.Red;
                            }
                            else
                            {
                                lblMsg.Content = "Please move the left legs";
                                lblMsg.Foreground = Brushes.Red;
                            }
                        }
                        else if ((int)angle1.Angle < 50)
                        {
                            StartOne = true;
                            lblMsg.Content = "Please move your right hand";
                            lblMsg.Foreground = Brushes.Red;
                        }
                        else
                        {
                            lblMsg.Content = "Please move your right hand";
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
            EmailService.SendEmail(PassCount, "Special Training");
            PassCount = 0;
            TblCount.Text = PassCount.ToString();
        }
    }
}
