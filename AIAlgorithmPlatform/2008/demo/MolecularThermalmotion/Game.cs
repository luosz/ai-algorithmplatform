﻿using System;
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
using System.Windows.Threading;
using System.Windows.Media.Media3D;
using Petzold.Media3D;

namespace MolecularThermalmotion
{
    class Game
    {
        GameWindows gameWindows = null; //new GameWindows();

        TimerPump timerPump;

        List<Molecule> MoleculeSet = new List<Molecule>();

        CollisionDetectionEngine CDE = new CollisionDetectionEngine();

        int moleculeNum = 21;
        public int MoleculeNum
        {
            get { return moleculeNum; }
            set { moleculeNum = value; }
        }

        double positionRange = 50;


        public double PositionRange
        {
            get { return positionRange; }
            set { positionRange = value; }
        }
        double velocityRange = 50;
        public double VelocityRange
        {
            get { return velocityRange; }
            set { velocityRange = value; }
        }

        double radius = 4;
        public double Radius
        {
            get { return radius; }
            set { radius = value; }
        }

        double length = 200;
        public double Length
        {
            get { return length; }
            set { length = value; }
        }

        double width = 100;
        public double Width
        {
            get { return width; }
            set { width = value; }
        }

        double height = 100;
        public double Height
        {
            get { return height; }
            set { height = value; }
        }

        Molecule whiteBall = null;//白球
        public Molecule WhiteBall
        {
            get { return whiteBall; }
        }

        Vector3D shotDirection = new Vector3D(-1, 0, 0);
        public Vector3D ShotDirection
        {
            get { return shotDirection; }
            set {

                shotDirection = value; 
                stick.UpdateStickPosition(new Vector3D(whiteBall.position.X, whiteBall.position.Y, whiteBall.position.Z), shotDirection);                
            }
        }

        double shootForceFactor = 100;
        public double ShootForceFactor
        {
            get { return shootForceFactor; }
            set { shootForceFactor = value; }
        }

        double e = 0.8;

        Stick stick = new Stick();

        Model3DGroup moleculeModel3DGroup;

        Point3D gridMapOrigin;

        Random random = new Random();
        private double GetRandomInRange(double range)
        {
            return ((random.NextDouble() - 0.5) * 2 * range);
        }


        void Initializtion()
        {
            //画星空
            {
                SphereMesh StarMesh = new SphereMesh();
                StarMesh.Slices = 8;
                StarMesh.Stacks = 4;
                StarMesh.Radius = 0.6;
                MeshGeometry3D Star = StarMesh.Geometry;

                var Star3DGroup = new Model3DGroup();
                Material specularMaterial = new SpecularMaterial(new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)), 1024);

                for (int i = 0; i < 1500; i++)
                {
                    //创建每个球体的GeometryModel并添加进显示窗口
                    MaterialGroup materialGroup = new MaterialGroup();

                    byte value = (byte)random.Next(150, 255);

                    DiffuseMaterial diffuseMaterial = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(value, value, value)));
                    materialGroup.Children.Add(diffuseMaterial);
                    materialGroup.Children.Add(specularMaterial);

                    var StarGeometryModel = new GeometryModel3D(Star, materialGroup);

                    Vector3D starPosition;

                    while (true)
                    {
                        double range = 300;
                        double x = GetRandomInRange(range);
                        double y = GetRandomInRange(range);
                        double z = GetRandomInRange(range);

                        if (Math.Abs(x) < Length && Math.Abs(y) < Width * 2 && Math.Abs(z) < Height * 2)
                        {
                            continue;
                        }
                        else
                        {
                            starPosition = new Vector3D(x, y, z);
                            break;
                        }
                    }

                    StarGeometryModel.Transform = new TranslateTransform3D(starPosition);

