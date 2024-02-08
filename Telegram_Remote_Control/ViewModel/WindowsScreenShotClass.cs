using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Telegram_Remote_Control.ViewModel
{
    static public class WindowsScreenShotClass
    {
        public static string CaptureMyScreen(string selectedFolder)
        {
            string name = $@"{selectedFolder}\{DateTime.Now.Ticks}.jpg";

            try
            {
                //Creating a Rectangle object which will
                //capture our Current Screen
                Rectangle captureRectangle = Screen.AllScreens[0].Bounds;

                //Creating a new Bitmap object
                Bitmap captureBitmap = new Bitmap(width: captureRectangle.Width, height: captureRectangle.Height, format: PixelFormat.Format32bppArgb);

                //Creating a New Graphics Object
                Graphics captureGraphics = Graphics.FromImage(captureBitmap);
                //Copying Image from The Screen
                captureGraphics.CopyFromScreen(captureRectangle.Left, captureRectangle.Top, 0, 0, captureRectangle.Size);
                //Saving the Image File 
                captureBitmap.Save(name, ImageFormat.Jpeg);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return name;
        }
    }
}
