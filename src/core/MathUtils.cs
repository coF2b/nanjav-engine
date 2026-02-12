namespace nanjav.core;

internal static class MathUtils
{
    public static float[] CreateOrtho(float left, float right, float bottom, float top, float near, float far)
    {
        var m = new float[16];

        float rl = 1.0f / (right - left);
        float tb = 1.0f / (top - bottom);
        float fn = 1.0f / (far - near);

        m[0] = 2f * rl;
        m[1] = 0f;
        m[2] = 0f;
        m[3] = 0f;

        m[4] = 0f;
        m[5] = 2f * tb;
        m[6] = 0f;
        m[7] = 0f;

        m[8] = 0f;
        m[9] = 0f;
        m[10] = -2f * fn;
        m[11] = 0f;

        m[12] = -(right + left) * rl;
        m[13] = -(top + bottom) * tb;
        m[14] = -(far + near) * fn;
        m[15] = 1f;

        return m;
    }
}
