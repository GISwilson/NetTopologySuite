using System;
using System.Collections;
using System.Text;

using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.Utilities;

namespace GisSharpBlog.NetTopologySuite.Noding
{

    /// <summary>
    /// Wraps a <see cref="Noder" /> and transforms its input into the integer domain.
    /// This is intended for use with Snap-Rounding noders,
    /// which typically are only intended to work in the integer domain.
    /// Offsets can be provided to increase the number of digits of available precision.
    /// </summary>
    public class ScaledNoder : INoder
    {
        private INoder noder = null;
        private double scaleFactor = 0;
        private double offsetX = 0;
        private double offsetY = 0;
        private bool isScaled = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScaledNoder"/> class.
        /// </summary>
        /// <param name="noder"></param>
        /// <param name="scaleFactor"></param>
        public ScaledNoder(INoder noder, double scaleFactor) 
            : this(noder, scaleFactor, 0, 0) { }      

        public ScaledNoder(INoder noder, double scaleFactor, double offsetX, double offsetY) 
        {
            this.noder = noder;
            this.scaleFactor = scaleFactor;
            // no need to scale if input precision is already integral
            isScaled = ! isIntegerPrecision;
        }

        /// <summary>
        /// 
        /// </summary>
        public bool isIntegerPrecision
        { 
            get
            {
                return scaleFactor == 1.0;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IList GetNodedSubstrings()
        {
            IList splitSS = noder.GetNodedSubstrings();
            if (isScaled) 
                Rescale(splitSS);
            return splitSS;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputSegStrings"></param>
        public void ComputeNodes(IList inputSegStrings)
        {
            IList intSegStrings = inputSegStrings;
            if(isScaled)
                intSegStrings = Scale(inputSegStrings);
            noder.ComputeNodes(intSegStrings);
        }

        /// <summary>
        /// 
        /// </summary>
        private class TrasformFunction : CollectionUtil.Function
        {
            private ScaledNoder noder = null;

            /// <summary>
            /// Initializes a new instance of the <see cref="TrasformFunction" /> class.
            /// </summary>
            /// <param name="noder"></param>
            public TrasformFunction(ScaledNoder noder)
            {
                this.noder = noder;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="obj"></param>
            /// <returns></returns>
            public object Execute(Object obj) 
            {
                SegmentString ss = (SegmentString) obj;
                return new SegmentString(noder.Scale(ss.Coordinates), ss.Data);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="segStrings"></param>
        /// <returns></returns>
        private IList Scale(IList segStrings)
        {
            return CollectionUtil.Transform(segStrings, new TrasformFunction(this));
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pts"></param>
        /// <returns></returns>
        private Coordinate[] Scale(Coordinate[] pts)
        {
            Coordinate[] roundPts = new Coordinate[pts.Length];
            for (int i = 0; i < pts.Length; i++)
                roundPts[i] = new Coordinate(Math.Round((pts[i].X - offsetX) * scaleFactor),
                                             Math.Round((pts[i].Y - offsetY) * scaleFactor));            
            return roundPts;
        }

        private class ApplyFunction : CollectionUtil.Function
        {
            private ScaledNoder noder = null;

            /// <summary>
            /// Initializes a new instance of the <see cref="ApplyFunction" /> class.
            /// </summary>
            /// <param name="noder"></param>
            public ApplyFunction(ScaledNoder noder)
            {
                this.noder = noder;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="obj"></param>
            /// <returns></returns>
            public Object Execute(object obj) 
            {
                SegmentString ss = (SegmentString) obj;
                noder.Rescale(ss.Coordinates);
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="segStrings"></param>
        private void Rescale(IList segStrings)
        {
            CollectionUtil.Apply(segStrings, new ApplyFunction(this));                                           
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pts"></param>
        private void Rescale(Coordinate[] pts)
        {
            for (int i = 0; i < pts.Length; i++) 
            {
                pts[i].X = pts[i].X / scaleFactor + offsetX;
                pts[i].Y = pts[i].Y / scaleFactor + offsetY;
            }
        }

    }
}
