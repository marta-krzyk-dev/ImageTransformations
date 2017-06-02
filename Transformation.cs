using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Image_filters
{
    class Transformation
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

        public static double SaturationAsynch(Colour kolor, byte wartosc, PictureBox picture_box)
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
            int przeskok = 3;
            switch (bmp.PixelFormat)
            {
                case (PixelFormat.Format24bppRgb):
                default: przeskok = 3; break;

                case (PixelFormat.Format32bppArgb):
                case (PixelFormat.Format32bppPArgb):
                case (PixelFormat.Format32bppRgb): przeskok = 4; break;
            }

            //little indian : bb gg rr aa
            int counter = 2;
            switch (kolor)
            {
                default:
                case (Colour.R): counter = 2; break;
                case (Colour.G): counter = 1; break;
                case (Colour.B): counter = 0; break;
                case (Colour.A): counter = 3; break;
            }

            /*for (; counter < rgbValues.Length; counter += przeskok)
                rgbValues[counter] = wartosc;*/
            var co_trzeci = Range(counter, rgbValues.Length, i => i += przeskok);

            Task.Run(() =>
                Parallel.ForEach(co_trzeci, i =>
                {
                    rgbValues[i] = wartosc;
                })
            );

            // Parallel.For(counter, rgbValues.Length, i => { rgbValues[i] = wartosc; /*i += przeskok;*/ });

            // Copy the RGB values back to the bitmap
            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, bytes);

            // Unlock the bits.
            bmp.UnlockBits(bmpData);

            // Draw the modified image.
            picture_box.Image = bmp;

            return sw.Elapsed.TotalSeconds;
        }

        public static void Turn180Degree(PictureBox picture_box)
        {
            Bitmap bmp = new Bitmap(picture_box.Image);
            bmp.RotateFlip(RotateFlipType.Rotate180FlipY);
            picture_box.Image = bmp;
        }

        public static void Merge2Images(PictureBox picture_box, PictureBox picture_box2)
        {
            Bitmap source1 = new Bitmap(picture_box.Image); // your source images - assuming they're the same size
            Bitmap source2 = new Bitmap(picture_box2.Image);

            //Enlarge the image
            if (source1.Height > source2.Height || source1.Width > source2.Width)
                source1 = new Bitmap(source1, source2.Size);
            else
                if (source2.Height > source1.Height || source2.Width > source1.Width)
                    source2 = new Bitmap(source2, source1.Size);

            var target = new Bitmap(source1.Width, source1.Height, PixelFormat.Format32bppArgb);
            var graphics = Graphics.FromImage(target);
            graphics.CompositingMode = CompositingMode.SourceOver; // this is the default, but just to be clear

            graphics.DrawImage(source1, 0, 0);
            graphics.DrawImage(source2, 0, 0);

            target.Save("filename.png", ImageFormat.Png);
            picture_box2.Image = target;

        }

        public static void Merge2Images2(PictureBox picture_box, PictureBox picture_box2)
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

        public static void Merge2Images3(PictureBox picture_box, PictureBox picture_box2)
        {
            Bitmap bmp1 = new Bitmap(picture_box.Image);
            Bitmap bmp2 = new Bitmap(picture_box2.Image);

            //Enlarge the image
            if (bmp1.Height < bmp2.Height || bmp1.Width < bmp2.Width)
                bmp1 = new Bitmap(bmp1, bmp2.Size);
            else
                if (bmp2.Height < bmp1.Height || bmp2.Width < bmp1.Width)
                    bmp2 = new Bitmap(bmp2, bmp1.Size);


            for (int i = 0; i < bmp1.Width && i < bmp2.Width; ++i)
                for (int j = (i % 2 == 0) ? 0 : 1; j < bmp1.Height && j < bmp2.Height; j += 2)
                    bmp1.SetPixel(i, j, bmp2.GetPixel(i, j));

            picture_box.Image = bmp1;
        }

        public static void Turn(RotateFlipType rotation_type, PictureBox picture_box)
        {
            Bitmap bmp = new Bitmap(picture_box.Image);
            bmp.RotateFlip(rotation_type);
            picture_box.Image = bmp;
        }

        public static void Blur(PictureBox picture_box)
        {
            Bitmap bmp = new Bitmap(picture_box.Image);

            // Lock the bitmap's bits.  
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            // Get the address of the first line.
            IntPtr ptr = bmpData.Scan0;

            // Declare an array to hold the bytes of the bitmap.
            int bytes = Math.Abs(bmpData.Stride) * bmp.Height; //szer * wys, Stride może być ujemny
            byte[] rgbValues = new byte[bytes];
            int szer = bmpData.Width;
            int wys = bmpData.Height;
            byte[,] rgbValues2 = new byte[szer, wys];

            byte[] r = new byte[bytes / 3];
            byte[] g = new byte[bytes / 3];
            byte[] b = new byte[bytes / 3];

            // Copy the RGB values into the array.
            Marshal.Copy(ptr, rgbValues, 0, bytes);//source, destination, start index, length

            //Skopiuj rgbValues do tabeli 2-wymiarowej
            int stride = bmpData.Stride; // szerokość rzędu pikseli.

            //The stride is the width of a single row of pixels (a scan line), rounded up to a four-byte boundary.If the stride is positive, the bitmap is top-down.If the stride is negative, the bitmap is bottom-up.

            for (int i = 0; i < szer; i++)
                for (int j = 0; j < wys; j++)
                    rgbValues2[i, j] = rgbValues[(i * wys) + j];


            //Przejdź przez każdy piksel bitmapy
            int count = 0;
            for (int column = 0; column < bmpData.Height; column++)
            {
                for (int row = 0; row < bmpData.Width; row++)
                {
                    b[count] = (byte)(rgbValues[(column * stride) + (row * 3)]);
                    g[count] = (byte)(rgbValues[(column * stride) + (row * 3) + 1]);
                    r[count] = (byte)(rgbValues[(column * stride) + (row * 3) + 2]);

                    b[count] = (byte)(rgbValues2[column, row]);
                    g[count] = (byte)(rgbValues2[column, row + 1]);
                    r[count] = (byte)(rgbValues2[column, row + 2]);
                    ++count;
                }
            }

            long suma_r = 0;
            long suma_g = 0;
            long suma_b = 0;

            for (int i = 1; i < szer - 1; ++i)
                for (int j = 1; j < wys - 1; ++j)
                {
                    suma_r = 0; suma_g = 0; suma_b = 0;

                    for (int k = i - 1; k < (i + 2); ++k)
                        for (int l = j - 1; l < (j + 2); ++l)
                        {
                            suma_r += rgbValues2[k, l];
                        }
                    rgbValues2[i, j] = (byte)(suma_r / 9);
                    //rgbValues2[i, j] = rgbValues[(i * wys) + j];
                }

            //Oblicz rozmycie
            for (int i = 0; i < b.Length; ++i)
            {

            }

            //Skopiuj zawartość 2-wymiar. rgbValues2 do 1-wymiar. rgbValues
            int c = 0;
            for (int i = 0; i < szer; ++i)
                for (int j = 0; j < wys; ++j)
                    rgbValues[c++] = rgbValues2[i, j];

            // Copy the RGB values back to the bitmap
            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, rgbValues.Length);

            // Unlock the bits.
            bmp.UnlockBits(bmpData);

            // Draw the modified image.
            //e.Graphics.DrawImage(bmp, 0, 150);
            picture_box.Image = bmp;

        }

        public static double Saturation(Colour kolor, byte wartosc, ref PictureBox picture_box)
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
            int przeskok = 3;
            switch (bmp.PixelFormat)
            {

                case (PixelFormat.Format24bppRgb):

                default: przeskok = 3; break;

                case (PixelFormat.Format32bppArgb):
                case (PixelFormat.Format32bppPArgb):
                case (PixelFormat.Format32bppRgb): przeskok = 4; break;

            }
            //little indian : bb gg rr aa
            int counter;
            switch (kolor)
            {
                default:
                case (Colour.R): counter = 2; break;
                case (Colour.G): counter = 1; break;
                case (Colour.B): counter = 0; break;
                case (Colour.A): counter = 3; break;
            }

            for (; counter < rgbValues.Length; counter += przeskok)
                rgbValues[counter] = wartosc;

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
