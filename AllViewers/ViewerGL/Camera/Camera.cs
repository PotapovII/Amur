using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace ViewerGL
{
    public class Camera
    {
        /// <summary>
        /// Координаты камеры 
        /// </summary>
        protected float x,y,z;
        /// <summary>
        /// Сдвиги по х и у
        /// </summary>
       // protected int bx, by;
        /// <summary>
        /// Поворот камеры по х
        /// </summary>
        protected float xRot;
        /// <summary>
        /// Поворот камеры по горизонту - ось z
        /// </summary>
        protected float zRot;

        public Camera()
        {
            x = 0f;
            y = 0f;
            z = 3f;
            xRot = 70f;
            zRot = -40f;
            //bx = 400;
            //by = 300;
        }
        /// <summary>
        /// Функция применения параметров положения камеры
        /// </summary>
        public void CameraApply()
        {
            GL.Rotate(-xRot, 1, 0, 0);
            GL.Rotate(-zRot, 0, 0, 1);
            // камера смотрит на точку положения игрока
            GL.Translate(-x, -y, -z);
        }
        /// <summary>
        /// Управление камерой с клавиатуры
        /// </summary>
        /// <param name="keyState"></param>
        public void CalkMoveCamera(KeyboardState keyState)
        {
            // Повороты камеры
            if (keyState.IsKeyDown(Key.Up) == true)
                xRot = ++xRot > 180 ? 180 : xRot;
            if (keyState.IsKeyDown(Key.Down) == true)
                xRot = --xRot < 0 ? 0 : xRot;
            if (keyState.IsKeyDown(Key.Left) == true)
                zRot++;
            if (keyState.IsKeyDown(Key.Right) == true)
                zRot--;
            // Движение камеры 
            float speed = 0;
            float ugol = -zRot / 180f * (float)Math.PI;
            if (keyState.IsKeyDown(Key.W) == true)
                speed = 0.1f;
            if (keyState.IsKeyDown(Key.S) == true)
                speed = -0.1f;
            if (keyState.IsKeyDown(Key.A) == true)
            {
                speed = 0.1f;
                ugol -= (float)Math.PI / 2;
            }
            if (keyState.IsKeyDown(Key.D) == true)
            {
                speed = 0.1f;
                ugol += (float)Math.PI / 2;
            }

            if (speed != 0)
            {
                x += (float)Math.Sin(ugol) * speed;
                y += (float)Math.Cos(ugol) * speed;
            }
        }

        ///// <summary>
        ///// Вращение камеры по осям Х и Z
        ///// </summary>
        //public void CameraRotate(float xAngle, float zAngle)
        //{
        //    xRot += xAngle;
        //    if (xRot < 0) xRot += 360;
        //    if (xRot > 360) xRot -= 360;
            
        //    zRot += zAngle;
        //    if (zRot < 0) zRot += 360;
        //    if (zRot > 360) zRot -= 360;
        //}

        //[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        //public static extern IntPtr GetForegroundWindow();

        //public void PlayerMove()
        //{
        //    MouseState mouse = Mouse.GetState();
        //    float scale = 5f;
        //    float xAngle = (bx - mouse.X)/ scale;
        //    float zAngle = (bx - mouse.X) / scale;
        //    CameraRotate(xAngle, zAngle);
        //    Mouse.SetPosition(bx, by);
        //}
        //public void Player_Move()
        //{
        //    //CameraAutoMoveByMous(400, 400, 0.2f);
        //    KeyboardState skey = Keyboard.GetState();

        //    int forwandMove =
        //        (skey.IsKeyDown(Key.W) == true ? 1 :
        //        (skey.IsKeyDown(Key.S) == true ? -1 : 0));
        //    int rightMove =
        //        (skey.IsKeyDown(Key.D) == true ? 1 :
        //        (skey.IsKeyDown(Key.A) == true ? -1 : 0));
        //    float speed = 0.2f;
        //    CameraMoveDirection(forwandMove, rightMove, speed);
        //}
        ///// <summary>
        ///// Вращение камеры с помощью мышки
        ///// </summary>
        //public void CameraAutoMoveByMous(int centerX, int centerY, float speed)
        //{
        //    //pStatic = new Point(centerX, centerY);
        //    //MouseState mouse = Mouse.GetState();
        //    //CameraRotate((pStatic.Y - mouse.Y) / 5f, (pStatic.X - mouse.X) / 5f);
        //    //Mouse.SetPosition(pStatic.X, pStatic.Y);
        //}
        ///// <summary>
        ///// Изменение размера экрана (вызывать в Resize)
        ///// </summary>
        //public void WinResize(int x, int y)
        //{
        //    GL.Viewport(0, 0, x, y);
        //    float k = x / (float)y;
        //    float sz = 0.1f;
        //    // установка единичной матрицы
        //    GL.LoadIdentity();
        //    GL.Frustum(-k*sz, k*sz, -sz, sz, 2*sz, 100);
        //}



        //public void CameraMoveDirection(int forwandMove,int rightMove, float speed)
        //{
        //    double M_PI = Math.PI;
        //    double M_PI_2 = M_PI / 2;
        //    double M_PI_4 = M_PI / 4;
        //    double ugol = - zRot/180 *Math.PI;
        //    if (forwandMove > 0)
        //        ugol += rightMove > 0 ? M_PI_4 : (rightMove < 0 ? -M_PI_4 : 0);
        //    if (forwandMove < 0)
        //        ugol += M_PI + (rightMove > 0 ? -M_PI_4 : (rightMove < 0 ? M_PI_4 : 0));
        //    if (forwandMove == 0)
        //    {
        //        ugol += rightMove > 0 ? M_PI_2 : -M_PI_2; 
        //    if (rightMove == 0) speed = 0;
        //    }
        //    //if (speed != 0)
        //    //{
        //    //    x += Math.Sin(ugol) * speed;
        //    //    y += Math.Cos(ugol) * speed;
        //    //}
        //}

    }
}
