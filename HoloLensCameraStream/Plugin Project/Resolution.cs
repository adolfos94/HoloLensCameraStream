//
// Copyright (c) 2017 Vulcan, Inc. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
//

namespace HoloLensCameraStream
{
    /// <summary>
    /// A structure for holding a resolution.
    /// </summary>
    public struct Resolution
    {
        /// <summary>
        /// The width property.
        /// </summary>
        public readonly int width;

        /// <summary>
        /// The height property.
        /// </summary>
        public readonly int height;

        /// <summary>
        /// The frame rate property.
        /// </summary>
        public readonly int frameRate;

        /// <summary>
        /// The aspect ratio property.
        /// </summary>
        public readonly float aspectRatio;

        public Resolution(int width, int height, int frameRate)
        {
            this.width = width;
            this.height = height;
            this.frameRate = frameRate;
            this.aspectRatio = (float)width / (float)height;
        }

        /// <summary>
        /// width@height:fps.
        /// </summary>
        public override string ToString()
        {
            string output;
            output = width + "@" + height + ":" + frameRate + "\n";
            return output;
        }
    }
}