                    Star3DGroup.Children.Add(StarGeometryModel);
                }

                //StarMesh.Slices = 8;
                //StarMesh.Stacks = 4;
                //StarMesh.Radius = 2;
                //Star = StarMesh.Geometry;

                //for (int i = 0; i < 500; i++)
                //{
                //    //创建每个球体的GeometryModel并添加进显示窗口
                //    MaterialGroup materialGroup = new MaterialGroup();

                //    byte value = (byte)random.Next(150, 255);

                //    DiffuseMaterial diffuseMaterial = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(value, value, value)));
                //    materialGroup.Children.Add(diffuseMaterial);
                //    materialGroup.Children.Add(specularMaterial);

                //    var StarGeometryModel = new GeometryModel3D(Star, materialGroup);

                //    Vector3D starPosition;

                //    while (true)
                //    {
                //        double range = 600;
                //        double x = GetRandomInRange(range);
                //        double y = GetRandomInRange(range);
                //        double z = GetRandomInRange(range);

                //        if (Math.Abs(x) < Length && Math.Abs(y) < Width * 1.5 && Math.Abs(z) < Height * 1.5)
                //        {
                //            continue;
                //        }
                //        else
                //        {
                //            starPosition = new Vector3D(x, y, z);
                //            break;
                //        }
                //    }

                //    StarGeometryModel.Transform = new TranslateTransform3D(starPosition);

                //    Star3DGroup.Children.Add(StarGeometryModel);
                //}

                {
                    ModelVisual3D starSetModel = new ModelVisual3D();
                    starSetModel.Content = Star3DGroup;
                    gameWindows.model.Children.Add(starSetModel);
                }
            }

            //画球台
            {
                BoxMesh boardMeshBase = new BoxMesh();

                boardMeshBase.Width = Length;
                boardMeshBase.Height = Height;
                boardMeshBase.Depth = Width;

                Geometry3D boardMesh = boardMeshBase.Geometry;
                GeometryModel3D boardGeometryModel = new GeometryModel3D(boardMesh, null);

                boardGeometryModel.Material = null;//材料

                ImageBrush imageBrush = new ImageBrush();
                imageBrush.ImageSource = new BitmapImage(new Uri("res/board.jpg", UriKind.Relative));
                //imageBrush.Viewbox = new Rect(0, 0, imageBrush.ImageSource.Width, imageBrush.ImageSource.Height);
                imageBrush.Viewbox = new Rect(0, 0, 100, 100);
                //imageBrush.TileMode = TileMode.FlipXY;
                imageBrush.TileMode = TileMode.Tile;
                imageBrush.ViewboxUnits = BrushMappingMode.Absolute;
                imageBrush.Viewport = new Rect(0, 0, 0.1, 0.1);

                MaterialGroup materialGroup = new MaterialGroup();
                //materialGroup.Children.Add(new SpecularMaterial(new SolidColorBrush(Colors.Gray), 1024));
                //materialGroup.Children.Add(new DiffuseMaterial(new SolidColorBrush(Colors.GreenYellow)));
                materialGroup.Children.Add(new DiffuseMaterial(imageBrush));

                boardGeometryModel.BackMaterial = materialGroup;

                ModelVisual3D moleculeSetModel = new ModelVisual3D();
                moleculeSetModel.Content = boardGeometryModel;
                gameWindows.model.Children.Add(moleculeSetModel);
            }

            gridMapOrigin = new Point3D(-0.5 * length, -0.5 * width, -0.5 * height);

            //画球体
            {
                SphereMesh sphereMesh = new SphereMesh();
                sphereMesh.Slices = 72 / 1;
                sphereMesh.Stacks = 36 / 1;
                sphereMesh.Radius = radius;
                MeshGeometry3D sphere = sphereMesh.Geometry;


                //用于保存所有小球的Model
                moleculeModel3DGroup = new Model3DGroup();

                Material material = (Material)gameWindows.viewport.Resources["ER_Vector___Glossy_Yellow___MediumMR2"];
                Material specularMaterial = new SpecularMaterial(new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)), 1024);

                //定义二维数组，分别存放molelecule的数量，和x,y,z的坐标
                double[,] moleculePositionSet = new double[moleculeNum, 3];
                moleculePositionSet[0, 0] = 50;
                moleculePositionSet[0, 1] = 0;
                moleculePositionSet[0, 2] = 0;
                //layer1 begin
                moleculePositionSet[1, 0] = 0.0000000001;
                moleculePositionSet[1, 1] = 0;
                moleculePositionSet[1, 2] = 0;
                //layer2
                moleculePositionSet[2, 0] = -8;
                moleculePositionSet[2, 1] = 4.8;
                moleculePositionSet[2, 2] = 0;
                moleculePositionSet[3, 0] = -8;
                moleculePositionSet[3, 1] = -2.4;
                moleculePositionSet[3, 2] = 4;
                moleculePositionSet[4, 0] = -8;
                moleculePositionSet[4, 1] = -2.4;
                moleculePositionSet[4, 2] = -4;
                //layer3
                moleculePositionSet[5, 0] = -16;
                moleculePositionSet[5, 1] = 9.6;
                moleculePositionSet[5, 2] = 0;
                moleculePositionSet[6, 0] = -16;
                moleculePositionSet[6, 1] = 2.4;
                moleculePositionSet[6, 2] = 4;
                moleculePositionSet[7, 0] = -16;
                moleculePositionSet[7, 1] = 2.4;
                moleculePositionSet[7, 2] = -4;

                moleculePositionSet[8, 0] = -16;
                moleculePositionSet[8, 1] = -4.8;
                moleculePositionSet[8, 2] = 0;
                moleculePositionSet[9, 0] = -16;
                moleculePositionSet[9, 1] = -4.8;
                moleculePositionSet[9, 2] = 8;
                moleculePositionSet[10, 0] = -16;
                moleculePositionSet[10, 1] = -4.8;
                moleculePositionSet[10, 2] = -8;

                //layer4 begin
                moleculePositionSet[11, 0] = -24;
                moleculePositionSet[11, 1] = 14.4;
                moleculePositionSet[11, 2] = 0;
                moleculePositionSet[12, 0] = -24;
                moleculePositionSet[12, 1] = 7.2;
                moleculePositionSet[12, 2] = 4;
                moleculePositionSet[13, 0] = -24;
                moleculePositionSet[13, 1] = 7.2;
                moleculePositionSet[13, 2] = -4;
                moleculePositionSet[14, 0] = -24;
                moleculePositionSet[14, 1] = 0;
                moleculePositionSet[14, 2] = 0;
                moleculePositionSet[15, 0] = -24;
                moleculePositionSet[15, 1] = 0;
                moleculePositionSet[15, 2] = 8;
                moleculePositionSet[16, 0] = -24;
                moleculePositionSet[16, 1] = 0;
                moleculePositionSet[16, 2] = -8;
                moleculePositionSet[17, 0] = -24;
                moleculePositionSet[17, 1] = -7.2;
                moleculePositionSet[17, 2] = 4;
                moleculePositionSet[18, 0] = -24;
                moleculePositionSet[18, 1] = -7.2;
                moleculePositionSet[18, 2] = -4;
                moleculePositionSet[19, 0] = -24;
                moleculePositionSet[19, 1] = -7.2;
                moleculePositionSet[19, 2] = 12;
                moleculePositionSet[20, 0] = -24;
                moleculePositionSet[20, 1] = -7.2;
                moleculePositionSet[20, 2] = -12;

                for (int i = 0; (i < moleculeNum); i++)
                {
                    //创建并初始化molecule的属性
                    Molecule molecule = new Molecule()
                    {
                        //position = new Point3D(GetRandomInRange(positionRange), GetRandomInRange(positionRange), GetRandomInRange(positionRange)),
                        position = new Point3D(moleculePositionSet[i, 0], moleculePositionSet[i, 1], moleculePositionSet[i, 2]),
                        //currentVelocity = new Vector3D(GetRandomInRange(velocityRange), GetRandomInRange(velocityRange), GetRandomInRange(velocityRange)),
                        mass = 1,
                        radius = radius
                    };

                    //创建每个球体的GeometryModel并添加进显示窗口
                    MaterialGroup materialGroup = new MaterialGroup();

                    int R,G,B;

                    while (true)
                    {
                        R = random.Next(255);
                        G = random.Next(255);
                        B = random.Next(255);

                        if(R + G + B < 350)
                        {
                            continue;
                        }
                        else
                        {
                            break;
                        }
                    }

                    DiffuseMaterial diffuseMaterial = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb((byte)R, (byte)G, (byte)B)));
                    materialGroup.Children.Add(diffuseMaterial);
                    materialGroup.Children.Add(specularMaterial);

                    molecule.MoleculeGeometryModel = new GeometryModel3D(sphere, materialGroup);
                    //molecule.MoleculeGeometryModel.BackMaterial = materialGroup;

                    molecule.MoleculeGeometryModel.Transform = new TranslateTransform3D(molecule.position.X, molecule.position.Y, molecule.position.Z);

                    moleculeModel3DGroup.Children.Add(molecule.MoleculeGeometryModel);

                    MoleculeSet.Add(molecule);
                }

                {
                    ModelVisual3D moleculeSetModel = new ModelVisual3D();
                    moleculeSetModel.Content = moleculeModel3DGroup;
                    gameWindows.model.Children.Add(moleculeSetModel);
                }

                //设置白球
                whiteBall = MoleculeSet[0];
                ((MaterialGroup)whiteBall.MoleculeGeometryModel.Material).Children[0] = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(255, 255, 255)));
                //Material specularMaterial = new SpecularMaterial(new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)), 1024);
                //DiffuseMaterial white= new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(255,255,255)));
                //whiteBall.Add(white);
            }

            //画球棍
            stick.Length = 120;
            stick.Init(gameWindows.model, new Vector3D(whiteBall.position.X, whiteBall.position.Y, whiteBall.position.Z), shotDirection);
            stick.Visible = false;

            //初始化碰撞检测引擎
            CDE.InitCollisionDetectionEngine(MoleculeSet, gridMapOrigin, length, width, height, radius * 2);

            CDE.CollisionResponse += delegate(int index1, int index2)
            {
                gameWindows.PlayBallCollisionSound();
                gameWindows.myMediaElement.Stop();
                gameWindows.myMediaElement.Play();

                PhysicEngine.UpdateVelocityByCollide(MoleculeSet[index1].position, MoleculeSet[index2].position, ref MoleculeSet[index1].currentVelocity, ref MoleculeSet[index2].currentVelocity,
                    MoleculeSet[index1].mass, MoleculeSet[index2].mass, MoleculeSet[index1].radius, MoleculeSet[index2].radius);
            };

            CDE.CollideWithWall += delegate(int index)
            {
                Molecule m = MoleculeSet[index];

                if (m.position.X < gridMapOrigin.X + m.radius)
                {
                    m.position.X = gridMapOrigin.X + m.radius;
                    m.currentVelocity.X = -m.currentVelocity.X;

                    if (BallInHole(m))
                    {
                        BallInHoleList.Add(m);
                        gameWindows.PlayBallCollisionWithWallSound();
                    }
                    else { gameWindows.PlayBallInHoleSound(); }
                }
                if (m.position.X > gridMapOrigin.X + length - m.radius)
                {
                    m.position.X = gridMapOrigin.X + length - m.radius;
                    m.currentVelocity.X = -m.currentVelocity.X;

                    if (BallInHole(m))
                    {
                        BallInHoleList.Add(m);
                        gameWindows.PlayBallCollisionWithWallSound();
                    }
                    else { gameWindows.PlayBallInHoleSound(); }
                }

                if (m.position.Y < gridMapOrigin.Y + m.radius)
                {
                    m.position.Y = gridMapOrigin.Y + m.radius;
                    m.currentVelocity.Y = -m.currentVelocity.Y;

                    if (BallInHole(m))
                    {
                        BallInHoleList.Add(m);
                        gameWindows.PlayBallCollisionWithWallSound();
                    }
                    else { gameWindows.PlayBallInHoleSound(); }
                }
                if (m.position.Y > gridMapOrigin.Y + width - m.radius)
                {
                    m.position.Y = gridMapOrigin.Y + width - m.radius;
                    m.currentVelocity.Y = -m.currentVelocity.Y;

                    if (BallInHole(m))
                    {
                        BallInHoleList.Add(m);
                        gameWindows.PlayBallCollisionWithWallSound();
                    }
                    else { gameWindows.PlayBallInHoleSound(); }
                }

                if (m.position.Z < gridMapOrigin.Z + m.radius)
                {
                    m.position.Z = gridMapOrigin.Z + m.radius;
                    m.currentVelocity.Z = -m.currentVelocity.Z;

                    if (BallInHole(m))
                    {
                        BallInHoleList.Add(m);
                        gameWindows.PlayBallCollisionWithWallSound();
                    }
                    else { gameWindows.PlayBallInHoleSound(); }
                }
                if (m.position.Z > gridMapOrigin.Z + height - m.radius)
                {
                    m.position.Z = gridMapOrigin.Z + height - m.radius;
                    m.currentVelocity.Z = -m.currentVelocity.Z;

                    if (BallInHole(m))
                    {
                        BallInHoleList.Add(m);
                        gameWindows.PlayBallCollisionWithWallSound();
                    }
                    else { gameWindows.PlayBallInHoleSound(); }
                }
            };
        }

        double holeRadius = 20;
        List<Molecule> BallInHoleList = new List<Molecule>();
        bool BallInHole(Molecule m)
        {
            int count = 0;

            if (m.position.X < gridMapOrigin.X + holeRadius)
            {
                count++;
            }
            if (m.position.X > gridMapOrigin.X + length - holeRadius)
            {
                count++;
            }

            if (m.position.Y < gridMapOrigin.Y + holeRadius)
            {
                count++;
            }
            if (m.position.Y > gridMapOrigin.Y + width - holeRadius)
            {
                count++;
            }

            if (m.position.Z < gridMapOrigin.Z + holeRadius)
            {
                count++;
            }
            if (m.position.Z > gridMapOrigin.Z + height - holeRadius)
            {
                count++;
            }

            if (count >= 3)
            {
                return true;
            }

            return false;
        }

        void PerformOneFrame(double timeInterval)
        {
            //把单位转换为秒
            double t = timeInterval / 1000;

            //碰撞检测（如果检测到碰撞在这里改变碰撞后小球的速度矢量）
            CDE.CollisionDetection();

            foreach (Molecule m in BallInHoleList)
            {
                if (m != whiteBall)
                {
                    m.position = new Point3D(0,0,0);
                    m.currentVelocity = new Vector3D(-1,-1,-1);
                    m.radius = 0.1;
                    moleculeModel3DGroup.Children.Remove(m.MoleculeGeometryModel);
                    //MoleculeSet.Remove(m);
                }
            }
            BallInHoleList.Clear();
          
            //通过小球的受力来更新小球的速度（重力，摩擦力，碰撞后瞬间的冲力等）
            UpdateMoleculeSetVelocityByForce(t);

            //用速度来update小球的位置
            UpdateMoleculeSetPositionByVelocity(t);            
        }

        /// <summary>
        /// 通过小球的受力来更新小球的速度（重力，摩擦力，碰撞后瞬间的冲力等）
        /// </summary>
        /// <param name="t"></param>
        private void UpdateMoleculeSetVelocityByForce(double t)
        {
            Vector3D force = new Vector3D();
            double k = 0.5;
            for (int i = 0; i < MoleculeSet.Count; i++)
            {
                Molecule m = MoleculeSet[i];
                //force.X = -k * (m.currentVelocity.X * Math.Abs(m.currentVelocity.X));
                //force.Y = -k * (m.currentVelocity.Y * Math.Abs(m.currentVelocity.Y));
                //force.Z = -k * (m.currentVelocity.Z * Math.Abs(m.currentVelocity.Z));

                force.X = (m.currentVelocity.X > 0 ? -1 : 1) * k * Math.Pow(Math.Abs(m.currentVelocity.X), 1);
                force.Y = (m.currentVelocity.Y > 0 ? -1 : 1) * k * Math.Pow(Math.Abs(m.currentVelocity.Y), 1);
                force.Z = (m.currentVelocity.Z > 0 ? -1 : 1) * k * Math.Pow(Math.Abs(m.currentVelocity.Z), 1);

                var tempVector = (force / m.mass) * t;

                if (Math.Abs(tempVector.X) > Math.Abs(m.currentVelocity.X))
                {
                    m.currentVelocity.X = 0;
                }
                else
                {
                    m.currentVelocity.X += tempVector.X;
                    if (Math.Abs(m.currentVelocity.X) < e)
                    { m.currentVelocity.X = 0; }
                }

                if (Math.Abs(tempVector.Y) > Math.Abs(m.currentVelocity.Y))
                {
                    m.currentVelocity.Y = 0;
                }
                else
                {
                    m.currentVelocity.Y += tempVector.Y;
                    if (Math.Abs(m.currentVelocity.Y) < e)
                    { m.currentVelocity.Y = 0; }
                }

                if (Math.Abs(tempVector.Z) > Math.Abs(m.currentVelocity.Z))
                {
                    m.currentVelocity.Z = 0;
                }
                else
                {
                    m.currentVelocity.Z += tempVector.Z;
                    if (Math.Abs(m.currentVelocity.Z) < e)
                    { m.currentVelocity.Z = 0; }
                }
            }
        }

        private void UpdateMoleculeSetPositionByVelocity(double t)
        {
            bool isStop = true;

            for (int i = 0; i < MoleculeSet.Count; i++)
            {
                Molecule m = MoleculeSet[i];

                bool ballIsStop = true;
                if (Math.Abs(m.currentVelocity.X) < e) { m.currentVelocity.X = 0; } else { ballIsStop = false; }
                if (Math.Abs(m.currentVelocity.Y) < e) { m.currentVelocity.Y = 0; } else { ballIsStop = false; }
                if (Math.Abs(m.currentVelocity.Z) < e) { m.currentVelocity.Z = 0; } else { ballIsStop = false; }

                if (ballIsStop)
                {
                    continue;
                }
                else
                {
                    isStop = false;

                    //通过小球的速度来更新小球位置
                    if (m.position != m.oldPosition)
                        PhysicEngine.UpdatePositionByVelocity(ref m.position, ref m.oldPosition, ref m.currentVelocity, t);

                    CDE.UpdateToGridmap(i);

                    //更新小球显示的位置
                    TranslateTransform3D temp = (TranslateTransform3D)(m.MoleculeGeometryModel.Transform);
                    temp.OffsetX = m.position.X;
                    temp.OffsetY = m.position.Y;
                    temp.OffsetZ = m.position.Z;
                }
            }

            if (isStop)
            {
                StopGameLoop();

                stick.UpdateStickPosition(new Vector3D(whiteBall.position.X, whiteBall.position.Y, whiteBall.position.Z), shotDirection);

                stick.Visible = true;

                gameWindows.SetShootDirectionAndForceFactor();
            }
        }

        public void InitGame()
        {
            gameWindows = new GameWindows(this);
            gameWindows.Show();
            Initializtion();
            timerPump = new TimerPump() { InvokedMethod = PerformOneFrame };
        }

        public void StartGame()
        {
            timerPump.BeginPumpWithTimeInterval(5);
        }

        public void StopGameLoop()
        {
            timerPump.StopPump();
        }

        public void ContinueGameLoop()
        {
            timerPump.Continue();
        }

        public void ShootWhiteBallAndContinueGameLoop()
        {
            stick.Visible = false;

            shotDirection.Normalize();

            whiteBall.currentVelocity = shootForceFactor * shotDirection;

            ContinueGameLoop();
        }
    }

    class Stick
    {
        ModelVisual3D model = null;
        ModelVisual3D stickVisualModel;
        Cylinder cylinder;
        Cylinder cylinder2;
        GeometryModel3D EndPoint;

        double length = 120;
        public double Length
        {
            get { return length; }
            set { length = value; }
        }

        bool visible = true;
        public bool Visible
        {
            get { return visible; }
            set {
                if (value != visible)
                {
                    if (value == true)
                    {
                        model.Children.Add(stickVisualModel);
                    }
                    else
                    {
                        model.Children.Remove(stickVisualModel);
                    }

                    visible = value; 
                }                
            }
        }

        public void Init(ModelVisual3D model, Vector3D v1, Vector3D shotDirection)
        {
            this.model = model;

            shotDirection.Normalize();
            Vector3D v2 = v1 - shotDirection * length;
            Vector3D v3 = v1 + shotDirection * 1000;

            Material material = new DiffuseMaterial(Brushes.AliceBlue);
            SphereMesh MeshGeneratorBase = new Petzold.Media3D.SphereMesh();
            MeshGeneratorBase.Radius = 2;
            MeshGeometry3D sphere = MeshGeneratorBase.Geometry;

            //EndPoint
            //Vector3D v2 = new Vector3D(-30, -30, 0);
            EndPoint = new GeometryModel3D(sphere, new DiffuseMaterial(Brushes.BurlyWood));
            EndPoint.Transform = new TranslateTransform3D(v2);
                        
            cylinder = new Cylinder();
            cylinder.Fold1 = 0.25;
            cylinder.Fold2 = 0.75;
            cylinder.Radius1 = 0.5;
            cylinder.Radius2 = 2;
            cylinder.Material = material;
            cylinder.Point1 = new Point3D(v1.X, v1.Y, v1.Z);
            cylinder.Point2 = new Point3D(v2.X, v2.Y, v2.Z);

            cylinder2 = new Cylinder();
            cylinder2.Fold1 = 0.25;
            cylinder2.Fold2 = 0.75;
            cylinder2.Radius1 = 0.1;
            cylinder2.Radius2 = 0.1;
            cylinder2.Material = new DiffuseMaterial(Brushes.Green);
            cylinder2.Point1 = new Point3D(v1.X, v1.Y, v1.Z);
            cylinder2.Point2 = new Point3D(v3.X, v3.Y, v3.Z);

            stickVisualModel = new ModelVisual3D();
            stickVisualModel.Content = EndPoint;
            stickVisualModel.Children.Add(cylinder);

            stickVisualModel.Children.Add(cylinder2);

            model.Children.Add(stickVisualModel);

            cylinder.Material = new DiffuseMaterial(Brushes.BurlyWood);
        }

        public void UpdateStickPosition(Vector3D v1, Vector3D shotDirection)
        {
            shotDirection.Normalize();
            Vector3D v2 = v1 - shotDirection * length;
            Vector3D v3 = v1 + shotDirection * 1000;

            cylinder.Point1 = new Point3D(v1.X, v1.Y, v1.Z);
            cylinder.Point2 = new Point3D(v2.X, v2.Y, v2.Z);

            cylinder2.Point1 = new Point3D(v1.X, v1.Y, v1.Z);
            cylinder2.Point2 = new Point3D(v3.X, v3.Y, v3.Z);

            EndPoint.Transform = new TranslateTransform3D(v2);
        }
    }
}
