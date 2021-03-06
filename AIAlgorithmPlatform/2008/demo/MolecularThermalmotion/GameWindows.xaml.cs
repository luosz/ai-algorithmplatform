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
using System.Windows.Shapes;
using System.Windows.Media.Media3D;
using M2M.Util;

namespace MolecularThermalmotion
{
    /// <summary>
    /// Interaction logic for GameWindows.xaml
    /// </summary>
    public partial class GameWindows : Window
    {
        public MediaElement myMediaElement;
        private Game game;
        internal Game CurrentGame
        {
            get { return game; }
            set { game = value; }
        }
              
        internal GameWindows(Game game)
        {
            InitializeComponent();

            myMediaElement = new MediaElement();
            myMediaElement.Source = new Uri("res/hit.wav", UriKind.Relative);
            myMediaElement.LoadedBehavior = MediaState.Manual;
            myMediaElement.Volume = 1;
            myMediaElement.Stop();
            rootGrid.Children.Add(myMediaElement);

            model.Transform = new Transform3DGroup();

            this.game = game;

            game.ShootForceFactor = progressBar1.Value;
        }

        public void PlayBallCollisionSound()
        {
            collisionSound.Stop();
            collisionSound.Play();
        }

        public void PlayBallInHoleSound()
        {
            collisionSound.Stop();
            collisionSound.Play();
        }

        public void PlayBallCollisionWithWallSound()
        {
            collisionSound.Stop();
            collisionSound.Play();
        }

        public void SetShootDirectionAndForceFactor()
        {
            //这时候使到那些control有效
        }

        // variables for mouse controlling
        private bool leftDown = false;
        private Point leftLastPos;
        private bool middleDown = false;
        private Point middleStartPos;
        private bool rightDown = false;
        private Point rightLastPos;
        
        private GameWindows()
        {
        }

        private void Grid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            camera.Position = new Point3D(camera.Position.X, camera.Position.Y, camera.Position.Z - e.Delta / 25D);
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            if (leftDown)
            {
                Point pos = Mouse.GetPosition(viewport);
                Point actualPos = new Point(pos.X - viewport.ActualWidth / 2, viewport.ActualHeight / 2 - pos.Y);
                double dx = actualPos.X - leftLastPos.X, dy = actualPos.Y - leftLastPos.Y;

                double mouseAngle = 0;
                if (dx != 0 && dy != 0)
                {
                    mouseAngle = Math.Asin(Math.Abs(dy) / Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2)));
                    if (dx < 0 && dy > 0) mouseAngle += Math.PI / 2;
                    else if (dx < 0 && dy < 0) mouseAngle += Math.PI;
                    else if (dx > 0 && dy < 0) mouseAngle += Math.PI * 1.5;
                }
                else if (dx == 0 && dy != 0) mouseAngle = Math.Sign(dy) > 0 ? Math.PI / 2 : Math.PI * 1.5;
                else if (dx != 0 && dy == 0) mouseAngle = Math.Sign(dx) > 0 ? 0 : Math.PI;

