#ifndef POI_COLOR
    #define POI_COLOR
    
    #ifndef pi
        #define pi float(3.14159265359)
    #endif
    
    static const float Epsilon = 1e-10;
    // The weights of RGB contributions to luminance.
    // Should sum to unity.
    static const float3 HCYwts = float3(0.299, 0.587, 0.114);
    static const float HCLgamma = 3;
    static const float HCLy0 = 100;
    static const float HCLmaxL = 0.530454533953517; // == exp(HCLgamma / HCLy0) - 0.5
    static const float3 wref = float3(1.0, 1.0, 1.0);
    #define TAU 6.28318531
    
    float3 HUEtoRGB(in float H)
    {
        float R = abs(H * 6 - 3) - 1;
        float G = 2 - abs(H * 6 - 2);
        float B = 2 - abs(H * 6 - 4);
        return saturate(float3(R, G, B));
    }
    
    float3 RGBtoHCV(in float3 RGB)
    {
        // Based on work by Sam Hocevar and Emil Persson
        float4 P = (RGB.g < RGB.b) ? float4(RGB.bg, -1.0, 2.0 / 3.0): float4(RGB.gb, 0.0, -1.0 / 3.0);
        float4 Q = (RGB.r < P.x) ? float4(P.xyw, RGB.r): float4(RGB.r, P.yzx);
        float C = Q.x - min(Q.w, Q.y);
        float H = abs((Q.w - Q.y) / (6 * C + Epsilon) + Q.z);
        return float3(H, C, Q.x);
    }
    
    float3 HSVtoRGB(in float3 HSV)
    {
        float3 RGB = HUEtoRGB(HSV.x);
        return((RGB - 1) * HSV.y + 1) * HSV.z;
    }
    
    float3 RGBtoHSV(in float3 RGB)
    {
        float3 HCV = RGBtoHCV(RGB);
        float S = HCV.y / (HCV.z + Epsilon);
        return float3(HCV.x, S, HCV.z);
    }
    
    float3 HSLtoRGB(in float3 HSL)
    {
        float3 RGB = HUEtoRGB(HSL.x);
        float C = (1 - abs(2 * HSL.z - 1)) * HSL.y;
        return(RGB - 0.5) * C + HSL.z;
    }
    
    float3 RGBtoHSL(in float3 RGB)
    {
        float3 HCV = RGBtoHCV(RGB);
        float L = HCV.z - HCV.y * 0.5;
        float S = HCV.y / (1 - abs(L * 2 - 1) + Epsilon);
        return float3(HCV.x, S, L);
    }
    
    float3 HCYtoRGB(in float3 HCY)
    {
        
        
        float3 RGB = HUEtoRGB(HCY.x);
        float Z = dot(RGB, HCYwts);
        if (HCY.z < Z)
        {
            HCY.y *= HCY.z / Z;
        }
        else if(Z < 1)
        {
            HCY.y *= (1 - HCY.z) / (1 - Z);
        }
        return(RGB - Z) * HCY.y + HCY.z;
    }
    
    float3 RGBtoHCY(in float3 RGB)
    {
        // Corrected by David Schaeffer
        float3 HCV = RGBtoHCV(RGB);
        float Y = dot(RGB, HCYwts);
        float Z = dot(HUEtoRGB(HCV.x), HCYwts);
        if (Y < Z)
        {
            HCV.y *= Z / (Epsilon + Y);
        }
        else
        {
            HCV.y *= (1 - Z) / (Epsilon + 1 - Y);
        }
        return float3(HCV.x, HCV.y, Y);
    }
    
    float3 HCLtoRGB(in float3 HCL)
    {
        float3 RGB = 0;
        if(HCL.z != 0)
        {
            float H = HCL.x;
            float C = HCL.y;
            float L = HCL.z * HCLmaxL;
            float Q = exp((1 - C / (2 * L)) * (HCLgamma / HCLy0));
            float U = (2 * L - C) / (2 * Q - 1);
            float V = C / Q;
            float A = (H + min(frac(2 * H) / 4, frac(-2 * H) / 8)) * pi * 2;
            float T;
            H *= 6;
            if(H <= 0.999)
            {
                T = tan(A);
                RGB.r = 1;
                RGB.g = T / (1 + T);
            }
            else if(H <= 1.001)
            {
                RGB.r = 1;
                RGB.g = 1;
            }
            else if(H <= 2)
            {
                T = tan(A);
                RGB.r = (1 + T) / T;
                RGB.g = 1;
            }
            else if(H <= 3)
            {
                T = tan(A);
                RGB.g = 1;
                RGB.b = 1 + T;
            }
            else if(H <= 3.999)
            {
                T = tan(A);
                RGB.g = 1 / (1 + T);
                RGB.b = 1;
            }
            else if(H <= 4.001)
            {
                RGB.g = 0;
                RGB.b = 1;
            }
            else if(H <= 5)
            {
                T = tan(A);
                RGB.r = -1 / T;
                RGB.b = 1;
            }
            else
            {
                T = tan(A);
                RGB.r = 1;
                RGB.b = -T;
            }
            RGB = RGB * V + U;
        }
        return RGB;
    }
    
    float3 RGBtoHCL(in float3 RGB)
    {
        float3 HCL;
        float H = 0;
        float U = min(RGB.r, min(RGB.g, RGB.b));
        float V = max(RGB.r, max(RGB.g, RGB.b));
        float Q = HCLgamma / HCLy0;
        HCL.y = V - U;
        if(HCL.y != 0)
        {
            H = atan2(RGB.g - RGB.b, RGB.r - RGB.g) / pi;
            Q *= U / V;
        }
        Q = exp(Q);
        HCL.x = frac(H / 2 - min(frac(H), frac(-H)) / 6);
        HCL.y *= Q;
        HCL.z = lerp(-U, V, Q) / (HCLmaxL * 2);
        return HCL;
    }
    
    //HSL MODIFT
    float3 ModifyViaHSL(float3 color, float3 HSLMod)
    {
        float3 colorHSL = RGBtoHSL(color);
        colorHSL.r = frac(colorHSL.r + HSLMod.r);
        colorHSL.g = saturate(colorHSL.g + HSLMod.g);
        colorHSL.b = saturate(colorHSL.b + HSLMod.b);
        return HSLtoRGB(colorHSL);
    }
    
    float3 hueShift(float3 col, float hueAdjust)
    {
        hueAdjust *= 2 * pi;
        const float3 k = float3(0.57735, 0.57735, 0.57735);
        half cosAngle = cos(hueAdjust);
        return col * cosAngle + cross(k, col) * sin(hueAdjust) + k * dot(k, col) * (1.0 - cosAngle);
    }
    
    float3 poiSaturation(float3 In, float Saturation)
    {
        float luma = dot(In, float3(0.2126729, 0.7151522, 0.0721750));
        return luma.xxx + Saturation.xxx * (In - luma.xxx);
    }
    // LCH
    float xyzF(float t)
    {
        return lerp(pow(t, 1. / 3.), 7.787037 * t + 0.139731, step(t, 0.00885645));
    }
    float xyzR(float t)
    {
        return lerp(t * t * t, 0.1284185 * (t - 0.139731), step(t, 0.20689655));
    }
    float3 rgb2lch(in float3 c)
    {
        c = mul(float3x3(0.4124, 0.3576, 0.1805,
        0.2126, 0.7152, 0.0722,
        0.0193, 0.1192, 0.9505), c);
        c.x = xyzF(c.x / wref.x);
        c.y = xyzF(c.y / wref.y);
        c.z = xyzF(c.z / wref.z);
        float3 lab = float3(max(0., 116.0 * c.y - 16.0), 500.0 * (c.x - c.y), 200.0 * (c.y - c.z));
        return float3(lab.x, length(float2(lab.y, lab.z)), atan2(lab.z, lab.y));
    }
    
    float3 lch2rgb(in float3 c)
    {
        c = float3(c.x, cos(c.z) * c.y, sin(c.z) * c.y);
        
        float lg = 1. / 116. * (c.x + 16.);
        float3 xyz = float3(wref.x * xyzR(lg + 0.002 * c.y),
        wref.y * xyzR(lg),
        wref.z * xyzR(lg - 0.005 * c.z));
        
        float3 rgb = mul(float3x3(3.2406, -1.5372, -0.4986,
        - 0.9689, 1.8758, 0.0415,
        0.0557, -0.2040, 1.0570), xyz);
        
        return rgb;
    }
    
    //cheaply lerp around a circle
    float lerpAng(in float a, in float b, in float x)
    {
        float ang = fmod(fmod((a - b), TAU) + pi * 3., TAU) - pi;
        return ang * x + b;
    }
    
    //Linear interpolation between two colors in Lch space
    float3 lerpLch(in float3 a, in float3 b, in float x)
    {
        float hue = lerpAng(a.z, b.z, x);
        return float3(lerp(b.xy, a.xy, x), hue);
    }
    
    float3 poiExpensiveColorBlend(float3 col1, float3 col2, float alpha)
    {
        return lch2rgb(lerpLch(rgb2lch(col1), rgb2lch(col2), alpha));
    }
    
#endif