using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;

namespace WinFormsHostForArcGIS
{
    public partial class Form1 : Form
    {
        private const int GraphicCount = 50000;
        private readonly Random mRandom = new Random(1);
        private readonly GraphicsOverlay mOverlay;
        private readonly Timer mTimer = new Timer();
        private readonly List<Graphic> mGraphics = new List<Graphic>(GraphicCount);
        private bool mGraphicsCreated;
        private bool mRemoveGraphics;
        private int mCount;

        public Form1()
        {
            InitializeComponent();
            var mapView = new MapView();
            mOverlay = new GraphicsOverlay();
            mapView.Map = new Map(Basemap.CreateTopographic());
            mElementHost.Child = mapView;
            mapView.GraphicsOverlays.Add(mOverlay);

            mTimer.Tick += (sender, args) => SwitchGraphics();
            mTimer.Interval = 1000;
            mTimer.Start();
        }

        private void GenerateRandomPoints(int count)
        {
            const double xMax = 40;
            const double yMax = 20;

            for (uint i = 0; i < count; i++)
            {
                var polarityX = mRandom.Next(2) == 0 ? -1 : 1;
                var polarityY = mRandom.Next(2) == 0 ? -1 : 1;
                var randomX = mRandom.NextDouble() * xMax * polarityX;
                var randomY = mRandom.NextDouble() * yMax * polarityY;
                var p = new MapPoint(randomX, randomY, SpatialReferences.Wgs84);
                var simpleMarkerSymbol = SymbolFactory.CreateEntityCompositeSymbol($"Person {i}");
                mGraphics.Add(new Graphic(p, simpleMarkerSymbol));
            }
        }

        private void SwitchGraphics()
        {
            Debug.WriteLine($"SwitchGraphics call count = {mCount++}");

            if (!mGraphicsCreated)
            {
                GenerateRandomPoints(GraphicCount);
                mGraphicsCreated = true;
            }

            if (mRemoveGraphics)
            {
                mOverlay.Graphics.Clear();
                mRemoveGraphics = false;
                return;
            }

            mRemoveGraphics = true;

            mOverlay.Graphics.AddRange(mGraphics);
        }
    }
}