                double axisAngle = mouseAngle + Math.PI / 2;
                Vector3D axis = new Vector3D(Math.Cos(axisAngle) * 4, Math.Sin(axisAngle) * 4, 0);
                double rotation = 0.002 * Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2));

                Transform3DGroup tg = model.Transform as Transform3DGroup;
                QuaternionRotation3D r = new QuaternionRotation3D(new Quaternion(axis, rotation * 180 / Math.PI));
                tg.Children.Add(new RotateTransform3D(r));

                leftLastPos = actualPos;
            }

            if(middleDown)
            {
                double value = progressBar1.Value + middleStartPos.Y - Mouse.GetPosition(viewport).Y;
                if (value > progressBar1.Maximum)
                    value = progressBar1.Maximum;
                if (value < progressBar1.Minimum)
                    value = progressBar1.Minimum;
                if (value != progressBar1.Value)
                {
                    progressBar1.Value = value;
                    game.ShootForceFactor = value;
                }
            }

            if(rightDown)
            {
                Point pos = Mouse.GetPosition(viewport);
                Point actualPos = new Point(pos.X - viewport.ActualWidth / 2, viewport.ActualHeight / 2 - pos.Y);

                //if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                //    game.ShotDirection = Trackball.RotateTheVector3D(game.ShotDirection, rightLastPos.X, rightLastPos.Y, actualPos.X, actualPos.Y, 0.1);
                //else
                //    game.ShotDirection = Trackball.RotateTheVector3D(game.ShotDirection, rightLastPos.X, rightLastPos.Y, actualPos.X, actualPos.Y);

                {
                    double dx = actualPos.X - rightLastPos.X, dy = actualPos.Y - rightLastPos.Y;

                    double mouseAngle = 0;
                    if (dx != 0 && dy != 0)
                    {
                        mouseAngle = Math.Asin(Math.Abs(dy) / Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2)));
                        if (dx < 0 && dy > 0) mouseAngle += Math.PI / 2;
                        else if (dx < 0 && dy < 0) mouseAngle += Math.PI;
                        else if (dx > 0 && dy < 0) mouseAngle += Math.PI * 1.5;
                    }
                    else if (dx == 0 && dy != 0) mouseAngle = Math.Sign(dy) > 0 ? Math.PI / 2 : Math.PI * 1.5;
                    else if (dx != 0 && dy == 0) mouseAngle = Math.Sign(dx) > 0 ? 0 : Math.PI;

                    double axisAngle = mouseAngle + Math.PI / 2;
                    Vector3D axis = new Vector3D(Math.Cos(axisAngle) * 4, Math.Sin(axisAngle) * 4, 0);
                    double rotation = 0.005 * Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2));

                    var tempMatrix = model.Transform.Value;
                    tempMatrix.Invert();
                    axis = axis * tempMatrix;

                    double angleRate;
                    if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                        angleRate = 180 / Math.PI * 0.1;
                    else
                        angleRate = 180 / Math.PI;

                    QuaternionRotation3D r = new QuaternionRotation3D(new Quaternion(axis, rotation * angleRate));

                    game.ShotDirection = game.ShotDirection * (new RotateTransform3D(r).Value);
                }

                rightLastPos = actualPos;
            }
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            switch (e.ChangedButton)
            {
                case MouseButton.Left:
                    leftDown = true;
                    Point pos = Mouse.GetPosition(viewport);
                    leftLastPos = new Point(pos.X - viewport.ActualWidth / 2, viewport.ActualHeight / 2 - pos.Y);
                    break;

                case MouseButton.Middle:
                    middleDown = true;
                    middleStartPos = Mouse.GetPosition(viewport);
                    break;

                case MouseButton.Right:
                    rightDown = true;
                    Point p = Mouse.GetPosition(viewport);
                    rightLastPos = new Point(p.X - viewport.ActualWidth / 2, viewport.ActualHeight / 2 - p.Y);
                    break;
            }
        }

        private void Grid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            switch (e.ChangedButton)
            {
                case MouseButton.Left:
                    leftDown = false;
                    break;

                case MouseButton.Middle:
                    middleDown = false;
                    game.ShootWhiteBallAndContinueGameLoop();
                    break;

                case MouseButton.Right:
                    rightDown = false;
                    break;
            }
        }

        private void sliderShootForeceFactor_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //game.ShootForceFactor = sliderShootForeceFactor.Value;
        }

        private void ShootButton_Click(object sender, RoutedEventArgs e)
        {
            //game.ShootWhiteBallAndContinueGameLoop();
        }

        private void ElipseButton_Released(Ellipse ellipse, Label label)
        {
            label.RenderTransform = new TranslateTransform(0, 0);
            ellipse.RenderTransform = new TranslateTransform(0, 0);
            System.Windows.Media.Effects.DropShadowBitmapEffect dsEffect = new System.Windows.Media.Effects.DropShadowBitmapEffect();
            dsEffect.Color = Color.FromRgb(0, 128, 0);
            ellipse.BitmapEffect = dsEffect;
            ellipse.Fill = new System.Windows.Media.LinearGradientBrush(Color.FromRgb(0, 0, 0), Color.FromRgb(255, 255, 255), 90);
        }

        private void ElipseButton_Pressed(Ellipse ellipse, Label label)
        {
            label.RenderTransform = new TranslateTransform(5, 5);
            ellipse.RenderTransform = new TranslateTransform(5, 5);
            ellipse.BitmapEffect = null;
            ellipse.Fill = new LinearGradientBrush(Color.FromRgb(255, 255, 255), Color.FromRgb(0, 0, 0), 90);
        }

        private void ElipseButton_Hover(Ellipse ellipse, Label label)
        {
            ellipse.Fill = new LinearGradientBrush(Color.FromRgb(255, 255, 255), Color.FromRgb(255, 255, 255), 90);
        }

        private void ellipseShoot_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ElipseButton_Released(ellipseShoot, labelShoot);
            game.ShootWhiteBallAndContinueGameLoop();
        }

        private void ellipseShoot_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ElipseButton_Pressed(ellipseShoot, labelShoot);
        }

        private void ellipseShoot_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
                ElipseButton_Hover(ellipseShoot, labelShoot);
        }

        private void ellipseShoot_MouseLeave(object sender, MouseEventArgs e)
        {
            ElipseButton_Released(ellipseShoot, labelShoot);
        }
//

        private void ellipsePause_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ElipseButton_Released(ellipsePause, labelPause);
            game.StopGameLoop();
            if (labelPause.Content.Equals("Continue"))
            {
                game.ContinueGameLoop();
                labelPause.Content = "Pause";
            }
            else
            {
                game.StopGameLoop();
                labelPause.Content = "Continue";
            }
        }

        private void ellipsePause_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ElipseButton_Pressed(ellipsePause, labelPause);
        }

        private void ellipsePause_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
                ElipseButton_Hover(ellipsePause, labelPause);
        }

        private void ellipsePause_MouseLeave(object sender, MouseEventArgs e)
        {
            ElipseButton_Released(ellipsePause, labelPause);
        }

        private void sliderTriangle_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point pTriangle = Mouse.GetPosition(sliderTriangle);
                pTriangle.X -= sliderTriangle.ActualWidth / 2;
                if (sliderTriangle.RenderTransform is TranslateTransform)
                    pTriangle.X += ((TranslateTransform)sliderTriangle.RenderTransform).X;
                if (pTriangle.X < sliderStick.ActualWidth && pTriangle.X >= 0)
                {
                    sliderTriangle.RenderTransform = new TranslateTransform(pTriangle.X, 0);
                    game.ShootForceFactor = pTriangle.X / sliderStick.ActualWidth * 400;
                }
            }
        }
    }
}
