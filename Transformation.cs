using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace Image_filters
{
    class ImageTransformation
    {
        private static IEnumerable<int> Range(int start, int end, Func<int, int> step)
        {
            //check parameters
            while (start <= end)
            {
                yield return start;
                start = step(start);
            }
        }

        public static double SaturationAsynch(Colour colour, byte value, PictureBox picture_box)
        {
            Stopwatch sw = Stopwatch.StartNew();

            // Create a new bitmap.
            Bitmap bmp = new Bitmap(picture_box.Image);

            // Lock the bitmap's bits.  
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            System.Drawing.Imaging.BitmapData bmpData =
                bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite,
                bmp.PixelFormat);

            // Get the address of the first line.
            IntPtr ptr = bmpData.Scan0;

            // Declare an array to hold the bytes of the bitmap.
            int bytes = Math.Abs(bmpData.Stride) * bmp.Height;
            byte[] rgbValues = new byte[bytes];

            // Copy the RGB values into the array.
            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);

            // Set every third value to 255. A 24bpp bitmap will look red.  
            int jump = 3;
            switch (bmp.PixelFormat)
            {
                case (PixelFormat.Format24bppRgb):
                default: jump = 3; break;

                case (PixelFormat.Format32bppArgb):
                case (PixelFormat.Format32bppPArgb):
                case (PixelFormat.Format32bppRgb): jump = 4; break;
            }

            //little indian : bb gg rr aa
            int counter = 2;
            switch (colour)
            {
                default:
                case (Colour.R): counter = 2; break;
                case (Colour.G): counter = 1; break;
                case (Colour.B): counter = 0; break;
                case (Colour.A): counter = 3; break;
            }

            var every_third_byte = Range(counter, rgbValues.Length, i => i += jump);

            Task.Run(() =>
                Parallel.ForEach(every_third_byte, i =>
                {
                    rgbValues[i] = value;
                })
            );

            // Copy the RGB values back to the bitmap
            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, bytes);

            // Unlock the bits.
            bmp.UnlockBits(bmpData);

            // Draw the modified image.
            picture_box.Image = bmp;

            return sw.Elapsed.TotalSeconds;
        }

        public static void Merge2Images(PictureBox picture_box, PictureBox picture_box2)
        {
            // Create new bitmaps
            Bitmap bmp = new Bitmap(picture_box.Image);
            Bitmap bmp2 = new Bitmap(picture_box2.Image);

            // Locking the bitmaps' bits 
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            System.Drawing.Imaging.BitmapData bmpData =
                bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite,
                bmp.PixelFormat);

            Rectangle rect2 = new Rectangle(0, 0, bmp.Width, bmp.Height);
            System.Drawing.Imaging.BitmapData bmpData2 =
                bmp2.LockBits(rect2, System.Drawing.Imaging.ImageLockMode.ReadWrite,
                bmp2.PixelFormat);

            // Get the addresses of the first lines
            IntPtr ptr = bmpData.Scan0;
            IntPtr ptr2 = bmpData2.Scan0;

            // Put the bytes of the bitmap into arrays
            int bytes = Math.Abs(bmpData.Stride) * bmp.Height;
            byte[] rgbValues = new byte[bytes];

            int bytes2 = Math.Abs(bmpData2.Stride) * bmp2.Height;
            byte[] rgbValues2 = new byte[bytes2];

            // Copy the RGB values into the array
            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);
            System.Runtime.InteropServices.Marshal.Copy(ptr2, rgbValues2, 0, bytes2);

            // Swap the first 4 bits with rgbValues2, leave the other 4 bits of rgbValues1
            int jump = 4;

            for (int counter = 0; counter < rgbValues.Length && counter < rgbValues2.Length; ++counter)
            {
                rgbValues[counter] = rgbValues2[counter];

                if (counter % jump == 0)
                    counter += jump;
            }
            // Copy the RGB values back to the bitmap
            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, bytes);

            // Unlock the bits.
            bmp.UnlockBits(bmpData);
            bmp2.UnlockBits(bmpData2);

            // Draw the modified image.
            picture_box.Image = bmp;
        }

        public static double Saturation(Colour colour, byte value, ref PictureBox picture_box)
        {
            Stopwatch sw = Stopwatch.StartNew();

            // Create a new bitmap.
            Bitmap bmp = new Bitmap(picture_box.Image);

            // Lock the bitmap's bits.  
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            System.Drawing.Imaging.BitmapData bmpData =
                bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite,
                bmp.PixelFormat);

            // Get the address of the first line.
            IntPtr ptr = bmpData.Scan0;

            // Declare an array to hold the bytes of the bitmap.
            int bytes = Math.Abs(bmpData.Stride) * bmp.Height;
            byte[] rgbValues = new byte[bytes];

            // Copy the RGB values into the array.
            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);

            // Set every third value to 255. A 24bpp bitmap will look red.  
            int jump = 3;
            switch (bmp.PixelFormat)
            {

                case (PixelFormat.Format24bppRgb):

                default: jump = 3; break;

                case (PixelFormat.Format32bppArgb):
                case (PixelFormat.Format32bppPArgb):
                case (PixelFormat.Format32bppRgb): jump = 4; break;

            }
            //little indian : bb gg rr aa
            int counter;
            switch (colour)
            {
                default:
                case (Colour.R): counter = 2; break;
                case (Colour.G): counter = 1; break;
                case (Colour.B): counter = 0; break;
                case (Colour.A): counter = 3; break;
            }

            for (; counter < rgbValues.Length; counter += jump)
                rgbValues[counter] = value;

            // Copy the RGB values back to the bitmap
            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, bytes);

            // Unlock the bits.
            bmp.UnlockBits(bmpData);

            // Draw the modified image.
            picture_box.Image = bmp;

            return sw.Elapsed.TotalSeconds;
        }
    }
}
