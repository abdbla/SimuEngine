using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace NodeMonog {
    struct LabColor {
        static Matrix SRGB_TO_XYZ_MAT = new Matrix(
            0.4124564f, 0.3575761f, 0.1804375f, 0.0f,
            0.2126729f, 0.7151522f, 0.0721750f, 0.0f,
            0.0193339f, 0.1191920f, 0.9503041f, 0.0f,
            0.0f, 0.0f, 0.0f, 1.0f
        );

        static Matrix XYZ_TO_SRGB_MAT = new Matrix(
            3.2404542f, -1.5371385f, -0.4985314f, 0.0f,
            -0.9692660f, 1.8760108f, 0.0415560f, 0.0f,
            0.0556434f, -0.2040259f, 1.0572252f, 0.0f,
            0.0f, 0.0f, 0.0f, 1.0f
        );

        static Vector3 D65_WHITE = new Vector3(95.047f, 100.0f, 108.883f);

        const float EPSILON = 216.0f / 24389.0f;
        const float KAPPA = 24389.0f / 27.0f;

        static Vector3 RgbToXyz(Vector3 rgb) {
            Func<float, float> inverseCompanding = x => (float)(x <= 0.04045 ? x / 12.92 : Math.Pow((double)(x + 0.055) / 1.055, (double)2.4));

            var companded = new Vector3(inverseCompanding(rgb.X), inverseCompanding(rgb.Y), inverseCompanding(rgb.Z));

            var final = Vector3.Transform(companded, SRGB_TO_XYZ_MAT);

            final *= 100.0f;
            return final;
        }

        static Vector3 XyzToLabWhite(Vector3 xyz, Vector3 white) {
            var xyz_r = xyz / white;
            Func<float, float> f = x => (float)(x > EPSILON ? Math.Pow(x, 1f/3f) : KAPPA * x + 16f / 116f);

            var f_x = f(xyz_r.X);
            var f_y = f(xyz_r.Y);
            var f_z = f(xyz_r.Z);

            var L = 116f * f_y - 16f;
            var a = 500f * (f_x - f_y);
            var b = 200f * (f_y - f_z);

            return new Vector3(L, a, b);
        }

        static Vector3 LabToXyzWhite(Vector3 lab, Vector3 white) {
            var l = lab.X;
            var a = lab.Y;
            var b = lab.Z;

            var f_y = (l + 16f) / 116;
            var f_x = a / 500f + f_y;
            var f_z = f_y - b / 200f;

            var y_r = (float)(l > KAPPA * EPSILON ? Math.Pow((l + 16f) / 116f, 3) : l / KAPPA);
            var x_r = (float)(Math.Pow(f_x, 3) > EPSILON ? Math.Pow(f_x, 3) : (116 * f_x - 16) / KAPPA);
            var z_r = (float)(Math.Pow(f_x, 3) > EPSILON ? Math.Pow(f_z, 3) : (116 * f_z - 16) / KAPPA);

            return new Vector3(x_r, y_r, z_r) * white;
         }

        static Vector3 XyzToRgb(Vector3 xyz) {
            xyz /= 100;
            Func<float, float> f = x => (float)(x < 0.0031308 ? 12.92 * x : 1.055 * Math.Pow(x, 1f / 2.4) - 0.055);
            var rgb = Vector3.Transform(xyz, XYZ_TO_SRGB_MAT);
            return new Vector3(f(rgb.X), f(rgb.Y), f(rgb.Z));
        }

        public static Vector3 RgbToLab(Color rgb) {
            Vector3 newRgb = new Vector3(rgb.R / 255f, rgb.G / 255f, rgb.B / 255f);
            var xyz = RgbToXyz(newRgb);
            var lab = XyzToLabWhite(xyz, D65_WHITE);
            return lab;
        }

        public static Color LabToRgb(Vector3 lab) {
            var xyz = LabToXyzWhite(lab, D65_WHITE);
            var vecRgb = XyzToRgb(xyz);
            return new Color(vecRgb.X, vecRgb.Y, vecRgb.Z);
        }
    }
}
