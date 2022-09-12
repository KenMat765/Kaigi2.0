using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utilities
{
    namespace ScreenCanvasExtention
    {
        public static class ScreenCanvas
        {
            static float sw = Screen.width, sh = Screen.height, cw = CanvasInfo.width, ch = CanvasInfo.height;
            public static Vector3 Screen2CanvasPos(this Vector3 screen_pos)
            {
                float X = screen_pos.x * cw / sw - cw / 2;
                float Y = screen_pos.y * ch / sh - ch / 2;
                Vector2 canvas_pos = new Vector2(X, Y);
                return canvas_pos;
            }
            public static Vector3 Canvas2ScreenPos(this Vector3 canvas_pos)
            {
                float X = canvas_pos.x * sw / cw + sw / 2;
                float Y = canvas_pos.y * sh / ch + sh / 2;
                Vector2 screen_pos = new Vector2(X, Y);
                return screen_pos;
            }
            public static Vector3 RescaleScreen2Canvas(this Vector3 screen_vec)
            {
                float rescale_X = screen_vec.x * cw / sw;
                float rescale_Y = screen_vec.y * ch / sh;
                Vector3 canvas_vec = new Vector3(rescale_X, rescale_Y);
                return canvas_vec;
            }
            public static Vector3 RescaleCanvas2Screen(this Vector3 canvas_vec)
            {
                float rescale_X = canvas_vec.x * sw / cw;
                float rescale_Y = canvas_vec.y * sh / ch;
                Vector3 screen_vec = new Vector3(rescale_X, rescale_Y);
                return screen_vec;
            }
        }
    }
}
