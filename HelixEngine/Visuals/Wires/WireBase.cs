//------------------------------------------
// WireBase.cs (c) 2007 by Charles Petzold
//
// The inspiration, concept, technique, and
//  some of the code for this class came
//  from the ScreenSpaceLines3D class
//  in 3DTools at www.codeplex.com/3DTools.
//------------------------------------------
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using HelixEngine.Helpers;

namespace HelixEngine.Wires
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class WireBase : ModelVisual3D
    {
        // Static fields for storing instances of WireBase.
        static List<WireBaseAndUltimateParent> listWireBases = new List<WireBaseAndUltimateParent>();
        static List<WireBaseAndUltimateParent> listRemove = new List<WireBaseAndUltimateParent>();

        // Instance fields.
        bool needRecalculation = true;
        Matrix3D matxVisualToScreen = Matrix3D.Identity;
        Matrix3D matxScreenToVisual;
        RotateTransform rotate = new RotateTransform();

        // Constructor
        // -----------
        public WireBase()
        {
            LineCollection = new Point3DCollection();

            // Create MeshGeometry3D.
            MeshGeometry3D mesh = new MeshGeometry3D();

            // Create MaterialGroup.
            MaterialGroup matgrp = new MaterialGroup();
            matgrp.Children.Add(new DiffuseMaterial(Brushes.Black));
            matgrp.Children.Add(new EmissiveMaterial(new SolidColorBrush(Color)));

            // Create GeometryModel3D.
            GeometryModel3D model = new GeometryModel3D(mesh, matgrp);

            // Remove this later
            model.BackMaterial = new DiffuseMaterial(Brushes.Red);

            // Set the Content property to the GeometryModel3D.
            Content = model;

            // Add to collection.
            listWireBases.Add(new WireBaseAndUltimateParent(this));
        }

        // Static constructor attaches Rendering handler for all instances.
        static WireBase()
        {
            CompositionTarget.Rendering += new EventHandler(OnRendering);
        }

        private static readonly DependencyPropertyKey LineCollectionKey =
            DependencyProperty.RegisterReadOnly("LineCollection",
                typeof(Point3DCollection),
                typeof(WireBase),
                new PropertyMetadata(null, RecalcPropertyChanged));

        /// <summary>
        ///     Identifies the BaseLines dependency property.
        /// </summary>
        public static readonly DependencyProperty LineCollectionProperty =
            LineCollectionKey.DependencyProperty;

        /// <summary>
        /// 
        /// </summary>
        public Point3DCollection LineCollection
        {
            private set { SetValue(LineCollectionKey, value); }
            get { return (Point3DCollection)GetValue(LineCollectionProperty); }
        }

        /// <summary>
        ///     Identifies the Color depencency property.
        /// </summary>
        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register("Color",
            typeof(Color),
            typeof(WireBase),
            new PropertyMetadata(Colors.Black, ColorPropertyChanged));

        /// <summary>
        /// 
        /// </summary>
        public Color Color
        {
            set { SetValue(ColorProperty, value); }
            get { return (Color)GetValue(ColorProperty); }
        }

        //
        // This is the only property that does not require a recalculation
        //  of the MeshGeometry3D.
        static void ColorPropertyChanged(DependencyObject obj,
                                         DependencyPropertyChangedEventArgs args)
        {
            WireBase wirebase = obj as WireBase;
            GeometryModel3D model = wirebase.Content as GeometryModel3D;
            MaterialGroup matgrp = model.Material as MaterialGroup;
            EmissiveMaterial mat = matgrp.Children[1] as EmissiveMaterial;
            mat.Brush = new SolidColorBrush((Color)args.NewValue);
        }

        /// <summary>
        ///     Identifies the Thickness dependency property.
        /// </summary>
        public static readonly DependencyProperty ThicknessProperty =
            DependencyProperty.Register("Thickness",
            typeof(double),
            typeof(WireBase),
            new PropertyMetadata(1.0, RecalcPropertyChanged));

        /// <summary>
        /// 
        /// </summary>
        public double Thickness
        {
            set { SetValue(ThicknessProperty, value); }
            get { return (double)GetValue(ThicknessProperty); }
        }


        // Rounding property        -- CHECK FOR VALUES < ZERO
        // -----------------
        public static readonly DependencyProperty RoundingProperty =
            DependencyProperty.Register("Rounding",
            typeof(int),
            typeof(WireBase),
            new PropertyMetadata(0, RecalcPropertyChanged));

        public int Rounding
        {
            set { SetValue(RoundingProperty, value); }
            get { return (int)GetValue(RoundingProperty); }
        }


        /// <summary>
        ///     Identifies the ArrowAngle dependency property.
        /// </summary>
        public static readonly DependencyProperty ArrowAngleProperty =
            DependencyProperty.Register("ArrowAngle",
                typeof(double), typeof(WireBase),
                new PropertyMetadata(45.0, RecalcPropertyChanged));

        /// <summary>
        ///     Gets or sets the angle between the two sides of the arrowhead.
        /// </summary>
        public double ArrowAngle
        {
            set { SetValue(ArrowAngleProperty, value); }
            get { return (double)GetValue(ArrowAngleProperty); }
        }

        /// <summary>
        ///     Identifies the ArrowLength dependency property.
        /// </summary>
        public static readonly DependencyProperty ArrowLengthProperty =
            DependencyProperty.Register("ArrowLength",
                typeof(double), typeof(WireBase),
                new PropertyMetadata(12.0, RecalcPropertyChanged));

        /// <summary>
        ///     Gets or sets the length of the two sides of the arrowhead.
        /// </summary>
        public double ArrowLength
        {
            set { SetValue(ArrowLengthProperty, value); }
            get { return (double)GetValue(ArrowLengthProperty); }
        }

        /// <summary>
        ///     Identifies the ArrowEnds dependency property.
        /// </summary>
        public static readonly DependencyProperty ArrowEndsProperty =
            DependencyProperty.Register("ArrowEnds",
                typeof(ArrowEnds), typeof(WireBase),
                new PropertyMetadata(ArrowEnds.None, RecalcPropertyChanged));

        /// <summary>
        ///     Gets or sets the property that determines which ends of the
        ///     line have arrows.
        /// </summary>
        public ArrowEnds ArrowEnds
        {
            set { SetValue(ArrowEndsProperty, value); }
            get { return (ArrowEnds)GetValue(ArrowEndsProperty); }
        }


        static void RecalcPropertyChanged(DependencyObject obj,
                                          DependencyPropertyChangedEventArgs args)
        {
            WireBase linebase = obj as WireBase;
            linebase.needRecalculation = true;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="args"></param>
        protected static void PropertyChanged(DependencyObject obj,
                                              DependencyPropertyChangedEventArgs args)
        {
            (obj as WireBase).PropertyChanged(args);
        }

        protected void PropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            Point3DCollection lines = LineCollection;
            LineCollection = null;

            Generate(args, lines);

            LineCollection = lines;
        }

        /// <summary>
        ///     Sets the coordinates of all the individual lines in the visual.
        /// </summary>
        /// <param name="args">
        ///     The <c>DependencyPropertyChangedEventArgs</c> object associated 
        ///     with the property-changed event that resulted in this method 
        ///     being called.
        /// </param>
        /// <param name="lines">
        ///     The <c>Point3DCollection</c> to be filled.
        /// </param>
        /// <remarks>
        ///     <para>
        ///         Classes that derive from <c>WireBase</c> override this
        ///         method to fill the <c>lines</c> collection.
        ///         It is custmary for implementations of this method to clear
        ///         the <c>lines</c> collection first before filling it. 
        ///         Each pair of successive members of the <c>lines</c>
        ///         collection indicate one straight line.
        ///     </para>
        /// </remarks>
        protected abstract void Generate(DependencyPropertyChangedEventArgs args,
                                         Point3DCollection lines);


        static void OnRendering(object sender, EventArgs args)
        {
            foreach (WireBaseAndUltimateParent wirebaseAndParent in listWireBases)
            {
                WireBase wirebase = wirebaseAndParent.wirebase;

                //disabled for winforms
                //DependencyObject obj = wirebase;
                //while (obj != null && !(obj is Window))
                //{
                //    obj = VisualTreeHelper.GetParent(obj);
                //}

                //if (wirebaseAndParent.window == null)
                //{
                //    if (obj != null)
                //    {
                //        wirebaseAndParent.window = obj as Window;
                //    }
                //    // Otherwise, the WireBase has no ultimate parent of type window,
                //    //  so there's no reason to try rendering it.
                //    else
                //    {
                //        continue;
                //    }
                //}

                //// A non-null 'window' field means the WireBase had an ultimate window
                ////  parent at one time. 
                //else
                //{
                //    // But now there's no ultimate window parent, so it's likely that
                //    //  this WireBase has been disconnected from a valid visual tree.
                //    if (obj == null)
                //    {
                //        listRemove.Add(wirebaseAndParent);
                //        continue;
                //    }
                //}

                wirebase.OnRendering();
            }

            // Possibly remove objects from the rendering list.
            if (listRemove.Count > 0)
            {
                foreach (WireBaseAndUltimateParent wirebaseAndParent in listRemove)
                {
                    listWireBases.Remove(wirebaseAndParent);
                }
                listRemove.Clear();
            }
        }

        void OnRendering()
        {
            if (LineCollection.Count == 0)
            {
                return;
            }

            Matrix3D matx = VisualInfo.GetTotalTransform(this);

            if (matx == VisualInfo.ZeroMatrix)
            {
                return;
            }

            // How can this happen????
            if (matx.IsIdentity)
            {
                return;
            }

            if (matx != matxVisualToScreen)
            {
                matxVisualToScreen = matx;
                matxScreenToVisual = matx;

                if (matxScreenToVisual.HasInverse)
                {
                    matxScreenToVisual.Invert();                // might not be possible !!!!!!
                    needRecalculation = true;
                }
                else
                    throw new ApplicationException("Here's where the problem is");

            }

            if (needRecalculation)
            {
                Recalculate();
                needRecalculation = false;
            }
        }

        void Recalculate()
        {
            GeometryModel3D model = Content as GeometryModel3D;
            MeshGeometry3D mesh = model.Geometry as MeshGeometry3D;
            Point3DCollection points = mesh.Positions;
            mesh.Positions = null;
            points.Clear();

            Int32Collection indices = mesh.TriangleIndices;
            mesh.TriangleIndices = null;
            indices.Clear();

            int indicesBase = 0;
            Point3DCollection lines = LineCollection;

            for (int line = 0; line < lines.Count - 1; line += 2)
            {
                Point3D pt1 = lines[line + 0];
                Point3D pt2 = lines[line + 1];

                DoLine(pt1, pt2, points, indices, ref indicesBase);

                if (line == 0 &&
                        (ArrowEnds & ArrowEnds.Start) == ArrowEnds.Start)
                    DoArrow(pt2, pt1, points, indices, ref indicesBase);

                if (line > lines.Count - 4 &&
                        (ArrowEnds & ArrowEnds.End) == ArrowEnds.End)
                    DoArrow(pt1, pt2, points, indices, ref indicesBase);
            }

            mesh.TriangleIndices = indices;
            mesh.Positions = points;
        }

        void DoArrow(Point3D pt1, Point3D pt2, Point3DCollection points,
                     Int32Collection indices, ref int indicesBase)
        {
            Point3D pt1Screen = pt1 * matxVisualToScreen;
            Point3D pt2Screen = pt2 * matxVisualToScreen;

            Vector vectArrow = new Vector(pt1Screen.X - pt2Screen.X,
                                          pt1Screen.Y - pt2Screen.Y);
            vectArrow.Normalize();
            vectArrow *= ArrowLength;

            Matrix matx = new Matrix();
            matx.Rotate(ArrowAngle / 2);
            Point3D ptArrow1 = Widen(pt2, vectArrow * matx);
            matx.Rotate(-ArrowAngle);
            Point3D ptArrow2 = Widen(pt2, vectArrow * matx);

            DoLine(pt2, ptArrow1, points, indices, ref indicesBase);
            DoLine(pt2, ptArrow2, points, indices, ref indicesBase);
        }

        void DoLine(Point3D pt1, Point3D pt2, Point3DCollection points,
                    Int32Collection indices, ref int indicesBase)
        {
            Point3D pt1Screen = pt1 * matxVisualToScreen;
            Point3D pt2Screen = pt2 * matxVisualToScreen;

            Vector3D vectLine = pt2Screen - pt1Screen;
            vectLine.Z = 0;
            vectLine.Normalize();

            Vector delta = (Thickness / 2) * new Vector(-vectLine.Y, vectLine.X);

            points.Add(Widen(pt1, delta));
            points.Add(Widen(pt1, -delta));
            points.Add(Widen(pt2, delta));
            points.Add(Widen(pt2, -delta));

            indices.Add(indicesBase);
            indices.Add(indicesBase + 2);
            indices.Add(indicesBase + 1);
            indices.Add(indicesBase + 1);
            indices.Add(indicesBase + 2);
            indices.Add(indicesBase + 3);

            indicesBase += 4;

            if (Rounding > 0)
            {
                AddRounding(pt1, delta, points, indices, ref indicesBase);
                AddRounding(pt2, -delta, points, indices, ref indicesBase);
            }
        }

        Point3D Widen(Point3D pointIn, Vector delta)
        {
            Point4D pt4In = (Point4D)pointIn;
            Point4D pt4Out = pt4In * matxVisualToScreen;

            pt4Out.X += delta.X * pt4Out.W;
            pt4Out.Y += delta.Y * pt4Out.W;

            pt4Out *= matxScreenToVisual;

            Point3D ptOut = new Point3D(pt4Out.X / pt4Out.W,
                                        pt4Out.Y / pt4Out.W,
                                        pt4Out.Z / pt4Out.W);
            return ptOut;
        }

        void AddRounding(Point3D ptIn, Vector delta, Point3DCollection points,
                         Int32Collection indices, ref int indicesCount)
        {
            points.Add(CalculatePoint(ptIn, new Vector(0, 0), 0));

            for (int i = 0; i <= Rounding; i++)
                points.Add(CalculatePoint(ptIn, delta, 180 * i / Rounding));

            for (int i = 0; i < Rounding; i++)
            {
                indices.Add(indicesCount);
                indices.Add(indicesCount + i + 2);
                indices.Add(indicesCount + i + 1);
            }

            indicesCount += Rounding + 2;
        }

        Point3D CalculatePoint(Point3D ptIn, Vector delta, double angle)
        {
            Point4D pt4In = (Point4D)ptIn;
            Point4D pt4Out = pt4In * matxVisualToScreen;

            rotate.Angle = angle;
            delta = (Vector)rotate.Transform((Point)delta);

            pt4Out.X += delta.X * pt4Out.W;
            pt4Out.Y += delta.Y * pt4Out.W;

            pt4Out *= matxScreenToVisual;

            Point3D ptOut = new Point3D(pt4Out.X / pt4Out.W,
                                        pt4Out.Y / pt4Out.W,
                                        pt4Out.Z / pt4Out.W);
            return ptOut;
        }
    }
}