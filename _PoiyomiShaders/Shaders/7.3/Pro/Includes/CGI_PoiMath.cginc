/*
MIT License

Copyright (c) 2019 wraikny

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

VertexTransformShader is dependent on:
*/

#ifndef POI_MATH
#define POI_MATH

#ifndef pi
    #define pi float(3.14159265359)
#endif

float4 quaternion_conjugate(float4 v)
{
    return float4(
        v.x, -v.yzw
    );
}

float4 quaternion_mul(float4 v1, float4 v2)
{
    float4 result1 = (v1.x * v2 + v1 * v2.x);
    
    float4 result2 = float4(
        - dot(v1.yzw, v2.yzw),
        cross(v1.yzw, v2.yzw)
    );
    
    return float4(result1 + result2);
}

// angle : radians
float4 get_quaternion_from_angle(float3 axis, float angle)
{
    return float4(
        cos(angle / 2.0),
        normalize(axis) * sin(angle / 2.0)
    );
}

float4 quaternion_from_vector(float3 inVec)
{
    return float4(0.0, inVec);
}

float degree_to_radius(float degree)
{
    return(
        degree / 180.0 * pi
    );
}

float3 rotate_with_quaternion(float3 inVec, float3 rotation)
{
    float4 qx = get_quaternion_from_angle(float3(1, 0, 0), degree_to_radius(rotation.x));
    float4 qy = get_quaternion_from_angle(float3(0, 1, 0), degree_to_radius(rotation.y));
    float4 qz = get_quaternion_from_angle(float3(0, 0, 1), degree_to_radius(rotation.z));
    
    #define MUL3(A, B, C) quaternion_mul(quaternion_mul((A), (B)), (C))
    float4 quaternion = normalize(MUL3(qx, qy, qz));
    float4 conjugate = quaternion_conjugate(quaternion);
    
    float4 inVecQ = quaternion_from_vector(inVec);
    
    float3 rotated = (
        MUL3(quaternion, inVecQ, conjugate)
    ).yzw;
    
    return rotated;
}

float4 transform(float4 input, float4 pos, float4 rotation, float4 scale)
{
    input.rgb *= (scale.xyz * scale.w);
    input = float4(rotate_with_quaternion(input.xyz, rotation.xyz/* * rotation.w*/) + (pos.xyz/* * pos.w*/), input.w);
    return input;
}

#endif