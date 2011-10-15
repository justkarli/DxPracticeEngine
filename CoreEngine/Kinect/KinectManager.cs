using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using OpenNI;
using SlimDX;

namespace CoreEngine.Kinect
{
    public class KinectManager
    {
        public KinectManager()
        {
            try
            {
                _context = new Context(@"..\..\Data\openniconfig.xml");
                _depth_generator = _context.FindExistingNode(NodeType.Depth) as DepthGenerator;
                if (_depth_generator == null)
                    throw new Exception(@"Error in Data\openniconfig.xml - No depth node found.");

                _user_generator = new UserGenerator(_context);
                _skeleton_caps = _user_generator.SkeletonCapability;
                _pose_detect_caps = _user_generator.PoseDetectionCapability;
                _calibration_pose = _skeleton_caps.CalibrationPose;

                // event handler for detection
                _user_generator.NewUser += (_user_generator_NewUser);
                _user_generator.LostUser += (_user_generator_LostUser);
                _pose_detect_caps.PoseDetected += (_pose_detect_caps_PoseDetected);
                _skeleton_caps.CalibrationEnd += (_skeleton_caps_CalibrationEnd);

                _skeleton_caps.SetSkeletonProfile(SkeletonProfile.All);

                // initialize joints 
                _joints = new Dictionary<int, Dictionary<SkeletonJoint, SkeletonJointPosition>>();
                _joint_orientation = new Dictionary<int, Dictionary<SkeletonJoint, SkeletonJointOrientation>>();

                // start generating data
                _user_generator.StartGenerating();

            }catch(Exception ex)
            {
                Console.WriteLine("Error initializing OpenNi.");
                Console.WriteLine(ex.Message);
            }

            // update timer for the depth image
            DispatcherTimer dispatcher_timer = new DispatcherTimer();
            dispatcher_timer.Tick += new EventHandler(dispatcher_timer_Tick);
            dispatcher_timer.Interval = new TimeSpan(0, 0, 0, 0, 10); // update every 10 ms
            dispatcher_timer.Start();
            Console.WriteLine("Finished loading");
        }

        void dispatcher_timer_Tick(object sender, EventArgs e)
        {
            try
            {
                _context.WaitAndUpdateAll();
            }
            catch { }

            UpdateSkeleton();
        }

        void _skeleton_caps_CalibrationEnd(object sender, CalibrationEndEventArgs e)
        {
            if (e.Success)
            {
                _skeleton_caps.StartTracking(e.ID);
                _joints.Add(e.ID, new Dictionary<SkeletonJoint, SkeletonJointPosition>());
                _joint_orientation.Add(e.ID, new Dictionary<SkeletonJoint, SkeletonJointOrientation>());
                Console.WriteLine("Calibration succeeded with user " + e.ID);
            }
            else
            {
                _pose_detect_caps.StartPoseDetection(_calibration_pose, e.ID);
                Console.WriteLine("Calibration failed, restarting pose detection with user " + e.ID);
            }
        }

        void _pose_detect_caps_PoseDetected(object sender, PoseDetectedEventArgs e)
        {
            _pose_detect_caps.StopPoseDetection(e.ID);
            _skeleton_caps.RequestCalibration(e.ID, true);
            Console.WriteLine("Detected pose of user " + e.ID + " start calibrating");
        }

        void _user_generator_LostUser(object sender, UserLostEventArgs e)
        {
            _joints.Remove(e.ID);
            _joint_orientation.Remove(e.ID);
            Console.WriteLine("Lost user " + e.ID);
        }

        void _user_generator_NewUser(object sender, NewUserEventArgs e)
        {
            _pose_detect_caps.StartPoseDetection(_calibration_pose, e.ID);
            Console.WriteLine("Found user: " + e.ID + " looking for pose now");
        }

        private void UpdateSkeleton()
        {
            int[] users = _user_generator.GetUsers();
            foreach(int user in users)
            {
                if(_skeleton_caps.IsTracking(user))
                {
                    Point3D center_of_mass = _user_generator.GetCoM(user);
                    center_of_mass = _depth_generator.ConvertRealWorldToProjective(center_of_mass);
                    SkeletonJointTransformation head = _skeleton_caps.GetSkeletonJoint(user, SkeletonJoint.Head);
                    //_eye = new Vector3(center_of_mass.X, center_of_mass.Y, -center_of_mass.Z);
                    Vector3 head_position = new Vector3(head.Position.Position.X * x_factor, -(head.Position.Position.Y + y_translation) * y_factor, 
                        head.Position.Position.Z * z_factor);

                    _eye = head_position;
                }
            }
        }

        public Vector3 Position { get { return _eye; } }
        public Vector3 NormalizedPosition { get { return _eye/_screenValue; } }
        
        public readonly Vector2 KinectResolution = new Vector2(640.0f, 480.0f);

        private Context _context;
        private DepthGenerator _depth_generator;
        private UserGenerator _user_generator;
        private SkeletonCapability _skeleton_caps;
        private PoseDetectionCapability _pose_detect_caps;

        private String _calibration_pose;

        private Dictionary<int, Dictionary<SkeletonJoint, SkeletonJointPosition>> _joints;
        private Dictionary<int, Dictionary<SkeletonJoint, SkeletonJointOrientation>> _joint_orientation;

        private Matrix _orientation_matrix;
        private Vector3 _eye = new Vector3(0, 0, -1);
        
        private const float x_factor = 0.9f;
        private const float y_factor = 0.57f;
        private const float z_factor = 2.55f;
        private const float y_translation = 200.0f;

        private const float _screenValue = 509;
    }
}
