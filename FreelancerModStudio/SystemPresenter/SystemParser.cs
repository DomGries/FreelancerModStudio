using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreelancerModStudio.Data;
using System.Windows.Media.Media3D;
using System.Globalization;
using FreelancerModStudio.Data.IO;

namespace FreelancerModStudio.SystemPresenter
{
    public class SystemParser
    {
        public void SetValues(ContentBase content, TableBlock block)
        {
            string positionString = "0,0,0";
            string rotationString = "0,0,0";
            string shapeString = "box";
            string scaleString = "1,1,1";
            string vignetteString = "";
            string flagsString = "";

            //get transformation of content
            foreach (EditorINIOption option in block.Block.Options)
            {
                if (option.Values.Count > 0)
                {
                    string value = option.Values[0].Value.ToString();
                    switch (option.Name.ToLower())
                    {
                        case "pos":
                            positionString = value;
                            break;
                        case "rotate":
                            rotationString = value;
                            break;
                        case "shape":
                            shapeString = value;
                            break;
                        case "size":
                            scaleString = value;
                            break;
                        case "vignette_type":
                            vignetteString = value;
                            break;
                        case "property_flags":
                            flagsString = value;
                            break;
                    }
                }
            }

            Vector3D position = ParsePosition(positionString);
            Rotation3D rotation;
            Vector3D scale;

            //set content values
            if (block.ObjectType == ContentType.Zone)
            {
                Zone zone = (Zone)content;

                ZoneShape shape = ParseShape(shapeString);
                scale = ParseScale(scaleString, shape);
                rotation = ParseRotation(rotationString, shape == ZoneShape.Cylinder);

                zone.Shape = shape;

                if (vignetteString != "")
                    zone.Type = ZoneType.Vignette;
                else if (flagsString == "131072")
                    zone.Type = ZoneType.Exclusion;
            }
            else if (block.ObjectType == ContentType.LightSource)
            {
                scale = new Vector3D(1, 1, 1);
                rotation = ParseRotation(rotationString, false);
            }
            else if (block.ObjectType == ContentType.System)
            {
                position = ParseUniverseVector(positionString);
                scale = new Vector3D(2, 2, 2);
                rotation = ParseRotation(rotationString, false);
            }
            else
            {
                scale = new Vector3D(block.Archtype.Radius, block.Archtype.Radius, block.Archtype.Radius) / 1000;
                rotation = ParseRotation(rotationString, false);
            }

            content.SetDisplay(position, rotation, scale);
        }

        double ParseDouble(string text, double defaultValue)
        {
            double value;
            if (double.TryParse(text, NumberStyles.Any, new CultureInfo("en-US", false), out value))
                return value;

            return defaultValue;
        }

        Vector3D ParseScale(string scale, ZoneShape shape)
        {
            string[] values = scale.Split(new char[] { ',' });

            if (shape == ZoneShape.Sphere && values.Length > 0)
            {
                double tempScale = ParseDouble(values[0], 1);
                return new Vector3D(tempScale, tempScale, tempScale) / 1000;
            }
            else if (shape == ZoneShape.Cylinder && values.Length > 1)
            {
                double tempScale1 = ParseDouble(values[0], 1);
                double tempScale2 = ParseDouble(values[1], 1);
                return new Vector3D(tempScale1, tempScale2, tempScale1) / 1000;
            }
            else if (values.Length > 2)
            {
                double tempScale1 = ParseDouble(values[0], 1);
                double tempScale2 = ParseDouble(values[1], 1);
                double tempScale3 = ParseDouble(values[2], 1);
                return new Vector3D(tempScale1, tempScale3, tempScale2) / 1000;
            }

            return new Vector3D(1, 1, 1);
        }

        Rotation3D ParseRotation(string vector, bool localTransform)
        {
            Vector3D tempRotation = ParseVector(vector);
            Vector3D factor = new Vector3D(GetFactor(tempRotation.X), GetFactor(tempRotation.Y), GetFactor(tempRotation.Z));

            tempRotation.X = GetPositive(tempRotation.X);
            tempRotation.Y = GetPositive(tempRotation.Y);
            tempRotation.Z = GetPositive(tempRotation.Z);

            if (localTransform)
            {
                double rotationZ = -(tempRotation.Z / 180);
                if (rotationZ == 0)
                    rotationZ = 1;

                return new AxisAngleRotation3D(new Vector3D(0, 0, tempRotation.Y * factor.Y * rotationZ), tempRotation.Y);
            }
            else
            {
                double max = Math.Max(Math.Max(tempRotation.X, tempRotation.Y), tempRotation.Z);
                if (max != 0)
                {
                    tempRotation = tempRotation / max;
                    return new AxisAngleRotation3D(new Vector3D(tempRotation.X * factor.X, tempRotation.Z * factor.Z, tempRotation.Y * factor.Y), max);
                }
            }

            return new AxisAngleRotation3D(new Vector3D(0, 0, 0), 0);
        }

        double GetFactor(double number)
        {
            if (number < 0)
                return -1;
            return 1;
        }

        double GetPositive(double number)
        {
            if (number < 0)
                return number * -1;
            return number;
        }

        ZoneShape ParseShape(string shape)
        {
            shape = shape.ToLower();
            if (shape == "box")
                return ZoneShape.Box;
            else if (shape == "sphere")
                return ZoneShape.Sphere;
            else if (shape == "cylinder")
                return ZoneShape.Cylinder;
            else
                return ZoneShape.Ellipsoid;
        }

        Vector3D ParseUniverseVector(string vector)
        {
            //Use Point.Parse after implementation of type handling
            string[] values = vector.Split(new char[] { ',' });
            if (values.Length > 1)
            {
                double tempScale1 = ParseDouble(values[0], 0);
                double tempScale2 = ParseDouble(values[1], 0);
                return new Vector3D(tempScale1 - 7, -tempScale2 + 7, 0) / 0.09;
            }
            return new Vector3D(0, 0, 0);
        }

        Vector3D ParseVector(string vector)
        {
            //Use Vector3D.Parse after implementation of type handling
            string[] values = vector.Split(new char[] { ',' });
            if (values.Length > 2)
            {
                double tempScale1 = ParseDouble(values[0], 0);
                double tempScale2 = ParseDouble(values[1], 0);
                double tempScale3 = ParseDouble(values[2], 0);
                return new Vector3D(tempScale1, tempScale2, tempScale3);
            }
            return new Vector3D(0, 0, 0);
        }

        Vector3D ParsePosition(string vector)
        {
            Vector3D tempVector = ParseVector(vector);
            return new Vector3D(tempVector.X, -tempVector.Z, tempVector.Y) / 1000;
        }
    }
}
