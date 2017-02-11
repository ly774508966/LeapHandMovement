using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.Diagnostics;
using Leap;

namespace WPFSample
{
    public partial class MainWindow : Window, ILeapEventDelegate
    {
        private Controller controller = new Controller();
        private LeapEventListener listener;
        private Boolean isClosing = false;

        public MainWindow()
        {
            InitializeComponent();
            this.controller = new Controller();
            this.listener = new LeapEventListener(this);
            controller.AddListener(listener);
        }

        delegate void LeapEventDelegate(string EventName);
        public void LeapEventNotification(string EventName)
        {
            if (this.CheckAccess())
            {
                switch (EventName)
                {
                    case "onInit":
                        Debug.WriteLine("Init");
                        break;
                    case "onConnect":
                        this.connectHandler();
                        break;
                    case "onFrame":
                        if (!this.isClosing)
                            this.newFrameHandler(this.controller.Frame());
                        break;
                }
            }
            else
            {
                Dispatcher.Invoke(new LeapEventDelegate(LeapEventNotification), new object[] { EventName });
            }
        }

        void connectHandler()
        {
            this.controller.SetPolicy(Controller.PolicyFlag.POLICY_IMAGES);
            this.controller.EnableGesture(Gesture.GestureType.TYPE_SWIPE);
            this.controller.Config.SetFloat("Gesture.Swipe.MinLength", 100.0f);
        }

        void newFrameHandler(Leap.Frame frame)
        {
            this.displayFrame.Text = frame.Id.ToString();
            this.displayConnection.Text = frame.IsValid.ToString();
            this.displayFingerNumber.Text = frame.Fingers.Count.ToString();
            this.displayHandNumber.Text = frame.Hands.Count.ToString();
            this.displayGestureNumber.Text = frame.Gestures().Count.ToString();
            HandList hands = frame.Hands;

            Hand firstHand = hands[0];
            Leap.Vector position = firstHand.PalmPosition;
            Leap.Vector velocity = firstHand.PalmVelocity;
            Leap.Vector direction = firstHand.Direction;

            this.displayMisc.Text = direction.ToString();
        }

        void MainWindow_Closing(object sender, EventArgs e)
        {
            this.isClosing = true;
            this.controller.RemoveListener(this.listener);
            this.controller.Dispose();
        }
    }

    public interface ILeapEventDelegate
    {
        void LeapEventNotification(string EventName);
    }

    public class LeapEventListener : Listener
    {
        ILeapEventDelegate eventDelegate;

        public LeapEventListener(ILeapEventDelegate delegateObject)
        {
            this.eventDelegate = delegateObject;
        }
        public override void OnInit(Controller controller)
        {
            this.eventDelegate.LeapEventNotification("onInit");
        }
        public override void OnConnect(Controller controller)
        {
            controller.SetPolicy(Controller.PolicyFlag.POLICY_IMAGES);
            controller.EnableGesture(Gesture.GestureType.TYPE_SWIPE);
            this.eventDelegate.LeapEventNotification("onConnect");
        }

        public override void OnFrame(Controller controller)
        {
            this.eventDelegate.LeapEventNotification("onFrame");
        }
        public override void OnExit(Controller controller)
        {
            this.eventDelegate.LeapEventNotification("onExit");
        }
        public override void OnDisconnect(Controller controller)
        {
            this.eventDelegate.LeapEventNotification("onDisconnect");
        }

    }

}