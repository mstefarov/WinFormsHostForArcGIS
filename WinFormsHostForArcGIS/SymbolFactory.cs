using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Color = System.Drawing.Color;

namespace WinFormsHostForArcGIS
{
    public static class SymbolFactory
    {
        #region Fields

        private const int TextOutlineSize = 3;
        private const int SymbolSpacer = 3;
        private const int AlphaChannelValue = 32;
        private const int TriangleSymbolAngle = 180;
        private const int TriangleSymbolSize = 7;

        private static readonly Color sTriangleSymbolColor = Color.FromArgb(0, 0, 0);
        private static readonly Color sTriangleSymbolHighlightlColor = Color.FromArgb(255, 255, 255);
        private static readonly Color sTextOutlineColor = Color.FromArgb(220, 255, 255, 255);

        private static readonly Image sScaledBitmap = new Bitmap(Properties.Resources.House, new Size(32, 32));


        #endregion Fields

        #region Public Methods

        public static Renderer CreateIconRenderer()
        {
            var values = new List<UniqueValue>
            {
                new UniqueValue("Normal", "Normal", CreateEntityCompositeSymbol("-", false), Values(1, 0)),
                new UniqueValue("Grayed", "Grayed", CreateEntityCompositeSymbol("-", true), Values(1, 1)),
                new UniqueValue("Normal2", "Normal2", CreateEntityCompositeSymbol("-\n-", false), Values(2, 0)),
                new UniqueValue("Grayed2", "Grayed2", CreateEntityCompositeSymbol("-\n-", true), Values(2, 1)),
                new UniqueValue("Normal3", "Normal3", CreateEntityCompositeSymbol("-\n-\n-", false), Values( 3, 0)),
                new UniqueValue("Grayed3", "Grayed3", CreateEntityCompositeSymbol("-\n-\n-", true), Values(3, 1)),
            };
            var uvr = new UniqueValueRenderer(
                new[] { Form1.LineCountAttribute, Form1.IsGrayedAttribute}, // attributes used to select the symbol
                values,
                defaultLabel: values[0].Label,
                defaultSymbol: values[0].Symbol
            );
            return uvr;
        }

        public static CompositeSymbol CreateEntityCompositeSymbol(string label, bool isGrayed)
        {
            // Composite symbol is used to collate all the constituent parts
            var compositeSymbol = new CompositeSymbol();

            // Reversed triangle symbol
            var triangleSymbol = CreatePointMarker(isGrayed);

            // Map point marker
            compositeSymbol.Symbols.Add(triangleSymbol);

            // Entity label
            double textHeight;
            var entityLabelSymbol = CreateEntityLabelSymbol(label, triangleSymbol, isGrayed, out textHeight);
            //compositeSymbol.Symbols.Add(entityLabelSymbol); // Don't add text symbols (we'll get to this later)

            // Entity icon symbol
            var entityIconSymbol = CreateEntityIconSymbol(entityLabelSymbol, textHeight);
            compositeSymbol.Symbols.Add(entityIconSymbol);

            return compositeSymbol;
        }

        public static TextSymbol CreateLabelSymbol(string label, bool isGrayed)
        {
            // The triangle is only used to get the correct OffsetY
            var triangleSymbol = CreatePointMarker(isGrayed);

            var entityLabelSymbol = CreateEntityLabelSymbol(label, triangleSymbol, isGrayed, out _);
            return entityLabelSymbol;
        }

        #endregion Public Methods

        #region Private Methods

        // Convenience method for wrapping arbitrary object lists in an IEnumerable
        private static IEnumerable<object> Values(params object[] values) => values;

        private static SimpleMarkerSymbol CreatePointMarker(bool isGrayed)
        {
            var symbolColor = isGrayed
                                    ? Color.FromArgb(AlphaChannelValue, sTriangleSymbolColor)
                                    : sTriangleSymbolColor;

            var triangleSymbol = new SimpleMarkerSymbol
            {
                Color = symbolColor,
                Style = SimpleMarkerSymbolStyle.Triangle,
                Angle = TriangleSymbolAngle,
                Size = TriangleSymbolSize
            };

            triangleSymbol.OffsetY = -(triangleSymbol.Size / 0.75) / 2;

            Color symbolOutlineColor = isGrayed
                                        ? Color.FromArgb(AlphaChannelValue, sTriangleSymbolHighlightlColor)
                                        : sTriangleSymbolHighlightlColor;

            triangleSymbol.Outline = new SimpleLineSymbol
            {
                Color = symbolOutlineColor,
                Width = 1
            };

            return triangleSymbol;
        }

        private static TextSymbol CreateEntityLabelSymbol(string text, SimpleMarkerSymbol pointMarker, bool isGrayed, out double textHeight)
        {
            var entityLabelSymbol = new TextSymbol
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                Text = text
            };

            if (text.Length > 0)
            {
                var lineCount = entityLabelSymbol.Text.Split('\n').Length;

                var fontFamily = new System.Windows.Media.FontFamily(entityLabelSymbol.FontFamily);

                var height = fontFamily.LineSpacing * entityLabelSymbol.Size;

                textHeight = lineCount * height;

                if (pointMarker != null)
                {
                    entityLabelSymbol.OffsetY = Math.Abs(pointMarker.OffsetY) + (pointMarker.Size + textHeight / 2);
                }
            }
            else
            {
                entityLabelSymbol.Text = String.Empty;
                textHeight = 0;
                if (pointMarker != null)
                {
                    entityLabelSymbol.OffsetY = Math.Abs(pointMarker.OffsetY);
                }
            }

            entityLabelSymbol.Color = Color.Black;

            entityLabelSymbol.HaloColor = sTextOutlineColor;

            // To workaround an Esri issue with TextSymbol selection with transparency where the text has unwanted ghosting effect.
            // The BorderLineSize is set to zero when entity symbol is determined to be grayed to improve the visibility.
            entityLabelSymbol.HaloWidth = isGrayed ? 0 : TextOutlineSize;

            return entityLabelSymbol;
        }

        //private static readonly Bitmap sBitmap = Properties.Resources.House;

        public static byte[] ToByteArray(Image image, ImageFormat format)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, format);
                return ms.ToArray();
            }
        }


        private static PictureMarkerSymbol CreateEntityIconSymbol(TextSymbol entityLabel, double textHeight)
        {
            var entityRuntimeImage = new RuntimeImage(ToByteArray(sScaledBitmap, ImageFormat.Png));

            var iconSymbol = new PictureMarkerSymbol(entityRuntimeImage)
            {
                Height = 32,
                Width = 32
            };

            iconSymbol.OffsetY = textHeight > 0 ?
                                    entityLabel.OffsetY + (textHeight / 2 + iconSymbol.Height / 2 + SymbolSpacer) :
                                    entityLabel.OffsetY + (iconSymbol.Height / 2 + SymbolSpacer);

            return iconSymbol;
        }

        #endregion Private Methods
    }
}
